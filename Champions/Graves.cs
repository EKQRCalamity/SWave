using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Base;
using SyncWave.Common.Extensions;
using SyncWave.Common.Helper;
using SyncWave.Common.Helper.Selectors;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncWave.Champions
{
    #region DamageCalculation

    internal class GravesQDamageCalc : DamageCalculation
    {
        internal int[] QManaCost = new int[] { 0, 80, 80, 80, 80, 80 };
        internal static int[] QDamage = new int[] { 0, 45, 60, 75, 90, 105 };
        internal static float ADScaling = 0.8F;
        internal bool IsReady()
        {
            return Env.QReady && Env.Me().Mana > QManaCost[Env.QLevel];
        }
        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (!IsReady() || target.Distance > Graves.QRange && Graves.QDrawDepends.IsOn)
                return damage;
            damage = QDamage[Env.QLevel] + (Env.Me().UnitStats.BonusAttackDamage * ADScaling);
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, damage);
        }
    }

    internal class GravesWDamageCalc : DamageCalculation
    {
        internal int[] WManaCost = new int[] { 0, 70, 75, 80, 85, 90 };

        internal static int[] WDamage = new int[] { 0, 60, 110, 160, 210, 260 };
        internal static float APScaling = 0.6F;
        internal bool IsReady()
        {
            return Env.WReady && Env.Me().Mana > WManaCost[Env.WLevel];
        }
        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (!IsReady() || target.Distance > Graves.WRange && Graves.WDrawDepends.IsOn)
                return damage;
            damage = WDamage[Env.WLevel] + (Env.Me().UnitStats.TotalAbilityPower * APScaling);
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
        }
    }

    internal class GravesRDamageCalc : DamageCalculation
    {
        internal int[] RManaCost = new int[] { 0, 100, 100, 100 };

        internal static int[] RDamage = new int[] { 0, 275, 425, 575 };
        internal static int[] RReducedDamage = new int[] { 0, 220, 340, 460 };

        internal static Common.Helper.Selectors.TargetSelector RSelector1 = new(Graves.RRange1);
        internal static Common.Helper.Selectors.TargetSelector RSelector2 = new(Graves.RRange2);

        internal static float ADScaling = 1.5F;
        internal static float ReducedADScaling = 1.2F;
        internal bool IsReady()
        {
            return Env.RReady && Env.Me().Mana > RManaCost[Env.RLevel];
        }

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (!IsReady())
                return damage;
            if (target.Distance > Graves.RRange2)
                return damage; 
            else if (target.Distance < Graves.RRange1)
            {
                if (Env.ModuleVersion == Common.Enums.V.InTesting)
                    Logger.Log($"RLevel: {Env.RLevel} - {RReducedDamage[Env.RLevel]} | Bonues AD: {Env.Me().UnitStats.BonusAttackDamage} => R{Env.Me().UnitStats.BonusAttackDamage * ReducedADScaling}");
                Prediction.MenuSelected.PredictionOutput pred = Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, Graves.RRange1, Graves.RShellWidth, Graves.RCastTime, Graves.RSpeed, true);
                if (pred.CollisionObjects.Any(x => x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient)))
                    damage = RReducedDamage[Env.RLevel] + (Env.Me().UnitStats.BonusAttackDamage * ReducedADScaling);
                else
                    damage = RDamage[Env.RLevel] + (Env.Me().UnitStats.BonusAttackDamage * ADScaling);
            }
            else if (target.Distance > Graves.RRange1 && target.Distance < Graves.RRange2)
                damage = RReducedDamage[Env.RLevel] + (Env.Me().UnitStats.BonusAttackDamage * ReducedADScaling);
            if (Env.ModuleVersion == Common.Enums.V.InTesting)
                Logger.Log($"{damage} - {target.Name} -> {DamageCalculator.CalculateActualDamage(Env.Me(), target, damage)}");
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, damage);
        }
    }

    #endregion

    internal class Graves : SyncWave.Base.Champion
    {
        #region Stats/Statics
        internal static GravesQDamageCalc QCalc = new GravesQDamageCalc();
        internal static GravesWDamageCalc WCalc = new GravesWDamageCalc();
        internal static GravesRDamageCalc RCalc = new GravesRDamageCalc();

        internal static Damage? _QDamage;
        internal static Damage? _WDamage;
        internal static Damage? _RDamage;

        internal static int QRange = 800;
        internal static int QShellWidth = 80;
        internal static int QSpeed = 3000;
        internal static float QCastTime = 0.25F;

        internal static int WRange = 950;
        internal static int WRadius = 200;
        internal static int WSpeed = 1500;
        internal static float WCastTime = 0.25F;

        internal static int RRange1 = 1100;
        internal static int RRange2 = 1690;
        internal static int RShellWidth = 200;
        internal static int RSpeed = 2100;
        internal static int RAngle = 60;
        internal static float RCastTime = 0.25F;
        #endregion

        internal override void Init()
        {
            Logger.Log("Graves Initializing...");
            InitMenu();
            CoreEvents.OnCoreMainTick += OnCoreMainTick;
            CoreEvents.OnCoreMainInputAsync += OnCoreMainInput;
            CoreEvents.OnCoreRender += OnCoreRender;
            Render.Init();
            _QDamage = new Damage("Q", (uint)QPrio.Value, QCalc, ColorConverter.GetColor(QColor.SelectedModeName));
            _WDamage = new Damage("W", (uint)WPrio.Value, WCalc, ColorConverter.GetColor(WColor.SelectedModeName));
            _RDamage = new Damage("R", (uint)RPrio.Value, RCalc, ColorConverter.GetColor(RColor.SelectedModeName));
            Logger.Log("Graves Initialized!");
        }

        #region Menu
        internal static Tab GravesTab = new Tab("SyncWave - Graves");

        internal static Group QGroup = new Group("Q Settings");
        internal static Switch QEnabled = new Switch("Enabled", true);
        internal static ModeDisplay QHitChance = new ModeDisplay() { Title = "Q Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };
        internal static Switch QDraw = new Switch("Draw Q Damage", true);
        internal static Switch QDrawDepends = new Switch("Draw only when in range", false);
        internal static ModeDisplay QModes = new ModeDisplay() { Title = "Drawing Mode", ModeNames = new() { "AboveHPBar", "AboveHPBarNoName" }, SelectedModeName = "AboveHPBar" };
        internal static Counter QPrio = new Counter("Draw Prio", 1, 1, 10);
        internal static ModeDisplay QColor = new ModeDisplay("Draw Color", Color.Green);

        internal static Group WGroup = new Group("W Settings");
        internal static Switch WEnabled = new Switch("Enabled", true);
        internal static ModeDisplay WHitChance = new ModeDisplay() { Title = "Q Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };
        internal static Switch WDraw = new Switch("Draw W Damage", true);
        internal static Switch WDrawDepends = new Switch("Draw only when in range", false);
        internal static ModeDisplay WModes = new ModeDisplay() { Title = "Drawing Mode", ModeNames = new() { "AboveHPBar", "AboveHPBarNoName" }, SelectedModeName = "AboveHPBar" };
        internal static Counter WPrio = new Counter("Draw Prio", 2, 1, 10);
        internal static ModeDisplay WColor = new ModeDisplay("Draw Color", Color.White);

        internal static Group RGroup = new Group("R Settings");
        internal static Switch REnabled = new Switch("Enabled", true);
        internal static ModeDisplay RHitChance = new ModeDisplay() { Title = "Q Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };
        internal static Switch RDraw = new Switch("Draw R Damage", true);
        internal static ModeDisplay RModes = new ModeDisplay() { Title = "Drawing Mode", ModeNames = new() { "AboveHPBar", "AboveHPBarNoName" }, SelectedModeName = "AboveHPBar" };
        internal static Counter RPrio = new Counter("Draw Prio", 4, 1, 10);
        internal static ModeDisplay RColor = new ModeDisplay("Draw Color", Color.Orange);
        
        internal void InitMenu()
        {
            MenuManagerProvider.AddTab(GravesTab);

            GravesTab.AddGroup(QGroup);
            QGroup.AddItem(QEnabled);
            QGroup.AddItem(QHitChance);
            QGroup.AddItem(QDraw);
            QGroup.AddItem(QDrawDepends);
            QGroup.AddItem(QModes);
            QGroup.AddItem(QPrio);
            QGroup.AddItem(QColor);

            GravesTab.AddGroup(WGroup);
            WGroup.AddItem(WEnabled);
            WGroup.AddItem(WHitChance);
            WGroup.AddItem(WDraw);
            WGroup.AddItem(WDrawDepends);
            WGroup.AddItem(WModes);
            WGroup.AddItem(WPrio);
            WGroup.AddItem(WColor);

            GravesTab.AddGroup(RGroup);
            RGroup.AddItem(REnabled);
            RGroup.AddItem(RHitChance);
            RGroup.AddItem(RDraw);
            RGroup.AddItem(RModes);
            RGroup.AddItem(RPrio);
            RGroup.AddItem(RColor);
        }
        #endregion

        #region Logic

        internal static bool CanCastQ() => Env.QReady && QEnabled.IsOn && Env.Me().enoughMana(QCalc.QManaCost[Env.QLevel]);
        internal static bool CanCastW() => Env.WReady && WEnabled.IsOn && Env.Me().enoughMana(WCalc.WManaCost[Env.WLevel]);
        internal static bool CanCastR() => Env.RReady && REnabled.IsOn && Env.Me().enoughMana(RCalc.RManaCost[Env.RLevel]);

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
        internal static Prediction.MenuSelected.PredictionOutput PredictQ(GameObjectBase target)
        {
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, QRange, QShellWidth, QCastTime, QSpeed, true);
        }

        internal static Prediction.MenuSelected.PredictionOutput PredictW(GameObjectBase target)
        {
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Circle, target, WRange, WRadius, WCastTime, WSpeed, true);
        }

        internal static Prediction.MenuSelected.PredictionOutput PredictR1(GameObjectBase target)
        {
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, RRange1, RShellWidth, RCastTime, RSpeed, true);
        }

        internal static void TryCastQ(GameObjectBase target)
        {
            if (target == null) return;
            if (CanCastQ() && target.IsValidTarget())
            {
                Prediction.MenuSelected.PredictionOutput pred = PredictQ(target);
                if (pred.HitChance >= GetHitchanceFromName(QHitChance.SelectedModeName))
                {
                    if (!pred.CollisionObjects.Any(x => !x.IsTargetable && !x.IsValidTarget()))
                        SpellCastProvider.CastSpell(CastSlot.Q, pred.CastPosition);
                }
            }
        }

        internal static void TryCastW(GameObjectBase target)
        {
            if (target == null) return;
            if (CanCastW() && target.IsValidTarget())
            {
                Prediction.MenuSelected.PredictionOutput pred = PredictW(target);
                if (pred.HitChance >= GetHitchanceFromName(WHitChance.SelectedModeName))
                {
                    if (!pred.CollisionObjects.Any(x => !x.IsTargetable && !x.IsValidTarget()))
                    {
                        SpellCastProvider.CastSpell(CastSlot.W, pred.CastPosition);
                    }
                }
            }
        }

        internal static void TryCastR(GameObjectBase target)
        {
            if (target == null) return;
            if (RCalc.CalculateDamage(target) > target.Health)
            {
                Prediction.MenuSelected.PredictionOutput pred = PredictR1(target);
                if (pred.HitChance >= GetHitchanceFromName(RHitChance.SelectedModeName))
                {
                    if (!pred.CollisionObjects.Any(x => !x.IsTargetable && !x.IsValidTarget()))
                    {
                        SpellCastProvider.CastSpell(CastSlot.R, pred.CastPosition);
                    }
                }
            }
        }

        #endregion

        #region Events

        internal Task OnCoreMainTick()
        {
            if (!QDraw.IsOn)
                _QDamage.IsOn = false;
            if (!WDraw.IsOn)
                _WDamage.IsOn = false;
            if (!RDraw.IsOn)
                _RDamage.IsOn = false;
            _QDamage.UpdateName((QModes.SelectedModeName == "AboveHPBar") ? "Q" : String.Empty);
            _QDamage.UpdateColor(ColorConverter.GetColor(QColor.SelectedModeName));
            _QDamage.UpdatePriority((uint)QPrio.Value);
            _WDamage.UpdateName((WModes.SelectedModeName == "AboveHPBar") ? "W" : String.Empty);
            _WDamage.UpdateColor(ColorConverter.GetColor(WColor.SelectedModeName));
            _WDamage.UpdatePriority((uint)WPrio.Value);
            _RDamage.UpdateName((RModes.SelectedModeName == "AboveHPBar") ? "R" : String.Empty);
            _RDamage.UpdateColor(ColorConverter.GetColor(RColor.SelectedModeName));
            _RDamage.UpdatePriority((uint)RPrio.Value);
            return Task.CompletedTask;
        }

        internal Task OnCoreMainInput()
        {
            GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance <= QRange - 40);
            GameObjectBase rtarget = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance <= RRange2 - 40);
            if (target != null)
            {
                TryCastQ(target);
                TryCastW(target);
            }
            if (rtarget != null)
            {
                TryCastR(rtarget);
            }
            return Task.CompletedTask;
        }

        internal void OnCoreRender()
        {
            if (QDraw.IsOn || WDraw.IsOn || RDraw.IsOn)
            {
                List<Hero> enemies = UnitManager.EnemyChampions.deepCopy();
                foreach (Hero enemy in enemies)
                {
                    if (enemy == null || !enemy.IsAlive || !enemy.IsTargetable) continue;
                    if (QDraw.IsOn)
                    {
                        if (!Render.HasDamage(_QDamage))
                            Render.AddDamage(_QDamage);
                        if (!_QDamage.IsOn)
                            _QDamage.IsOn = true;
                    }
                    if (WDraw.IsOn)
                    {
                        if (!Render.HasDamage(_WDamage))
                            Render.AddDamage(_WDamage);
                        if (!_WDamage.IsOn)
                            _WDamage.IsOn = true;
                    }
                    if (RDraw.IsOn)
                    {
                        if (!Render.HasDamage(_RDamage))
                            Render.AddDamage(_RDamage);
                        if (!_RDamage.IsOn)
                            _RDamage.IsOn = true;
                    }
                }
            }
        }
        #endregion
    }
}
