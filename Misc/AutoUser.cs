using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.Logic;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.Events;
using Oasys.SDK.Menu;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Oasys.Common.GameObject.Clients.ExtendedInstances.HeroInventory;

namespace SyncWave.Misc
{
    internal class AutoUser
    {

        // Menu Section
        #region Menu
        internal static int TabIndex = -1;
        internal static int PotGroupIndex = -1;
        internal static Group? PotGroup;
        internal static int PotEnabledIndex = -1;
        internal static int PotHPCounterIndex = -1;
        internal static int PotCastModeIndex = -1;
        internal static int PotWhenEnemiesNearIndex = -1;
        internal static int PotNearRangeIndex = -1;
        internal static int HealGroupIndex = -1;
        internal static Group? HealGroup;
        internal static int HealEnabledIndex = -1;
        internal static int HealHPCounterIndex = -1;
        internal static int HealCastModeIndex = -1;
        internal static int HealWhenEnemiesNearIndex = -1;
        internal static int HealNearRangeIndex = -1;
        internal static int ShieldGroupIndex = -1;
        internal static Group? ShieldGroup;
        internal static int ShieldEnabledIndex = -1;
        internal static int ShieldHPCounterIndex = -1;
        internal static int ShieldCastModeIndex = -1;
        internal static int ShieldWhenEnemiesNearIndex = -1;
        internal static int ShieldNearRangeIndex = -1;
        internal static int CleanseGroupIndex = -1;
        internal static Group? CleanseGroup;
        internal static int CleanseEnabledIndex = -1;
        internal static int CleanseCastModeIndex = -1;
        internal static int CleanseWhenEnemiesNearIndex = -1;
        internal static int CleanseNearRangeIndex = -1;


        internal Tab Tab => (Menu.tab == null)? MenuManager.GetTab(Menu.Init()) : Menu.tab;
        internal Group potGroup => (PotGroup == null) ? Tab.GetGroup(PotGroupIndex) : PotGroup;
        internal Group healGroup => (HealGroup == null) ? Tab.GetGroup(HealGroupIndex) : HealGroup;
        internal Group shieldGroup => (ShieldGroup == null) ? Tab.GetGroup(ShieldGroupIndex) : ShieldGroup;
        internal Group cleanseGroup => (CleanseGroup == null) ? Tab.GetGroup(CleanseGroupIndex) : CleanseGroup;
        internal bool PotEnabled => potGroup.GetItem<Switch>(PotEnabledIndex).IsOn;
        internal bool HealEnabled => healGroup.GetItem<Switch>(HealEnabledIndex).IsOn;
        internal bool ShieldEnabled => shieldGroup.GetItem<Switch>(ShieldEnabledIndex).IsOn;
        internal bool CleanseEnabled => cleanseGroup.GetItem<Switch>(CleanseEnabledIndex).IsOn;
        internal int PotHP => potGroup.GetItem<Counter>(PotHPCounterIndex).Value;
        internal int HealHP => healGroup.GetItem<Counter>(HealHPCounterIndex).Value;
        internal int ShieldHP => shieldGroup.GetItem<Counter>(ShieldHPCounterIndex).Value;
        internal string PotCastMode => potGroup.GetItem<ModeDisplay>(PotCastModeIndex).SelectedModeName;
        internal string HealCastMode => healGroup.GetItem<ModeDisplay>(HealCastModeIndex).SelectedModeName;
        internal string ShieldCastMode => shieldGroup.GetItem<ModeDisplay>(ShieldCastModeIndex).SelectedModeName;
        internal string CleanseCastMode => cleanseGroup.GetItem<ModeDisplay>(CleanseCastModeIndex).SelectedModeName;
        internal bool PotEnemiesNear => potGroup.GetItem<Switch>(PotWhenEnemiesNearIndex).IsOn;
        internal bool HealEnemiesNear => healGroup.GetItem<Switch>(HealWhenEnemiesNearIndex).IsOn;
        internal bool ShieldEnemiesNear => shieldGroup.GetItem<Switch>(ShieldWhenEnemiesNearIndex).IsOn;
        internal bool CleanseEnemiesNear => cleanseGroup.GetItem<Switch>(CleanseWhenEnemiesNearIndex).IsOn;
        internal int PotRange => potGroup.GetItem<Counter>(PotNearRangeIndex).Value;
        internal int HealRange => healGroup.GetItem<Counter>(HealNearRangeIndex).Value;
        internal int ShieldRange => shieldGroup.GetItem<Counter>(ShieldNearRangeIndex).Value;
        internal int CleanseRange => cleanseGroup.GetItem<Counter>(CleanseNearRangeIndex).Value;
        #endregion

        internal int Tick = 0;

        internal int LastPotUseTick = 0;

        internal CastSlot? PotSlot = null;
        internal CastSlot? HealSlot = null;
        internal CastSlot? ShieldSlot = null;
        internal CastSlot? CleanseSlot = null;

        internal int HealthPotionId = 2003;
        internal int RefillablePotionId = 2031;
        internal int CorruptionhPotionId = 2033;
        internal bool HasPots()
        {
            foreach (Item item in Env.Me().Inventory.GetItemList())
            {
                if (item.ID == ItemID.Health_Potion || item.ID == ItemID.Corrupting_Potion || item.ID == ItemID.Refillable_Potion)
                {
                    PotSlot = (CastSlot)item.SpellCastSlot;
                    return true;
                }
            }
            return false;
        }

        internal bool HasHeal()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.Summoner1).SpellData.SpellName.Contains("Heal") && Env.Spells.GetSpellClass(SpellSlot.Summoner1).IsSpellReady)
            {
                ShieldSlot = CastSlot.Summoner1;
                return true;
            }
            else if (Env.Spells.GetSpellClass(SpellSlot.Summoner2).SpellData.SpellName.Contains("Heal") && Env.Spells.GetSpellClass(SpellSlot.Summoner2).IsSpellReady)
            {
                ShieldSlot = CastSlot.Summoner2;
                return true;
            }
            return false;
        }

        internal CastSlot? HealCastSlot()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.Summoner1).SpellData.SpellName.Contains("Heal") && Env.Spells.GetSpellClass(SpellSlot.Summoner1).IsSpellReady)
            {
                return CastSlot.Summoner1;
            }
            else if (Env.Spells.GetSpellClass(SpellSlot.Summoner2).SpellData.SpellName.Contains("Heal") && Env.Spells.GetSpellClass(SpellSlot.Summoner2).IsSpellReady)
            {
                return CastSlot.Summoner2;
            }
            return null;
        }

        internal bool HasShield()
        {

            if (Env.Spells.GetSpellClass(SpellSlot.Summoner1).SpellData.SpellName.Contains("Barrier") && Env.Spells.GetSpellClass(SpellSlot.Summoner1).IsSpellReady)
            {
                ShieldSlot = CastSlot.Summoner1;
                return true;
            } else if (Env.Spells.GetSpellClass(SpellSlot.Summoner2).SpellData.SpellName.Contains("Barrier") && Env.Spells.GetSpellClass(SpellSlot.Summoner2).IsSpellReady)
            {
                ShieldSlot = CastSlot.Summoner2;
                return true;
            }
            return false;
        }

        internal CastSlot? ShieldCastSlot()
        {

            if (Env.Spells.GetSpellClass(SpellSlot.Summoner1).SpellData.SpellName.Contains("Barrier") && Env.Spells.GetSpellClass(SpellSlot.Summoner1).IsSpellReady)
            {
                return CastSlot.Summoner1;
            }
            else if (Env.Spells.GetSpellClass(SpellSlot.Summoner2).SpellData.SpellName.Contains("Barrier") && Env.Spells.GetSpellClass(SpellSlot.Summoner2).IsSpellReady)
            {
                return CastSlot.Summoner2;
            }
            return null;
        }

        internal bool HasCleanse()
        {
            Logger.Log($"Sum1: {Env.Spells.GetSpellClass(SpellSlot.Summoner1).SpellData.SpellName} Sum2: {Env.Spells.GetSpellClass(SpellSlot.Summoner2).SpellData.SpellName}");
            if (Env.Spells.GetSpellClass(SpellSlot.Summoner1).SpellData.SpellName.Contains("Boost") && Env.Spells.GetSpellClass(SpellSlot.Summoner1).IsSpellReady)
            {
                ShieldSlot = CastSlot.Summoner1;
                return true;
            }
            else if (Env.Spells.GetSpellClass(SpellSlot.Summoner2).SpellData.SpellName.Contains("Boost") && Env.Spells.GetSpellClass(SpellSlot.Summoner2).IsSpellReady)
            {
                ShieldSlot = CastSlot.Summoner2;
                return true;
            }
            return false;
        }

        internal CastSlot? CleanseCastSlot()
        {
            Logger.Log($"Sum1: {Env.Spells.GetSpellClass(SpellSlot.Summoner1).SpellData.SpellName} Sum2: {Env.Spells.GetSpellClass(SpellSlot.Summoner2).SpellData.SpellName}");
            if (Env.Spells.GetSpellClass(SpellSlot.Summoner1).SpellData.SpellName.Contains("Boost") && Env.Spells.GetSpellClass(SpellSlot.Summoner1).IsSpellReady)
            {
                return CastSlot.Summoner1;
                
            }
            else if (Env.Spells.GetSpellClass(SpellSlot.Summoner2).SpellData.SpellName.Contains("Boost") && Env.Spells.GetSpellClass(SpellSlot.Summoner2).IsSpellReady)
            {
                return CastSlot.Summoner2;
            }
            return null;
        }

        internal void Init()
        {
            SetupMenu();
            CoreEvents.OnCoreMainTick += MainTick;
            CoreEvents.OnCoreMainInputAsync += MainInput;
        }

        internal void UsePots()
        {
            if (!PotEnabled || !HasPots())
                return;
            if (Env.Me().HealthPercent <= PotHP && Tick - 100 > LastPotUseTick)
            {
                if (PotEnemiesNear && new Common.Helper.Selectors.TargetSelector(PotRange).XTargetsInRange(1, PotRange, Common.Helper.Selectors.Modes.Enemy))
                {
                    LastPotUseTick = Tick;
                    SpellCastProvider.CastSpell((CastSlot)PotSlot);
                } else if (!PotEnemiesNear)
                {
                    LastPotUseTick = Tick;
                    SpellCastProvider.CastSpell((CastSlot)PotSlot);
                }
            }
        }

        internal void UseHeal()
        {
            if (!HealEnabled || !HasHeal() || HealCastSlot() == null)
                return;
            if (Env.Me().HealthPercent <= HealHP)
            {
                if (HealEnemiesNear && new Common.Helper.Selectors.TargetSelector(HealRange).XTargetsInRange(1, HealRange, Common.Helper.Selectors.Modes.Enemy))
                {
                    SpellCastProvider.CastSpell((CastSlot)HealCastSlot());
                } else if (!HealEnemiesNear)
                {
                    SpellCastProvider.CastSpell((CastSlot)HealCastSlot());
                }
            }
        }

        internal void UseShield()
        {
            if (!ShieldEnabled || !HasShield() || ShieldCastSlot() == null)
                return;
            if (Env.Me().HealthPercent <= ShieldHP)
            {
                if (HealEnemiesNear && new Common.Helper.Selectors.TargetSelector(ShieldRange).XTargetsInRange(1, ShieldRange, Common.Helper.Selectors.Modes.Enemy))
                {
                    SpellCastProvider.CastSpell((CastSlot)ShieldCastSlot());
                } else if (!HealEnemiesNear)
                {
                    SpellCastProvider.CastSpell((CastSlot)ShieldCastSlot());
                }
            }
        }

        internal static bool IsCrowdControllButCanCleanse(BuffEntry buff, bool slowIsCC)
        {
            return buff.IsActive && buff.Stacks >= 1 &&
                   ((slowIsCC && buff.EntryType == BuffType.Slow) ||
                   buff.EntryType == BuffType.Stun || buff.EntryType == BuffType.Taunt ||
                   buff.EntryType == BuffType.Snare || buff.EntryType == BuffType.Charm ||
                   buff.EntryType == BuffType.Silence || buff.EntryType == BuffType.Blind ||
                   buff.EntryType == BuffType.Fear || buff.EntryType == BuffType.Polymorph ||
                   buff.EntryType == BuffType.Flee || buff.EntryType == BuffType.Sleep) &&
                   !buff.Name.Equals("yonerstun", System.StringComparison.OrdinalIgnoreCase) &&
                   !buff.Name.Equals("landslidedebuff", System.StringComparison.OrdinalIgnoreCase) &&
                   !buff.Name.Equals("CassiopeiaWSlow", System.StringComparison.OrdinalIgnoreCase) &&
                   !buff.Name.Equals("megaadhesiveslow", System.StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsCrowdControlledButCanCleanse<T>(T obj) where T : GameObjectBase
        {
            return obj.BuffManager.GetBuffList().Any(x => IsCrowdControllButCanCleanse(x, false));
        }

        internal void UseCleanse()
        {
            if (!CleanseEnabled || !HasCleanse() || CleanseCastSlot == null)
                return;
            Logger.Log(IsCrowdControlledButCanCleanse(Env.Me()));
            if (IsCrowdControlledButCanCleanse(Env.Me()))
            {
                if (CleanseEnemiesNear && new Common.Helper.Selectors.TargetSelector(CleanseRange).XTargetsInRange(1, CleanseRange, Common.Helper.Selectors.Modes.Enemy))
                {
                    SpellCastProvider.CastSpell((CastSlot)CleanseCastSlot());
                }
                else if (!CleanseEnemiesNear)
                {
                    SpellCastProvider.CastSpell((CastSlot)CleanseCastSlot());
                }
            }
        }

        private Task MainTick()
        {
            Tick++;
            if (!Env.Me().IsAlive)
                return Task.CompletedTask;
            if (Tick % 5 == 0)
            {
                if (PotEnabled && PotCastMode == "OnTick")
                {
                    UsePots();
                }
                if (HealEnabled && HealCastMode == "OnTick")
                {
                    UseHeal();
                }
                if (ShieldEnabled && ShieldCastMode == "OnTick")
                {
                    UseShield();
                }
                if (CleanseEnabled && CleanseCastMode == "OnTick")
                {
                    UseCleanse();
                }
            }
            return Task.CompletedTask;
        }

        private Task MainInput()
        {
            if (!Env.Me().IsAlive)
                return Task.CompletedTask;
            if (Tick % 5 == 0)
            {
                if (PotEnabled && PotCastMode == "InCombo")
                {
                    UsePots();
                }
                if (HealEnabled && HealCastMode == "InCombo")
                {
                    UseHeal();
                }
                if (ShieldEnabled && ShieldCastMode == "InCombo")
                {
                    UseShield();
                }
                if (CleanseEnabled && CleanseCastMode == "InCombo")
                {
                    UseCleanse();
                }
            }
            return Task.CompletedTask;
        }


        internal void SetupMenu()
        {
            TabIndex = Menu.Init();
            PotGroup = new Group("Auto Pots");
            HealGroup = new Group("Auto Heal");
            ShieldGroup = new Group("Auto Shield");
            CleanseGroup = new Group("Auto Cleanse");
            PotGroupIndex = Tab.AddGroup(PotGroup);
            HealGroupIndex = Tab.AddGroup(HealGroup);
            ShieldGroupIndex = Tab.AddGroup(ShieldGroup);
            CleanseGroupIndex = Tab.AddGroup(CleanseGroup);
            PotEnabledIndex = PotGroup.AddItem(new Switch() { Title = "Enabled", IsOn = false });
            HealEnabledIndex = HealGroup.AddItem(new Switch() { Title = "Enabled", IsOn = false });
            ShieldEnabledIndex = ShieldGroup.AddItem(new Switch() { Title = "Enabled", IsOn = false });
            CleanseEnabledIndex = CleanseGroup.AddItem(new Switch() { Title = "Enabled", IsOn = false });
            PotHPCounterIndex = PotGroup.AddItem(new Counter() { Title = "Pot usage HP%", Value = 20, MaxValue = 100, MinValue = 0 });
            HealHPCounterIndex = HealGroup.AddItem(new Counter() { Title = "Heal usage HP%", Value = 20, MaxValue = 100, MinValue = 0 });
            ShieldHPCounterIndex = ShieldGroup.AddItem(new Counter() { Title = "Shield usage HP%", Value = 20, MaxValue = 100, MinValue = 0 });
            PotCastModeIndex = PotGroup.AddItem(new ModeDisplay() { Title = "Cast Mode", ModeNames = new() { "OnTick", "InCombo" }, SelectedModeName = "InCombo" });
            HealCastModeIndex = HealGroup.AddItem(new ModeDisplay() { Title = "Cast Mode", ModeNames = new() { "OnTick", "InCombo" }, SelectedModeName = "InCombo" });
            ShieldCastModeIndex = ShieldGroup.AddItem(new ModeDisplay() { Title = "Cast Mode", ModeNames = new() { "OnTick", "InCombo" }, SelectedModeName = "InCombo" });
            CleanseCastModeIndex = CleanseGroup.AddItem(new ModeDisplay() { Title = "Cast Mode", ModeNames = new() { "OnTick", "InCombo" }, SelectedModeName = "InCombo" });
            PotWhenEnemiesNearIndex = PotGroup.AddItem(new Switch() { Title = "Only when enemies near", IsOn = true });
            HealWhenEnemiesNearIndex = HealGroup.AddItem(new Switch() { Title = "Only when enemies near", IsOn = true });
            ShieldWhenEnemiesNearIndex = ShieldGroup.AddItem(new Switch() { Title = "Only when enemies near", IsOn = true });
            CleanseWhenEnemiesNearIndex = CleanseGroup.AddItem(new Switch() { Title = "Only when enemies near", IsOn = true });
            PotNearRangeIndex = PotGroup.AddItem(new Counter() { Title = "Range to consider near", Value = 1500, MinValue = 0, MaxValue = 2500, ValueFrequency = 5 });
            HealNearRangeIndex = HealGroup.AddItem(new Counter() { Title = "Range to consider near", Value = 1500, MinValue = 0, MaxValue = 2500, ValueFrequency = 5 });
            ShieldNearRangeIndex = ShieldGroup.AddItem(new Counter() { Title = "Range to consider near", Value = 1500, MinValue = 0, MaxValue = 2500, ValueFrequency = 5 });
            CleanseNearRangeIndex = CleanseGroup.AddItem(new Counter() { Title = "Range to consider near", Value = 1500, MinValue = 0, MaxValue = 2500, ValueFrequency = 5 });
        }
    }
}
