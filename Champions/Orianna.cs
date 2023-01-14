using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Menu;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Base;
using SyncWave.Common.Extensions;
using SyncWave.Common.Helper;
using SyncWave.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    internal class QOriannaCalc : DamageCalculation
    {
        internal static int[] Damage = new int[] { 0, 60, 90, 120, 150, 180 };

        internal static float APScaling = 0.5F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            if (Env.QReady && Env.QLevel >= 1)
            {
                return DamageCalculator.CalculateActualDamage(
                    Env.Me(),
                    target,
                    0,
                    (Damage[Env.QLevel] + (Env.Me().UnitStats.TotalAbilityPower * APScaling)),
                    0
                    );
            }
            return 0;
        }
    }

    internal class WOriannaCalc : DamageCalculation
    {
        internal static int[] Damage = new int[] { 0, 60, 105, 150, 195, 240 };
        internal static float APScaling = 0.7F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            if (Env.WReady&& Env.WLevel >= 1)
            {
                return DamageCalculator.CalculateActualDamage(
                    Env.Me(),
                    target,
                    0,
                    (Damage[Env.WLevel] + (Env.Me().UnitStats.TotalAbilityPower * APScaling)),
                    0
                    );
            }
            return 0;
        }
    }

    internal class EOriannaCalc : DamageCalculation
    {
        internal static int[] Damage = new int[] { 0, 60, 90, 120, 150, 180 };
        internal static float APScaling = 0.3F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            if (Env.EReady && Env.ELevel >= 1)
            {
                return DamageCalculator.CalculateActualDamage(
                    Env.Me(),
                    target,
                    0,
                    (Damage[Env.ELevel] + (Env.Me().UnitStats.TotalAbilityPower * APScaling)),
                    0
                    );
            }
            return 0;
        }
    }

    internal class ROriannaCalc : DamageCalculation
    {
        internal static int[] Damage = new int[] { 0, 200, 275, 350 };
        internal static float APScaling = 0.8F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            if (Env.RReady && Env.RLevel >= 1)
            {
                return DamageCalculator.CalculateActualDamage(
                    Env.Me(),
                    target,
                    0,
                    (Damage[Env.RLevel] + (Env.Me().UnitStats.TotalAbilityPower * APScaling)),
                    0
                    );
            }
            return 0;
        }
    }

    internal class Orianna : Module
    {
        #region Logic

        internal bool CanCastQ() => Env.QReady && QEnabled.IsOn && Env.Me().enoughMana(QManaCost[Env.QLevel]) && Env.Me().IsAlive;
        internal bool CanCastW() => Env.WReady && WEnabled.IsOn && Env.Me().enoughMana(WManaCost[Env.WLevel]) && Env.Me().IsAlive;
        internal bool CanCastE() => Env.EReady && EEnabled.IsOn && Env.Me().enoughMana(EManaCost[Env.ELevel]) && Env.Me().IsAlive;
        internal bool CanCastR() => Env.RReady && REnabled.IsOn && Env.Me().enoughMana(RManaCost[Env.RLevel]) && Env.Me().IsAlive;

        internal static GameObjectBase Ball { get; set; } = UnitManager.AllNativeObjects.FirstOrDefault(x => x.Name == "TheDoomBall" && x.IsAlive && x.Health >= 1);

        internal static bool IsBallOnMe()
        {
            var buffs = UnitManager.MyChampion.BuffManager.Buffs.deepCopy();
            var buff = buffs.FirstOrDefault(x => x.Name == "orianaghostself");
            return buff != null && buff.IsActive && buff.Stacks > 0;
        }

        internal static Hero? getBallHolder()
        {
            if (Ball == null)
            {
                List<Hero> clients = UnitManager.AllyChampions.deepCopy();
                foreach (Hero client in clients)
                {
                    List<BuffEntry> buffs = client.BuffManager.Buffs.deepCopy();
                    BuffEntry buff = buffs.FirstOrDefault(x => x.Name == "orianaghost" || x.Name == "orianaghostself");
                    if (client.IsAlive && buff != null && buff.IsActive && buff.Stacks > 0)
                        return client;
                }
            }
            return null;
        }

        internal static Vector3 GetBallPosition()
        {
            if (Ball == null)
            {
                Hero? holder = getBallHolder();
                if (holder != null)
                {
                    return holder.Position;
                }
                else
                {
                    return Env.Me().Position;
                }

            }
            else
            {
                return Ball.Position;
            }
        }
        
        internal static Vector3 QPosition() => GetBallPosition();

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

        Common.SpellAim.AimSpell? Q;

        #endregion

        #region Stats
        internal static QOriannaCalc QCalc = new();
        internal static WOriannaCalc WCalc = new();
        internal static EOriannaCalc ECalc = new();
        internal static ROriannaCalc RCalc = new();

        internal static Damage? _QDamage;
        internal static Damage? _WDamage;
        internal static Damage? _EDamage;
        internal static Damage? _RDamage;

        internal static int[] QManaCost = new int[] { 0, 30, 35, 40, 45, 50 };
        internal static int[] WManaCost = new int[] { 0, 70, 80, 90, 100, 110 };
        internal static int[] EManaCost = new int[] { 0, 60, 60, 60, 60, 60 };
        internal static int[] RManaCost = new int[] { 0, 100, 100, 100 };

        internal static int[] QDamage = new int[] { 0, 60, 90, 120, 150, 180 };
        internal static int[] WDamage = new int[] { 0, 60, 105, 150, 195, 240 };
        internal static int[] EDamage = new int[] { 0, 60, 90, 120, 150, 180 };
        internal static int[] RDamage = new int[] { 0, 200, 275, 350 };

        internal static int[] EShield = new int[] { 0, 55, 90, 125, 160, 195 };

        internal static float QDamageScaling = 0.5F;
        internal static float WDamageScaling = 0.7F;
        internal static float EDamageScaling = 0.3F;
        internal static float RDamageScaling = 0.8F;

        internal static float EShieldScaling = 0.45F;

        internal static int QRange = 815;
        internal static int QImpactRange = 175;
        internal static int QWidth = 160;
        internal static int QSpeed = 1400;

        internal static int WRadius = 225;

        internal static int LeashRange = 1290;
        internal static int ERange = 1120;
        internal static int EWidth = 160;
        internal static int ESpeed = 1850;

        internal static int RRadius = 415;
        #endregion

        #region Menu
        internal static Tab OriannaTab = new Tab("SyncWave - Orianna");
        internal static Group QGroup = new Group("Q Settings");
        internal static Switch QEnabled = new Switch("Enabled", true);
        internal static ModeDisplay QHitChance = new ModeDisplay() { Title = "Q Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };
        internal static Switch DrawQ = new Switch("Draw Q Damage", true);
        internal static ModeDisplay DrawQMode = new ModeDisplay() { Title = "Draw Mode", ModeNames = new() { "AboveHPBar", "AboveWithoutName" } };
        internal static Counter DrawQPrio = new Counter("Draw Prio", 5, 1, 10);
        internal static InfoDisplay QPrioInfo = new InfoDisplay() { Title = "Prio", Information = "Prio doesnt work for OnHPBar" };
        internal static ModeDisplay DrawQColor = new ModeDisplay("Draw Color", Color.DodgerBlue);

        internal static Group WGroup = new Group("W Settings");
        internal static Switch WEnabled = new Switch("Enabled", true);
        internal static Switch DrawW = new Switch("Draw W Damage", true);
        internal static ModeDisplay DrawWMode = new ModeDisplay() { Title = "Draw Mode", ModeNames = new() { "AboveHPBar", "AboveWithoutName" } };
        internal static Counter DrawWPrio = new Counter("Draw Prio", 5, 1, 10);
        internal static InfoDisplay WPrioInfo = new InfoDisplay() { Title = "Prio", Information = "Prio doesnt work for OnHPBar" };
        internal static ModeDisplay DrawWColor = new ModeDisplay("Draw Color", Color.DodgerBlue);

        internal static Group EGroup = new Group("E Settings");
        internal static Switch EEnabled = new Switch("Enabled", true);
        internal static Switch EShieldEnabled = new Switch("Shield Enabled", true);
        internal static Counter EShieldHP = new Counter("Shield HP% Threshold", 20, 0, 100);
        internal static Switch DrawE = new Switch("Draw E Damage", true);
        internal static ModeDisplay DrawEMode = new ModeDisplay() { Title = "Draw Mode", ModeNames = new() { "AboveHPBar", "AboveWithoutName" } };
        internal static Counter DrawEPrio = new Counter("Draw Prio", 5, 1, 10);
        internal static InfoDisplay EPrioInfo = new InfoDisplay() { Title = "Prio", Information = "Prio doesnt work for OnHPBar" };
        internal static ModeDisplay DrawEColor = new ModeDisplay("Draw Color", Color.DodgerBlue);

        internal static Group RGroup = new Group("R Settings");
        internal static Switch REnabled = new Switch("Enabled", true);
        internal static Switch RUseOnKill = new Switch("Use on Kill", true);
        internal static Counter REnemies = new Counter("Use on enemies near", 2, 0, 5);
        internal static Switch DrawR = new Switch("Draw R Damage", true);
        internal static ModeDisplay DrawRMode = new ModeDisplay() { Title = "Draw Mode", ModeNames = new() { "AboveHPBar", "AboveWithoutName" } };
        internal static Counter DrawRPrio = new Counter("Draw Prio", 5, 1, 10);
        internal static InfoDisplay RPrioInfo = new InfoDisplay() { Title = "Prio", Information = "Prio doesnt work for OnHPBar" };
        internal static ModeDisplay DrawRColor = new ModeDisplay("Draw Color", Color.DodgerBlue);

        internal static void InitMenu()
        {
            MenuManager.AddTab(OriannaTab);
            OriannaTab.AddGroup(QGroup);
            QGroup.AddItem(QEnabled);
            QGroup.AddItem(QHitChance);
            QGroup.AddItem(DrawQ);
            QGroup.AddItem(DrawQMode);
            QGroup.AddItem(DrawQPrio);
            QGroup.AddItem(DrawQColor);

            OriannaTab.AddGroup(WGroup);
            WGroup.AddItem(WEnabled);
            WGroup.AddItem(DrawW);
            WGroup.AddItem(DrawWMode);
            WGroup.AddItem(DrawWPrio);
            WGroup.AddItem(DrawWColor);

            OriannaTab.AddGroup(EGroup);
            EGroup.AddItem(EEnabled);
            EGroup.AddItem(EShieldEnabled);
            EGroup.AddItem(EShieldHP);
            EGroup.AddItem(DrawE);
            EGroup.AddItem(DrawEMode);
            EGroup.AddItem(DrawEPrio);
            EGroup.AddItem(DrawEColor);

            OriannaTab.AddGroup(RGroup);
            RGroup.AddItem(REnabled);
            RGroup.AddItem(RUseOnKill);
            RGroup.AddItem(REnemies);
            RGroup.AddItem(DrawR);
            RGroup.AddItem(DrawRMode);
            RGroup.AddItem(DrawRPrio);
            RGroup.AddItem(DrawRColor);
        }

        #endregion
        internal override void Init()
        {
            Logger.Log("Orianna Initializing...");
            InitMenu();
            CoreEvents.OnCoreMainTick += OnCoreMainTick;
            CoreEvents.OnCoreMainInputAsync += MainInput;
            Render.Init();
            _QDamage = new Damage("Q", (uint)DrawQPrio.Value, QCalc, ColorConverter.GetColor(DrawQColor.SelectedModeName));
            _WDamage = new Damage("W", (uint)DrawWPrio.Value, WCalc, ColorConverter.GetColor(DrawWColor.SelectedModeName));
            _EDamage = new Damage("E", (uint)DrawEPrio.Value, ECalc, ColorConverter.GetColor(DrawEColor.SelectedModeName));
            _RDamage = new Damage("R", (uint)DrawRPrio.Value, RCalc, ColorConverter.GetColor(DrawRColor.SelectedModeName));
            Render.AddDamage(_QDamage);
            Render.AddDamage(_WDamage);
            Render.AddDamage(_EDamage);
            Render.AddDamage(_RDamage);
            RangeDrawer.WDisabled = true;
            RangeDrawer.RDisabled = true;
            Q = new(QRange, OriannaTab, CastSlot.Q, Oasys.Common.Enums.GameEnums.SpellSlot.Q);
            Q.SetPrediction(
                Prediction.MenuSelected.PredictionType.Line,
                QRange,
                QWidth,
                0,
                QSpeed,
                true,
                QPosition());
            Logger.Log("Orianna Initialized.");

        }

        #region Casts
        internal Prediction.MenuSelected.PredictionOutput PredictQ(GameObjectBase target)
        {
            return Prediction.MenuSelected.GetPrediction(
                Prediction.MenuSelected.PredictionType.Line,
                target,
                QRange,
                QWidth,
                0,
                QSpeed,
                QPosition(),
                true);
        }

        internal void TryCastQ(GameObjectBase target)
        {
            if (target == null) return;
            if (CanCastQ() && target.IsValidTarget())
            {
                Prediction.MenuSelected.PredictionOutput pred = PredictQ(target);
                if (pred.HitChance >= QHitChance.SelectedModeName.GetHitchanceFromName())
                {
                    if (!pred.CollisionObjects.Any(x => !x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIMinionClient) && !!x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient)))
                    {
                        SpellCastProvider.CastSpell(CastSlot.Q, pred.CastPosition);
                    }
                }
            }
        }

        internal GameObjectBase? GetWTarget()
        {
            foreach (GameObjectBase target in UnitManager.EnemyChampions)
            {
                Logger.Log($"{target.DistanceTo(QPosition()) < WRadius} - {target.ModelName}");
                if (target.DistanceTo(QPosition()) < WRadius && target.IsAlive)
                    return target;
            }
            return null;
        }
        internal void TryCastW()
        {
            GameObjectBase? target = GetWTarget();
            if (target == null) return;
            if (CanCastW() && target.IsValidTarget())
            {
                SpellCastProvider.CastSpell(CastSlot.W, 0);
            }
        } 

        internal GameObjectBase? GetETarget()
        {
            foreach (GameObjectBase target in UnitManager.EnemyChampions)
            {
                if (!target.IsAlive)
                    continue;
                foreach (GameObjectBase ally in UnitManager.AllyChampions)
                {
                    Vector2 in1 = Vector2.Zero;
                    Vector2 in2 = Vector2.Zero;
                    int intersections = Oasys.Common.Logic.Geometry.LineCircleIntersection(target.Position.ToW2S().X, target.Position.ToW2S().Y, target.UnitComponentInfo.UnitBoundingRadius, QPosition().ToW2S(), ally.Position.ToW2S(), out in1, out in2);
                    bool isLineCollision = (intersections >= 1);
                    if (isLineCollision)
                    {
                        return ally;
                    }
                }
            }
            return null;
        }

        internal GameObjectBase? GetEShieldTarget()
        {
            int currentPrio = 0;
            GameObjectBase? target = null;
            foreach (GameObjectBase ally in UnitManager.AllyChampions)
            {
                if (ally.HealthPercent > EShieldHP.Value)
                    continue;
                if (target == null)
                    target = ally;
                if (target.HealthPercent > ally.HealthPercent)
                    target = ally;
            }
            return target;
        }

        internal void TryCastE()
        {
            if (!CanCastE())
                return;
            GameObjectBase? eTarget = GetEShieldTarget();
            if (eTarget == null && eTarget.IsAlive)
            {
                GameObjectBase? target = GetETarget();
                if (target != null)
                {
                    eTarget = target;
                }
                if (target == null || !target.IsValidTarget())
                    return;
            }
            SpellCastProvider.CastSpell(CastSlot.E, eTarget.Position, 0);
        }

        internal GameObjectBase? GetRTarget()
        {
            GameObjectBase? currentTarget = null;
            foreach (GameObjectBase target in UnitManager.EnemyChampions)
            {
                if (target.DistanceTo(QPosition()) < RRadius && target.IsAlive)
                {
                    if (currentTarget == null || (currentTarget.Health - RCalc.CalculateDamage(currentTarget)) > (target.Health - RCalc.CalculateDamage(target)))
                        currentTarget = target;
                }
            }
            return currentTarget;
        }

        internal void TryCastR()
        {
            GameObjectBase? target = GetRTarget();
            if (target == null) return;
            if (CanCastR() && target.IsValidTarget())
            {
                if ((target.Health - RCalc.CalculateDamage(target)) < 0 && RUseOnKill.IsOn)
                    SpellCastProvider.CastSpell(CastSlot.R, 0);
                if (EnemiesInRange(RRadius, QPosition()) >= REnemies.Value)
                    SpellCastProvider.CastSpell(CastSlot.R, 0);
            }
        }

        #endregion

        private Task MainInput()
        {
            if (QEnabled.IsOn)
            {
                GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance < QRange && x.IsAlive);
                TryCastQ(target);
            }
            if (WEnabled.IsOn)
            {
                TryCastW();
            }
            if (EEnabled.IsOn)
            {
                TryCastE();
            }
            if (REnabled.IsOn)
                TryCastR();
            return Task.CompletedTask;
        }

        private Task OnCoreMainTick()
        {
            if (Ball == null || !Ball.IsAlive || Ball.Health < 1 || !IsBallOnMe())
            {
                Ball = UnitManager.AllNativeObjects.FirstOrDefault(x => x.Name == "TheDoomBall" && x.IsAlive && x.Health >= 1);
            }

            Q.SetPrediction(
                Prediction.MenuSelected.PredictionType.Line,
                QRange,
                QWidth,
                0,
                QSpeed,
                true,
                QPosition());
            _QDamage.IsOn = DrawQ.IsOn && Env.QLevel >= 1;
            _QDamage.UpdateName((DrawQMode.SelectedModeName == "AboveHPBar") ? "Q" : String.Empty);
            _QDamage.UpdateColor(ColorConverter.GetColor(DrawQColor.SelectedModeName));
            _QDamage.UpdatePriority((uint)DrawQPrio.Value);
            _WDamage.IsOn = DrawW.IsOn && Env.WLevel >= 1;
            _WDamage.UpdateName((DrawWMode.SelectedModeName == "AboveHPBar") ? "W" : String.Empty);
            _WDamage.UpdateColor(ColorConverter.GetColor(DrawWColor.SelectedModeName));
            _WDamage.UpdatePriority((uint)DrawWPrio.Value);
            _EDamage.IsOn = DrawE.IsOn && Env.ELevel >= 1;
            _EDamage.UpdateName((DrawEMode.SelectedModeName == "AboveHPBar") ? "E" : String.Empty);
            _EDamage.UpdateColor(ColorConverter.GetColor(DrawEColor.SelectedModeName));
            _EDamage.UpdatePriority((uint)DrawEPrio.Value);
            _RDamage.IsOn = DrawR.IsOn && Env.RLevel >= 1;
            _RDamage.UpdateName((DrawRMode.SelectedModeName == "AboveHPBar") ? "R" : String.Empty);
            _RDamage.UpdateColor(ColorConverter.GetColor(DrawRColor.SelectedModeName));
            _RDamage.UpdatePriority((uint)DrawRPrio.Value);
            return Task.CompletedTask;
        }
    }
}
