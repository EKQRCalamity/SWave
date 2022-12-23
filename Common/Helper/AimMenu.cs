using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Tools.Devices;
using Oasys.SDK.InputProviders;
using Oasys.SDK.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncWave.Common.Helper
{
    internal static class AimMenu
    {
        internal static bool Initialized = false;
        internal static Group? AimGroup = null;
        internal static Switch UseToggle = new Switch("Use ToggleKey", false);
        internal static KeyBinding ToggleKey = new KeyBinding("ToggleKey", System.Windows.Forms.Keys.O);
        internal static bool Toggled = false;
        internal static void Init(Tab tab)
        {
            if (!Initialized || AimGroup == null)
            {
                AimGroup = new Group("Cast Aim");
                tab.AddGroup(AimGroup);
                AimGroup.AddItem(UseToggle);
                AimGroup.AddItem(ToggleKey);
                Initialized = true;
                KeyboardProvider.OnKeyPress += ToggleKeyHandler;
            }
        }

        internal static void ToggleKeyHandler(Keys keyBeingPressed, Keyboard.KeyPressState pressState) 
        {
            if (keyBeingPressed == ToggleKey.SelectedKey && UseToggle.IsOn && !Toggled && pressState == Keyboard.KeyPressState.Up)
            {
                Toggled = true;
            }
            else if (keyBeingPressed == ToggleKey.SelectedKey && Toggled && pressState == Keyboard.KeyPressState.Up)
            {
                Toggled = false;
            }
        }
    }
}
