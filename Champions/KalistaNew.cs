
using Oasys.Common.Enums.GameEnums;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using SyncWave.Base;
using SyncWave.Common.Helper;
using SyncWave.Common.Spells;
using Oasys.SDK.Rendering;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncWave.Champions 
{
    internal class KalistaQDamageCalculator : DamageCalculation 
    {
        internal static int[] BaseDamage = new int[] {0, 20, 85, 150, 215, 280};
        internal static float ADScaling = 1F;
        internal bool QCanTransferToTarget(Oasys.SDK.Prediction.MenuSelected.PredictionOutput pred) 
        {
            bool transfer = true;
            foreach (GameObjectBase target in pred.CollisionObjects) 
            {
                if (!target.CanKill(CalculateDamage(target)))
                    transfer = false;
            }
            return transfer;
        }

        internal override float CalculateDamage(GameObjectBase target) 
        {
            float damage = 0;
            if (Env.QLevel > 0 && target.IsAlive && target.IsValidTarget()) 
            {
                damage = BaseDamage[Env.QLevel] + (Env.Me().UnitStats.TotalAttackDamage * ADScaling);
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, damage);
            }
            return damage;
        }
    }

    internal class KalistaEDamageCalculator : DamageCalculation 
    {
        internal static int[] BaseDamage = new int[] { 0, 20, 30, 40, 50, 60};
        internal float ADScaling = 0.7F;

        internal static int[] AdditionalDamage = new int[] { 0, 10, 16, 22, 28, 34};
        internal static float[] AdditionalScaling = new float[] { 0, 0.232F, 0.2755F, 0.319F, 0.3625F, 0.406F };

        internal float GetEStacks(GameObjectBase target)
        {
            if (target.IsAlive && target.IsVisible) {
                return target.BuffManager.ActiveBuffs.FirstOrDefault(x => x.Name.Contains("kalistaexpungemarker"))?.Stacks ?? 0;
            }
            return 0;
        }

        internal override float CalculateDamage(GameObjectBase target) 
        {
            try
            {
                float damage = 0;
                float stacks = GetEStacks(target);
                if (Env.ELevel > 0 && target.IsAlive && target.IsValidTarget() && stacks > 0)
                {
                    damage = BaseDamage[Env.ELevel];
                    damage += ADScaling * Env.Me().UnitStats.TotalAttackDamage;
                    for (; stacks > 0; stacks--)
                    {
                        damage += AdditionalDamage[Env.ELevel] + (AdditionalScaling[Env.ELevel] * Env.Me().UnitStats.TotalAttackDamage);
                    }
                    damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, damage);
                }
                return damage;
            }catch(Exception ex)
            {
                Logger.Log($"Catched exception: {ex.ToString()}");
                return 0;
            }
            
        }
    }

    internal class KalistaNew : Module 
    {
        internal Tab MainTab = new Tab("SyncWave - Kalista");
        internal Group QGroup = new Group("Q Settings");
        internal Group EGroup = new Group("E Settings");

        internal static float QCastTime = 0.25F;
        internal static int QRange = 1200;
        internal static int QSpeed = 2400;
        internal static int QWidth = 80;

        internal static float ECastTime = 0.25F;
        internal static int ERange = 1100;

        KalistaQDamageCalculator qCalc = new();
        KalistaEDamageCalculator eCalc = new();

        LineSpell qLine;
        UntargetedMultiSpell eExecute;

        internal override void Init() 
        {
            MenuManagerProvider.AddTab(MainTab);
            MainTab.AddGroup(QGroup);
            MainTab.AddGroup(EGroup);
            qLine = new LineSpell(MainTab, QGroup, CastSlot.Q, SpellSlot.Q, true, qCalc, QRange, QWidth, QCastTime, 70, false, false, false, false, true, 99);
            eExecute = new UntargetedMultiSpell(MainTab, EGroup, CastSlot.E, SpellSlot.E, true, eCalc, ERange, ECastTime, 30, true, false, false);
            Damage qDamage = new Damage(MainTab, QGroup, "Q", (uint)6, qCalc, SharpDX.Color.DodgerBlue);
            Damage eDamage = new Damage(MainTab, EGroup, "E", (uint)7, eCalc, SharpDX.Color.Orange);
            Render.AddDamage(qDamage);
            Render.AddDamage(eDamage);
            CoreEvents.OnCoreMainTick += MainTick;
        }

        internal Task MainTick() 
        {
            qLine.SetPrediction(QCastTime, QSpeed, true);
            return Task.CompletedTask;
        }
    }
}