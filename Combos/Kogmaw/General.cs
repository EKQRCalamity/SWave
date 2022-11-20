using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Logic;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Combos.Kogmaw
{
    internal class General : Base.Combo
    {
        internal override string Name => "General";
        internal override int MinMana => Champions.Kogmaw.WManaCost[Env.WLevel];
        internal override int MinRange => (Env.Me().UnitStats.AttackRange + Champions.Kogmaw.WExtraRange[Env.WLevel] < Champions.Kogmaw.ERange)? Champions.Kogmaw.ERange : (int)Env.Me().UnitStats.AttackRange + Champions.Kogmaw.WExtraRange[Env.WLevel];
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);
        internal override float Damage => GetFullDamageRaw();
        internal override float GetFullDamageRaw()
        {
            float damage = 0;
            if (Env.Spells.GetSpellClass(SpellSlot.Q).IsSpellReady)
            {
                damage += Champions.Kogmaw.Q.GetFullDamageRaw();
            }
            if (Env.Spells.GetSpellClass(SpellSlot.E).IsSpellReady)
            {
                damage += Champions.Kogmaw.E.GetFullDamageRaw();
            }
            return damage;
        }

        internal override float GetDamage(GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
        }
        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Kogmaw.TabIndex).GetGroup(Champions.Kogmaw.ComboGroupIndex).GetItem<Switch>(Champions.Kogmaw.GeneralIndex).IsOn)
                return true;
            return false;
        }
        internal override bool CanKill(GameObjectBase target)
        {
            if (target.DistanceToPlayer() >= MinRange)
                return false;
            float actualDamage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
            return (target.Health - actualDamage) <= 0;
        }
        internal override bool CanKill()
        {
            GameObjectBase? Enemy = Champions.Kogmaw.R.TargetSelector.GetLowestHealthTarget();
            if (Enemy != null)
            {
                return CanKill(Enemy);
            }
            return false;
        }

        internal override bool SpellsReady()
        {
            if (Champions.Kogmaw.Q.SpellsReady() || Champions.Kogmaw.W.SpellsReady() || Champions.Kogmaw.E.SpellsReady() && Enabled())
                return true;
            return false;
        }

        internal override bool Run()
        {
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthPrioTarget();
            if (Enemy != null)
            {
                Champions.Kogmaw.currentCombo = this;
                Champions.Kogmaw.currentTarget = Enemy;
                return Run(Enemy);
            }
            Champions.Kogmaw.currentCombo = null;
            Champions.Kogmaw.currentTarget = null;
            return false;
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;

            if (Champions.Kogmaw.E.Run(target))
                spellCasted = true;
            if (Champions.Kogmaw.Q.Run(target))
                spellCasted = true;
            if (Champions.Kogmaw.W.Run())
                spellCasted = true;
            if (Champions.Kogmaw.R.Run(target))
                spellCasted = true;
            return spellCasted;
        }
    }
}
