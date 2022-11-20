using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Menu;
using Oasys.SDK.SpellCasting;
using SyncWave.Champions;
using SyncWave.Common.Extensions;


namespace SyncWave.Combos.Tahmkench
{
    internal class RCast : Base.Combo
    {
        internal override string Name => "RCast";
        internal override int MinMana => Champions.Tahmkench.RManaCost[Env.RLevel];
        internal override int MinRange => Champions.Tahmkench.RRange;
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);

        internal override float Damage => GetFullDamageRaw();
        internal override float GetFullDamageRaw()
        {
            return Champions.Tahmkench.RDamage[Env.RLevel] + (Env.Me().UnitStats.TotalAbilityPower * Scaling());
        }

        internal float Scaling()
        {
            float totalAP = Env.Me().UnitStats.TotalAbilityPower;
            float res = totalAP / 100;
            float scaling = 0.00F;
            for (int i = 0; i < res; i++)
            {
                scaling += 0.05F;
            }
            return scaling;
        }

        internal override float GetDamage(GameObjectBase target)
        {
            float damage = GetFullDamageRaw() + (target.MaxHealth * Champions.Tahmkench.RMaxHealthDmgScaling);
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
        }
        internal override bool CanKill(GameObjectBase target)
        {
            if (target.DistanceToPlayer() >= MinRange)
                return false;
            float actualDamage = GetDamage(target);
            return (target.Health - actualDamage) <= 0;
        }

        internal bool CanKill(GameObjectBase target, int range)
        {
            if (target.DistanceToPlayer() >= range)
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

        internal override bool Enabled()
        {
            if (MenuManager.GetTab(Champions.Tahmkench.TabIndex).GetGroup(Champions.Tahmkench.AbilityGroupIndex).GetItem<Switch>(Champions.Tahmkench.AbilityRIndex).IsOn)
                return true;
            return false;
        }
        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.R).IsSpellReady && Enabled() && EnoughMana())
                return true;
            return false;
        }

        internal override bool Run()
        {
            string castMode = Champions.Tahmkench.GetMode(Champions.Tahmkench.AbilityGroupIndex, Champions.Tahmkench.AbilityRCastMode);
            GameObjectBase? target = null;
            if (castMode == "Ally")
            {
                target = TargetSelector.GetLowestHealthPrioTarget(Common.Helper.Selectors.Modes.Ally);
            } else if (castMode == "Enemy")
            {
                target = TargetSelector.GetLowestHealthPrioTarget(Common.Helper.Selectors.Modes.Enemy);

            } else if (castMode == "EnemyOnKill")
            {
                GameObjectBase? t = TargetSelector.GetLowestHealthPrioTarget(Common.Helper.Selectors.Modes.Enemy);
                if (t != null && CanKill(t))
                    target = t;
            } else
            {
                GameObjectBase? ally = TargetSelector.GetLowestHealthPrioTarget(Common.Helper.Selectors.Modes.Ally); ;
                if (ally.HealthPercent <= MenuManager.GetTab(Champions.Tahmkench.TabIndex).GetGroup(Champions.Tahmkench.AbilityGroupIndex).GetItem<Counter>(Champions.Tahmkench.AbilityRAllyHP).Value) 
                {
                    target = ally;
                } else
                {
                    GameObjectBase? t = TargetSelector.GetLowestHealthPrioTarget(Common.Helper.Selectors.Modes.Enemy);
                    if (t != null && CanKill(t))
                        target = t;
                }
                
            }

            if (target != null)
            {
                return Run(target);
            }
            return false;
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;
            if (SpellsReady() && target.IsAlive && target.IsVisible && target.IsTargetable && Enabled())
            {
                if (target.DistanceToPlayer() <= MinRange)
                {
                    SpellCastProvider.CastSpell(CastSlot.R, target.Position, Champions.Tahmkench.RCastTime);
                    spellCasted = true;
                }
            }
            return spellCasted;
        }
    }
}
