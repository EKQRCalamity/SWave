using Oasys.Common.Enums.GameEnums;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using SharpDX;
using SyncWave.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Spells
{
    internal class TargetedSpell : SpellCastBase
    {
        internal float castTime;

        public TargetedSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, DamageCalculation effectCalculator, Func<GameObjectBase, bool> targetSelector, int range, float CastTime = 0,int minMana = 0)
        {
            castTime = CastTime;
            MainTab = mainTab;
            SpellGroup= group;
            SpellSlot= spellSlot;
            CastSlot= castSlot;
            IsOn = new Oasys.Common.Menu.ItemComponents.Switch("Enabled", enabled);
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            Range = range;
            TargetSelector = targetSelector;
            group.AddItem(IsOn);
            group.AddItem(MinMana);
            CoreEvents.OnCoreMainTick += MainTick;
            
        }

        internal Task MainTick()
        {
            if (MainInput && IsOn.IsOn)
            {
                CoreEvents.OnCoreMainInputAsync += MainInputFunction;
            }
            if (Harass && IsOn.IsOn)
            {
                CoreEvents.OnCoreHarassInputAsync += MainInputFunction;
            }
            if (Push && IsOn.IsOn)
            {
                CoreEvents.OnCoreLaneclearInputAsync += PushInputFunction;
            }
            if (MainInput && IsOn.IsOn)
            {
                CoreEvents.OnCoreLasthitInputAsync += LastHitInputFunction;
            }
            return Task.CompletedTask;
        }

        private Task LastHitInputFunction()
        {
            ObjectTypeFlag[] flags = new ObjectTypeFlag[] { ObjectTypeFlag.AIMinionClient };
            foreach (GameObjectBase enemy in UnitManager.GetEnemies(flags))
            {
                if (enemy.IsAlive && enemy.IsTargetable && enemy.IsValidTarget() && enemy.Distance < Range)
                {
                    if ((enemy.Health - EffectCalculator.CalculateDamage(enemy)) < 0 && Env.Me().Mana > MinMana.Value && this.SpellIsReady())
                    {
                        SpellCastProvider.CastSpell(CastSlot, enemy.Position, castTime);
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task PushInputFunction()
        {
            ObjectTypeFlag[] flags = new ObjectTypeFlag[] { ObjectTypeFlag.AIMinionClient };
            foreach (GameObjectBase enemy in UnitManager.GetEnemies(flags))
            {
                if (enemy.IsAlive && enemy.IsTargetable && enemy.IsValidTarget() && enemy.Distance < Range && this.SpellIsReady())
                {
                    if (Env.Me().Mana > MinMana.Value)
                    {
                        SpellCastProvider.CastSpell(CastSlot, enemy.Position, castTime);
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task MainInputFunction()
        {
            GameObjectBase? target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, (x => x.Distance < Range));
            if (target != null)
            {
                if (target.IsAlive && target.IsTargetable && target.IsValidTarget() && target.IsObject(ObjectTypeFlag.AIHeroClient) && this.SpellIsReady())
                {
                    SpellCastProvider.CastSpell(CastSlot, target.Position, castTime);
                }
            }
            return Task.CompletedTask;
        }
    }
}
