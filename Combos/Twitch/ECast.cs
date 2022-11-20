using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Settings;
using Oasys.Common.Tools;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Common.Extensions;

namespace SyncWave.Combos.Twitch
{
    internal class ECast : Base.Combo
    {
        internal override string Name => "ECast";

        internal override int MinMana => Champions.Twitch.EManaCost[Env.ELevel];


        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);
        internal override int MinRange => Champions.Twitch.ERange;

        internal override float Damage => GetFullDamageRaw(); 

        internal float EStacks(GameObjectBase enemy)
        {
            List<BuffEntry> buffs = enemy.BuffManager.ActiveBuffs.deepCopy();
            return buffs.FirstOrDefault(x => x.Name == "TwitchDeadlyVenom")?.Stacks ?? 0;
        }

        internal override float GetFullDamageRaw()
        {
            return 0F;
        }

        internal override float GetDamage(GameObjectBase target)
        {
            float damage = 0;
            float stacks = EStacks(target);
            if (stacks >= 1)
            {
                float baseDmg = Champions.Twitch.EDamage[Env.ELevel];
                float phyDmgPerStack = Champions.Twitch.EDamagePerLevel[Env.ELevel] + (Env.Me().UnitStats.BonusAttackDamage * Champions.Twitch.EDamageScaling);
                float magDmgPerStack = (Env.Me().UnitStats.TotalAbilityPower * Champions.Twitch.EDamageScaling);
                float phyDmg = baseDmg + (phyDmgPerStack * stacks);
                float magDmg = magDmgPerStack * stacks;
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, phyDmg, magDmg, 0);
            }
            return damage;
        }

        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Twitch.TabIndex).GetGroup(Champions.Twitch.AbilityGroupIndex).GetItem<Switch>(Champions.Twitch.AbilityEIndex).IsOn)
                return true;
            return false;
        }

        internal override bool CanKill(GameObjectBase target)
        {
            if (target.DistanceToPlayer() >= MinRange)
                return false;
            float actualDamage = GetDamage(target);
            return (target.Health - actualDamage) <= 0;
        }

        internal override bool CanKill()
        {
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthPrioTarget();
            if (Enemy != null)
            {
                return CanKill(Enemy);
            }
            return false;
        }

        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.E).IsSpellReady && Enabled())
                return true;
            return false;
        }

        internal override bool Run()
        {
            List<GameObjectBase> enemies = TargetSelector.GetTargetsInRange();
            foreach (GameObjectBase enemy in enemies)
            {
                if (CanKill(enemy))
                    return Run(enemy);
            }

            return false;
        }

        internal override bool Run(GameObjectBase target)
        {
            if (SpellsReady() && Enabled())
            {
                if (CanKill(target) && TargetSelector.TargetInRange(target) && target.IsAlive && target.IsTargetable)
                {
                    SpellCastProvider.CastSpell(CastSlot.E);
                    return true;
                }
            }
            return false;
        }
    }
}
