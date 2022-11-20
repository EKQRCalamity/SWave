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

namespace SyncWave.Combos.Tahmkench
{
    internal class ECast : Base.Combo
    {
        internal override string Name => "ECast";
        internal override int MinMana => 0;
        internal override int MinRange => 0;
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);

        internal override float Damage => GetFullDamageRaw();
        internal override float GetFullDamageRaw()
        {
            return 0F;
        }
        internal override float GetDamage(GameObjectBase target)
        {
            return 0F;
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
            if (MenuManager.GetTab(Champions.Tahmkench.TabIndex).GetGroup(Champions.Tahmkench.AbilityGroupIndex).GetItem<Switch>(Champions.Tahmkench.AbilityEIndex).IsOn)
                return true;
            return false;
        }
        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.E).IsSpellReady && Enabled() && EnoughMana())
                return true;
            return false;
        }
        internal override bool Run()
        {
            return false;
        }

        internal override bool Run(GameObjectBase target)
        {
            return Run();
        }
    }
}
