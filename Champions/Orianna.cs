

using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Menu;
using Oasys.SDK.Rendering;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Base;
using SyncWave.Combos.Orianna;
using SyncWave.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    internal class Orianna : SyncWave.Base.Champion
    {
        internal override List<Combo> Combos => new() { new SyncWave.Combos.Orianna.General(), new SyncWave.Combos.Orianna.Trade(), new SyncWave.Combos.Orianna.TradeWithE() };

        internal static Combo? currentCombo = null;

        internal static QCast Q = new Combos.Orianna.QCast();
        internal static WCast W = new Combos.Orianna.WCast();
        internal static ECast E = new Combos.Orianna.ECast();
        internal static RCast R = new Combos.Orianna.RCast();

        internal static GameObjectBase Ball { get; set; } = UnitManager.AllNativeObjects.FirstOrDefault(x => x.Name == "TheDoomBall" && x.IsAlive && x.Health >= 1);

        internal static bool IsBallOnMe()
        {
            var buffs = UnitManager.MyChampion.BuffManager.Buffs.deepCopy();
            var buff = buffs.FirstOrDefault(x => x.Name == "orianaghostself");
            return buff != null && buff.IsActive && buff.Stacks > 0;
        }

        internal static Hero? getBallHolder()
        {
            if (Ball == null)
            {
                List<Hero> clients = UnitManager.AllyChampions.deepCopy();
                foreach (Hero client in clients)
                {
                    List<BuffEntry> buffs = client.BuffManager.Buffs.deepCopy();
                    BuffEntry buff = buffs.FirstOrDefault(x => x.Name == "orianaghost" || x.Name == "orianaghostself");
                    if (client.IsAlive && buff != null && buff.IsActive && buff.Stacks > 0)
                        return client;
                }
            }
            return null;
        }

        internal static Vector3 GetBallPosition()
        { 
            if (Ball == null)
            {
                Hero? holder = getBallHolder();
                if (holder != null)
                {
                    return holder.Position;
                } else
                {
                    return Env.Me().Position;
                }

            } else
            {
                return Ball.Position;
            }
        }
        //(Ball == null)? Env.Me().Position : (IsBallOnMe())? Env.Me().Position : Ball.Position
        internal static Vector3 QPosition() => GetBallPosition();
        internal static Vector3 qPosition => QPosition();


        internal static GameObjectBase? currentTarget = null;

        internal static int[] QManaCost = new int[] { 0, 30, 35, 40, 45, 50 };
        internal static int[] WManaCost = new int[] { 0, 70, 80, 90, 100, 110 };
        internal static int[] EManaCost = new int[] { 0, 60, 60, 60, 60, 60 };
        internal static int[] RManaCost = new int[] { 0, 100, 100, 100};

        internal static int[] QDamage = new int[] { 0, 60, 90, 120, 150, 180 };
        internal static int[] WDamage = new int[] { 0, 60, 105, 150, 195, 240 };
        internal static int[] EDamage = new int[] { 0, 60, 90, 120, 150, 180 };
        internal static int[] RDamage = new int[] { 0, 200, 275, 350 };

        internal static int[] EShield = new int[] { 0, 55, 90, 125, 160, 195 };

        internal static float QDamageScaling = 0.5F;
        internal static float WDamageScaling = 0.7F;
        internal static float EDamageScaling = 0.3F;
        internal static float RDamageScaling = 0.8F;

        internal static float EShieldScaling = 0.45F;

        internal static int QRange = 815;
        internal static int QImpactRange = 175;
        internal static int QWidth = 160;
        internal static int QSpeed = 1400;

        internal static int WRadius = 225;

        internal static int LeashRange = 1290;
        internal static int ERange = 1120;
        internal static int EWidth = 160;
        internal static int ESpeed = 1850;

        internal static int RRadius = 415;

        internal static int TabIndex = -1;
        internal static int EnabledIndex = -1;
        internal static int GroupIndex = -1;
        internal static int DrawGroupIndex = -1;
        internal static int DrawQPosition = -1;
        internal static int DrawQPosMode = -1;
        internal static int DrawWRadius = -1;
        internal static int DrawRRadius = -1;
        internal static int DrawCurrentCombo = -1;
        internal static int DrawComboDamage = -1;
        internal static int OnlyDrawWhenNotOnMe = -1;
        internal static int GroupCombosIndex = -1;
        internal static int GeneralComboIndex = -1;
        internal static int TradeComboIndex = -1;
        internal static int TradeWithEComboIndex = -1;
        internal static int AbilityGroupIndex = -1;
        internal static int AbilityQIndex = -1;
        internal static int AbilityWIndex = -1;
        internal static int AbilityEIndex = -1;
        internal static int AbilityRIndex = -1;
        internal static int AbilityRMinEnemiesHit = -1;

        internal override void Init()
        {
            Logger.Log("Orianna Init Called!");
            InitMenu();
            CoreEvents.OnCoreMainInputAsync += OnCoreMainInput;
            CoreEvents.OnCoreMainTick += OnCoreMainTick;
            CoreEvents.OnCoreRender += OnCoreRender;
        }

        internal void InitMenu()
        {
            Tab OriannaTab = new Tab("SyncWave - Orianna");
            TabIndex = MenuManagerProvider.AddTab(OriannaTab);
            EnabledIndex = OriannaTab.AddItem(new Switch() { IsOn = true, Title = "Enabled" });
            Group AbilityGroup = new Group("Abilities");
            AbilityQIndex = AbilityGroup.AddItem(new Switch() { IsOn = true, Title = "Q Enabled" });
            AbilityWIndex = AbilityGroup.AddItem(new Switch() { IsOn = true, Title = "W Enabled" });
            AbilityEIndex = AbilityGroup.AddItem(new Switch() { IsOn = true, Title = "E Enabled" });
            AbilityRIndex = AbilityGroup.AddItem(new Switch() { IsOn = true, Title = "R Enabled" });
            AbilityRMinEnemiesHit = AbilityGroup.AddItem(new Counter() { MinValue = 1, MaxValue = 5, Title = "Enemies in R", Value = 1, ValueFrequency = 1 });
            Group drawGroup = new Group("Drawings");
            DrawQPosition = drawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw Q Position" });
            DrawQPosMode = drawGroup.AddItem(new ModeDisplay() { ModeNames = new List<string>() { "Range", "Name", "Mixed" }, Title = "Q Draw Mode", SelectedModeName = "Mixed" });
            DrawWRadius = drawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw W Radius" });
            DrawRRadius = drawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw R Radius" });
            OnlyDrawWhenNotOnMe = drawGroup.AddItem(new Switch() { IsOn = true, Title = "Only draw when Ball not on self" });
            DrawCurrentCombo = drawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw Current Combo" });
            DrawComboDamage = drawGroup.AddItem(new Switch() { IsOn = true, Title = "Draw Combo Damage" });
            Group ComboGroup = new Group("Combos");
            GeneralComboIndex = ComboGroup.AddItem(new Switch() { IsOn = true, Title = "Gerneral Combo" });
            TradeComboIndex = ComboGroup.AddItem(new Switch() { IsOn = true, Title = "Trade Combo" });
            TradeWithEComboIndex = ComboGroup.AddItem(new Switch() { IsOn = true, Title = "Trade+E Combo" });
            GroupCombosIndex = OriannaTab.AddGroup(ComboGroup);
            AbilityGroupIndex = OriannaTab.AddGroup(AbilityGroup);
            DrawGroupIndex = OriannaTab.AddGroup(drawGroup);
        }

        internal static bool isOn(int groupIndex, int switchIndex)
        {
            return MenuManager.GetTab(Champions.Orianna.TabIndex).GetGroup(groupIndex).GetItem<Switch>(switchIndex).IsOn;
        }

        internal static string GetMode(int groupIndex, int displayIndex)
        {
            return MenuManager.GetTab(Champions.Orianna.TabIndex).GetGroup(groupIndex).GetItem<ModeDisplay>(displayIndex).SelectedModeName;
        }

        internal static bool IsMode(int groupIndex, int displayIndex, string mode)
        {
            return GetMode(groupIndex, displayIndex) == mode;
        }

        private void OnCoreRender()
        {
            if (!MenuManager.GetTab(TabIndex).GetItem<Switch>(EnabledIndex).IsOn)
                return;
            bool draw = true;
            if (isOn(DrawGroupIndex, OnlyDrawWhenNotOnMe))
            {
                draw = IsBallOnMe() == false;
            }
            if (isOn(DrawGroupIndex, DrawQPosition) && draw)
            {
                if (QPosition().IsOnScreen())
                {
                    if (IsMode(DrawGroupIndex, DrawQPosMode, "Name"))
                    {
                        RenderFactory.DrawText("The Ball", QPosition().ToW2S(), new Color(Color.Black.ToColor3(), 0.8F), true);
                    }
                    else if (IsMode(DrawGroupIndex, DrawQPosMode, "Range"))
                    {
                        RenderFactory.DrawNativeCircle(QPosition(), 60, new Color(Color.Black.ToColor3(), 0.8F), 1);
                    }
                    else
                    {
                        RenderFactory.DrawText("The Ball", QPosition().ToW2S(), new Color(Color.Black.ToColor3(), 0.8F), true);
                        RenderFactory.DrawNativeCircle(QPosition(), 60, new Color(Color.Black.ToColor3(), 0.8F), 1);
                    }
                }
            }
            if (isOn(DrawGroupIndex, DrawWRadius) && draw && Champions.Orianna.W.SpellsReady())
            {
                RenderFactory.DrawNativeCircle(QPosition(), WRadius, new Color(Color.OrangeRed.ToColor3(), 0.8F), 1, false);
            }
            if (isOn(DrawGroupIndex, DrawRRadius) && draw && Champions.Orianna.R.SpellsReady())
            {
                RenderFactory.DrawNativeCircle(QPosition(), RRadius, new Color(Color.DodgerBlue.ToColor3(), 0.8F), 1, false);
            }

            if (isOn(DrawGroupIndex, DrawCurrentCombo))
            {
                if (currentCombo != null && Env.Me().Position.IsOnScreen())
                {
                    Vector3 pos = Env.Me().Position;
                    pos.Y += 30;
                    RenderFactory.DrawText($"Combo: {currentCombo.Name}", 14, pos.ToW2S(), Color.Black, true);
                }
            }
            if (isOn(DrawGroupIndex, DrawComboDamage)) {
                if (currentTarget != null && currentCombo != null)
                {
                    RenderFactory.DrawHPBarDamage(currentTarget, DamageCalculator.CalculateActualDamage(Env.Me(), currentTarget, 0, currentCombo.GetFullDamageRaw(), 0));
                }
            }
        }

        internal static void ListBuffs()
        {
            foreach (BuffEntry buff in Env.Me().BuffManager.ActiveBuffs)
            {
                Logger.Log($"Buff name: {buff.Name}");
                Logger.Log($"Buff stacks: {buff.Stacks}");
                Logger.Log($"Buff isActive: {buff.IsActive}");
                Logger.Log($"Buff count: {buff.BuffCountInt}");
                Logger.Log($"Buff alt: {buff.BuffCountAlt}");
            }
        }

        internal static void ListBallEntities()
        {
            foreach (GameObjectBase ball in UnitManager.AllNativeObjects.Where(x => x.Name.Contains("Ball") && x.IsAlive && x.Health >= 1))
            {
                Logger.Log($"Name: {ball.Name}");
                Logger.Log($"IsAlive: {ball.IsAlive}");
                Logger.Log($"Health: {ball.Health}");
            }
        }

        private Task OnCoreMainTick()
        {
            if (Ball == null || !Ball.IsAlive || Ball.Health < 1 || !IsBallOnMe())
            {
                Ball = UnitManager.AllNativeObjects.FirstOrDefault(x => x.Name == "TheDoomBall" && x.IsAlive && x.Health >= 1);
            } 
            return Task.CompletedTask;
        }

        internal Task OnCoreMainInput()
        {
            //Logger.Log("MainInput");
            try
            {
                if (MenuManager.GetTab(TabIndex).GetItem<Switch>(EnabledIndex).IsOn)
                {
                    Combo combo = new Common.Helper.Selectors.Orianna.ComboSelector().Select();
                    if (combo.EnoughMana())
                    {
                        currentCombo = combo;
                        combo.Run();
                    }
                    else
                    {
                        currentCombo = null;
                    }
                } else if (currentCombo != null)
                {
                    currentCombo = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Exception occured at MainInput. {ex.Message}");
            }
            return Task.CompletedTask;

        }
    }
}
