using Oasys.Common.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Misc
{
    internal static class PingMenu
    {
        internal static bool Initialized = false;

        internal static int TabIndex = -1;
        internal static Tab? tab = null;

        internal static int Init()
        {
            if (!Initialized || TabIndex == -1 || tab == null)
            {
                tab = new Tab("SyncWave - Pings");
                TabIndex = MenuManagerProvider.AddTab(tab);
                Initialized = true;
                return TabIndex;
            }
            else
            {
                return TabIndex;
            }
        }
    }
}
