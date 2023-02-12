using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Events;
using Oasys.SDK.Menu;
using Oasys.SDK.Rendering;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SharpDX.DXGI;
using SyncWave.Base;
using SyncWave.Combos.Kogmaw;
using SyncWave.Common.Extensions;
using SyncWave.Common.Helper;
using SyncWave.Common.Spells;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    internal class KogmawQCalc : DamageCalculation
    {
        internal static int[] BaseDamage = new int[] { 0, 90, 140, 190, 240, 290 };
        internal static float APScaling = 0.7F;

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

    internal class KogmawECalc : DamageCalculation
    {
        internal static int[] BaseDamage = new int[] { 0, 75, 120, 165, 210, 255 };
        internal static float APScaling = 0.7F;

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

    internal class KogmawRCalc : DamageCalculation
    {
        internal static int[] BaseDamage = new int[] { 0, 100, 140, 180 };
        internal static float APScaling = 0.35F;
        internal static float BADScaling = 0.65F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.QLevel > 0 && target.IsAlive && target.IsValidTarget())
            {
                damage = BaseDamage[Env.QLevel] + (Env.Me().UnitStats.TotalAbilityPower * APScaling) + (Env.Me().UnitStats.BonusArmor * BADScaling);
                if (target.HealthPercent > 40)
                {
                    damage = damage + (damage * (target.HealthPercent * 0.883F) / 100);
                } else
                {
                    damage = damage * 2;
                }
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal class Kogmaw : SyncWave.Base.Module
    {
        internal Tab MainTab = new Tab("SyncWave - Kog'Maw");
        internal Group QGroup = new Group("Q Settings");
        internal Group EGroup = new Group("E Settings");
        internal Group RGroup = new Group("R Settings");

        internal static float QCastTime = 0.25F;
        internal static int QRange = 1200;
        internal static int QSpeed = 1650;
        internal static int QWidth = 140;

        internal static float ECastTime = 0.25F;
        internal static int ERange = 1360;
        internal static int ESpeed = 1400;
        internal static int EWidth = 240;

        internal static float RCastTime = 0.25F;
        internal static int[] RTargetRange = new int[] { 0, 1300, 1550, 1800 };
        internal static int RRadius = 240;

        KogmawQCalc qCalc = new KogmawQCalc();
        KogmawECalc eCalc = new KogmawECalc();
        KogmawRCalc rCalc = new KogmawRCalc();

        LineSpell qLine;
        LineSpell eLine;
        CircleSpell rCircle;

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            MainTab.AddGroup(QGroup);
            MainTab.AddGroup(EGroup);
            MainTab.AddGroup(RGroup);
            qLine = new LineSpell(MainTab, QGroup, CastSlot.Q, SpellSlot.Q, true, qCalc, QRange, QWidth, QCastTime, 40, false, false, false, false, true, 99);
            eLine = new LineSpell(MainTab, EGroup, CastSlot.E, SpellSlot.E, true, eCalc, ERange, EWidth, ECastTime, 40, false, false, false, false, true, 99);
            rCircle = new CircleSpell(MainTab, RGroup, CastSlot.R, SpellSlot.R, true, rCalc, RTargetRange[1], RRadius, RCastTime, 100, true, false, false, false);
            Damage qDamage = new Damage(MainTab, QGroup, "Q", (uint)6, qCalc, SharpDX.Color.DodgerBlue);
            Damage eDamage = new Damage(MainTab, EGroup, "E", (uint)7, eCalc, SharpDX.Color.Orange);
            Damage rDamage = new Damage(MainTab, RGroup, "R", (uint)9, rCalc, SharpDX.Color.Red);
            Render.AddDamage(qDamage);
            Render.AddDamage(eDamage);
            Render.AddDamage(rDamage);
            CoreEvents.OnCoreMainTick += MainTick;
        }

        private Task MainTick()
        {
            qLine.SetPrediction(QCastTime, QSpeed, true);
            eLine.SetPrediction(ECastTime, ESpeed, true);
            rCircle.Range = RTargetRange[Env.RLevel];
            rCircle.SetPrediction(0.6F, 100000, false);
            return Task.CompletedTask;
        }
    }

}
