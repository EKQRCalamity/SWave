using Oasys.Common.Enums.GameEnums;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using SharpDX.DXGI;
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
    internal class LuxQCalc : DamageCalculation
    {
        internal static int[] BaseDamage = new int[] { 0, 80, 120, 160, 200, 240 };
        internal static float APScaling = 0.6F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.QLevel > 0 && target.IsAlive && target.IsValidTarget())
            {
                damage = BaseDamage[Env.QLevel] + (Env.Me().UnitStats.TotalAbilityPower * APScaling);
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }
    internal class LuxECalc : DamageCalculation
    {
        internal static int[] BaseDamage = new int[] { 0, 70, 120, 170, 220, 270 };
        internal static float APScaling = 0.8F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.ELevel > 0 && target.IsAlive && target.IsValidTarget())
            {
                damage = BaseDamage[Env.ELevel] + (Env.Me().UnitStats.TotalAbilityPower * APScaling);
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }
    internal class LuxRCalc : DamageCalculation
    {
        internal static int[] BaseDamage = new int[] { 0, 300, 400, 500 };
        internal static float APScaling = 1.2F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.RLevel > 0 && target.IsAlive && target.IsValidTarget())
            {
                damage = BaseDamage[Env.RLevel] + (Env.Me().UnitStats.TotalAbilityPower * APScaling);
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }
    internal class Lux : Module
    {
        internal Tab MainTab = new Tab("SyncWave - Lux");
        internal Group QGroup = new Group("Q Settings");
        internal Group EGroup = new Group("E Settings");
        internal Group RGroup = new Group("R Settings");

        internal static float QCastTime = 0.25F;
        internal static int QRange = 1240;
        internal static int QSpeed = 1200;
        internal static int QWidth = 140;

        internal static float ECastTime = 0.25F;
        internal static int ERange = 1100;
        internal static int ERadius = 310;
        internal static int ESpeed = 1200;

        internal static float RCastTime = 0.25F;
        internal static int RRange = 3400;
        internal static int RSpeed = 10000;
        internal static int RWidth = 200;

        LuxQCalc qCalc = new();
        LuxECalc eCalc = new();
        LuxRCalc rCalc = new();

        LineSpell qLine;
        CircleSpell eCircle;
        LineSpell rLine;

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            MainTab.AddGroup(QGroup);
            MainTab.AddGroup(EGroup);
            MainTab.AddGroup(RGroup);
            qLine = new LineSpell(MainTab, QGroup, CastSlot.Q, SpellSlot.Q, true, qCalc, QRange, QWidth, QCastTime, 50, false, false, false, false, true, 99);
            eCircle = new CircleSpell(MainTab, EGroup, CastSlot.E, SpellSlot.E, true, eCalc, ERange, ERadius, ECastTime, 110, false, true, false, false);
            rLine = new LineSpell(MainTab, RGroup, CastSlot.R, SpellSlot.R, true, rCalc, RRange, RWidth, RCastTime, 100, true, false, false, false, false);
            Damage qDamage = new Damage(MainTab, QGroup, "Q", (uint)6, qCalc, SharpDX.Color.Aqua);
            Damage eDamage = new Damage(MainTab, EGroup, "E", (uint)5, eCalc, SharpDX.Color.Salmon);
            Damage rDamage = new Damage(MainTab, RGroup, "R", (uint)9, rCalc, SharpDX.Color.SandyBrown);
            Render.AddDamage(qDamage);
            Render.AddDamage(eDamage);
            Render.AddDamage(rDamage);
            CoreEvents.OnCoreMainTick += MainTick;
        }

        private Task MainTick()
        {
            qLine.SetPrediction(QCastTime, QSpeed, true);
            eCircle.SetPrediction(ECastTime, ESpeed, false);
            rLine.SetPrediction(RCastTime, RSpeed, false);
            return Task.CompletedTask;
        }
    }
}
