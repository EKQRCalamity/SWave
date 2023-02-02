using Oasys.Common.Enums.GameEnums;
using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.SpellCasting;
using SyncWave.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncWave.Common.Spells
{
    internal class SelfcontainingMidPointCircleSpell : SpellCastBase
    {
        internal float castTime;

        public SelfcontainingMidPointCircleSpell(Tab mainTab, Group spellGroup, CastSlot castSlot, SpellSlot spellSlot, bool enabled, DamageCalculation effectCalculator, int range, float CastTime = 0, int minMana = 0, bool canKill = false, bool harass = false)
        {
            castTime = CastTime;
            MainTab = mainTab;
            SpellGroup = spellGroup;
            CastSlot = castSlot;
            SpellSlot = spellSlot;
            isOn = new Oasys.Common.Menu.ItemComponents.Switch("Enabled", enabled);
            IsOn = x => x.IsAlive;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            HarassIsOn = new Switch("Harass", false);
            LaneclearIsOn = new Switch("Laneclear", false);
            LasthitIsOn = new Switch("Lasthit", false);
            CanKill = canKill;
            Range = range;
            TargetSelector = x => x.IsAlive && x.Distance < Range;
            EffectCalculator = effectCalculator;
            spellGroup.AddItem(isOn);
            spellGroup.AddItem(MinMana);
            if (harass)
            {
                spellGroup.AddItem(HarassIsOn);
                Harass = true;
            }
            CoreEvents.OnCoreMainTick += MainTick;
        }

        public SelfcontainingMidPointCircleSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, DamageCalculation effectCalculator, Func<GameObjectBase, bool> targetSelector, int range, float CastTime = 0, int minMana = 0, bool canKill = false, bool harass = false)
        {
            castTime = CastTime;
            MainTab = mainTab;
            SpellGroup = group;
            SpellSlot = spellSlot;
            CastSlot = castSlot;
            isOn = new Oasys.Common.Menu.ItemComponents.Switch("Enabled", enabled);
            IsOn = x => x.IsAlive;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            HarassIsOn = new Switch("Harass", false);
            LaneclearIsOn = new Switch("Laneclear", false);
            LasthitIsOn = new Switch("Lasthit", false);
            CanKill = canKill;
            Range = range;
            TargetSelector = targetSelector;
            EffectCalculator = effectCalculator;
            group.AddItem(isOn);
            group.AddItem(MinMana);
            if (harass)
            {
                group.AddItem(HarassIsOn);
                Harass = true;
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
            GameObjectBase? target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, TargetSelector);
            if (target != null && isOn.IsOn && IsOn(Env.Me()) && target.Distance < Range && Env.Me().Mana > MinMana.Value)
            {
                if ((CanKill) ? target.Health - EffectCalculator.CalculateDamage(target) < 0 : true)
                {
                    SpellCastProvider.CastSpell(CastSlot);
                }
            }
            return Task.CompletedTask;
        }

        private Task MainInputFunction()
        {
            GameObjectBase? target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, TargetSelector);
            if (target != null && isOn.IsOn && IsOn(Env.Me()) && target.Distance < Range && Env.Me().Mana > MinMana.Value)
            {
                if ((CanKill)? target.Health - EffectCalculator.CalculateDamage(target) < 0 : true)
                {
                    SpellCastProvider.CastSpell(CastSlot);
                }
            }
            return Task.CompletedTask;
        }
    }
}
