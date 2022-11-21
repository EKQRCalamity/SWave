using Oasys.Common.GameObject;
using Oasys.SDK;
using SyncWave.Base;
using SyncWave.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    #region DamageCalculation

    internal class GravesQDamageCalc : DamageCalculation
    {
        internal static int[] QManaCost = new int[] { 0, 80, 80, 80, 80, 80 };
        internal static int[] QDamage = new int[] { 0, 45, 60, 75, 90, 105 };
        internal static float ADScaling = 0.8F;
        internal bool IsReady()
        {
            return Env.QReady && Env.Me().Mana > QManaCost[Env.QLevel];
        }
        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (!IsReady())
                return damage;
            damage = QDamage[Env.QLevel] + (Env.Me().UnitStats.BonusAttackDamage * ADScaling);
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, damage);
        }
    }

    internal class GravesWDamageCalc : DamageCalculation
    {
        internal static int[] WManaCost = new int[] { 0, 70, 75, 80, 85, 90 };

        internal static int[] WDamage = new int[] { 0, 60, 110, 160, 210, 260 };
        internal static float APScaling = 0.6F;
        internal bool IsReady()
        {
            return Env.WReady && Env.Me().Mana > WManaCost[Env.WLevel];
        }
        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (!IsReady())
                return damage;
            damage = WDamage[Env.WLevel] + (Env.Me().UnitStats.TotalAbilityPower * APScaling);
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
        }
    }

    internal class GravesRDamageCalc : DamageCalculation
    {
        internal static int[] RManaCost = new int[] { 0, 100, 100, 100 };

        internal static int[] RDamage = new int[] { 0, 275, 425, 575 };
        internal static float ADScaling = 1.5F;
        internal bool IsReady()
        {
            return Env.RReady && Env.Me().Mana > RManaCost[Env.RLevel];
        }

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (!IsReady())
                return damage;
            damage = RDamage[Env.RLevel] + (Env.Me().UnitStats.BonusAttackDamage * ADScaling);
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
        #endregion

        internal override void Init()
        {

        }
    }
}
