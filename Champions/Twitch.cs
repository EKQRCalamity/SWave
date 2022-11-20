using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Events;
using Oasys.SDK.Menu;
using Oasys.SDK.Rendering;
using Oasys.SDK.Tools;
using SharpDX;
using SharpDX.DirectInput;
using SyncWave.Base;
using SyncWave.Combos.Twitch;
using SyncWave.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    internal class Twitch : SyncWave.Base.Champion
    {
        internal override List<Combo> Combos => new()
        {
            new Combos.Twitch.General()
            
        };

        internal static GameObjectBase? currentTarget = null;
        
        internal static List<GameObjectBase> getEnemiesWithStacks()
        {
            List<GameObjectBase> enemies = new();
            foreach (GameObjectBase hero in UnitManager.EnemyChampions.deepCopy())
            {
                float stacks = Champions.Twitch.E.EStacks(hero);
                if (stacks >= 1)
                {
                    enemies.Add(hero);
                }
            }
            return enemies;
        }

        internal static QCast Q = new QCast();
        internal static WCast W = new WCast();
        internal static ECast E = new ECast();

        internal static int[] QManaCost = new int[] { 0, 40, 40, 40, 40, 40 };
        internal static int[] WManaCost = new int[] { 0, 70, 70, 70, 70, 70 };
        internal static int[] EManaCost = new int[] { 0, 50, 60, 70, 80, 90 };
        internal static int[] RManaCost = new int[] { 0, 100, 100, 100 };

        internal static int[] PDamage = new int[] { 1, 2, 3, 4, 5 };
        internal static int[] EDamage = new int[] { 0, 20, 30, 40, 50, 60 };
        internal static int[] EDamagePerLevel = new int[] { 0, 15, 20, 25, 30, 35};
        internal static int[] RDamage = new int[] { 0, 40, 55, 70 };

        internal static float EDamageScaling = 0.35F;

        internal static int WRange = 950;
        internal static int WSpeed = 1400;
        internal static float WCastTime = 0.25F;
        internal static int WRadius = 260;

        internal static int ERange = 1200;

        internal static float ECastTime = 0.25F;

        internal static int RRange = 1100;
        internal static int RWidth = 120;
        internal static int RSpeed = 4000;

        internal static int TabIndex = -1;
        internal static int EnabledIndex = -1;
        internal static int AbilityGroupIndex = -1;
        internal static int AbilityQIndex = -1;
        internal static int AbilityWIndex = -1;
        internal static int AbilityEIndex = -1;
        internal static int AbilityEModeIndex = -1;
        internal static int DrawGroupIndex = -1;
        internal static int QTimeIndex = -1;
        internal static int EDamageIndex = -1;
        internal static int EDamageDrawModeIndex = -1;

        internal override void Init()
        {
            Logger.Log("Twitch Init called!");
            InitMenu();
            CoreEvents.OnCoreMainInputAsync += OnCoreMainInput;
            CoreEvents.OnCoreRender += OnCoreRender;
        }

        internal void InitMenu()
        {
            Tab TwitchTab = new Tab("SyncWave - Twitch");
            EnabledIndex = TwitchTab.AddItem(new Switch() { IsOn = true, Title = "Enabled" });
            TabIndex = MenuManagerProvider.AddTab(TwitchTab);
            Group AbilityGroup = new Group("Abilities");
            AbilityGroupIndex = TwitchTab.AddGroup(AbilityGroup);
            AbilityQIndex = AbilityGroup.AddItem(new Switch() { IsOn = true, Title = "Q Enabled" });
            AbilityWIndex = AbilityGroup.AddItem(new Switch() { IsOn = true, Title = "W Enabled" });
            AbilityEIndex = AbilityGroup.AddItem(new Switch() { IsOn = true, Title = "E Enabled" });
            AbilityEModeIndex = AbilityGroup.AddItem(new ModeDisplay() { Title = "E Cast Mode", SelectedModeName = "Killable Enemy near", ModeNames = new() { "Killable Enemy near", "Target Only" } });
            Group DrawGroup = new Group("Drawings");
            DrawGroupIndex = TwitchTab.AddGroup(DrawGroup);
            QTimeIndex = DrawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw Q Time" });
            EDamageIndex = DrawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw E Damage" });
            EDamageDrawModeIndex = DrawGroup.AddItem(new ModeDisplay() { ModeNames = new List<string>() { "AllEnemies", "Target Only" }, SelectedModeName = "AllEnemies", Title = "E Damage Draw Mode" });

        }

        internal static bool isOn(int groupIndex, int switchIndex)
        {
            return MenuManager.GetTab(Champions.Twitch.TabIndex).GetGroup(groupIndex).GetItem<Switch>(switchIndex).IsOn;
        }

        internal static string GetMode(int groupIndex, int displayIndex)
        {
            return MenuManager.GetTab(Champions.Twitch.TabIndex).GetGroup(groupIndex).GetItem<ModeDisplay>(displayIndex).SelectedModeName;
        }

        internal static bool IsMode(int groupIndex, int displayIndex, string mode)
        {
            return GetMode(groupIndex, displayIndex) == mode;
        }

        private void OnCoreRender()
        {
            if (isOn(DrawGroupIndex, EDamageIndex))
            {
                if (MenuManager.GetTab(TabIndex).GetGroup(DrawGroupIndex).GetItem<ModeDisplay>(EDamageDrawModeIndex).SelectedModeName == "Target Only")
                {
                    if (currentTarget != null)
                    {
                        RenderFactory.DrawHPBarDamage(currentTarget, Champions.Twitch.E.GetDamage(currentTarget));
                    }
                }
                else
                {
                    List<GameObjectBase> heros = getEnemiesWithStacks();
                    foreach (GameObjectBase hero in heros)
                    {
                        RenderFactory.DrawHPBarDamage(hero, Champions.Twitch.E.GetDamage(hero));
                    }
                }
            }
            if (isOn(DrawGroupIndex, QTimeIndex))
            {
                float qTime = Champions.Twitch.Q.QTime();
                if (qTime != -1)
                {
                    Vector2 pos = Env.Me().Position.ToW2S();
                    pos.Y -= 20;
                    RenderFactory.DrawText($"{qTime.ToString("n2")}s", pos, new Color(Color.Black.ToColor3(), 0.8F), true);
                }
            }
        }

        private Task OnCoreMainInput()
        {
            try
            {
                Combo combo = new Common.Helper.Selectors.Twitch.ComboSelector().Select();
                if (combo.EnoughMana())
                {
                    combo.Run();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Exception occured OnMainInput {ex.Message}");
            }
            
            return Task.CompletedTask;
        }
    }

}
