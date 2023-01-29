using Oasys.Common.Enums.GameEnums;
using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using SyncWave.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Spells
{
    internal class DashSpell : SpellCastBase
    {
        internal float castTime;
        internal ModeDisplay DashMode;

        internal bool EnemyInRange()
        {
            bool inRange = false;
            foreach (AIHeroClient client in UnitManager.EnemyChampions)
            {
                if (client.Distance < Range)
                {
                    inRange = true;
                    break;
                }
            }
            return inRange;
        }

        public DashSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, bool afterAA, float _castTime = 0, int minMana = 0, int range = 0)
        {
            castTime = _castTime;
            MainTab = mainTab;
            SpellGroup = group;
            SpellSlot = spellSlot;
            CastSlot = castSlot;
            isOn = new Oasys.Common.Menu.ItemComponents.Switch("Enabled", enabled);
            IsOn = x => x.IsAlive;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            DashMode = new ModeDisplay() { Title = "Mode", SelectedModeName = "ToMouse" };
            Range = (range == 0)? (int)Env.Me().TrueAttackRange : range;
            group.AddItem(isOn);
            group.AddItem(MinMana);
            group.AddItem(DashMode);
            CoreEvents.OnCoreMainInputAsync += MainInputFunction;
        }
        private Task MainInputFunction()
        {
            if (EnemyInRange() && isOn.IsOn && IsOn(Env.Me()) && SpellIsReady())
            {
                SpellCastProvider.CastSpell(CastSlot, castTime);
            }
            return Task.FromResult(0);
        }
    }
}
