using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Tools;
using SyncWave.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Misc
{
    internal static class TargetPing
    {
        internal static Group TargetPingGroup = new Group("Target Ping");
        internal static Switch IsEnabled = new Switch("Enabled", false);
        internal static Counter AddDelay = new Counter("Delay till next ping (in s)", 10, 0, 20000);
        internal static ModeDisplay PingType = new ModeDisplay() { Title = "Ping Type", ModeNames = new() { "Neutral","Retreat","Push","OnMyWay","AllIn", "Assist", "Hold", "EnemyMissing", "Bait", "EnemyNoVision", "Vision", "RequestVision" } };
        
        internal static float LastCastTime = 0;

        internal static PingSlot PingFromMode(string mode)
        {
            return mode.ToLower() switch
            {
                "retreat" => PingSlot.Retreat,
                "push" => PingSlot.Push,
                "onmyway" => PingSlot.On_My_Way,
                "allin" => PingSlot.All_In,
                "assist" => PingSlot.Assist,
                "hold" => PingSlot.Hold,
                "enemymissing" => PingSlot.Enemy_Missing,
                "bait" => PingSlot.Bait,
                "enemynovision" => PingSlot.Enemy_No_Vision,
                "vision" => PingSlot.Vision,
                "requestvision" => PingSlot.Request_Vision,
                _ => PingSlot.Neutral
            };
        }

        internal static void Init()
        {
            PingMenu.tab.AddGroup(TargetPingGroup);
            TargetPingGroup.AddItem(IsEnabled);
            TargetPingGroup.AddItem(AddDelay);
            TargetPingGroup.AddItem(PingType);
            CoreEvents.OnCoreMainTick += MainTick;
            LastCastTime = GameEngine.GameTime;
            Logger.Log("TargetPing setup");
        }

        internal static Task MainTick()
        {
            if (GameEngine.GameTime - LastCastTime > AddDelay.Value)
            {
                GameObjectBase target = TargetSelector.GetBestChampionTarget();
                if (IsEnabled.IsOn && target != null && target.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient))
                {
                    PingManager.PingTo(PingFromMode(PingType.SelectedModeName), target.Position.ToW2S()); 
                    LastCastTime = GameEngine.GameTime;
                }
            }
            return Task.CompletedTask;
        }
    }
}
