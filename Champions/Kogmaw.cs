using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.Events;
using Oasys.SDK.Menu;
using Oasys.SDK.Rendering;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Base;
using SyncWave.Combos.Kogmaw;
using SyncWave.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    internal class Kogmaw : SyncWave.Base.Champion
    {
        internal override List<Combo> Combos => new()
        {
            new SyncWave.Combos.Kogmaw.General(),
            new SyncWave.Combos.Kogmaw.RKillSteal()
        };

        internal bool CanKillIn(GameObjectBase target, int time, int range, float damage)
        {
            if (target.DistanceToPlayer() >= range)
                return false;
            return target.PredictHealth(time) - damage <= 0;
        }

        internal static GameObjectBase? currentTarget = null;
        internal static Combo? currentCombo = null;

        internal static QCast Q = new QCast();
        internal static WCast W = new WCast();
        internal static ECast E = new ECast();
        internal static RCast R = new RCast();

        internal static int[] QManaCost = new int[] { 0, 40, 40, 40, 40, 40 };
        internal static int[] QDamage = new int[] { 0, 90, 140, 190, 240, 290 };
        internal static float QCastTime = 0.25F;
        internal static float QScaling = 0.7F;
        internal static int QSpeed = 1650;
        internal static int QRange = 1200;
        internal static int QWidth = 140;

        internal static int[] WManaCost = new int[] { 0, 40, 40, 40, 40, 40 };
        internal static int[] WExtraRange = new int[] { 0, 130, 150, 170, 190, 210 };

        internal static int[] EManaCost = new int[] { 0, 60, 70, 80, 90, 100 };
        internal static int[] EDamage = new int[] { 0, 75, 120, 165, 210, 255 };
        internal static float ECastTime = 0.25F;
        internal static float EScaling = 0.7F;
        internal static int ESpeed = 1400;
        internal static int ERange = 1360;
        internal static int EWidth = 240;

        internal static int[] RManaCost = new int[] { 0, 40, 80, 120, 160, 200, 240, 280, 320, 360, 400 };
        internal static int[] RRange = new int[] { 0, 1300, 1550, 1800 };
        internal static int[] RDamage = new int[] { 0, 100, 140, 180 };
        internal static int[] RDamageEnhanced = new int[] { 0, 200, 280, 360 };
        internal static float RADScaling = 0.65F;
        internal static float RADScalingEnhanced = 1.3F;
        internal static float RAPScaling = 0.35F;
        internal static float RAPScalingEnhanced = 0.7F;
        internal static float RCastTime = 0.25F;
        internal static int RRadius = 240;


        internal static int TabIndex = -1;
        internal static int EnabledIndex = -1;
        internal static int ComboGroupIndex = -1;
        internal static int GeneralIndex = -1;
        internal static int RKillSteal = -1;
        internal static int AbilityGroupIndex = -1;
        internal static int AbilityQIndex = -1;
        internal static int AbilityWIndex = -1;
        internal static int AbilityEIndex = -1;
        internal static int AbilityRIndex = -1;
        internal static int AbilityRCastModeIndex = -1;
        internal static int XHealthRIndex = -1;
        internal static int DrawGroupIndex = -1;
        internal static int WRangeIndex = -1;
        internal static int WRangeOnlyWhenReadyIndex = -1;
        internal static int RCanKillIndex = -1;
        internal static int RCanKillModeIndex = -1;
        internal static int CurrentComboIndex = -1;
        internal static int DamageIndex = -1;
        internal static int DrawDamageModeIndex = -1;
        internal override void Init()
        {
            Logger.Log("Kogmaw Init called!");
            InitMenu();
            CoreEvents.OnCoreMainInputAsync += OnCoreMainInput;
            CoreEvents.OnCoreRender += OnCoreRender;
        }

        internal void InitMenu()
        {
            Tab KogmawTab = new Tab("SyncWave - Kog'Maw");
            EnabledIndex = KogmawTab.AddItem(new Switch() { IsOn = true, Title = "Enabled" });
            TabIndex = MenuManagerProvider.AddTab(KogmawTab);
            Group ComboGroup = new Group("Combos");
            ComboGroupIndex = KogmawTab.AddGroup(ComboGroup);
            GeneralIndex = ComboGroup.AddItem(new Switch() { Title = "General Enabled", IsOn = true });
            RKillSteal = ComboGroup.AddItem(new Switch() { Title = "RKillSteal Enabled", IsOn = true });
            Group AbilityGroup = new Group("Abilities");
            AbilityGroupIndex = KogmawTab.AddGroup(AbilityGroup);
            AbilityQIndex = AbilityGroup.AddItem(new Switch() { Title = "Q Enabled", IsOn = true });
            AbilityWIndex = AbilityGroup.AddItem(new Switch() { Title = "W Enabled", IsOn = true });
            AbilityEIndex = AbilityGroup.AddItem(new Switch() { Title = "E Enabled", IsOn = true });
            AbilityRIndex = AbilityGroup.AddItem(new Switch() { Title = "R Enabled", IsOn = true });
            AbilityRCastModeIndex = AbilityGroup.AddItem(new ModeDisplay() { Title = "R Cast Mode", ModeNames = new() { "Under X Health", "Under X Health%", "Can Kill", "Mixed" }, SelectedModeName = "Mixed" });
            XHealthRIndex = AbilityGroup.AddItem(new Counter() { Title = "X Health/Health%", MaxValue = 500, MinValue = 5, Value = 25, ValueFrequency = 5 });
            Group DrawGroup = new Group("Drawings");
            DrawGroupIndex = KogmawTab.AddGroup(DrawGroup);
            WRangeIndex = DrawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw W Range" });
            WRangeOnlyWhenReadyIndex = DrawGroup.AddItem(new Switch() { IsOn = true, Title = "Only Draw W Range when ready" });
            RCanKillIndex = DrawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw R Can Kill" });
            RCanKillModeIndex = DrawGroup.AddItem(new ModeDisplay() { Title = "R Can Kill Draw Mode", SelectedModeName = "Circle", ModeNames = new() { "Circle", "Text", "Health Bar" } });
            CurrentComboIndex = DrawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw Current Combo" });
            DamageIndex = DrawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw Damage" });
            DrawDamageModeIndex = DrawGroup.AddItem(new ModeDisplay() { Title = "Damage Draw Mode", ModeNames = new() { "Combo", "R", "Mixed" }, SelectedModeName = "Mixed" });
        }

        internal static bool isOn(int groupIndex, int switchIndex)
        {
            return MenuManager.GetTab(TabIndex).GetGroup(groupIndex).GetItem<Switch>(switchIndex).IsOn;
        }

        internal static string GetMode(int groupIndex, int displayIndex)
        {
            return MenuManager.GetTab(TabIndex).GetGroup(groupIndex).GetItem<ModeDisplay>(displayIndex).SelectedModeName;
        }

        internal static bool IsMode(int groupIndex, int displayIndex, string mode)
        {
            return GetMode(groupIndex, displayIndex) == mode;
        }

        private void OnCoreRender()
        {
            if (isOn(DrawGroupIndex, WRangeIndex))
            {
                if (isOn(DrawGroupIndex, WRangeOnlyWhenReadyIndex) && Champions.Kogmaw.W.SpellsReady())
                {
                    RenderFactory.DrawNativeCircle(Env.Me().Position, W.MinRange, new Color(Color.DodgerBlue.ToColor3(), 0.6F), 1, false);
                } else if (!isOn(DrawGroupIndex, WRangeOnlyWhenReadyIndex))
                {
                    RenderFactory.DrawNativeCircle(Env.Me().Position, W.MinRange, new Color(Color.DodgerBlue.ToColor3(), 0.6F), 1, false);
                }
            }
            if (isOn(DrawGroupIndex, RCanKillIndex))
            {
                List<GameObjectBase> list = R.TargetSelector.GetVisibleTargetsInRange().deepCopy();
                foreach (GameObjectBase enemy in list)
                {
                    if (R.CanKill(enemy) && enemy.Position.IsOnScreen())
                    {
                        if (MenuManager.GetTab(TabIndex).GetGroup(DrawGroupIndex).GetItem<ModeDisplay>(RCanKillModeIndex).SelectedModeName == "Circle")
                        {
                            RenderFactory.DrawNativeCircle(enemy.Position, 60, new Color(Color.Red.ToColor3(), 55F), 3, false);
                        } else if (MenuManager.GetTab(TabIndex).GetGroup(DrawGroupIndex).GetItem<ModeDisplay>(RCanKillModeIndex).SelectedModeName == "Text")
                        {
                            Vector3 pos = enemy.Position;
                            //pos.Y += 50;
                            RenderFactory.DrawText("Can Kill", 14, pos.ToW2S(), new Color(Color.Red.ToColor3(), 55F), true);
                        } else if (isOn(DrawGroupIndex, DamageIndex) && MenuManager.GetTab(TabIndex).GetGroup(DrawGroupIndex).GetItem<ModeDisplay>(DrawDamageModeIndex).SelectedModeName == "Mixed")
                        {
                            RenderFactory.DrawHPBarDamage(enemy, R.GetDamage(enemy));
                        } else
                        {
                            RenderFactory.DrawHPBarDamage(enemy, R.GetDamage(enemy), new Color(Color.Red.ToColor3(), 0.7F));
                        }
                    }
                }
            }
            if (isOn(DrawGroupIndex, DamageIndex))
            {
                if (IsMode(DrawGroupIndex, DrawDamageModeIndex, "Combo"))
                {
                    if (currentCombo != null && currentTarget != null && Env.Me().Position.IsOnScreen())
                    {
                        RenderFactory.DrawHPBarDamage(currentTarget, currentCombo.GetDamage(currentTarget));
                    }
                } else if (IsMode(DrawGroupIndex, DrawDamageModeIndex, "R"))
                {
                    if (Env.Me().Position.IsOnScreen())
                    {
                        List<GameObjectBase> enemiesInRange = R.TargetSelector.GetVisibleTargetsInRange().deepCopy();
                        foreach (GameObjectBase enemy in enemiesInRange)
                        {
                            RenderFactory.DrawHPBarDamage(enemy, R.GetDamage(enemy));
                        }
                    }
                } else if (!isOn(DrawGroupIndex, DamageIndex))
                {
                    List<GameObjectBase> enemiesInRange = R.TargetSelector.GetVisibleTargetsInRange().deepCopy();
                    foreach (GameObjectBase enemy in enemiesInRange)
                    {
                        if (R.CanKill(enemy))
                            RenderFactory.DrawHPBarDamage(enemy, R.GetDamage(enemy));
                        else
                            RenderFactory.DrawHPBarDamage(enemy, currentCombo.GetDamage(enemy));
                    }
                }
            }
        }

        private Task OnCoreMainInput()
        {
            try {
                Combo combo = new Common.Helper.Selectors.Kogmaw.ComboSelector().Select();
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
