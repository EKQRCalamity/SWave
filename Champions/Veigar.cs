using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.SDK;
using SyncWave.Base;
using SyncWave.Common.Helper;
using SyncWave.Common.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    internal sealed class VeigarQCalc : DamageCalculation
    {
        internal int[] BaseDamage = new int[] { 0, 80, 120, 160, 200, 240 };
        internal float APScaling = 0.6f;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.QLevel >= 1)
            {
                damage = BaseDamage[Env.QLevel];
                damage += APScaling * Env.Me().UnitStats.TotalAbilityPower;
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class VeigarWCalc : DamageCalculation
    {
        internal int[] BaseDamage = new int[] { 0, 100, 150, 200, 250, 300 };
        internal float APScaling = 1.0f;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.WLevel >= 1)
            {
                damage = BaseDamage[Env.WLevel];
                damage += APScaling * Env.Me().UnitStats.TotalAbilityPower;
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class VeigarRCalc : DamageCalculation
    {
        internal int[] BaseDamage = new int[] { 0, 175, 250, 325 };
        internal float APScaling = 0.75f;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.RLevel >= 1)
            {
                damage = BaseDamage[Env.RLevel];
                damage += APScaling * Env.Me().UnitStats.TotalAbilityPower;
                damage +=  damage * (target.MissingHealthPercent / 100);
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }
    internal class Veigar : Module
    {
        internal Tab MainTab = new Tab("SyncWave - Veigar");
        internal Group QGroup = new Group("Q Settings");
        internal Group WGroup = new Group("W Settings");
        internal Group RGroup = new Group("R Settings");

        internal static int QRange = 890;
        internal static int QWidth = 140;
        internal static int QSpeed = 2200;
        internal static float QCastTime = 0.25F;

        internal static int WRange = 900;
        internal static int WRadius = 240;
        internal static float WCastTime = 0.25F;

        internal static int RRange = 650;
        internal static float RCastTime = 0.25F;

        VeigarQCalc QCalc = new VeigarQCalc();
        VeigarWCalc WCalc = new VeigarWCalc();
        VeigarRCalc RCalc = new VeigarRCalc();

        LineSpell qLine;
        CircleSpell wCircle;
        TargetedSpell rTargeted;

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            MainTab.AddGroup(QGroup);
            MainTab.AddGroup(WGroup);
            MainTab.AddGroup(RGroup);
            qLine = new LineSpell(MainTab, QGroup, Oasys.SDK.SpellCasting.CastSlot.Q, Oasys.Common.Enums.GameEnums.SpellSlot.Q, true, QCalc, QRange, QWidth, QCastTime, 50, false, true, true, true, false);
            wCircle = new CircleSpell(MainTab, WGroup, Oasys.SDK.SpellCasting.CastSlot.W, Oasys.Common.Enums.GameEnums.SpellSlot.W, true, WCalc, WRange, WRadius, WCastTime, 90, false, true, true, false);
            rTargeted = new TargetedSpell(MainTab, RGroup, Oasys.SDK.SpellCasting.CastSlot.R, Oasys.Common.Enums.GameEnums.SpellSlot.R, true, RCalc, RRange, RCastTime, 100, true, false, false, false);
            Damage qDamage = new Damage(MainTab, QGroup, "Q", (uint)6, QCalc, SharpDX.Color.Purple);
            Damage wDamage = new Damage(MainTab, WGroup, "W", (uint)4, WCalc, SharpDX.Color.MediumPurple);
            Damage rDamage = new Damage(MainTab, RGroup, "R", (uint)7, RCalc, SharpDX.Color.AliceBlue);
            Render.AddDamage(qDamage);
            Render.AddDamage(wDamage);
            Render.AddDamage(rDamage);
            CoreEvents.OnCoreMainTick += MainTick;
        }

        internal Task MainTick()
        {
            qLine.SetPrediction(0.25F, QSpeed, true);
            wCircle.SetPrediction(0.25F, 10000, false);
            
            return Task.CompletedTask;
        }
    }
}
