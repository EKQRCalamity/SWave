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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncWave.Misc
{
    internal class PerfectSmite
    {
        #region Menu
        internal static int TabIndex = -1;
        internal static int GroupIndex = -1;
        internal static int EnabledIndex = -1;
        internal static int ModeIndex = -1;
        internal static int ToggleButton = -1;
        internal static int DrawSmiteDamage = -1;
        internal static int DamageColorIndex = -1;
        internal static int AlphaIndex = -1;
        internal static int UseCampsIndex = -1;
        internal static int DrawCampsIndex = -1;
        internal static int UseBuffsIndex = -1;
        internal static int DrawBuffsIndex = -1;
        internal static int UseEpicIndex = -1;
        internal static int DrawEpicIndex = -1;
        #endregion

        internal static bool KeyIsToggled = false;

        internal static bool Testing = false;

        internal void Init()
        {
            SetupMenu();
            CoreEvents.OnCoreRender += OnCoreRender;
            CoreEvents.OnCoreMainTick += MainTick;
            CoreEvents.OnCoreMainInputAsync += MainInput;
            CoreEvents.OnCoreLaneclearInputAsync += MainInput;
            KeyboardProvider.OnKeyPress += OnPress;
        }

        private void SetupMenu()
        {
            TabIndex = Menu.Init();
            Group group = new Group("Perfect Smite");
            GroupIndex = Menu.tab.AddGroup(group);
            EnabledIndex = group.AddItem(new Switch() { IsOn = true, Title = "Enabled" });
            ModeIndex = group.AddItem(new ModeDisplay() { Title = "Cast Mode", ModeNames = new() { "ToggleKey", "OnTick", "InCombo" } });
            ToggleButton = group.AddItem(new KeyBinding() { Title = "Toggle Key", SelectedKey = System.Windows.Forms.Keys.S });
            DrawSmiteDamage = group.AddItem(new Switch() { Title = "Draw Smite Damage", IsOn = true });
            DamageColorIndex = group.AddItem(new ModeDisplay() { Title = "Damage Color", ModeNames = ColorConverter.GetColors(), SelectedModeName = "Red" });
            AlphaIndex = group.AddItem(new Counter() { Title = "Alpha", MinValue = 0, MaxValue = 255, Value = 200, ValueFrequency = 5 });
            UseCampsIndex = group.AddItem(new Switch() { Title = "Use on common Camps", IsOn = false });
            DrawCampsIndex = group.AddItem(new Switch() { Title = "Draw for common Camps", IsOn = false });
            UseBuffsIndex = group.AddItem(new Switch() { Title = "Use on Buffs", IsOn = true });
            DrawBuffsIndex = group.AddItem(new Switch() { Title = "Draw for Buffs", IsOn = false });
            UseEpicIndex = group.AddItem(new Switch() { Title = "Use on Epic Monsters", IsOn = true });
            DrawEpicIndex = group.AddItem(new Switch() { Title = "Draw for Epic Monsters", IsOn = true });
        }

        internal Group Group
        {
            get => Menu.tab.GetGroup(GroupIndex);
        }

        internal bool Enabled
        {
            get => Group.GetItem<Switch>(EnabledIndex).IsOn;
            set => Group.GetItem<Switch>(EnabledIndex).IsOn = value;
        }

        internal string Mode
        {
            get => Group.GetItem<ModeDisplay>(ModeIndex).SelectedModeName;
            set => Group.GetItem<ModeDisplay>(ModeIndex).SelectedModeName = value;
        }

        internal Keys Key
        {
            get => Group.GetItem<KeyBinding>(ToggleButton).SelectedKey;
            set => Group.GetItem<KeyBinding>(ToggleButton).SelectedKey = value;
        }

        internal bool DrawDamage
        {
            get => Group.GetItem<Switch>(DrawSmiteDamage).IsOn;
            set => Group.GetItem<Switch>(DrawSmiteDamage).IsOn = value;
        }

        internal bool Buffs
        {
            get => Group.GetItem<Switch>(UseBuffsIndex).IsOn;
            set => Group.GetItem<Switch>(UseBuffsIndex).IsOn = value;
        }

        internal bool Camps
        {
            get => Group.GetItem<Switch>(UseCampsIndex).IsOn;
            set => Group.GetItem<Switch>(UseCampsIndex).IsOn = value;
        }

        internal bool Epics
        {

            get => Group.GetItem<Switch>(UseEpicIndex).IsOn;
            set => Group.GetItem<Switch>(UseEpicIndex).IsOn = value;
        }

        internal bool DrawBuffs
        {
            get => Group.GetItem<Switch>(DrawBuffsIndex).IsOn;
            set => Group.GetItem<Switch>(DrawBuffsIndex).IsOn = value;
        }

        internal bool DrawCamps
        {
            get => Group.GetItem<Switch>(DrawCampsIndex).IsOn;
            set => Group.GetItem<Switch>(DrawCampsIndex).IsOn = value;
        }

        internal bool DrawEpics
        {

            get => Group.GetItem<Switch>(DrawEpicIndex).IsOn;
            set => Group.GetItem<Switch>(DrawEpicIndex).IsOn = value;
        }

        internal int Alpha
        {
            get => Group.GetItem<Counter>(AlphaIndex).Value;
            set => Group.GetItem<Counter>(AlphaIndex).Value = value;
        }

        internal string DamageColorName => Menu.tab.GetGroup(GroupIndex).GetItem<ModeDisplay>(DamageColorIndex).SelectedModeName;

        internal Color DamageColor => ColorConverter.GetColorWithAlpha(ColorConverter.GetColor(DamageColorName), Alpha);

        internal List<JungleMob> Monsters()
        {
            List<JungleMob> jungleMobs = UnitManager.AllyJungleMobs.Where(x => x.IsAlive && x.Distance <= 1000 && x.W2S.IsValid() && x.Health > 0 && x.IsValidTarget()).ToList();
            jungleMobs.AddRange(UnitManager.EnemyJungleMobs.Where(x => x.IsAlive && x.Distance <= 1000 && x.W2S.IsValid() && x.Health > 0 && x.IsValidTarget()).ToList());
            return jungleMobs;
        }

        private void OnPress(System.Windows.Forms.Keys keyBeingPressed, Keyboard.KeyPressState pressState)
        {
            if (GetSmite() != null && pressState == Keyboard.KeyPressState.Down && keyBeingPressed == Key && KeyIsToggled == false)
            {
                KeyIsToggled = true;
            }
            else if (pressState == Keyboard.KeyPressState.Down && keyBeingPressed == Key && KeyIsToggled == true)
            {
                KeyIsToggled = false;
            }
            return;
        }

        internal CastSlot GetCastSlot()
        {
            SpellBook spellBook = Env.Me().GetSpellBook();
            if (SummonerSpellsProvider.IHaveSpellOnSlot(SummonerSpellsEnum.Smite, SummonerSpellSlot.First))
                return CastSlot.Summoner1;
            else
                return CastSlot.Summoner2;
        }

        internal void TryCast()
        {
            List<JungleMob> jungleMobs = Monsters().deepCopy();
            float smiteDmg = SmiteDamage();
            CastSlot slot = GetCastSlot();
            SpellClass? smite = GetSmite();
            foreach (JungleMob mob in jungleMobs)
            {
                if (smite != null)
                    Logger.Log($"Smite: CD: {smite.Cooldown} Charges: {smite.Charges} Ready: {smite.IsSpellReady}");
                if (mob.Health > smiteDmg || mob.Health < 0 || !mob.IsAlive || mob.Distance > 500 || smite == null || smite.Charges < 1)
                    continue;
                if (GetMobType(mob) == MobType.Type.None)
                    continue;
                if (GetMobType(mob) == MobType.Type.Epic && Epics && smite.IsSpellReady)
                {
                    SpellCastProvider.CastSpell(slot, mob.Position);
                    break;
                }
                if (GetMobType(mob) == MobType.Type.Buff && Buffs && smite.IsSpellReady)
                {
                    SpellCastProvider.CastSpell(slot, mob.Position);
                    break;
                }
                if (GetMobType(mob) == MobType.Type.Common && Camps && smite.IsSpellReady)
                {
                    SpellCastProvider.CastSpell(slot, mob.Position);
                    break;
                }

            }
        }

        private Task MainInput()
        {
            if (Mode != "InCombo" || !Enabled)
                return Task.CompletedTask;
            TryCast();
            return Task.CompletedTask;
        }

        private Task MainTick()
        {
            if (Mode != "OnTick" && Mode != "ToggleKey" || !Enabled)
                return Task.CompletedTask;
            if (Mode == "OnTick")
                TryCast();
            else if (Mode == "ToggleKey" && KeyIsToggled)
                TryCast();
            return Task.CompletedTask;
        }

        internal MobType.Type GetMobType(JungleMob mob)
        {
            if (mob.UnitComponentInfo.SkinName.Contains("SRU_Red", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Contains("SRU_Blue", StringComparison.OrdinalIgnoreCase))
                return MobType.Type.Buff;
            if (mob.UnitComponentInfo.SkinName.Equals("SRU_Krug", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Contains("SRU_Gromp", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Equals("SRU_Murkwolf", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Contains("Super", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Equals("SRU_Razorbeak", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Equals("SRU_Crab", StringComparison.OrdinalIgnoreCase))
                return MobType.Type.Common;
            if (mob.UnitComponentInfo.SkinName.Contains("SRU_RiftHerald", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Contains("SRU_Baron", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Contains("SRU_Dragon", StringComparison.OrdinalIgnoreCase))
                return MobType.Type.Epic;
            return MobType.Type.None;
        }

        internal SpellClass? GetSmite()
        {
            SpellClass sum1 = Env.Me().GetSpellBook().GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Summoner1);
            if (Testing)
                Logger.Log(sum1.SpellData.SpellName);
            SpellClass sum2 = Env.Me().GetSpellBook().GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Summoner2);
            if (Testing)
                Logger.Log(sum2.SpellData.SpellName);
            SpellClass? Smite = (sum1.SpellData.SpellName.Contains("SummonerSmite")) ? sum1 : sum2.SpellData.SpellName.Contains("SummonerSmite") ? sum2 : null;
            return Smite;
        }

        internal float SmiteDamage()
        {
            SpellClass? Smite = GetSmite();
            if (Smite == null)
                return 0F;
            if (Testing)
                Logger.Log(Smite.SpellData.SpellName);
            if (Smite.SpellData.SpellName.Contains("SummonerSmite") && Smite.IsSpellReady)
            {
                if (Smite.SpellData.SpellName.Contains("SmiteDuel") || Smite.SpellData.SpellName.Contains("SmitePlayerGanker"))
                    return 900;
                if (Smite.SpellData.SpellName.Contains("SmiteAvatar"))
                    return 1200;
                return 600;
            }
            return 0;
        }

        private void Draw(JungleMob mob, float smiteDmg, Color color)
        {
            if (mob.Health > 0 && mob.IsAlive && mob.Health < smiteDmg)
            {
                Common.Helper.Drawings.DrawHPBar(mob, ColorConverter.GetColorWithAlpha(color, 255), 1);
            } else if (mob.Health > 0 && mob.IsAlive)
            {
                RenderFactory.DrawHPBarDamage(mob, smiteDmg, color);
            }
        }

        private void OnCoreRender()
        {
            if (Mode == "ToggleKey" && KeyIsToggled)
            {
                Vector2 pos = Env.Me().Position.ToW2S();
                pos.Y += 30;
                RenderFactory.DrawText("Auto smite on", 12, pos, Color.Black, true);
            }
            if (!DrawDamage)
                return;
            List<JungleMob> jungleMobs = Monsters().deepCopy();
            float smiteDmg = SmiteDamage();
            foreach (JungleMob mob in jungleMobs)
            {
                if (GetMobType(mob) == MobType.Type.None)
                    continue;
                if (GetMobType(mob) == MobType.Type.Common && DrawCamps)
                {
                    Draw(mob, smiteDmg, DamageColor);
                    continue;
                } 
                if (GetMobType(mob) == MobType.Type.Buff && DrawBuffs)
                {
                    Draw(mob, smiteDmg, DamageColor);
                    continue;
                }
                if (GetMobType(mob) == MobType.Type.Epic && DrawEpics)
                {
                    Draw(mob, smiteDmg, DamageColor);
                }
            }
        }
    }
}