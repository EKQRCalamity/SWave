using System;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.SDK;
using Oasys.SDK.Tools;
using SyncWave.Base;
using SyncWave.Common.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncWave.Common.Helper;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using SharpDX;
using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Enums.GameEnums;
using Oasys.SDK.SpellCasting;
using Oasys.Common.Extensions;
using Oasys.SDK.Rendering;
using Oasys.Common.Logic.Helpers.GameData;

namespace SyncWave.Champions
{

    internal class NidaleeQCalc : DamageCalculation
    {
        internal static int[] JavelinDamage = new int[] { 0, 70, 90, 110, 130, 150 };
        internal static float JavelinAPScaling = 0.5F;
        internal static float[] JavelinDistanceDamage = new float[] { 0, 0.4F, 0.8F, 1.2F, 1.6F, 2F };

        internal static int[] CougarQDamage = new int[] { 0, 5, 30, 55, 80 };
        internal static float[] CougarQDamageMod = new float[] { 0, 1, 1.25F, 1.5F, 1.75F };
        internal static float CougarQAPScaling = 0.4F;
        internal static float CougarQADScaling = 0.75F;

        internal static float LichBaneADScaling = 0.75F;
        internal static float LichBaneAPScaling = 0.5F;

        internal static float LudensScaling = 0.1F;

        internal static float NHScaling = 0.15F;

        internal static bool HasLichBane()
        {
            return Env.Me().Inventory.GetItemList
                ().Any(x => x.ID == ItemID.Lich_Bane);
        }

        internal static float LichBaneDmg()
        {
            float damage = 0;
            if (!HasLichBane())
                return damage;
            damage = Env.Me().UnitStats.BaseAttackDamage * LichBaneADScaling + Env.Me().UnitStats.TotalAbilityPower * LichBaneAPScaling;
            return damage;
        }

        internal static bool HasLudens()
        {
            return Env.Me().Inventory.GetItemList
                ().Any(x => x.ID == ItemID.Ludens_Tempest);
        }

        internal static float LudensDmg()
        {

            float damage = 0;
            if (!HasLudens())
                return damage;
            HeroInventory.Item? item = Env.Me().Inventory.GetItemList().deepCopy().Where(x => x.ID == ItemID.Ludens_Tempest).FirstOrDefault();
            if (item != null && item.IsReady)
                damage = 100 + (Env.Me().UnitStats.TotalAbilityPower * LudensScaling);
            return damage;
        }

        internal static bool HasNightharvester()
        {
            return Env.Me().Inventory.GetItemList
                ().Any(x => x.ID == ItemID.Night_Harvester);
        }

        internal static float NightharvesterDmg()
        {

            float damage = 0;
            if (!HasNightharvester())
                return damage;
            HeroInventory.Item? item = Env.Me().Inventory.GetItemList().deepCopy().Where(x => x.ID == ItemID.Night_Harvester).FirstOrDefault();
            if (item != null && item.IsReady)
                damage = 125 + (Env.Me().UnitStats.TotalAbilityPower * NHScaling);
            return damage;
        }

        internal override float CalculateDamage(GameObjectBase target)
        {
            var transformed = Nidalee.IsTransformed();
            float damage = 0;
            float Distance = target.Distance;
            if (!transformed && Distance > 1500 || !Env.QReady)
                return damage;
            if (!transformed)
            {
                int distanceMod = (Distance < 525)
                    ? 0 : (Distance > 1300 && Distance < 1500)
                    ? 5 : (Distance < 1300 && Distance > 525)
                    ? (int)Math.Floor((Distance - 525) / 155) : 0;
                damage = JavelinDamage[Env.QLevel] + (Env.Me().UnitStats
                    .TotalAbilityPower * JavelinAPScaling);
                //Logger.Log(distanceMod);
                damage = damage + damage * JavelinDistanceDamage[distanceMod];
                damage += LudensDmg();
                damage += NightharvesterDmg();
            } else
            {
                damage = CougarQDamage[Env.RLevel] 
                    + (Env.Me().UnitStats.TotalAbilityPower * CougarQAPScaling) 
                    + (Env.Me().UnitStats.TotalAttackDamage * CougarQADScaling);
                //Logger.Log($"Calc 1 : {damage}");
                damage = damage + ((float)Math.Floor(target.MissingHealthPercent) * CougarQDamageMod[Env.RLevel]);
                //Logger.Log($"Calc 2 : {damage}");
                damage *= (Nidalee.IsHunted(target)) ? 1.4F : 1;
                //Logger.Log($"Calc 3 : {damage}");
                damage += LichBaneDmg();
                //Logger.Log($"Calc 4 : {damage}");
            }
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
        }
    }

    internal class NidaleeWCalc : DamageCalculation
    {
        internal static int[] WDamage = new int[] { 0, 60, 110, 160, 210 };
        internal static float WAPScaling = 0.3F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            var transformed = Nidalee.IsTransformed();
            float damage = 0;
            if (!transformed)
                return damage;
            damage = WDamage[Env.RLevel] + (Env.Me().UnitStats.TotalAbilityPower * WAPScaling);
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
        }
    }

    internal class NidaleeECalc : DamageCalculation
    {
        internal static int[] BaseHealing = new int[] { 0, 35, 50, 65, 80, 95 };
        internal static float HealingScaling = 0.275F;

        internal static int[] BaseDamage = new int[] { 0, 80, 140, 200, 260 };
        internal static float DamageAPScaling = 0.45F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            var transformed = Nidalee.IsTransformed();
            float damage = 0;
            if (!transformed)
                return damage;
            damage = BaseDamage[Env.RLevel] + (Env.Me().UnitStats.TotalAbilityPower * DamageAPScaling);
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
        }

        internal float CalculateHealing(GameObjectBase target)
        {
            var transformed = Nidalee.IsTransformed();
            float healing = 0;
            if (transformed)
                return healing;
            healing = BaseHealing[Env.ELevel] + (Env.Me().UnitStats.TotalAbilityPower * HealingScaling);
            healing = healing * ((target.MissingHealthPercent < 95) ? 1 + ((target.MissingHealthPercent * 0.95F) / 100) : 2);
            return healing;
        }
    }

    internal class Nidalee : Base.Champion
    {
        internal static int MainTick = 0;

        internal static NidaleeQCalc QCalc = new NidaleeQCalc();
        internal static NidaleeWCalc WCalc = new NidaleeWCalc();
        internal static NidaleeECalc ECalc = new NidaleeECalc();

        internal static Damage? _QDamage;
        internal static Damage? _WDamage;
        internal static Damage? _EDamage;

        internal static int[] JavelinManaCost = new int[] { 0, 50, 55, 60, 65, 70 };
        internal static int JavelinRange = 1500;
        internal static int JavelinSpeed = 1300;
        internal static int JavelinWidth = 80;
        internal static float JavelinCastTime = 0.25F;

        internal static int StandardWRange = 375;
        internal static int ExtendedWRange = 750;

        internal static int StandardWEffectRadius = 200;
        internal static int ExtendedWEffectRadius = 250;

        internal static int[] EManaCost = new int[] { 0, 50, 55, 60, 65, 70 };
        internal static int ECougarRange = 310;
        internal static int ECastRange = 900;
        internal static float ECastTime = 0.25F;

        internal static bool IsTransformed()
        {
            if (Env.ModuleVersion == Common.Enums.V.InTesting && MainTick % 10 == 1)
                Logger.Log($"Q name: {Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q).SpellData.SpellName.ToLower()} != javelintoss");
            return Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q).SpellData.SpellName.ToLower() != "javelintoss";
        }

        internal static bool IsHunted(GameObjectBase target)
        {
            if (Env.ModuleVersion == Common.Enums.V.InTesting && MainTick % 10 == 1)
                Logger.Log(target.BuffManager.HasActiveBuff("nidaleepassivehunted"));
            return target.BuffManager.HasActiveBuff("nidaleepassivehunted");
        }

        internal override void Init()
        {
            Logger.Log("Initializing Nidalee..");
            InitMenu();
            CoreEvents.OnCoreMainTick += OnCoreMainTick;
            CoreEvents.OnCoreMainInputAsync += OnCoreMainInput;
            Render.Init();
            _QDamage = new Damage("Q", 8, QCalc, ColorConverter.GetColor(DrawQColor.SelectedModeName));
            _WDamage = new Damage("W", 6, WCalc, ColorConverter.GetColor(DrawWColor.SelectedModeName));
            _EDamage = new Damage("E", 4, ECalc, ColorConverter.GetColor(DrawEColor.SelectedModeName));
            CoreEvents.OnCoreRender += OnCoreRender;
            Logger.Log("Nidalee initialized!");
        }

        #region Menu
        internal static Tab NidTab = new Tab("SyncWave - Nidalee");

        internal static Group QGroup = new Group("Q Settings");
        internal static Switch SpearEnabled = new Switch("Q Spear Enabled", true);
        internal static Switch CougarEnabled = new Switch("Q Cougar Enabled", true);
        internal static ModeDisplay QHitChance = new ModeDisplay() { Title = "Q Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };
        internal static Switch SpearDraw = new Switch("Q Damage Draw", true);
        internal static Counter DrawQPrio = new Counter("Draw Prio", 5, 1, 10);
        internal static ModeDisplay DrawQColor = new ModeDisplay("Draw Color", Color.Red);
        internal static ModeDisplay DrawQMode = new ModeDisplay() { Title = "Draw Mode", ModeNames = new() { "AboveHPBar", "AboveWithoutName" } };

        internal static Group WGroup = new Group("W Settings");
        internal static Switch WEnabled = new Switch("W Enabled", true);
        internal static ModeDisplay WHitChance = new ModeDisplay() { Title = "W Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };
        internal static Switch WDraw = new Switch("W Drawings", true);
        internal static Counter DrawWPrio = new Counter("Draw Prio", 5, 1, 10);
        internal static ModeDisplay DrawWColor = new ModeDisplay("Draw Color", Color.Green);
        internal static ModeDisplay DrawWMode = new ModeDisplay() { Title = "Draw Mode", ModeNames = new() { "AboveHPBar", "AboveWithoutName" } };

        internal static Group EGroup = new Group("E Settings");
        internal static Switch EHealEnabled = new Switch("E Heal Enabled", true);
        internal static Counter EHPThreshold = new Counter("E Heal Threshold", 20, 0, 100);
        internal static Switch DrawEHeal = new Switch("Draw E Heal", true);
        internal static Switch EEnabled = new Switch("E Enabled", true);
        internal static Switch EDraw = new Switch("E Draw", true);
        internal static ModeDisplay EHitChance = new ModeDisplay() { Title = "E Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };
        internal static Counter DrawEPrio = new Counter("Draw Prio", 5, 1, 10);
        internal static ModeDisplay DrawEColor = new ModeDisplay("Draw Color", Color.Blue);
        internal static ModeDisplay DrawEMode = new ModeDisplay() { Title = "Draw Mode", ModeNames = new() { "AboveHPBar", "AboveWithoutName" } };

        internal void InitMenu()
        {
            MenuManagerProvider.AddTab(NidTab);

            NidTab.AddGroup(QGroup);
            QGroup.AddItem(SpearEnabled);
            QGroup.AddItem(CougarEnabled);
            QGroup.AddItem(QHitChance);
            QGroup.AddItem(SpearDraw);
            QGroup.AddItem(DrawQPrio);
            QGroup.AddItem(DrawQColor);
            QGroup.AddItem(DrawQMode);

            NidTab.AddGroup(WGroup);
            WGroup.AddItem(WEnabled);
            WGroup.AddItem(WHitChance);
            WGroup.AddItem(WDraw);
            WGroup.AddItem(DrawWPrio);
            WGroup.AddItem(DrawWColor);
            WGroup.AddItem(DrawWMode);

            NidTab.AddGroup(EGroup);
            EGroup.AddItem(EHealEnabled);
            EGroup.AddItem(EHPThreshold);
            EGroup.AddItem(DrawEHeal);
            EGroup.AddItem(EEnabled);
            EGroup.AddItem(EHitChance);
            EGroup.AddItem(DrawEPrio);
            EGroup.AddItem(DrawEColor);
            EGroup.AddItem(DrawEMode);
        }
        #endregion

        #region Logic

        internal bool CanCastQ() => Env.QReady && SpearEnabled.IsOn && (IsTransformed()) ? true : Env.Me().enoughMana(JavelinManaCost[Env.QLevel]);
        internal bool CanCastW() => Env.WReady && WEnabled.IsOn;
        internal bool CanCastE() => Env.EReady && EEnabled.IsOn && (IsTransformed())? true : Env.Me().enoughMana(EManaCost[Env.ELevel]);
        

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
        #endregion

        #region Casts

        internal Prediction.MenuSelected.PredictionOutput PredictQ(GameObjectBase target)
        {
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, JavelinRange, JavelinWidth, JavelinCastTime, JavelinSpeed, true);
        }

        internal void TryCastQ()
        {
            GameObjectBase speartarget = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance < JavelinRange - 40);
            GameObjectBase cougarTarget = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null);
            if (speartarget != null && !IsTransformed() && SpearEnabled.IsOn)
            {
                foreach (GameObjectBase enemy in UnitManager.EnemyChampions.deepCopy().Where(x => x.Distance < JavelinRange - 40 && x.IsVisible && x.IsValidTarget()))
                {
                    Prediction.MenuSelected.PredictionOutput pred = PredictQ(enemy);
                    if (pred.CollisionObjects.Any(x => !x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient)))
                        continue;
                    if (pred.HitChance >= GetHitchanceFromName(QHitChance.SelectedModeName) && CanCastQ() && pred.CollisionObjects.All(x => x.IsObject(ObjectTypeFlag.AIHeroClient)))
                    {
                        SpellCastProvider.CastSpell(CastSlot.Q, pred.CastPosition, JavelinCastTime);
                        break;
                    }
                }
            } else if (cougarTarget != null && IsTransformed() && CougarEnabled.IsOn && CanCastQ())
            {
                SpellCastProvider.CastSpell(CastSlot.Q, cougarTarget.Position);
            }
        }

        internal Prediction.MenuSelected.PredictionOutput PredictW(GameObjectBase target)
        {
            bool hunted = IsHunted(target);
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Circle, target, ((hunted) ? ExtendedWRange : StandardWRange), ((hunted) ? ExtendedWEffectRadius : StandardWEffectRadius), 0, JavelinSpeed, false);
        }

        internal void TryCastW()
        {
            GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance < (IsHunted(x)? ExtendedWRange : StandardWRange));
            if (target != null && IsTransformed())
            {
                Prediction.MenuSelected.PredictionOutput pred = PredictW(target);
                if (pred.HitChance >= GetHitchanceFromName(WHitChance.SelectedModeName) && CanCastW())
                {
                    SpellCastProvider.CastSpell(CastSlot.W, pred.CastPosition, 0);
                }
            }
        }

        internal Prediction.MenuSelected.PredictionOutput PredictE(GameObjectBase target)
        {
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Cone, target, ECougarRange, 180, ECastTime, JavelinSpeed, false);
        }

        internal void TryCastE()
        {
            GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance < ECastRange);
            if (target != null && target.IsValidTarget() && CanCastE())
            {
                Prediction.MenuSelected.PredictionOutput pred = PredictE(target);
                if (pred.HitChance >= GetHitchanceFromName(EHitChance.SelectedModeName))
                {
                    SpellCastProvider.CastSpell(CastSlot.E, pred.CastPosition, ECastTime);
                }
            }
        }

        internal void TryHeal()
        {
            GameObjectBase? target = new Common.Helper.Selectors.TargetSelector(ECastRange, Common.Helper.Selectors.Modes.Ally).GetLowestHealthPrioTarget(Common.Helper.Selectors.Modes.Ally); 
            if (target != null && target.HealthPercent < EHPThreshold.Value)
            {
                Vector3 pos = target.Position;
                pos.Y -= 10;
                SpellCastProvider.CastSpell(CastSlot.E, pos, ECastTime);
            }
        } 
        #endregion

        #region Events
        private Task OnCoreMainTick()
        {
            MainTick += 1;
            _QDamage.IsOn = SpearDraw.IsOn;
            _WDamage.IsOn = WDraw.IsOn;
            _EDamage.IsOn = EDraw.IsOn;
            _QDamage.UpdateName((DrawQMode.SelectedModeName == "AboveHPBar") ? "Q" : String.Empty);
            _QDamage.UpdateColor(ColorConverter.GetColor(DrawQColor.SelectedModeName));
            _QDamage.UpdatePriority((uint)DrawQPrio.Value);
            _WDamage.UpdateName((DrawWMode.SelectedModeName == "AboveHPBar") ? "W" : String.Empty);
            _WDamage.UpdateColor(ColorConverter.GetColor(DrawWColor.SelectedModeName));
            _WDamage.UpdatePriority((uint)DrawWPrio.Value);
            _EDamage.UpdateName((DrawEMode.SelectedModeName == "AboveHPBar") ? "E" : String.Empty);
            _EDamage.UpdateColor(ColorConverter.GetColor(DrawEColor.SelectedModeName));
            _EDamage.UpdatePriority((uint)DrawEPrio.Value);
            return Task.CompletedTask;
        }

        private Task OnCoreMainInput()
        {
            TryCastQ();
            TryCastW();
            TryHeal();
            TryCastE();
            return Task.CompletedTask;
        }

        private void OnCoreRender()
        {
            if (SpearDraw.IsOn || WDraw.IsOn || EDraw.IsOn)
            {
                List<Hero> enemies = UnitManager.EnemyChampions.deepCopy();
                foreach (Hero enemy in enemies)
                {
                    if (enemy == null || !enemy.IsAlive || !enemy.IsTargetable || !enemy.IsVisible) continue;
                    if (SpearDraw.IsOn && Env.QLevel >= 1)
                    {
                        if (!Render.HasDamage(_QDamage))
                            Render.AddDamage(_QDamage);
                        if (!_QDamage.IsOn)
                            _QDamage.IsOn = true;
                    }
                    if (WDraw.IsOn && Env.WLevel >= 1)
                    {
                        if (!Render.HasDamage(_WDamage))
                            Render.AddDamage(_WDamage);
                        if (!_WDamage.IsOn)
                            _WDamage.IsOn = true;
                    }
                    if (EDraw.IsOn && Env.ELevel >= 1)
                    {
                        if (!Render.HasDamage(_EDamage))
                            Render.AddDamage(_EDamage);
                        if (!_EDamage.IsOn)
                            _EDamage.IsOn = true;
                    }
                }
            }
            List<Hero> allies = UnitManager.AllyChampions.deepCopy();
            foreach (Hero ally in allies)
            {
                if (DrawEHeal.IsOn && Env.ELevel >= 1)
                {
                    RenderFactory.DrawHPBarHeal(ally, ECalc.CalculateHealing(ally), Color.Green);
                }
            }
        }

        #endregion
    }
}
