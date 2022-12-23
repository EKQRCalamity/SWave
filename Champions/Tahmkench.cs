using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Tools.Devices;
using Oasys.SDK;
using Oasys.SDK.InputProviders;
using Oasys.SDK.Menu;
using Oasys.SDK.Rendering;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Base;
using SyncWave.Combos.Tahmkench;
using SyncWave.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncWave.Champions
{
    internal class Tahmkench : SyncWave.Base.Module
    {
        internal List<Combo> Combos => new()
        {
            new Combos.Tahmkench.General()
        };

        internal static bool KeyPressed = false;

        internal static QCast Q = new Combos.Tahmkench.QCast();
        internal static WCast W = new Combos.Tahmkench.WCast();
        internal static ECast E = new Combos.Tahmkench.ECast();
        internal static RCast R = new Combos.Tahmkench.RCast();

        internal static GameObjectBase? currentTarget = null;
        internal static Combo? currentCombo = null;

        internal static float[] PassiveExtraDamage = new float[] { 8, 11.06F, 14.12F, 17.18F, 20.24F, 23.29F, 26.35F, 29.41F, 32.47F, 35.53F, 38.59F, 41.65F, 44.71F, 47.76F, 50.82F, 53.88F, 56.94F, 60F };

        internal static int[] QManaCost = new int[] { 0, 50, 46, 42, 38, 34 };
        internal static int[] QDamage = new int[] { 0, 80, 130, 180, 230, 280 };
        internal static int[] QHeal = new int[] { 0, 10, 15, 20, 25, 30 };
        internal static float[] QHealMissingHealth = new float[] { 0, 0.03F, 0.035F, 0.04F, 0.045F, 0.05F };
        internal static float QCastTime = 0.25F;
        internal static float QScaling = 0.9F;
        internal static int QSpeed = 2800;
        internal static int QRange = 900;
        internal static int QWidth = 140;

        internal static int[] WManaCost = new int[] { 0, 60, 75, 90, 105, 120 };
        internal static int[] WDamage = new int[] { 0, 100, 135, 170, 205, 240 };
        internal static int[] WRange = new int[] { 0, 1000, 1050, 1100, 1150, 1200 };
        internal static float WExtraDelay = 0.15F;
        internal static float WCastTime = 1.35F;
        internal static float WScaling = 1.25F;
        internal static int WWidth = 275;

        internal static float ECastTime = 0.25F;

        internal static int[] RDamage = new int[] { 0, 100, 250, 400 };
        internal static int[] RManaCost = new int[] { 0, 100, 100, 100 };
        internal static int RRange = 250;
        internal static float RCastTime = 0.25F;
        internal static float RMaxHealthDmgScaling = 0.15F;
        internal static float RAPScaling = 0.05F;

        internal static int TabIndex = -1;
        internal static int EnabledIndex = -1;
        internal static int AbilityGroupIndex = -1;
        internal static int AbilityQIndex = -1;
        internal static int AbilityWIndex = -1;
        internal static int AbilityWKeyIndex = -1;
        internal static int AbilityEIndex = -1;
        internal static int AbilityRIndex = -1;
        internal static int AbilityRCastMode = -1;
        internal static int AbilityRAllyHP = -1;
        internal static int DrawGroupIndex = -1;
        internal static int DrawQHealIndex = -1;
        internal static int DrawWRangeIndex = -1;
        internal static int DrawRCanKillIndex = -1;
        internal static int DrawRCanKillModeIndex = -1;
        internal static int DrawComboDamage = -1;

        internal override void Init()
        {
            Logger.Log("Tahm Kench Init called!");
            InitMenu();
            CoreEvents.OnCoreMainInputAsync += OnCoreMainInput;
            CoreEvents.OnCoreRender += OnCoreRender;
            KeyboardProvider.OnKeyPress += OnPress;

        }

        private void OnPress(System.Windows.Forms.Keys keyBeingPressed, Keyboard.KeyPressState pressState)
        {
            var keyPressed = GetItem<KeyBinding>(TabIndex, AbilityGroupIndex, AbilityWKeyIndex);
            if (pressState == Keyboard.KeyPressState.Down && keyBeingPressed == keyPressed.SelectedKey)
            {
                KeyPressed = true;
            } else if (pressState == Keyboard.KeyPressState.Up && keyBeingPressed == keyPressed.SelectedKey)
            {
                KeyPressed = false;
            }
            return;
        }

        internal void InitMenu()
        {
            Tab TahmTab = new Tab("SyncWave - Tahm Kench");
            EnabledIndex = TahmTab.AddItem(new Switch() { IsOn = true, Title = "Enabled" });
            TabIndex = MenuManagerProvider.AddTab(TahmTab);
            Group AbilityGroup = new Group("Abilities");
            AbilityQIndex = AbilityGroup.AddItem(new Switch() { IsOn = true, Title = "Q Enabled"});
            AbilityWIndex = AbilityGroup.AddItem(new Switch() { IsOn = true, Title = "W Enabled" });
            AbilityWKeyIndex = AbilityGroup.AddItem(new KeyBinding() { Title = "Use W key", SelectedKey = Keys.S });
            AbilityEIndex = AbilityGroup.AddItem(new Switch() { IsOn = true, Title = "E Enabled" });
            AbilityRIndex = AbilityGroup.AddItem(new Switch() { IsOn = true, Title = "R Enabled" });
            AbilityRCastMode = AbilityGroup.AddItem(new ModeDisplay() { Title = "R Cast Mode", SelectedModeName = "Mixed", ModeNames = new() { "Ally", "EnemyOnKill", "Enemy", "Mixed" } });
            AbilityRAllyHP = AbilityGroup.AddItem(new Counter() { MaxValue = 100, MinValue = 5, ValueFrequency = 5, Value = 15, Title = "Ally R HP%" });
            AbilityGroupIndex = TahmTab.AddGroup(AbilityGroup);
            Group DrawGroup = new Group("Drawings");
            DrawQHealIndex = DrawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw Q Heal" });
            DrawWRangeIndex = DrawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw W Range" });
            DrawRCanKillIndex = DrawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw R Can Kill" });
            DrawRCanKillModeIndex = DrawGroup.AddItem(new ModeDisplay() { SelectedModeName = "Text", Title = "R Draw Modes", ModeNames = new() { "Text", "Circle", "HPBar" } });
            DrawComboDamage = DrawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw Combo Damage" });
            DrawGroupIndex = TahmTab.AddGroup(DrawGroup);
        }

        internal static bool isOn(int groupIndex, int switchIndex)
        {
            return MenuManager.GetTab(TabIndex).GetGroup(groupIndex).GetItem<Switch>(switchIndex).IsOn;
        }

        internal static string GetMode(int groupIndex, int displayIndex)
        {
            return MenuManager.GetTab(TabIndex).GetGroup(groupIndex).GetItem<ModeDisplay>(displayIndex).SelectedModeName;
        }

        internal static T GetItem<T>(int tabIndex, int groupIndex, int itemIndex) where T : Oasys.Common.Menu.TabItem
        {
            return MenuManager.GetTab(tabIndex).GetGroup(groupIndex).GetItem<T>(itemIndex);
        }

        internal float GetPStack(GameObjectBase enemy)
        {
            List<BuffEntry> buffs = enemy.BuffManager.ActiveBuffs.deepCopy();
            float stacks = 0;
            foreach (BuffEntry buff in buffs)
            {
                if (buff.Name == "tahmkenchpdebuffcounter")
                    if (stacks <= buff.Stacks)
                        stacks = buff.Stacks;
            }
            return stacks;
        }

        internal void OnCoreRender()
        {
            if (!MenuManager.GetTab(TabIndex).GetItem<Switch>(EnabledIndex).IsOn)
                return;
                //Logger.Log($"DrawIndexes: {TabIndex}, {DrawGroupIndex}, {DrawWRangeIndex}, {DrawRCanKillIndex}, {DrawRCanKillModeIndex}, {DrawComboDamage}");
            if (MenuManagerProvider.GetTab(TabIndex).GetGroup(DrawGroupIndex).GetItem<Switch>(DrawQHealIndex).IsOn)
            {
                if (Q.SpellsReady())
                {
                    RenderFactory.DrawHPBarHeal(Env.Me(), Q.GetFullHealing(), new Color(Color.Blue.ToColor3()));
                }
            }
                
            if (MenuManagerProvider.GetTab(TabIndex).GetGroup(DrawGroupIndex).GetItem<Switch>(DrawWRangeIndex).IsOn)
            {
                RenderFactory.DrawNativeCircle(Env.Me().Position, W.MinRange, new Color(Color.DodgerBlue.ToColor3(), 0.4F), 2, false);
            }
            if (MenuManagerProvider.GetTab(TabIndex).GetGroup(DrawGroupIndex).GetItem<Switch>(DrawRCanKillIndex).IsOn && KeyPressed)
            {
                List<GameObjectBase> list = W.TargetSelector.GetTargetsInRange().deepCopy();
                foreach (GameObjectBase enemy in list)
                {
                    if (R.CanKill(enemy, W.MinRange) && enemy.Position.IsOnScreen() && enemy.IsAlive)
                    {
                        if (MenuManagerProvider.GetTab(TabIndex).GetGroup(DrawGroupIndex).GetItem<ModeDisplay>(DrawRCanKillModeIndex).SelectedModeName == "Text")
                        {
                            Vector2 pos = enemy.Position.ToW2S();
                            pos.Y += 40;
                            RenderFactory.DrawText("Can Kill", pos, new Color(Color.Red.ToColor3(), 0.85F));
                        }
                        else if (MenuManagerProvider.GetTab(TabIndex).GetGroup(DrawGroupIndex).GetItem<ModeDisplay>(DrawRCanKillModeIndex).SelectedModeName == "Circle")
                        {
                            RenderFactory.DrawNativeCircle(enemy.Position, 60, new Color(Color.Red.ToColor3(), 1F), 4, false);
                        }
                        else
                        {
                            RenderFactory.DrawHPBarDamage(enemy, R.GetDamage(enemy), new Color(Color.White.ToColor3(), 0.8F));
                        }
                    }
                }
            }
            if (MenuManagerProvider.GetTab(TabIndex).GetGroup(DrawGroupIndex).GetItem<Switch>(DrawComboDamage).IsOn)
            {
                if (currentCombo != null && currentTarget != null && Env.Me().Position.IsOnScreen())
                {
                    RenderFactory.DrawHPBarDamage(currentTarget, currentCombo.GetDamage(currentTarget));
                }
            }
        }

        internal Task OnCoreMainInput()
        {
            if (MenuManager.GetTab(TabIndex).GetItem<Switch>(EnabledIndex).IsOn)
            {
                Combo combo = new Common.Helper.Selectors.Tahmkench.ComboSelector().Select();
                if (combo.EnoughMana())
                {
                    currentCombo = combo;
                    combo.Run();
                }
                else
                {
                    currentCombo = null;
                }
            } else
            {
                currentCombo = null;
            }
            

            return Task.CompletedTask;
        }
    }
}
