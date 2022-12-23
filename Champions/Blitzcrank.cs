using Newtonsoft.Json.Linq;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Tools.Devices;
using Oasys.SDK;
using Oasys.SDK.InputProviders;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Base;
using SyncWave.Common.Extensions;
using SyncWave.Common.Helper;
using SyncWave.Common.SpellAim;
using SyncWave.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncWave.Champions
{
    internal sealed class BlitzQCalculation : DamageCalculation
    {
        internal static int[] BaseDamage = new int[] { 0, 105, 150, 195, 240, 285};
        internal static float APScaling = 1.2F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.QReady && Env.QLevel >= 1)
            {
                damage = BaseDamage[Env.QLevel] + (Env.Me().UnitStats.TotalAbilityPower * APScaling);
                return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class BlitzECalculation : DamageCalculation
    {
        internal static float AADamage(GameObjectBase
             x) => DamageCalculator.CalculateActualDamage(Env.Me(), x, Env.Me().UnitStats.TotalAttackDamage);
        internal static float ExtraDamage(GameObjectBase
            x) => DamageCalculator.CalculateActualDamage(Env.Me(), x, ((Env.Me().UnitStats.TotalAttackDamage * 0.75F) + (Env.Me().UnitStats.TotalAbilityPower * 0.25F)));

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.EReady && Env.ELevel >= 1)
            {
                return AADamage(target) + ExtraDamage(target);
            }
            return damage;
        }
    }

    internal sealed class BlitzRCalculation : DamageCalculation
    {
        internal static int[] BaseDamage = new int[] { 0, 275, 400, 525 };
        internal static float APScaling = 1F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.RReady && Env.RLevel >= 1)
            {
                damage = BaseDamage[Env.RLevel] + (Env.Me().UnitStats.TotalAbilityPower * APScaling);
                return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }
    internal class Blitzcrank : Base.Module
    {

        internal static bool CanCastQ() => Env.QReady && QEnabled.IsOn && Env.Me().enoughMana(QManaCost) && Env.Me().IsAlive;
        internal static bool CanCastE() => Env.EReady && EEnabled.IsOn && Env.Me().enoughMana(EManaCost) && Env.Me().IsAlive;
        internal static bool CanCastR() => Env.RReady && REnabled.IsOn && Env.Me().enoughMana(RManaCost) && Env.Me().IsAlive;

        internal static BlitzQCalculation QCalc = new();
        internal static BlitzECalculation ECalc = new();
        internal static BlitzRCalculation RCalc = new();

        internal static Damage? _QDamage;
        internal static Damage? _EDamage;
        internal static Damage? _RDamage;

        internal static int QRange = 1115;
        internal static int QWidth = 140;
        internal static int QSpeed = 1800;
        internal static int QManaCost = 100;
        internal static float QCastTime = 0.25F;

        internal static int EManaCost = 40;

        internal static int RRange = 600;
        internal static int RManaCost = 100;
        internal static float RCastTime = 0.25F;

        internal static Tab BlitzTab = new Tab("SyncWave - Blitzcrank");
        internal static Switch SpellAimQ = new("Q Enabled", true);
        internal static ModeDisplay SpellAimTarget = new() { Title = "Target Select", ModeNames = new() { "CloseToMouse", "TargetSelector" }, SelectedModeName = "CloseToMouse" };

        internal static Group QGroup = new Group("Q Settings");
        internal static Switch QEnabled = new("Enabled", true);
        internal static ModeDisplay QHitChance = new ModeDisplay() { Title = "Q Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };
        internal static Switch QDraw = new Switch("Draw Q Damage", true);
        internal static ModeDisplay QModes = new ModeDisplay() { Title = "Drawing Mode", ModeNames = new() { "AboveHPBar", "AboveHPBarNoName" }, SelectedModeName = "AboveHPBar" };
        internal static Counter QPrio = new Counter("Draw Prio", 7, 1, 10);
        internal static ModeDisplay QColor = new ModeDisplay("Draw Color", Color.Green);

        internal static Group EGroup = new Group("E Settings");
        internal static Switch EEnabled = new("Enabled", true);
        internal static Switch EDraw = new Switch("Draw E Damage", true);
        internal static ModeDisplay EModes = new ModeDisplay() { Title = "Drawing Mode", ModeNames = new() { "AboveHPBar", "AboveHPBarNoName" }, SelectedModeName = "AboveHPBar" };
        internal static Counter EPrio = new Counter("Draw Prio", 5, 1, 10);
        internal static ModeDisplay EColor = new ModeDisplay("Draw Color", Color.Green);

        internal static Group RGroup = new Group("R Settings");
        internal static Switch REnabled = new("Enabled", true);
        internal static Switch ROnKill = new("Use OnKill", true);
        internal static Counter REnemies = new("Enemies Hit", 2, 0, 5);
        internal static Switch RDraw = new Switch("Draw E Damage", true);
        internal static ModeDisplay RModes = new ModeDisplay() { Title = "Drawing Mode", ModeNames = new() { "AboveHPBar", "AboveHPBarNoName" }, SelectedModeName = "AboveHPBar" };
        internal static Counter RPrio = new Counter("Draw Prio", 5, 1, 10);
        internal static ModeDisplay RColor = new ModeDisplay("Draw Color", Color.Green);

        internal static void InitMenu()
        {
            MenuManagerProvider.AddTab(BlitzTab);

            BlitzTab.AddGroup(QGroup);
            QGroup.AddItem(QEnabled);
            QGroup.AddItem(QHitChance);
            QGroup.AddItem(QDraw);
            QGroup.AddItem(QModes);
            QGroup.AddItem(QPrio);
            QGroup.AddItem(QColor);

            BlitzTab.AddGroup(EGroup);
            EGroup.AddItem(EEnabled);
            EGroup.AddItem(EDraw);
            EGroup.AddItem(EModes);
            EGroup.AddItem(EPrio);
            EGroup.AddItem(EColor);

            BlitzTab.AddGroup(RGroup);
            RGroup.AddItem(REnabled);
            RGroup.AddItem(ROnKill);
            RGroup.AddItem(REnemies);
            RGroup.AddItem(RDraw);
            RGroup.AddItem(RModes);
            RGroup.AddItem(RPrio);
            RGroup.AddItem(RColor);
        }

        internal override void Init()
        {
            Logger.Log("Blitz Initializing...");
            InitMenu();
            CoreEvents.OnCoreMainTick += MainTick;
            CoreEvents.OnCoreMainInputAsync += MainInput;
            Orbwalker.OnOrbwalkerBeforeBasicAttack += ECast;
            Render.Init();
            _QDamage = new("Q", (uint)QPrio.Value, QCalc, Color.Green);
            _EDamage = new("E", (uint)EPrio.Value, ECalc, Color.Red);
            _RDamage = new("R", (uint)RPrio.Value, RCalc, Color.Orange);
            Render.AddDamage(_QDamage);
            Render.AddDamage(_EDamage);
            Render.AddDamage(_RDamage);
            RangeDrawer.WDisabled = true;
            RangeDrawer.EDisabled = true;
            Logger.Log("Blitz Initialized!");
            AimSpell q = new AimSpell(QRange + 20, BlitzTab, CastSlot.Q, Oasys.Common.Enums.GameEnums.SpellSlot.Q);
            q.SetPrediction(Oasys.SDK.Prediction.MenuSelected.PredictionType.Line, QRange, QWidth, QCastTime, QSpeed, true);
        }

        private void ECast(float gameTime, GameObjectBase target)
        {
            if (Env.EReady && Env.ELevel >= 1 && EEnabled.IsOn && CanCastE())
            {
                SpellCastProvider.CastSpell(CastSlot.E);
            }
        }

        internal static int EnemiesInRange(float range, Vector3 originPos)
        {
            int n = 0;
            foreach (GameObjectBase enemy in UnitManager.EnemyChampions)
            {
                if (enemy.DistanceTo(originPos) < range)
                    n++;
            }
            return n;
        }

        internal static bool EnemiesInRange(float n, float range, Vector3 originPos)
        {
            return EnemiesInRange(range, originPos) >= n;
        }

        internal static void TryCastQ()
        {
            GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance < QRange);
            if (target != null)
            {
                Oasys.SDK.Prediction.MenuSelected.PredictionOutput pred = Oasys.SDK.Prediction.MenuSelected.GetPrediction(Oasys.SDK.Prediction.MenuSelected.PredictionType.Line, target, QRange, QWidth, QCastTime, QSpeed);
                if (pred.HitChance >= QHitChance.SelectedModeName.GetHitchanceFromName())
                {
                    if (!pred.CollisionObjects.Any(x => !x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient)))
                    {
                        SpellCastProvider.CastSpell(CastSlot.Q, pred.CastPosition, QCastTime);
                    }
                }
            }
        }

        internal static GameObjectBase? RTarget()
        {
            GameObjectBase? currentTarget = null;
            foreach (GameObjectBase target in UnitManager.EnemyChampions)
            {
                if (target.Distance < RRange && target.IsAlive)
                {
                    if (currentTarget == null || (currentTarget.Health - RCalc.CalculateDamage(currentTarget)) > (target.Health - RCalc.CalculateDamage(target)))
                        currentTarget = target;
                }
            }
            return currentTarget;
        }

        internal static bool TryCastR1()
        {
            GameObjectBase? target = RTarget();
            if (target == null) return false;
            if (CanCastR() && ROnKill.IsOn && target.IsValidTarget())
            {
                if ((target.Health - RCalc.CalculateDamage(target) < 0)) {
                    SpellCastProvider.CastSpell(CastSlot.R);
                    return true;
                }
            }
            return false;
        }

        internal static void TryCastR2()
        {
            int enemies = EnemiesInRange(RRange, Env.Me().Position);
            if (enemies != 0 && enemies >= REnemies.Value)
            {
                SpellCastProvider.CastSpell(CastSlot.R);
            }
        }

        internal static Task MainInput()
        {
            if (CanCastQ() && QEnabled.IsOn)
            {
                TryCastQ();
            }
            bool cast = false;
            if (CanCastR() && ROnKill.IsOn && REnabled.IsOn)
            {
                cast = TryCastR1();
            }

            if (CanCastR() && REnabled.IsOn && !cast)
            {
                TryCastR2();
            }
            return Task.CompletedTask;
        }

        internal static Task MainTick()
        {
            _QDamage.IsOn = QDraw.IsOn && Env.QLevel >= 1;
            _QDamage.UpdateName((QModes.SelectedModeName == "AboveHPBar") ? "Q" : String.Empty);
            _QDamage.UpdateColor(ColorConverter.GetColor(QColor.SelectedModeName));
            _QDamage.UpdatePriority((uint)QPrio.Value);
            _EDamage.IsOn = EDraw.IsOn && Env.ELevel >= 1;
            _EDamage.UpdateName((EModes.SelectedModeName == "AboveHPBar") ? "E" : String.Empty);
            _EDamage.UpdateColor(ColorConverter.GetColor(EColor.SelectedModeName));
            _EDamage.UpdatePriority((uint)EPrio.Value);
            _RDamage.IsOn = RDraw.IsOn && Env.RLevel >= 1;
            _RDamage.UpdateName((RModes.SelectedModeName == "AboveHPBar") ? "R" : String.Empty);
            _RDamage.UpdateColor(ColorConverter.GetColor(RColor.SelectedModeName));
            _RDamage.UpdatePriority((uint)RPrio.Value);
            return Task.CompletedTask;
        }
    }
}
