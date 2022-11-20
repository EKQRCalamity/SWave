using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Menu;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SyncWave.Common.Extensions;
using System;

namespace SyncWave.Combos.Tahmkench
{
    internal class General : Base.Combo
    {
        internal override string Name => "General";
        internal override int MinMana => 0;
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);

        internal override int MinRange => Champions.Tahmkench.W.MinRange;
        internal override float Damage => GetFullDamageRaw();
        internal override float GetFullDamageRaw()
        {
            float damage = 0;
            if (Champions.Tahmkench.Q.SpellsReady())
            {
                damage += Champions.Tahmkench.Q.GetFullDamageRaw();
            }
            if (Champions.Tahmkench.W.SpellsReady())
            {
                damage += Champions.Tahmkench.W.GetFullDamageRaw();
            }/*
            if (Env.Spells.GetSpellClass(SpellSlot.R).IsSpellReady && Champions.Tahmkench.R.SpellsReady())
            {
                damage += Champions.Tahmkench.R.GetFullDamageRaw();
            }*/
            return damage;
        }
        internal override float GetDamage(GameObjectBase target)
        {
            float damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
            return damage;
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
        internal override bool Enabled()
        {
            return true;
        }
        internal override bool SpellsReady()
        {
            if (Champions.Tahmkench.Q.SpellsReady() || Champions.Tahmkench.W.SpellsReady() || Champions.Tahmkench.E.SpellsReady() || Champions.Tahmkench.R.SpellsReady())
                return true;
            return false;
        }
        internal override bool Run()
        {
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthPrioTarget();
            if (Enemy != null)
            {
                Champions.Tahmkench.currentCombo = this;
                Champions.Tahmkench.currentTarget = Enemy;
                return Run(Enemy);
            }
            Champions.Tahmkench.currentCombo = null;
            Champions.Tahmkench.currentTarget = null;
            return false;
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;
            if (Champions.Tahmkench.Q.SpellsReady())
            {
                if (Champions.Tahmkench.Q.Run(target))
                    spellCasted = true;
            }
            if (Champions.Tahmkench.W.SpellsReady())
            {
                if (Champions.Tahmkench.W.Run(target))
                    spellCasted = true;
            }
            if (Champions.Tahmkench.E.SpellsReady())
            {
                if (Champions.Tahmkench.E.Run(target))
                    spellCasted = true;
            }
            if (Champions.Tahmkench.R.SpellsReady())
                if (Champions.Tahmkench.R.Run(target))
                    spellCasted = true;
            return spellCasted;
        }
    }
}
