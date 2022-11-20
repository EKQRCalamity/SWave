using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Combos.Kogmaw
{
    internal class RKillSteal : Base.Combo
    {
        internal override string Name => "R KillSteal";
        internal override int MinRange => Champions.Kogmaw.RRange[Env.RLevel];
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);
        internal override int MinMana => Champions.Kogmaw.R.MinMana;
        internal override float Damage => Champions.Kogmaw.R.Damage;
        internal override float GetFullDamageRaw()
        {
            return Champions.Kogmaw.R.GetFullDamageRaw();
        }
        internal override float GetDamage(GameObjectBase target)
        {
            return Champions.Kogmaw.R.GetDamage(target);
        }
        internal override bool CanKill()
        {
            return Champions.Kogmaw.R.CanKill();
        }
        internal override bool CanKill(GameObjectBase target)
        {
            return Champions.Kogmaw.R.CanKill(target);
        }
        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Kogmaw.TabIndex).GetGroup(Champions.Kogmaw.ComboGroupIndex).GetItem<Switch>(Champions.Kogmaw.RKillSteal).IsOn)
                return true;
            return false;
        }

        internal override bool SpellsReady()
        {
            if (Champions.Kogmaw.R.SpellsReady() && Enabled())
                return true;
            return false;
        }

        internal bool CanKillIn(GameObjectBase target, int time)
        {
            if (target.DistanceToPlayer() >= MinRange)
                return false;
            return target.PredictHealth(time) - Champions.Kogmaw.R.GetDamage(target) <= 0;
        }

        internal override bool Run()
        {
            List<GameObjectBase> enemies = TargetSelector.GetTargetsInRange();
            foreach (GameObjectBase enemy in enemies)
            {
                if (CanKill(enemy) || CanKillIn(enemy, 850) && enemy.IsAlive)
                {
                    Champions.Kogmaw.currentCombo = this;
                    Champions.Kogmaw.currentTarget = enemy;
                    return Run(enemy);
                }
            }
            Champions.Kogmaw.currentCombo = null;
            Champions.Kogmaw.currentTarget = null;
            return false;
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;
            if (Champions.Kogmaw.R.SpellsReady() && Enabled())
            {
                spellCasted = Champions.Kogmaw.R.Run(target);
            }
            return spellCasted;
        }
    }
}
