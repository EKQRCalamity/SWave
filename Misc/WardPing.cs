using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Tools.Devices;
using Oasys.SDK;
using Oasys.SDK.InputProviders;
using Oasys.SDK.Rendering;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Common.Enums;
using SyncWave.Common.Extensions;
using SyncWave.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncWave.Misc
{
    internal static class WardPing
    {
        internal static Group group = new Group("Ward Ping");
        internal static Switch Enabled = new Switch("Enabled", true);
        internal static Switch EnableMinimap = new Switch("Cast on minimap", true);
        internal static InfoDisplay Info = new InfoDisplay() { Information = "Turned off = Ping only on screen." };
        internal static Switch NotInFights = new Switch("Don't cast in fight.", true);
        internal static Switch UseToggleButton = new Switch("Use Toggle Button", false);
        internal static KeyBinding ToggleButton = new KeyBinding() { Title = "Toggle Button", SelectedKey = Keys.N };
        internal static InfoDisplay FightInfo = new() { Information = "Checks for Target in AA range and CastingSpell" };
        internal static Counter MinDist = new Counter("Min Distance for Ping", 0, 0, 10000) { ValueFrequency = 10 };
        internal static Counter MaxDist = new Counter("Max Distance for Ping", 4000, 0, 10000) { ValueFrequency = 10 };
        internal static Counter PingIntervalDelay = new Counter("Delay between Pings(s)", 10, 0, 10000);
        internal static FloatCounter PingDelay = new FloatCounter() { Title = "Ping delay(s)", Value = 0.5F, MaxValue = 2.5F, MinValue = 0.1F, ValueFrequency = 0.1F };
        internal static Counter WardCounter = new Counter("Wards pinged", 0, 0, 100000) {ValueFrequency = 0 };
        internal static Counter MoneyCounter = new Counter("Money Gained Estimate", 0, 0, 500000) { ValueFrequency = 0 };

        internal static float LastCall = 0;
        internal static bool KeyIsToggled = false;

        private static void OnPress(System.Windows.Forms.Keys keyBeingPressed, Oasys.Common.Tools.Devices.Keyboard.KeyPressState pressState)
        {
            if (pressState == Oasys.Common.Tools.Devices.Keyboard.KeyPressState.Down && keyBeingPressed == ToggleButton.SelectedKey && KeyIsToggled == false)
            {
                KeyIsToggled = true;
            }
            else if (pressState == Oasys.Common.Tools.Devices.Keyboard.KeyPressState.Down && keyBeingPressed == ToggleButton.SelectedKey && KeyIsToggled == true)
            {
                KeyIsToggled = false;
            }
            return;
        }

        internal static void Init()
        {
            PingManager.Ping(PingSlot.Neutral);
            PingMenu.tab.AddGroup(group);
            group.AddItem(Enabled);
            group.AddItem(EnableMinimap);
            group.AddItem(Info);
            group.AddItem(NotInFights);
            group.AddItem(UseToggleButton);
            group.AddItem(ToggleButton);
            group.AddItem(MinDist);
            group.AddItem(MaxDist);
            group.AddItem(PingIntervalDelay);
            group.AddItem(PingDelay);
            group.AddItem(WardCounter);
            group.AddItem(MoneyCounter);
            WardCounter.Value = 0;
            MoneyCounter.Value = 0;
            CoreEvents.OnCoreRender += Draw;
            GameEvents.OnCreateObject += GameEvents_OnCreateObject;
            Keyboard.OnKeyPress += OnPress;
            GameEvents.OnGameMatchComplete += ResetWardCount;
        }

        private static Task ResetWardCount()
        {
            WardCounter.Value = 0;
            MoneyCounter.Value = 0;
            return Task.CompletedTask;
        }

        private static void Draw()
        {
            if (KeyIsToggled && UseToggleButton.IsOn && Enabled.IsOn)
            {
                Vector2 pos = Env.Me().Position.ToW2S();
                pos.Y -= 48;
                RenderFactory.DrawText("Ward Ping enabled", pos, Color.Black);
            }
        }

        internal static bool IsFighting()
        {
            GameObjectBase target = TargetSelector.GetBestChampionTarget();
            if (target == null || !Env.Me().IsCastingSpell) return false;
            return true;
        }

        private static Task GameEvents_OnCreateObject(List<Oasys.Common.GameObject.Clients.AIBaseClient> callbackObjectList, Oasys.Common.GameObject.Clients.AIBaseClient callbackObject, float callbackGameTime)
        {
            // SightWard JammerDevice
            if (callbackObject.Name.Contains("SightWard", StringComparison.OrdinalIgnoreCase) || callbackObject.Name.Contains("JammerDevice", StringComparison.OrdinalIgnoreCase))
            {
                if (UseToggleButton.IsOn && !KeyIsToggled)
                    return Task.CompletedTask;
                if (callbackObject.Distance > MaxDist.Value || callbackObject.Distance < MinDist.Value || !Enabled.IsOn)
                    return Task.CompletedTask;
                if (IsFighting() && NotInFights.IsOn)
                    return Task.CompletedTask;
                if (callbackObject.Team != Env.Me().Team && callbackObject.Team != Oasys.Common.Enums.GameEnums.TeamFlag.Unknown)
                {
                    if (callbackObject.IsVisible && callbackObject.IsAlive && (GameEngine.GameTime - LastCall) > PingIntervalDelay.Value)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds((double)PingDelay.Value));
                        if (callbackObject.Position.IsOnScreen())
                        {
                            PingManager.PingTo(PingSlot.Vision, callbackObject.Position.ToW2S());
                            LastCall = GameEngine.GameTime;
                            WardCounter.Value += 1;
                            MoneyCounter.Value += 5;
                        } else if (EnableMinimap.IsOn)
                        {
                            PingManager.PingTo(PingSlot.Vision, callbackObject.Position.ToWorldToMap());
                            LastCall = GameEngine.GameTime;
                            WardCounter.Value += 1;
                            MoneyCounter.Value += 5;
                        }
                        
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
