using Oasys.Common;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Tools.Devices;
using Oasys.SDK;
using Oasys.SDK.InputProviders;
using Oasys.SDK.Rendering;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Common.Extensions;
using SyncWave.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncWave.Misc
{
    internal class MinionMarker
    {

        // Menu Section
        #region Menu
        internal static int TabIndex = -1;
        internal static int GroupIndex = -1;
        internal static int EnabledIndex = -1;
        internal static int EnableDisableIndex = -1;
        internal static int KeyIndex = -1;
        internal static int DrawLastHit = -1;
        internal static int DrawLastHitModes = -1;
        internal static int DrawWithGradientIndex = -1;
        internal static int DrawSoon = -1;
        internal static int DrawColorCanLastHit = -1;
        internal static int DrawColorCanLastHitSoon = -1;
        #endregion

        internal static bool KeyIsToggled = true;

        internal void Init()
        {
            SetupMenu();
            CoreEvents.OnCoreRender += OnCoreRender;
            KeyboardProvider.OnKeyPress += OnPressHandler;
        }

        private void OnPressHandler(Keys keyBeingPressed, Keyboard.KeyPressState pressState)
        {
            if (pressState == Keyboard.KeyPressState.Down && keyBeingPressed == Key && KeyIsToggled == false)
            {
                KeyIsToggled = true;
            }
            else if (pressState == Keyboard.KeyPressState.Down && keyBeingPressed == Key && KeyIsToggled == true)
            {
                KeyIsToggled = false;
            }
            return;
        }

        internal void SetupMenu()
        {
            TabIndex = Menu.Init();
            Group group = new Group("Minion Hit Marker");
            GroupIndex = Menu.tab.AddGroup(group);
            EnabledIndex = group.AddItem(new Switch() { IsOn = true, Title = "Enabled" });
            EnableDisableIndex = group.AddItem(new Switch { IsOn = true, Title = "Use Enable/Disable Key" });
            KeyIndex = group.AddItem(new KeyBinding() { Title = "Enable/Disable Key", SelectedKey = System.Windows.Forms.Keys.M });
            DrawLastHit = group.AddItem(new Switch() { IsOn = true, Title = "Draw Last Hittable" });
            DrawLastHitModes = group.AddItem(new ModeDisplay() { Title = "Last Hittable Modes", ModeNames = new() { "Dot", "Circle", "FilledCircle", "HealthBar" }, SelectedModeName = "Dot" });
            DrawWithGradientIndex = group.AddItem(new Switch() { IsOn = false, Title = "Draw with gradient" });
            DrawSoon = group.AddItem(new Switch() { IsOn = false, Title = "Draw Soon Last Hittable" });
            DrawColorCanLastHit = group.AddItem(new ModeDisplay() { Title = "Color Last Hittable", ModeNames = ColorConverter.GetColors(), SelectedModeName = "White" });
            DrawColorCanLastHitSoon = group.AddItem(new ModeDisplay() { Title = "Color Soon Last Hittable", ModeNames = ColorConverter.GetColors(), SelectedModeName = "Yellow" });

        }

        internal bool Enabled
        {
            get => ((!Menu.tab.GetGroup(GroupIndex).GetItem<Switch>(EnabledIndex).IsOn)? false : (!SwitchEnabled)? true : KeyIsToggled);
            set => Menu.tab.GetGroup(GroupIndex).GetItem<Switch>(EnabledIndex).IsOn = value;
        }

        internal bool SwitchEnabled => Menu.tab.GetGroup(GroupIndex).GetItem<Switch>(EnableDisableIndex).IsOn;

        internal Keys Key => Menu.tab.GetGroup(GroupIndex).GetItem<KeyBinding>(KeyIndex).SelectedKey;

        internal bool IsOn(int switchIndex)
        {
            return Menu.tab.GetGroup(GroupIndex).GetItem<Switch>(switchIndex).IsOn;
        }

        internal bool IsMode(int displayIndex, string modename)
        {
            return Menu.tab.GetGroup(GroupIndex).GetItem<ModeDisplay>(displayIndex).SelectedModeName == modename;
        }

        internal List<Minion> GetMinions()
        {
            return UnitManager.EnemyMinions.ToList().Where(x => x.IsAlive && x.Distance <= 2000 && x.W2S.IsValid() && x.IsVisible && x.IsValidTarget()).ToList();
        }

        internal string LastHitColorName => Menu.tab.GetGroup(GroupIndex).GetItem<ModeDisplay>(DrawColorCanLastHit).SelectedModeName;


        internal string LastHitSoonColorName => Menu.tab.GetGroup(GroupIndex).GetItem<ModeDisplay>(DrawColorCanLastHitSoon).SelectedModeName;

        internal bool DrawColorGradient => Menu.tab.GetGroup(GroupIndex).GetItem<Switch>(DrawWithGradientIndex).IsOn;

        internal Color DrawLastHitColor => ColorConverter.GetColor(LastHitColorName);

        internal Color DrawLastHitColorSoon => ColorConverter.GetColor(LastHitSoonColorName);

        internal Color GetGradientColor(Minion minion)
        {
            if (AAsLeft(minion) < 1)
                return ColorConverter.GetColorWithAlpha(DrawLastHitColor, 255);
            if (AAsLeft(minion) >= 1 && AAsLeft(minion) <= 2)
                return ColorHelper.ColorInterpolation(DrawLastHitColor, DrawLastHitColorSoon, AAsLeft(minion) - 1);
            return ColorConverter.GetColorWithAlpha(DrawLastHitColorSoon, 0);
        }

        internal Color GetColor(Minion minion)
        {
            if (DrawColorGradient)
                return GetGradientColor(minion);
            if (CanLastHit(minion))
                return ColorConverter.GetColorWithAlpha(DrawLastHitColor, 255);
            else if (AAsLeft(minion) <= 2)
                return ColorConverter.GetColorWithAlpha(DrawLastHitColorSoon, 255);
            return ColorConverter.GetColorWithAlpha(DrawLastHitColorSoon, 0);
        }

        internal void DrawDot(Minion minion)
        {
            Vector2 position = minion.HealthBarScreenPosition;
            position.X += 1;
            position.Y -= 2;
            RenderFactory.DrawText("⦾", position, GetColor(minion));
        }

        internal void DrawCircle(Minion minion, bool filled)
        {
            RenderFactory.DrawNativeCircle(minion.Position, 60, ColorConverter.GetColorWithAlpha(GetColor(minion), 100), 2, filled);
        }

        internal bool CanLastHit(GameObjectBase target) => TargetSelector.AttacksLeftToKill(target) <= 1;

        internal float AAsLeft(GameObjectBase target) => TargetSelector.AttacksLeftToKill(target);

        internal void OnCoreRender()
        {
            if (Enabled)
            {
                if (IsOn(DrawLastHit))
                {
                    List<Minion> minions = GetMinions().deepCopy();
                    if (IsMode(DrawLastHitModes, "Dot"))
                    {
                        foreach (Minion minion in minions)
                        {
                            if (AAsLeft(minion) <= (IsOn(DrawSoon)? 2 : 1))
                                DrawDot(minion);
                        }
                    }
                    else if (IsMode(DrawLastHitModes, "Circle") || IsMode(DrawLastHitModes, "FilledCircle"))
                    {
                        foreach (Minion minion in minions)
                        {
                            if (AAsLeft(minion) <= (IsOn(DrawSoon) ? 2 : 1))
                                DrawCircle(minion, IsMode(DrawLastHitModes, "FilledCircle"));
                        }
                    } else
                    {
                        foreach (Minion minion in minions)
                        {
                            if (AAsLeft(minion) <= (IsOn(DrawSoon) ? 2 : 1))
                                Common.Helper.Drawings.DrawHPBar(minion, GetColor(minion), 2);
                        }
                    }
                }
            }
        }

    }
}