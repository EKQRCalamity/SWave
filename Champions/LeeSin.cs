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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    internal sealed class LeeQCalculation : DamageCalculation
    {
        internal static int[] BaseDamage = new int[] { 0, 55, 80, 105, 130, 155 };

        internal static float ADDamageScaling = 1;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.QReady && Env.QLevel >= 1)
            {
                damage = BaseDamage[Env.QLevel] + (Env.Me().UnitStats.BonusAttackDamage * ADDamageScaling);
                return DamageCalculator.CalculateActualDamage(Env.Me(), target, damage);
            }
            return damage;
        }
    }

    internal sealed class LeeECalculation : DamageCalculation
    {
        internal static int[] BaseDamage = new int[] { 0, 100, 130, 160, 190, 220 };

        internal static float ADDamageScaling = 1;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.EReady && Env.ELevel >= 1)
            {
                damage = BaseDamage[Env.ELevel] + (Env.Me().UnitStats.BonusAttackDamage * ADDamageScaling);
                return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class LeeRCalculation : DamageCalculation
    {
        internal static int[] BaseDamage = new int[] { 0, 175, 400, 625 };

        internal static float ADDamageScaling = 2;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.RReady && Env.RLevel>= 1)
            {
                damage = BaseDamage[Env.ELevel] + (Env.Me().UnitStats.BonusAttackDamage * ADDamageScaling);
                return DamageCalculator.CalculateActualDamage(Env.Me(), target, damage);
            }
            return damage;
        }
    }

    internal class Leesin : Base.Module
    {
        #region Stats/Statics
        internal static LeeQCalculation QCalc = new LeeQCalculation();
        internal static LeeECalculation ECalc = new LeeECalculation();
        internal static LeeRCalculation RCalc = new LeeRCalculation();

        internal static Damage? _QDamage;
        internal static Damage? _EDamage;
        internal static Damage? _RDamage;

        internal static int QRange = 1200;
        internal static int QSpeed = 1800;
        internal static int QWidth = 120;
        internal static float QCastTime = 0.25F;

        internal static float ECastTime = 0.25F;
        internal static int ERadius = 450;

        internal static float RCastTime = 0.25F;
        internal static int RTargetRange = 375;
        internal static int RCollisionWidth = 160;

        internal static int EnergyCost = 50;
        #endregion

        #region Menu
        internal static Tab LeeSinTab = new Tab("SyncWave - Lee Sin");

        internal static Group QGroup = new Group("Q Settings");
        internal static Switch QEnabled = new Switch("Enabled", true);
        internal static ModeDisplay QHitChance = new ModeDisplay() { Title = "Q Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };

        internal static Group EGroup = new Group("E Settings");
        internal static Switch EEnabled = new Switch("Enabled", true);
        internal static Counter ERangeOffset = new Counter("Radius Offset", 0, -300, 300);

        internal static Group RGroup = new Group("R Settings");
        internal static Switch REnabled = new Switch("Enabled", true);
        internal static InfoDisplay RInfo = new() { Information = "Will be used when can kill." };
        
        private void InitMenu()
        {
            MenuManagerProvider.AddTab(LeeSinTab);

            LeeSinTab.AddGroup(QGroup);
            QGroup.AddItem(QEnabled);
            QGroup.AddItem(QHitChance);

            LeeSinTab.AddGroup(EGroup);
            EGroup.AddItem(EEnabled);
            EGroup.AddItem(ERangeOffset);

            LeeSinTab.AddGroup(RGroup);
            RGroup.AddItem(REnabled);
            RGroup.AddItem(RInfo);
        }
        #endregion

        internal override void Init()
        {
            Logger.Log("LeeSin Initializing...");
            InitMenu();
            CoreEvents.OnCoreMainInputAsync += MainInput;
            Render.Init();
            _QDamage = new Damage(LeeSinTab, QGroup, "Q", (uint)7, QCalc, Color.Green);
            _EDamage = new Damage(LeeSinTab, EGroup, "E", (uint)5, ECalc, Color.Orange);
            _RDamage = new Damage(LeeSinTab, RGroup, "R", (uint)4, RCalc, Color.Blue);
            Logger.Log("LeeSin Initialized!");
            Common.Spells.AimSpell Q = new(QRange, LeeSinTab, CastSlot.Q, Oasys.Common.Enums.GameEnums.SpellSlot.Q);
            Q.SetPrediction(Prediction.MenuSelected.PredictionType.Line, QRange, QWidth, QCastTime, QSpeed, true);
            Common.Spells.AimSpell R = new(RTargetRange, LeeSinTab, CastSlot.R, Oasys.Common.Enums.GameEnums.SpellSlot.R);
        }

        #region Logic
        internal static bool CanCastQ() => Env.QReady && QEnabled.IsOn && Env.Me().enoughMana(EnergyCost);
        internal static bool CanCastE() => Env.EReady && EEnabled.IsOn && Env.Me().enoughMana(EnergyCost);
        internal static bool CanCastR() => Env.RReady && REnabled.IsOn;

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
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, QRange, QWidth, QCastTime, QSpeed);
        }

        internal static void TryCastQ(GameObjectBase target)
        {
            if (target == null || Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q).SpellData.SpellName.Contains("two", StringComparison.OrdinalIgnoreCase)) return;
            if (CanCastQ() && target.IsValidTarget())
            {
                Prediction.MenuSelected.PredictionOutput pred = PredictQ(target);
                if (pred.HitChance >= GetHitchanceFromName(QHitChance.SelectedModeName))
                {
                    if (!pred.CollisionObjects.Any(x => !x.IsTargetable && !x.IsValidTarget()))
                        SpellCastProvider.CastSpell(CastSlot.Q, pred.CastPosition, QCastTime) ;
                }
            }
        }

        internal static void TryCastE(GameObjectBase target)
        {
            if (target == null || Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q).SpellData.SpellName.Contains("two", StringComparison.OrdinalIgnoreCase)) return;
            if (CanCastE() && target.IsValidTarget())
            {
                if (target.Distance < (ERadius + ERangeOffset.Value))
                {
                    SpellCastProvider.CastSpell(CastSlot.E, ECastTime);
                }
            }
        }

        internal static void TryCastR(GameObjectBase target)
        {
            if (target == null) return;
            if (CanCastR() && target.IsValidTarget())
            {
                if ((target.Health - RCalc.CalculateDamage(target)) < 0)
                {
                    Vector3 pos = target.Position;
                    pos.Y -= 10;
                    SpellCastProvider.CastSpell(CastSlot.R, pos, RCastTime);
                }
            }
        }
        #endregion

        #region Events

        private static Task MainInput()
        {
            GameObjectBase qtarget = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance <= QRange - 40);
            GameObjectBase etarget = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance <= (ERadius + ERangeOffset.Value));
            GameObjectBase rtarget = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance <= RTargetRange);
            if (qtarget != null)
            {
                TryCastQ(qtarget);
            }
            if (etarget != null)
            {
                TryCastE(etarget);
            }
            if (rtarget != null)
            {
                TryCastR(rtarget);
            }
            return Task.CompletedTask;
        }
        #endregion
    }
}
