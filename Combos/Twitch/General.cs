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
using Oasys.Common.Tools;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;

namespace SyncWave.Combos.Twitch
{
    internal class General : Base.Combo
    {
        internal override string Name => "General";
        internal override int MinMana => Champions.Orianna.QManaCost[Env.QLevel];

        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);
        internal override int MinRange => Champions.Orianna.QRange;

        internal override float Damage => GetFullDamageRaw();

        internal override float GetFullDamageRaw()
        {
            float Damage = 0;
            if (Env.Spells.GetSpellClass(SpellSlot.E).IsSpellReady)
            {
                Damage += Champions.Orianna.EDamage[Env.ELevel] + (Env.Me().UnitStats.TotalAbilityPower * Champions.Orianna.EDamageScaling);
            }
            return Damage;
        }
        internal override float GetDamage(GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
        }
        internal override bool Enabled()
        {
            return true;
        }

        internal override bool CanKill(GameObjectBase target)
        {
            if (target.DistanceToPlayer() >= MinRange)
                return false;
            return (Champions.Twitch.E.CanKill(target) || Champions.Twitch.W.CanKill(target));
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
            if (Env.Spells.GetSpellClass(SpellSlot.Q).IsSpellReady || Env.Spells.GetSpellClass(SpellSlot.W).IsSpellReady || Env.Spells.GetSpellClass(SpellSlot.E).IsSpellReady)
                return true;
            return false;
        }

        internal override bool Run()
        {
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthPrioTarget();
            if (Enemy != null)
            {
                Champions.Twitch.currentTarget = Enemy;
                return Run(Enemy);
            }
            Champions.Twitch.currentTarget = null;
            return false;
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;

            if (MenuManagerProvider.GetTab(Champions.Twitch.TabIndex).GetItem<Switch>(Champions.Twitch.EnabledIndex).IsOn)
            {
                bool qCasted = Champions.Twitch.Q.Run(target);
                Champions.Twitch.E.Run();
                bool wCasted = Champions.Twitch.W.Run(target);
                Champions.Twitch.E.Run();
                bool eCasted = Champions.Twitch.E.Run(target);

                if (qCasted || wCasted || eCasted)
                    spellCasted = true;
            }
            
            return spellCasted;

        }
    }
}