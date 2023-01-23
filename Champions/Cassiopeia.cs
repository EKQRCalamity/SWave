﻿using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.Menu;
using Oasys.SDK;
using Oasys.SDK.Tools;
using SyncWave.Base;
using SyncWave.Common.Helper;
using SyncWave.Common.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    internal sealed class CassioQCalc : DamageCalculation
    {
        internal int[] QBaseDamage = new int[] { 0, 70, 110, 145, 180, 215 };
        internal float QAPScaling = 0.9F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.QLevel >= 1)
            {
                damage = QBaseDamage[Env.QLevel];
                damage += QAPScaling * Env.Me().UnitStats.TotalAbilityPower;
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class CassioWCalc : DamageCalculation
    {
        internal int[] WBaseDamage = new int[] { 0, 20, 25, 30, 35, 40 };
        internal float WAPScaling = 0.15F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.QLevel >= 1)
            {
                damage = 2 *WBaseDamage[Env.QLevel];
                damage += (2 * WAPScaling) * Env.Me().UnitStats.TotalAbilityPower;
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class CassioECalc : DamageCalculation
    {
        internal int[] EBaseDamage = new int[] { 0, 52, 56, 60, 64, 68, 72, 76, 80, 84, 88, 92, 96, 100, 104, 108, 112, 116, 120 };
        internal int[] EBonusDamage = new int[] { 0, 20, 40, 60, 80, 100 };
        internal float EAPScaling = 0.1F;
        internal float EBonusAPScaling = 0.6F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.QLevel >= 1)
            {
                damage = EBaseDamage[((Env.Level > 18)? 18 : Env.Level) - 1];
                damage += EAPScaling * Env.Me().UnitStats.TotalAbilityPower;
                if (Cassiopeia.IsPoisoned(target))
                {
                    damage += EBonusDamage[Env.ELevel] + (EBonusAPScaling * Env.Me().UnitStats.TotalAbilityPower);
                }

                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }
    internal class Cassiopeia : Module
    {
        internal Tab MainTab = new Tab("SyncWave - Cassio");
        internal Group QGroup = new Group("Q Settings");
        internal Group WGroup = new Group("W Settings");
        internal Group EGroup = new Group("E Settings");

        internal static int QRange = 850;
        internal static int QRadius = 200;
        internal static float QCastTime = 0.25F;

        internal static int WRange = 700;
        internal static int WSpeed = 3000;
        internal static int WRadius = 200;
        internal static float WCastTime = 0.25F;

        internal static int ERange = 700;
        internal static int ESpeed = 2500;
        internal static float ECastTime = 0.125F;

        CassioQCalc QCalc = new CassioQCalc();
        CassioWCalc WCalc = new CassioWCalc();
        CassioECalc ECalc = new CassioECalc();

        CircleSpell qCircle;
        CircleSpell wCircle;
        TargetedSpell eTargeted;

        internal static bool IsPoisoned(GameObjectBase target)
        {
            BuffEntry poisonBuff = null;
            foreach (BuffEntry buff in target.BuffManager.GetBuffList())
            {
                if (buff.Name.Contains("cassiopeia", StringComparison.OrdinalIgnoreCase) && buff.IsActive && buff.Stacks >= 1)
                {
                    poisonBuff = buff;
                    break;
                }
            }
            // Works too
            //Logger.Log($"Test: {target.BuffManager.GetBuffList().Any(buff => buff.IsActive &&
            //       buff.Name.Contains("cassiopeia", StringComparison.OrdinalIgnoreCase))}");
            return poisonBuff != null;
        }

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            MainTab.AddGroup(QGroup);
            MainTab.AddGroup(WGroup);
            MainTab.AddGroup(EGroup);
            qCircle = new CircleSpell(MainTab, QGroup, Oasys.SDK.SpellCasting.CastSlot.Q, Oasys.Common.Enums.GameEnums.SpellSlot.Q, true, QCalc, QRange, QRadius, QCastTime, 50, false, true, false, false);
            wCircle = new CircleSpell(MainTab, WGroup, Oasys.SDK.SpellCasting.CastSlot.W, Oasys.Common.Enums.GameEnums.SpellSlot.W, true, WCalc, WRange, WRadius, WCastTime, 110, false, true, false, false);
            eTargeted = new TargetedSpell(MainTab, EGroup, Oasys.SDK.SpellCasting.CastSlot.E, Oasys.Common.Enums.GameEnums.SpellSlot.E, true, ECalc, target => target.DistanceTo(Env.Me().Position) < ERange - 20 && IsPoisoned(target) || ECalc.CalculateDamage(target) > target.PredictHealth(150), ERange - 20, ECastTime, 50, false, true, false, true);
            qCircle.SetPrediction(0.4F, 10000, false);
            wCircle.SetPrediction(0.25F, ESpeed, false);
            Damage qDamage = new Damage(MainTab, QGroup, "Q", (uint)6, QCalc, SharpDX.Color.Purple);
            Damage wDamage = new Damage(MainTab, WGroup, "W", (uint)4, WCalc, SharpDX.Color.MediumPurple);
            Damage eDamage = new Damage(MainTab, EGroup, "E", (uint)8, ECalc, SharpDX.Color.ForestGreen);
            Render.AddDamage(qDamage);
            Render.AddDamage(wDamage);
            Render.AddDamage(eDamage);
            CoreEvents.OnCoreMainTick += MainTick;
        }

        private Task MainTick()
        {
            qCircle.SetPrediction(0.4F, 10000, false);
            wCircle.SetPrediction(0.25F, ESpeed, false);
            return Task.CompletedTask;
        }
    }
}
