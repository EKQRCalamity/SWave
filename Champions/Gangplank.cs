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
    internal class Gangplank : Module
    {
        internal Tab MainTab = new Tab("SyncWave - GP");
        internal Group QGroup = new Group("Q Settings");
        internal List<AIBaseClient> Barrels => GetGangplankBarrels();

        private List<AIBaseClient> GetGangplankBarrels()
        {
            List<AIBaseClient> barrels = new List<AIBaseClient>();
            foreach (AIBaseClient barrel in UnitManager.PlacementObjects.Where(x => x.ModelName.Contains("Barrel", StringComparison.OrdinalIgnoreCase)))
            {
                barrels.Add(barrel);
            }
            return barrels ;
        }

        internal override void Init()
        {
            QPQDamage QDamage = new QPQDamage();
            MenuManagerProvider
                .AddTab(MainTab);
            MainTab.AddGroup(QGroup);
            TargetedSpell targetSpell = new TargetedSpell(MainTab, QGroup, Oasys.SDK.SpellCasting.CastSlot.Q, Oasys.Common.Enums.GameEnums.SpellSlot.Q, true, QDamage, (x => x.IsAlive && x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient)), 625, 55);
            Damage qDamage = new Damage("Q", (uint)6, QDamage, SharpDX.Color.AliceBlue);
            Render.AddDamage(qDamage);
            qDamage.IsOn = true;
            GameEvents.OnCreateObject += ObjectInspector;
        }

        private Task ObjectInspector(List<AIBaseClient> callbackObjectList, AIBaseClient callbackObject, float callbackGameTime)
        {
            if (callbackObject.ModelName.Contains("Barrel", StringComparison.OrdinalIgnoreCase) && callbackObject.Health == 3)
                Logger.Log(callbackObject);
            return Task.CompletedTask;
        }
    }
}
