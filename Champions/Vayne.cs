using Oasys.Common.Menu;
using SyncWave.Base;
using SyncWave.Common.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    internal class Vayne : Module
    {
        internal Tab MainTab = new Tab("SyncWave - Vayne");
        internal Group QGroup = new Group("Q Settings");
        internal Group EGroup = new Group("E Settings");

        internal DashSpell qDash;
        internal PushSpell ePush;

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            MainTab.AddGroup(QGroup);
            MainTab.AddGroup(EGroup);
            qDash = new DashSpell(MainTab, QGroup, Oasys.SDK.SpellCasting.CastSlot.Q, Oasys.Common.Enums.GameEnums.SpellSlot.Q, true, true, 0, 30, (int)Env.Me().TrueAttackRange - 20);
            ePush = new PushSpell(MainTab, EGroup, Oasys.SDK.SpellCasting.CastSlot.E, Oasys.Common.Enums.GameEnums.SpellSlot.E, true, 550, 475, 0.25F, 90, true, true, 150, false, false);
        }
    }
}
