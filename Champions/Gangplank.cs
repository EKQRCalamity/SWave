using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.SDK;
using SyncWave.Base;
using SyncWave.Common.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncWave.Common.Helper;
using System.Windows.Forms;
using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject.Clients;
using Oasys.SDK.Tools;
using SharpDX;

namespace SyncWave.Champions
{ 
    internal class QPQDamage : DamageCalculation
    {
        internal int[] QBaseDamage = new int[] { 0, 10, 40, 70, 100, 130 };
        internal float ADScaling = 1.0f;
        internal float Speed = 2600.0f;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (target.IsValidTarget() && target.IsAlive && target.IsTargetable)
            {
                damage = QBaseDamage[Env.QLevel];
                damage = damage + (Env.Me().UnitStats.TotalAttackDamage * ADScaling);
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, damage);
            }
            return damage;
        }
    }

    internal class _RDamage : DamageCalculation {
        internal int[] RBaseDamage = new int[] { 0, 480, 840, 1200 };
        internal float APScaling = 1.2f;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (target.IsValidTarget() && target.IsAlive && target.IsTargetable)
            {
                damage = RBaseDamage[Env.RLevel];
                damage = damage + (Env.Me().UnitStats.TotalAbiltiyPower * APScaling);
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal class GPBarrelPos {
        internal AIBaseClient SelectedBarrel { get; set; }
        internal AIBaseClient SelectedTarget { get; set; }
        internal Vector3 SelectedPosition { get; set; }
        internal bool ValidPosition => SelectedPosition != Vector3.Zero;
        internal bool ValidTarget => SelectedTarget != null && SelectedTarget.IsValidTarget();
        internal bool ValidBarrel => SelectedBarrel != null && SelectedBarrel.IsAlive && SelectedBarrel.IsTargetable;

        public GPBarrelPos(AIBaseClient selectedBarrel, AIBaseClient selectedTarget, Vector3 selectedPosition) {
            SelectedBarrel = selectedBarrel;
            SelectedTarget = selectedTarget;
            SelectedPosition = selectedPosition;
        }
    }

    internal class Gangplank : Module
    {
        internal Tab MainTab = new Tab("SyncWave - GP");
        internal Group QGroup = new Group("Q Settings");
        internal Group RGroup = new Group("R Settings");
        internal List<AIBaseClient> Barrels => GetGangplankBarrels();
        internal int QRange = 650;
        internal int ERange = 1000;
        internal int EExplosionRange = 360;
        internal int EBindRange = 345;
        internal Random randomGen = new Random();

        private List<AIBaseClient> GetGangplankBarrels()
        {
            List<AIBaseClient> barrels = new List<AIBaseClient>();
            foreach (AIBaseClient barrel in UnitManager.PlacementObjects.Where(x => x.ModelName.Contains("Barrel", StringComparison.OrdinalIgnoreCase)))
            {
                barrels.Add(barrel);
            }
            return barrels ;
        }

        private GPBarrelPos GetBarrelPlacementPosition(AIBaseClient selectedBarrel=null, AIBaseClient selectedTarget=null) 
        {
            îf (selectedBarrel == null) 
            {
                List<AIBaseClient> barrels = Barrels.deepCopy();
                AIBaseClient target;
                if (selectedTarget == null) {
                    target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, (x=>x.Distance < ERange && x.IsTargetable && x.IsAlive));
                } else {
                    target = selectedTarget;
                }
                foreach (AIBaseClient barrel in barrels) 
                {
                    if (target.DistanceTo(barrel) < (EBindRange * 2) && target.Distance < ERange && Env.Me().DistanceTo(barrel) < QRange) 
                    {
                        if (target.DistanceTo(barrel) < EExplosionRange) {
                            return new(barrel, target, Vector3.Zero);
                        }
                        Vector3 barrelPos = target.Position.Extend(Env.Me().Position, target.Position, -(randomGen.Next(5,35)));
                        return new(barrel, target, barrelPos);
                    }
                }                
            }
            return new(null, null, Vector3.Zero);
        }

        QPQDamage QDamage = new QPQDamage();
        _RDamage RDamage = new RDamage();

        internal override void Init()
        {
            MenuManagerProvider
                .AddTab(MainTab);
            MainTab.AddGroup(QGroup);
            TargetedSpell targetSpell = new TargetedSpell(MainTab, QGroup, Oasys.SDK.SpellCasting.CastSlot.Q, Oasys.Common.Enums.GameEnums.SpellSlot.Q, true, QDamage, (x => x.IsAlive && x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient)), 625, 55);
            CircleSpell rSpell = new CircleSpell(MainTab, RGroup, Oasys.SDK.SpellCasting.CastSlot.R, Oasys.Common.Enums.GameEnums.SpellSlot.Q, true, _RDamage, (x => x.IsAlive && x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient)), 20000, 580)
            Damage qDamage = new Damage("Q", (uint)6, QDamage, SharpDX.Color.AliceBlue);
            Damage rDamage = new Damage("R", (uint)4, RDamage, SharpDX.Color.Red);
            Render.AddDamage(qDamage);
            Render.AddDamage(rDamage);
            qDamage.IsOn = true;
            GameEvents.OnCreateObject += ObjectInspector;
            CoreEvents.OnCoreMainInput += MainInput;
        }

        private Task MainInput() 
        {
            bool orgiIsOn = QDamage.IsOn;
            QDamage.IsOn = false;
            GPBarrelPos BarrelObj = GetBarrelPlacementPosition(null, null)
            if (BarrelObj.ValidBarrel && BarrelObj.ValidTarget) 
            {
                if (Env.EReady &6 Env.ELevel >= 1 && Env.QReady && BarrelObj.SelectedBarrel.Health < 1.5) 
                {
                    if (!BarrelObj.ValidPosition) {
                        SpelLCastProvider.CastSpell(CastSlot.Q, BarrelObj.SelectedBarrel.Position);
                    } else {
                        SpellCastProvider.CastSpell(CastSlot.E, BarrelObj.SelectedPosition);
                        SpelLCastProvider.CastSpell(CastSlot.Q, BarrelObj.SelectedBarrel.Position);
                    }
                }
            }
            QDamage.IsOn = origIsOn;
            return Task.CompletedTask;
        }

        private Task ObjectInspector(List<AIBaseClient> callbackObjectList, AIBaseClient callbackObject, float callbackGameTime)
        {
            if (callbackObject.ModelName.Contains("Barrel", StringComparison.OrdinalIgnoreCase) && callbackObject.Health == 3)
                Logger.Log(callbackObject);
            return Task.CompletedTask;
        }
    }
}
