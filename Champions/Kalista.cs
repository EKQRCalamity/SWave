using Oasys.Common.Enums.GameEnums;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.GameObject.ObjectClass;
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
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    #region DamageCalculation

    internal class KalistaQDamageCalc : DamageCalculation
    {
        internal bool QCanTransferToTarget(Prediction.MenuSelected.PredictionOutput prediction)
        {
            foreach (GameObjectBase target in prediction.CollisionObjects)
            {
                if (target.CanKill(CalculateDamage(target)) && target.IsTargetable)
                    continue;
                else
                    return false;
            }
            return true;
        }

        internal override float CalculateDamage(GameObjectBase target)
        {
            float dmg = 0;
            if (Env.QLevel >= 1 && Env.QReady)
            {
                dmg = Kalista.QDamage[Env.QLevel] + (Kalista.QScaling * Env.Me().UnitStats.TotalAttackDamage);
            }
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, dmg, 0);
        }
    }

    internal class KalistaEDamageCalc : DamageCalculation
    {
        internal float GetEStacks(GameObjectBase target)
        {
            List<BuffEntry> buffs = target.BuffManager.ActiveBuffs.deepCopy();
            return buffs.FirstOrDefault(x => x.Name.Contains("kalistaexpungemarker"))?.Stacks ?? 0;
        }

        internal float GetDamageFromStacks(GameObjectBase target, float stacks)
        {
            float damage = 0;
            if (stacks > 0)
            {
                float baseDamage = Champions.Kalista.EDamage[Env.ELevel] + (Champions.Kalista.EScaling * Env.Me().UnitStats.TotalAttackDamage);
                damage = baseDamage + ((Champions.Kalista.EAddSpearDamage[Env.ELevel] + (Champions.Kalista.EAddSpearScaling[Env.ELevel] * Env.Me().UnitStats.TotalAttackDamage)) * (stacks - 1));
                var skin = target.UnitComponentInfo.SkinName.ToLower();
                if (skin.Contains("dragon") || skin.Contains("baron") || skin.Contains("herald"))
                {
                    damage = damage / 2;
                }
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, damage);
            }
            return damage;
        }

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            float stacks = GetEStacks(target);
            if (stacks >= 1)
            {
                damage = GetDamageFromStacks(target, stacks);
            }
            return damage;
        }
    }
    #endregion

    internal class Kalista : SyncWave.Base.Module
    {
        #region Statics/Stats
        internal static KalistaQDamageCalc QCalc = new();

        internal static KalistaEDamageCalc ECalc = new();

        internal static Damage? _QDamage;
        internal static Damage? _EDamage;

        
        internal int TickCycles = 0;
        internal static Hero? BoundAlly { get; set; }

        internal static int[] QManaCost = new int[] { 0, 50, 55, 60, 65, 70 };
        internal static int[] EManaCost = new int[] { 0, 30, 30, 30, 30, 30 };
        internal static int[] RManaCost = new int[] { 0, 100, 100, 100 };

        internal static int[] QDamage = new int[] { 0, 20, 85, 150, 215, 280 };
        internal static float QScaling = 1;
        internal static float QCastTime = 0.25F;
        internal static int QRange = 1200;
        internal static int QSpeed = 2400;
        internal static int QWidth = 80;

        internal static int[] EDamage = new int[] { 0, 20, 30, 40, 50, 60 };
        internal static float EScaling = 0.7F;
        internal static int[] EAddSpearDamage = new int[] { 0, 10, 16, 22, 28, 34 };
        internal static float[] EAddSpearScaling = new float[] { 0, 0.232F, 0.2755F, 0.319F, 0.3625F, 0.406F };
        internal static float ECastTime = 0.25F;
        internal static int ERange = 1100;

        internal static int RTargetRange = 1200;
        internal static int RTetherRadius = 1100;
        #endregion

        internal override void Init()
        {
            Logger.Log("Kalista Initializing...");
            InitMenu();
            CoreEvents.OnCoreMainTick += OnCoreMainTick;
            CoreEvents.OnCoreMainInputAsync += OnCoreMainInput;
            CoreEvents.OnCoreLaneclearInputAsync += OnCoreLaneClear;
            CoreEvents.OnCoreRender += OnCoreRender;
            Render.Init();
            _QDamage = new Damage("Q", (uint)DrawQPrio.Value, QCalc, ColorConverter.GetColor(DrawQColor.SelectedModeName));
            _EDamage = new Damage("E", (uint)DrawEPrio.Value, ECalc, ColorConverter.GetColor(DrawEColor.SelectedModeName));
            Logger.Log("Kalista Initialized!");
            Common.SpellAim.AimSpell Q = new Common.SpellAim.AimSpell(QRange, KalistaTab, CastSlot.Q, SpellSlot.Q);
            Q.SetPrediction(Prediction.MenuSelected.PredictionType.Line, Champions.Kalista.QRange - 40, Champions.Kalista.QWidth, Champions.Kalista.QCastTime, Champions.Kalista.QSpeed, true);
            Render.AddDamage(_QDamage);
            Render.AddDamage( _EDamage);
        }

        #region Menu
        internal static Tab KalistaTab = new Tab("SyncWave - Kalista");

        internal static Group General = new Group("General");
        internal static Switch Enabled = new Switch("Enabled", true);
        internal static Switch LaneClearWoTarget = new Switch("Laneclear w/o target", true);

        internal static Group QGroup = new Group("Q Settings");
        internal static Switch QEnabled = new Switch("Enabled", true);
        internal static Switch QTransfer = new Switch("Can Transfer Spears", true);
        internal static ModeDisplay QHitChance = new ModeDisplay() { Title = "Q Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };
        internal static Switch DrawQ = new Switch("Draw Q Damage", true);
        internal static ModeDisplay DrawQMode = new ModeDisplay() { Title = "Draw Mode", ModeNames = new() { "AboveHPBar", "AboveWithoutName", "OnHPBar" } };
        internal static Counter DrawQPrio = new Counter("Draw Prio", 5, 1, 10);
        internal static InfoDisplay QPrioInfo = new InfoDisplay() { Title = "Prio", Information = "Prio doesnt work for OnHPBar" };
        internal static ModeDisplay DrawQColor = new ModeDisplay("Draw Color", Color.DodgerBlue);
        
        internal static Group WGroup = new Group("W Settings");
        internal static Switch WEnabled = new Switch("Enabled", true);
        internal static Counter WMaxCounter = new Counter("Max Distance", 4000, 0, 50000);
        internal static Counter WMinCounter = new Counter("Min Distance", 2000, 0, 50000);
        internal static InfoDisplay WInfo = new InfoDisplay() { Title = "Usage", Information = "W will be used when Dragon Spawns/gets attacked." };
        
        internal static Group EGroup = new Group("E Settings");
        internal static Switch EEnabled = new Switch("Enabled", true);
        internal static Switch DrawE = new Switch("Draw E Damage", true);
        internal static Switch UseEInLanceclear = new Switch("Use E in Laneclear", true);
        internal static ModeDisplay DrawEMode = new ModeDisplay() { Title = "Draw Mode", ModeNames = new() { "AboveHPBar", "AboveWithoutName", "OnHPBar" } };
        internal static Counter DrawEPrio = new Counter("Draw Prio", 8, 1, 10);
        internal static InfoDisplay EPrioInfo = new InfoDisplay() { Title = "Prio", Information = "Prio doesnt work for OnHPBar" };
        internal static ModeDisplay DrawEColor = new ModeDisplay("Draw Color", Color.OrangeRed);
        
        internal static Group RGroup = new Group("R Settings");
        internal static Switch REnabled = new Switch("Enabled", true);
        internal static Counter RAllyHPPercent = new Counter("Ally HP%", 25, 0, 100);
        internal static Counter CastEnemyNear = new Counter("Cast Enemy Nearer", RTargetRange, 0, 100000);
    
        internal void InitMenu()
        {
            MenuManagerProvider.AddTab(KalistaTab);

            KalistaTab.AddGroup(General);
            General.AddItem(LaneClearWoTarget);

            KalistaTab.AddGroup(QGroup);
            QGroup.AddItem(QEnabled);
            QGroup.AddItem(QTransfer);
            QGroup.AddItem(QHitChance);
            QGroup.AddItem(DrawQ);
            QGroup.AddItem(DrawQMode);
            QGroup.AddItem(DrawQPrio);
            QGroup.AddItem(DrawQColor);

            KalistaTab.AddGroup(WGroup);
            WGroup.AddItem(WEnabled);
            WGroup.AddItem(WMaxCounter);
            WGroup.AddItem(WMinCounter);
            WGroup.AddItem(WInfo);

            KalistaTab.AddGroup(EGroup);
            EGroup.AddItem(EEnabled);
            EGroup.AddItem(UseEInLanceclear);
            EGroup.AddItem(DrawEMode);
            EGroup.AddItem(DrawEPrio);
            EGroup.AddItem(DrawEColor);

            KalistaTab.AddGroup(RGroup);
            RGroup.AddItem(REnabled);
            RGroup.AddItem(RAllyHPPercent);
            RGroup.AddItem(CastEnemyNear);
        }
        #endregion

        #region Logic

        internal bool CanCastQ() => Env.QReady && QEnabled.IsOn && Env.Me().enoughMana(QManaCost[Env.QLevel]) && Enabled.IsOn;
        internal bool CanCastW() => Env.WReady && Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.W).Charges > 0 && WEnabled.IsOn && Env.Me().enoughMana(0) && Enabled.IsOn;
        internal bool CanCastE() => Env.EReady && EEnabled.IsOn && Env.Me().enoughMana(EManaCost[Env.ELevel]) && Enabled.IsOn;
        internal bool CanCastR() => Env.RReady && REnabled.IsOn && Env.Me().enoughMana(RManaCost[Env.RLevel]) && Enabled.IsOn;

        internal static List<GameObjectBase> getEnemiesWithStacks(ObjectTypeFlag[] flags)
        {
            List<GameObjectBase> enemies = new();
            foreach (GameObjectBase hero in UnitManager.GetEnemies(flags).deepCopy())
            {
                float stacks = ECalc.GetEStacks(hero);
                if (stacks >= 1 && hero.IsAlive && hero.IsVisible)
                {
                    enemies.Add(hero);
                }
            }
            return enemies;
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

        internal bool ShouldCastE(bool combo = false)
        {
            List<ObjectTypeFlag> flags = new() { ObjectTypeFlag.AIHeroClient };
            List<GameObjectBase> targets = getEnemiesWithStacks(flags.ToArray()).deepCopy();
            foreach (GameObjectBase target in targets)
            {
                if (target.Distance <= ERange && target.IsTargetable && target.CanKill(ECalc.CalculateDamage(target)) && target.IsAlive)
                    return true;
            }
            if (!combo)
            {
                foreach (GameObjectBase target in UnitManager.EnemyMinions)
                {
                    if (target.Distance <= ERange && target.IsTargetable && target.CanKill(ECalc.CalculateDamage(target)) && target.IsAlive)
                        return true;
                }
            }
            return false;
        }

        internal bool EnemyNearer()
        {
            foreach (GameObjectBase target in UnitManager.EnemyChampions)
            {
                if (EnemyNearer(target))
                    return true;
            }
            return false;
        }

        internal bool EnemyNearer(GameObjectBase target)
        {
            if (BoundAlly == null) return false;
            return target.DistanceTo(BoundAlly.Position) <= CastEnemyNear.Value;
        }

        internal bool ShouldCastR()
        {
            if (BoundAlly == null)
                return false;
            return BoundAlly.MissingHealthPercent <= RAllyHPPercent.Value && BoundAlly.IsAlive && BoundAlly.Distance <= RTetherRadius && BoundAlly.IsTargetable && EnemyNearer();
        }

        internal void SetTargetChampsOnly(bool value)
        {
            try
            {
                Oasys.Common.Settings.Orbwalker.HoldTargetChampsOnly = value;
            }
            catch (Exception ex)
            {
                if (Env.ModuleVersion == Common.Enums.V.Development)
                    Logger.Log(ex.Message);
            }
        }

        #endregion

        #region Casts
        internal Prediction.MenuSelected.PredictionOutput PredictQ(GameObjectBase target)
        {
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, Champions.Kalista.QRange - 40, Champions.Kalista.QWidth, Champions.Kalista.QCastTime, Champions.Kalista.QSpeed, true);
        }

        internal void TryCastQ(GameObjectBase target)
        {
            if (target == null) return;
            if (CanCastQ() && target.IsValidTarget())
            {
                Prediction.MenuSelected.PredictionOutput pred = PredictQ(target);
                if (pred.HitChance >= GetHitchanceFromName(QHitChance.SelectedModeName))
                {
                    if (!pred.Collision || QTransfer.IsOn && QCalc.QCanTransferToTarget(pred))
                    {
                        SpellCastProvider.CastSpell(CastSlot.Q, pred.CastPosition);
                    }
                }
            }
        }

        internal void TryCastE(bool combo = false)
        {
            if (ShouldCastE(combo) && CanCastE())
            {
                SpellCastProvider.CastSpell(CastSlot.E);
            }
        }
        
        internal void TryCastR()
        {
            if (ShouldCastR() && CanCastR())
            {
                SpellCastProvider.CastSpell(CastSlot.R);
            }
                
        }
        #endregion

        #region Events

        internal Task OnCoreMainTick()
        {
            #region FindBoundALly
            TickCycles++;
            if (BoundAlly == null && TickCycles % 10 == 0 && Env.Me().Level >= 6 && Enabled.IsOn)
            {
                BoundAlly = UnitManager.AllyChampions.deepCopy().FirstOrDefault(ally => ally.BuffManager.GetBuffList().deepCopy().Any(x => x.Name.Contains("kalistacoopstrikeally", StringComparison.OrdinalIgnoreCase)));
                if (BoundAlly != null)
                    Logger.Log($"Found bound ally: {BoundAlly.ModelName}");
            }
            #endregion
            if (Env.ModuleVersion == Common.Enums.V.None)
                Logger.Log("Updating Damages");
            if (!DrawE.IsOn || DrawEMode.SelectedModeName == "OnHPBar")
            {
                _EDamage.IsOn = false;
            }
            else
                _EDamage.IsOn = true;
            if (!DrawQ.IsOn || DrawQMode.SelectedModeName == "OnHPBar")
            {
                _QDamage.IsOn = false;
            }
            else
                _QDamage.IsOn = true;
            _QDamage.UpdateName((DrawQMode.SelectedModeName == "AboveHPBar") ? "Q" : String.Empty);
            _QDamage.UpdateColor(ColorConverter.GetColor(DrawQColor.SelectedModeName));
            _QDamage.UpdatePriority((uint)DrawQPrio.Value);
            _EDamage.UpdateName((DrawEMode.SelectedModeName == "AboveHPBar") ? "E" : String.Empty);
            _EDamage.UpdateColor(ColorConverter.GetColor(DrawEColor.SelectedModeName));
            _EDamage.UpdatePriority((uint)DrawEPrio.Value);
            return Task.CompletedTask;
        }

        internal Task OnCoreMainInput()
        {
            if (!Enabled.IsOn)
                return Task.CompletedTask;
            GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance <= Env.Me().TrueAttackRange);
            bool _origTarget = Oasys.Common.Settings.Orbwalker.HoldTargetChampsOnly;
            if (LaneClearWoTarget.IsOn && target == null && Oasys.SDK.Orbwalker.TargetHero == null)
            {
                SetTargetChampsOnly(false);
                Oasys.SDK.Orbwalker.SelectedTarget = UnitManager.EnemyMinions.deepCopy().OrderBy(x => x.Distance).FirstOrDefault(x => Oasys.SDK.TargetSelector.IsAttackable(x) && Oasys.SDK.TargetSelector.IsInRange(x));
                if (Oasys.SDK.Orbwalker.SelectedTarget is null)
                {
                    Oasys.SDK.Orbwalker.SelectedTarget = UnitManager.EnemyTowers.deepCopy().OrderBy(x => x.Distance).FirstOrDefault(x => Oasys.SDK.TargetSelector.IsAttackable(x) && Oasys.SDK.TargetSelector.IsInRange(x));
                    if (Oasys.SDK.Orbwalker.SelectedTarget is null)
                    {
                        Oasys.SDK.Orbwalker.SelectedTarget = UnitManager.EnemyJungleMobs.deepCopy().OrderBy(x => x.Distance).FirstOrDefault(x => Oasys.SDK.TargetSelector.IsAttackable(x) && Oasys.SDK.TargetSelector.IsInRange(x));
                        if (Oasys.SDK.Orbwalker.SelectedTarget is null)
                        {
                            Oasys.SDK.Orbwalker.SelectedTarget = UnitManager.EnemyInhibitors.deepCopy().OrderBy(x => x.Distance).FirstOrDefault(x => Oasys.SDK.TargetSelector.IsAttackable(x) && Oasys.SDK.TargetSelector.IsInRange(x));
                        }
                    }
                }
            }
            if (target != null)
                SetTargetChampsOnly(_origTarget);
            #region QCast
            if (QEnabled.IsOn)
            {
                GameObjectBase target2 = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance <= QRange-20);
                TryCastQ(target2);
            }
            #endregion
            #region ECast
            if (EEnabled.IsOn)
                TryCastE(true);
            #endregion
            #region RCast
            if (REnabled.IsOn)
                TryCastR();
            #endregion
            return Task.CompletedTask;
        }

        internal Task OnCoreLaneClear()
        {
            if (!Enabled.IsOn)
                return Task.CompletedTask;
            #region ECast
            if (EEnabled.IsOn && UseEInLanceclear.IsOn)
                TryCastE(false);
            #endregion
            return Task.CompletedTask;
        }

        internal void OnCoreRender()
        {
            if (!Enabled.IsOn)
                return;
            if (DrawE.IsOn || DrawQ.IsOn)
            {
                if (Env.ModuleVersion == Common.Enums.V.None)
                    Logger.Log("In general Drawing");
                ObjectTypeFlag[] flags = new ObjectTypeFlag[] { ObjectTypeFlag.AIHeroClient, ObjectTypeFlag.AIMinionClient, ObjectTypeFlag.NeutralCampClient };
                List<GameObjectBase> targets = getEnemiesWithStacks(flags).deepCopy();
                foreach (GameObjectBase target in targets)
                {
                    if (DrawE.IsOn && Env.ELevel >= 1)
                    {
                        if (DrawEMode.SelectedModeName == "OnHPBar")
                        {
                            RenderFactory.DrawHPBarDamage(target, ECalc.CalculateDamage(target));
                        }

                    }
                }
                targets = UnitManager.EnemyChampions.deepCopy().ToList<GameObjectBase>();
                foreach (GameObjectBase target in targets)
                {
                    if (DrawQ.IsOn && Env.QLevel >= 1)
                    {
                        if (DrawQMode.SelectedModeName == "OnHPBar")
                        {
                            if (Env.ModuleVersion == Common.Enums.V.Preview)
                                Logger.Log("Should Draw Q");
                            RenderFactory.DrawHPBarDamage(target, QCalc.CalculateDamage(target));
                        }
                    }
                }
            }
        }
        #endregion
    }
}
