using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
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
    internal class QCast : Base.Combo
    {
        internal override string Name => "QCast";
        internal override int MinMana => Champions.Twitch.QManaCost[Env.QLevel];

        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);
        internal override int MinRange => 0;
        internal override float Damage => GetFullDamageRaw();

        internal float QTime()
        {
            List<BuffEntry> Buffs = UnitManager.MyChampion.BuffManager.ActiveBuffs.deepCopy();
            BuffEntry? qBuff = Buffs.FirstOrDefault(x => x.Name == "TwitchHideInShadows" && x.Stacks >= 1);
            float qTimeRemaining = -1;
            if (qBuff != null)
            {
                qTimeRemaining = qBuff.RemainingDurationMs / 1000;
            }
            return qTimeRemaining;
        }

        internal override float GetFullDamageRaw()
        {
            return 0F;
        }

        internal override float GetDamage(GameObjectBase target)
        {
            return 0F;
        }
        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Twitch.TabIndex).GetGroup(Champions.Twitch.AbilityGroupIndex).GetItem<Switch>(Champions.Twitch.AbilityQIndex).IsOn)
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
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthPrioTarget();
            if (Enemy != null)
            {
                return CanKill(Enemy);
            }
            return false;
        }

        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.Q).IsSpellReady && Enabled())
                return true;
            return false;
        }

        internal override bool Run()
        {
            if (SpellsReady() && Enabled())
            {
                SpellCastProvider.CastSpell(CastSlot.Q);
                return true;
            }
            return false;
        }

        internal override bool Run(GameObjectBase target)
        {
            return Run();
        }
    }
}