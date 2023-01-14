using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Tools.Devices;
using Oasys.SDK.InputProviders;
using Oasys.SDK.SpellCasting;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncWave.Misc
{
    internal static class OrbwalkFlash
    {
        internal static Group FlashGroup = new("Flash Safe");
        internal static Switch Enabled = new("Enabled", true);
        internal static InfoDisplay Info = new() { Information = "Will relocate flash when the Orbwalker is attacking the enemy." };
        internal static void Init()
        {
            InitMenu();
            KeyboardProvider.OnKeyPress += KeyPressHandler;
        }

        internal static void InitMenu()
        {
            Menu.Init();
            Menu.tab.AddGroup(FlashGroup);
            FlashGroup.AddItem(Enabled);
            FlashGroup.AddItem(Info);
        }

        internal static SpellClass? GetFlash()
        {
            SpellClass sum1 = Env.Me().GetSpellBook().GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Summoner1);
            SpellClass sum2 = Env.Me().GetSpellBook().GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Summoner2);
            SpellClass? Smite = (sum1.SpellData.SpellName.Contains("SummonerSmite")) ? sum1 : sum2.SpellData.SpellName.Contains("SummonerSmite") ? sum2 : null;
            return Smite;
        }

        internal static bool HasFlash()
        {
            if (GetFlash() != null)
                return true;
            return false;
        }

        internal static CastSlot GetCastSlot()
        {
            if (SummonerSpellsProvider.IHaveSpellOnSlot(SummonerSpellsEnum.Flash, SummonerSpellSlot.First))
                return CastSlot.Summoner1;
            else
                return CastSlot.Summoner2;
        }

        private static void KeyPressHandler(Keys keyBeingPressed, Keyboard.KeyPressState pressState)
        {
            if (Enabled.IsOn && HasFlash() && keyBeingPressed == Keys.D && pressState == Keyboard.KeyPressState.Down && MouseProvider.InUse)
            {
                if (GetFlash() != null)
                {
                    Vector2 pos = new(MouseProvider.RestorePosition.X, MouseProvider.RestorePosition.Y);
                    Oasys.SDK.SpellCasting.SpellCastProvider.CastSpell(GetCastSlot(), pos);
                }
            }
        }
    }
}
