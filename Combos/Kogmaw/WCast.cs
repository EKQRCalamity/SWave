using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using Oasys.Common.Settings;
using Oasys.Common.GameObject;

namespace SyncWave.Combos.Kogmaw
{
    internal class WCast : Base.Combo
    {
        internal override string Name => "WCast";
        internal override int MinMana => Champions.Kogmaw.WManaCost[Env.WLevel];
        internal override int MinRange => (int)Env.Me().UnitStats.AttackRange + Champions.Kogmaw.WExtraRange[Env.WLevel];
        internal override float Damage => GetFullDamageRaw();
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);
        internal override float GetFullDamageRaw()
        {
            return 0;
        }
        internal override float GetDamage(GameObjectBase target)
        {
            return 0;
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
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthTarget();
            if (Enemy != null)
            {
                return CanKill(Enemy);
            }
            return false;
        }

        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Kogmaw.TabIndex).GetGroup(Champions.Kogmaw.AbilityGroupIndex).GetItem<Switch>(Champions.Kogmaw.AbilityWIndex).IsOn)
                return true;
            return false;
        }


        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.W).IsSpellReady && Enabled() && EnoughMana())
                return true;
            return false;
        }

        internal override bool Run()
        {
            if (SpellsReady() && Enabled())
            {
                SpellCastProvider.CastSpell(CastSlot.W);
                return true;
            }
            return false;
        }


        internal override bool Run(GameObjectBase target)
        {
            if (target.IsVisible && target.IsTargetable && target.IsAlive)
                return Run();
            return false;
        }
    }
}
