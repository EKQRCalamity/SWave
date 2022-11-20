using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Settings;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using SharpDX;

namespace SyncWave.Combos.Orianna
{
    internal class Trade : Base.Combo
    {
        internal override string Name => "Trade";

        internal override int MinMana => Champions.Orianna.QManaCost[Env.QLevel] + Champions.Orianna.WManaCost[Env.WLevel];

        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);

        internal override int MinRange => Champions.Orianna.QRange - 10;

        internal override float Damage => GetFullDamageRaw();

        internal override float GetFullDamageRaw()
        {
            float Damage = 0;
            if (Env.Spells.GetSpellClass(SpellSlot.Q).IsSpellReady)
            {
                Damage = Champions.Orianna.Q.GetFullDamageRaw();
            }
            if (Env.Spells.GetSpellClass(SpellSlot.W).IsSpellReady)
            {
                Damage += Champions.Orianna.W.GetFullDamageRaw();
            }
            return Damage;
        }
        internal override float GetDamage(GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
        }
        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Orianna.TabIndex).GetGroup(Champions.Orianna.GroupCombosIndex).GetItem<Switch>(Champions.Orianna.TradeComboIndex).IsOn)
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
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthTarget();
            if (Enemy != null)
            {
                return CanKill(Enemy);
            }
            return false;
        }

        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.Q).IsSpellReady && Env.Spells.GetSpellClass(SpellSlot.W).IsSpellReady)
                return true;
            return false;
        }

        internal override bool Run()
        {
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthPrioTarget();
            if (Enemy != null)
            {
                Champions.Orianna.currentCombo = this;
                Champions.Orianna.currentTarget = Enemy;
                return Run(Enemy);
            }
            Champions.Orianna.currentTarget = null;
            Champions.Orianna.currentCombo = null;
            return false;
        }


        internal override bool Run(GameObjectBase target)
        {
            if (Champions.Orianna.Q.Run())
            {
                Champions.Orianna.W.Run();
                return true;
            }
            return false;
        }
    }
}
