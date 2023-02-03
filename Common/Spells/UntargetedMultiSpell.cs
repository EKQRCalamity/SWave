using Oasys.Common.Enums.GameEnums;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Spells
{
    internal sealed class UntargetedMultiSpell : SpellCastBase
    {
        internal float castTime;

        public UntargetedMultiSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, DamageCalculation effectCalculator, int range, float CastTime = 0,int minMana = 0, bool canKill = false, bool laneclear = false, bool lasthit = false)
        {
            castTime = CastTime;
            EffectCalculator = effectCalculator;
            MainTab = mainTab;
            SpellGroup= group;
            SpellSlot= spellSlot;
            CastSlot= castSlot;
            isOn = new Oasys.Common.Menu.ItemComponents.Switch("Enabled", enabled);
            IsOn = x => x.IsAlive;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            LaneclearIsOn = new Switch("Laneclear", false);
            LasthitIsOn = new Switch("Lasthit", false);
            CanKill = canKill;
            Range = range;
            TargetSelector = x => x.IsAlive && x.Distance < Range;
            group.AddItem(isOn);
            group.AddItem(MinMana);
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

        public UntargetedMultiSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, DamageCalculation effectCalculator, Func<GameObjectBase, bool> targetSelector, int range, float CastTime = 0, int minMana = 0, bool canKill = false, bool laneclear = false, bool lasthit = false)
        {
            castTime = CastTime;
            EffectCalculator = effectCalculator;
            MainTab = mainTab;
            SpellGroup = group;
            SpellSlot = spellSlot;
            CastSlot = castSlot;
            isOn = new Oasys.Common.Menu.ItemComponents.Switch("Enabled", enabled);
            IsOn = x => x.IsAlive;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            LaneclearIsOn = new Switch("Laneclear", false);
            LasthitIsOn = new Switch("Lasthit", false);
            CanKill = canKill;
            Range = range;
            TargetSelector = targetSelector;
            group.AddItem(isOn);
            group.AddItem(MinMana);
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

        public UntargetedMultiSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, DamageCalculation effectCalculator, Func<GameObjectBase, bool> targetSelector, int range, Func<GameObjectBase, bool> isOnFunc, float CastTime = 0, int minMana = 0, bool canKill = false, bool laneclear = false, bool lasthit = false)
        {
            castTime = CastTime;
            EffectCalculator = effectCalculator;
            MainTab = mainTab;
            SpellGroup = group;
            SpellSlot = spellSlot;
            CastSlot = castSlot;
            isOn = new Oasys.Common.Menu.ItemComponents.Switch("Enabled", enabled);
            IsOn = isOnFunc;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            LaneclearIsOn = new Switch("Laneclear", false);
            LasthitIsOn = new Switch("Lasthit", false);
            CanKill = canKill;
            Range = range;
            TargetSelector = targetSelector;
            group.AddItem(isOn);
            group.AddItem(MinMana);
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
            if (Push && LaneclearIsOn.IsOn && !LaneclearInitialized)
            {
                CoreEvents.OnCoreLaneclearInputAsync += LastHitInputFunction;
                LaneclearInitialized = true;
            }
            if (LastHit && LasthitIsOn.IsOn && !LasthitInitialized)
            {
                CoreEvents.OnCoreLasthitInputAsync += LastHitInputFunction;
                LasthitInitialized = true;
            }
            return Task.CompletedTask;
        }

        private Task LastHitInputFunction()
        {
            if (!LasthitIsOn.IsOn || !isOn.IsOn || !IsOn(Env.Me()))
                return Task.CompletedTask;
            foreach (GameObjectBase enemy in UnitManager.EnemyMinions)
            {
                if (enemy.IsAlive && enemy.IsTargetable && enemy.Distance < Range)
                {
                    if ((enemy.Health - EffectCalculator.CalculateDamage(enemy)) < 0 && Env.Me().Mana > MinMana.Value && this.SpellIsReady())
                    {
                        SpellCastProvider.CastSpell(CastSlot, castTime);
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task MainInputFunction()
        {
            foreach (GameObjectBase target in UnitManager.EnemyChampions.Where(TargetSelector)) 
            {
                if (target.Distance < Range) {
                    if (target != null && isOn.IsOn && IsOn(Env.Me()) && Env.Me().Mana > MinMana.Value && target.Distance < Range)
                    {
                        if (target.IsAlive && target.IsTargetable && target.IsValidTarget() && target.IsObject(ObjectTypeFlag.AIHeroClient) && this.SpellIsReady())
                        {
                            if ((CanKill) ? target.Health - EffectCalculator.CalculateDamage(target) < 0 : true)
                                SpellCastProvider.CastSpell(CastSlot, castTime);
                        }
                    }
                }
                
            }
            return Task.CompletedTask;
        }
    }
}
