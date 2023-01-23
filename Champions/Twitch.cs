using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Rendering;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Base;
using SyncWave.Common.Extensions;
using SyncWave.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    #region DamageCalculation
    internal class PassiveTwitchCalc : DamageCalculation
    {
        internal static float PassiveDuration(GameObjectBase enemy)
        {
            List<BuffEntry> buffs = enemy.BuffManager.ActiveBuffs.deepCopy();
            BuffEntry? buff = buffs.FirstOrDefault(x => x.Name == "TwitchDeadlyVenom");
            if (buff == null)
                return 0;
            return buff.EndTime - GameEngine.GameTime;
        }

        internal static float EStacks(GameObjectBase enemy)
        {
            List<BuffEntry> buffs = enemy.BuffManager.ActiveBuffs.deepCopy();
            return buffs.FirstOrDefault(x => x.Name == "TwitchDeadlyVenom")?.Stacks ?? 0;
        }

        internal static float GetDamage(GameObjectBase target)
        {
            float damage = 0;
            float stacks = EStacks(target);
            float APScaling = 0.03F * stacks;
            float RawDamage = (float)(1 * Math.Floor((decimal)(Env.Me().Level / 4))) * stacks;
            damage = (RawDamage + (APScaling * Env.Me().UnitStats.TotalAbilityPower)) * (float)PassiveDuration(target);
            return damage;
        }

        internal override float CalculateDamage(GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, 0, GetDamage(target));
        }
    }

    internal class ETwitchCalc : DamageCalculation
    {
        internal static float EStacks(GameObjectBase enemy)
        {
            List<BuffEntry> buffs = enemy.BuffManager.ActiveBuffs.deepCopy();
            return buffs.FirstOrDefault(x => x.Name == "TwitchDeadlyVenom")?.Stacks ?? 0;
        }

        internal static int[] Damage = new int[] { 0, 20, 30, 40, 50, 60 };
        internal static int[] PerStackDmg = new int[] { 0, 15, 20, 25, 30, 35 };
        internal static float ADScaling = 0.35F;
        internal static float APScaling = 0.35F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            if (Env.EReady && Env.ELevel > 0 && EStacks(target) >= 1)
            {
                float damage = Damage[Env.ELevel];
                float apstackDamage = (Env.Me().UnitStats.TotalAbilityPower * APScaling) * EStacks(target);
                float stackDamage = (PerStackDmg[Env.ELevel] + (Env.Me().UnitStats.BonusAttackDamage * ADScaling)) * EStacks(target);
                return DamageCalculator.CalculateActualDamage(
                    Env.Me(),
                    target,
                    damage + stackDamage,
                    apstackDamage,
                    0
                    );
            }
            return 0;
        }
    }

    #endregion

    internal class Twitch : SyncWave.Base.Module
    {
        #region Static/Stats
        internal static PassiveTwitchCalc PCalc = new();
        internal static ETwitchCalc ECalc = new();

        internal static Damage? _PDamage;
        internal static Damage? _EDamage;

        internal static int[] QManaCost = new int[] { 0, 40, 40, 40, 40, 40 };
        internal static int[] WManaCost = new int[] { 0, 70, 70, 70, 70, 70 };
        internal static int[] EManaCost = new int[] { 0, 50, 60, 70, 80, 90 };
        internal static int[] RManaCost = new int[] { 0, 100, 100, 100 };

        internal static int WRange = 950;
        internal static int WSpeed = 1400;
        internal static float WCastTime = 0.25F;
        internal static int WRadius = 260;

        internal static int ERange = 1200;

        internal static float ECastTime = 0.25F;

        internal static int RRange = 1100;
        internal static int RWidth = 120;
        internal static int RSpeed = 4000;
        #endregion

        internal override void Init()
        {
            Logger.Log("Twitch Initializing...");
            InitMenu();
            CoreEvents.OnCoreMainInputAsync += OnCoreMainInput;
            CoreEvents.OnCoreRender += Draw;
            Render.Init();
            _PDamage = new(TwitchTab, PGroup, "P", (uint)4, PCalc, Color.Green);
            _EDamage = new(TwitchTab, EGroup, "E", (uint)6, ECalc, Color.Red);
            Render.AddDamage(_PDamage);
            Render.AddDamage(_EDamage);
            Logger.Log("Twitch Initialized!");
        }
        internal float QTime()
        {
            List<BuffEntry> Buffs = UnitManager.MyChampion.BuffManager.ActiveBuffs.deepCopy();
            BuffEntry? qBuff = Buffs.FirstOrDefault(x => x.Name == "TwitchHideInShadows" && x.Stacks >= 1);
            float qTimeRemaining = -1;
            if (qBuff != null)
            {
                qTimeRemaining = qBuff.RemainingDurationMs / 1000;
            }
            return qTimeRemaining;
        }
        private void Draw()
        {
            float qtime = QTime();
            if (QDrawTime.IsOn && qtime != -1)
            {
                if (QUseOffset.IsOn)
                {
                    Vector2 pos = new(XCoord.Value, YCoord.Value);
                    RenderFactory.DrawText($"{qtime.ToString("n2")}s", pos, Color.Black, true);
                }
                else
                {
                    Vector2 pos = Env.Me().Position.ToW2S();
                    pos.Y -= 20;
                    RenderFactory.DrawText($"{qtime.ToString("n2")}s", pos, Color.Black, true);
                }
            }
        }

        private Task OnCoreMainInput()
        {
            if (QEnabled.IsOn && Env.QReady && Env.WLevel >= 1)
            {
                TryCastQ();
            }
            if (WEnabled.IsOn)
            {
                TryCastW();
            }
            if (EEnabled.IsOn && Env.EReady && Env.ELevel >= 1)
            {
                TryCastE();
            }
            return Task.CompletedTask;
        }

        private void TryCastE()
        {
            foreach (GameObjectBase target in UnitManager.EnemyChampions)
            {
                if (target.IsAlive && target.IsVisible)
                {
                    if (target.Health - ECalc.CalculateDamage(target) < 0)
                    {
                        SpellCastProvider.CastSpell(CastSlot.E);
                        break;
                    }
                }
            }
        }

        internal static Prediction.MenuSelected.HitChance GetHitchanceFromName(string name)
        {
            return name.ToLower() switch
            {
                "immobile" => Prediction.MenuSelected.HitChance.Immobile,
                "veryhigh" => Prediction.MenuSelected.HitChance.VeryHigh,
                "high" => Prediction.MenuSelected.HitChance.High,
                "medium" => Prediction.MenuSelected.HitChance.Medium,
                "low" => Prediction.MenuSelected.HitChance.Low,
                "dashing" => Prediction.MenuSelected.HitChance.Dashing,
                "outofrange" => Prediction.MenuSelected.HitChance.OutOfRange,
                "unknown" => Prediction.MenuSelected.HitChance.Unknown,
                _ => Prediction.MenuSelected.HitChance.Impossible
            };
        }
        private void TryCastW()
        {
            GameObjectBase? target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance < WRange);
            if (target != null && Env.WReady && Env.WLevel >= 1)
            {
                Prediction.MenuSelected.PredictionOutput pred = Prediction.MenuSelected.GetPrediction(
                    Prediction.MenuSelected.PredictionType.Circle,
                    target,
                    WRange,
                    WRadius,
                    0,
                    WSpeed,
                    false
                    );
                if (pred.HitChance >= GetHitchanceFromName(WHitChance.SelectedModeName) && target.Position.IsOnScreen())
                {
                    SpellCastProvider.CastSpell(CastSlot.W, target.Position);
                }
            }
        }

        private void TryCastQ()
        {
            foreach (GameObjectBase target in UnitManager.EnemyChampions)
            {
                if (!target.IsAlive || !target.IsVisible)
                    continue;
                if (target.Distance < QRange.Value)
                {
                    SpellCastProvider.CastSpell(CastSlot.Q);
                }
            }
        }


        internal static Tab TwitchTab = new Tab("SyncWave - Twitch");

        internal static Group PGroup = new("Passive Settings");

        internal static Group QGroup = new("Q Settings");
        internal static Switch QEnabled = new("Enabled", true);
        internal static Counter QRange = new("Q Use Range", 1100, 0, 3000);
        internal static Switch QDrawTime = new("Draw Q Time", true);
        internal static Switch QUseOffset = new("Use Coordinates on screen", false);
        internal static Counter XCoord = new("X Coordinate", 960, 0, 3000);
        internal static Counter YCoord = new("Y Coordinate", 540, 0, 3000);
        internal static InfoDisplay Info = new() { Information = "Default draw position is on yourself" };

        internal static Group WGroup = new("W Settings");
        internal static Switch WEnabled = new("Enabled", true);
        internal static ModeDisplay WHitChance = new() { Title = "W Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };

        internal static Group EGroup = new("E Settings");
        internal static Switch EEnabled = new("Enabled", true);

        private void InitMenu()
        {
            MenuManagerProvider.AddTab(TwitchTab);

            TwitchTab.AddGroup(PGroup);

            TwitchTab.AddGroup(QGroup);
            QGroup.AddItem(QEnabled);
            QGroup.AddItem(QRange);
            QGroup.AddItem(QDrawTime);
            QGroup.AddItem(QUseOffset);
            QGroup.AddItem(XCoord);
            QGroup.AddItem(YCoord);
            QGroup.AddItem(Info);

            TwitchTab.AddGroup(WGroup);
            WGroup.AddItem(WEnabled);
            WGroup.AddItem(WHitChance);

            TwitchTab.AddGroup(EGroup);
            EGroup.AddItem(EEnabled);
        }
    }
}