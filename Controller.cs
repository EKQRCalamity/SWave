using Oasys.SDK;
using Oasys;
using Oasys.SDK.Events;
using SyncWave.Common.Enums;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.Tools;
using System.Threading.Tasks;
using SyncWave.Misc;
using SyncWave.Common.Helper;

namespace SyncWave
{
    public class Controller
    {
        internal static int Tick = 0;
        [OasysModuleEntryPoint]

        public static void Entry()
        {
            GameEvents.OnGameLoadComplete += Init;
        } 

        private static Task Init()
        {
            PingManager.GetPositionOnPingWheel(PingSlot.Neutral);
            CoreEvents.OnCoreMainTick += TickFunc;
            
            Logger.Log($"{Env.Me().ModelName}");
            if (Env.SupportedChamps.Contains(Env.Me().ModelName))
            {
                Base.Module champion = Base.Module.GetFromName(Env.Me().ModelName);
                Logger.Log(champion.GetType() + " loaded!");
                Logger.Log("Setting up TargetSelector Menu...");
                SyncWave.Common.Helper.Selectors.TargetSelector.MenuInit();
                //Logger.Log($"Target Selector should be set up!");
                if (Env.ModuleVersion == V.InTesting)
                    Logger.Log($"{Env.Me().ModelName}");
                champion.Init();
            }
            Logger.Log("Setting up Pings");
            PingMenu.Init();
            TargetPing.Init();
            WardPing.Init();
            if (GameEngine.InGameInfo.GameType == Oasys.Common.Enums.GameEnums.GameTypes.Classic || GameEngine.InGameInfo.GameType == Oasys.Common.Enums.GameEnums.GameTypes.PracticeTool)
            {
                IngeniousWards
                    .Init();
            }
            OrbwalkFlash.Init();
            //SpellBuffer.Init();
            Logger.Log($"Setting up Misc");
            new AutoUser().Init();
            new MinionMarker().Init();
            new MoreDrawings().Init();
            new PerfectSmite().Init();
            RangeDrawer.Init();
            if (Env.ModuleVersion == V.InTesting)
            {
                TestDamage.Init();
                //PingTest.Init();
            }
            return Task.CompletedTask;
        }

        private static Task TickFunc()
        {
            Tick += 10;
            return Task.CompletedTask;
        }
    }
}