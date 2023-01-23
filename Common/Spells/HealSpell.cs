using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject;
using Oasys.SDK.SpellCasting;
using Oasys.SDK;
using SyncWave.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.Events;
using Oasys.Common.Extensions;

namespace SyncWave.Common.Spells
{
    internal sealed class HealSpell : SpellCastBase
    {
        internal float castTime;
        internal Counter HPCounter;
        public HealSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, DamageCalculation effectCalculator, Func<GameObjectBase, bool> targetSelector, int range, float CastTime = 0, int minMana = 0, int HPcounter = 25, bool harass = false, bool laneclear = false, bool lasthit = false)
        {
            castTime = CastTime;
            MainTab = mainTab;
            SpellGroup = group;
            SpellSlot = spellSlot;
            CastSlot = castSlot;
            isOn = new Oasys.Common.Menu.ItemComponents.Switch("Enabled", enabled);
            IsOn = x => x.IsAlive;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            HPCounter = new Counter("HP%", HPcounter, 0, 100);
            EffectCalculator = effectCalculator;
            Range = range;
            TargetSelector = targetSelector;
            group.AddItem(isOn);
            group.AddItem(MinMana);
            group.AddItem(HPCounter);
            if (harass)
            {
                group.AddItem(HarassIsOn);
                Harass = true;
            }
            if (lasthit)
            {
                group.AddItem(LasthitIsOn);
                LastHit = true;
            }
            if (laneclear)
            {
                group.AddItem(LaneclearIsOn);
                Push = true;
            }
            CoreEvents.OnCoreMainTick += MainTick;

        }

        public HealSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, DamageCalculation effectCalculator, Func<GameObjectBase, bool> targetSelector, int range, Func<GameObjectBase, bool> isOnFunc, float CastTime = 0, int minMana = 0, int HPcounter = 25, bool harass = false, bool laneclear = false, bool lasthit = false)
        {
            castTime = CastTime;
            MainTab = mainTab;
            SpellGroup = group;
            SpellSlot = spellSlot;
            CastSlot = castSlot;
            isOn = new Oasys.Common.Menu.ItemComponents.Switch("Enabled", enabled);
            IsOn = isOnFunc;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            HPCounter = new Counter("HP%", HPcounter, 0, 100);
            EffectCalculator = effectCalculator;
            Range = range;
            TargetSelector = targetSelector;
            group.AddItem(isOn);
            group.AddItem(MinMana);
            group.AddItem(HPCounter);
            if (harass)
            {
                group.AddItem(HarassIsOn);
                Harass = true;
            }
            if (lasthit)
            {
                group.AddItem(LasthitIsOn);
                LastHit = true;
            }
            if (laneclear)
            {
                group.AddItem(LaneclearIsOn);
                Push = true;
            }
            CoreEvents.OnCoreMainTick += MainTick;

        }

        internal Task MainTick()
        {
            if (MainInput && isOn.IsOn && !Initialized)
            {
                CoreEvents.OnCoreMainInputAsync += MainInputFunction;
                Initialized = true;
            }
            if (Harass && HarassIsOn.IsOn && !HarassInitialized)
            {
                CoreEvents.OnCoreHarassInputAsync += HarassInputFunction;
                HarassInitialized = true;
            }
            return Task.CompletedTask;
        }

        private Task HarassInputFunction()
        {
            GameObjectBase? target = UnitManager.AllyChampions.Where(x => !x.IsTargetDummy && !x.IsMe).FirstOrDefault(x => x.Distance < Range && Oasys.Common.Logic.TargetSelector.IsAttackable(x, false) && x.HealthPercent <= HPCounter.Value);
            if (target != null && HarassIsOn.IsOn && isOn.IsOn && IsOn(Env.Me()))
            {
                if (target.IsAlive && target.IsTargetable && target.IsValidTarget() && target.IsObject(ObjectTypeFlag.AIHeroClient) && this.SpellIsReady())
                {
                    if ((CanKill) ? target.Health - EffectCalculator.CalculateDamage(target) < 0 : true)
                        SpellCastProvider.CastSpell(CastSlot, target.Position, castTime);
                }
            }
            return Task.CompletedTask;
        }

        private Task MainInputFunction()
        {
            GameObjectBase? target = UnitManager.AllyChampions.Where(x => !x.IsTargetDummy && !x.IsMe).FirstOrDefault(x => x.Distance < Range && Oasys.Common.Logic.TargetSelector.IsAttackable(x, false) && x.HealthPercent <= HPCounter.Value);
            if (target != null && isOn.IsOn && IsOn(Env.Me()))
            {
                if (target.IsAlive && target.IsTargetable && target.IsValidTarget() && target.IsObject(ObjectTypeFlag.AIHeroClient) && this.SpellIsReady())
                {
                    if ((CanKill) ? target.Health - EffectCalculator.CalculateDamage(target) < 0 : true)
                        SpellCastProvider.CastSpell(CastSlot, target.Position, castTime);
                }
            }
            return Task.CompletedTask;
        }
    }
}
