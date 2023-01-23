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
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Base;
using SyncWave.Common.Extensions;
using SyncWave.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Oasys.Common.GameObject.Clients.ExtendedInstances.HeroInventory;

namespace SyncWave.Misc
{
    internal class GaleforceCalc : DamageCalculation
    {
        internal float PreCalc(GameObjectBase target, float healthOffset)
        {
            float[] damagePerLevel = new float[] { 65F, 70F, 75F, 80F, 85F, 90F, 95F, 100F, 105F };
            float damage = 0;
            if (Env.Level <= 9)
                damage = 60F;
            else if (Env.Level >= 10 && Env.Level <= 18)
                damage = damagePerLevel[Env.Level - 10];
            else if (Env.Level > 18)
                damage = damagePerLevel[damagePerLevel.Length - 1];
            damage += Env.Me().UnitStats.BonusAttackDamage * 0.15F;
            float missHealth = target.MissingHealth + healthOffset;
            missHealth = (missHealth > target.MaxHealth)? target.MaxHealth : missHealth;
            int n = (int)Math.Floor((missHealth / target.MaxHealth) * 100 % 7);
            damage = damage * (1F + ((n >= 10) ? 0.5F : n * 0.05F));
            return damage;
        }

        internal override float CalculateDamage(GameObjectBase target)
        {
            float firstMissileDamage = PreCalc(target, 0);
            float secondMissileDamage = PreCalc(target, firstMissileDamage);
            float thirdMissileDamage = PreCalc(target, firstMissileDamage + secondMissileDamage);
            float damage = firstMissileDamage + secondMissileDamage + thirdMissileDamage;
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
        }
    }

    internal class MoreDrawings
    {
        // Menu Section
        #region Menu
        internal static int TabIndex = -1;
        internal static int GroupIndex = -1;
        internal static int EnabledIndex = -1;
        internal static int DrawCollectorExecuteIndex = -1;
        internal static int DrawCollectorModeIndex = -1;
        internal static int DrawCollectorColorIndex = -1;
        internal static int DrawCollectorYOffsetIndex = -1;
        internal static int DrawGaleforceIndex = -1;
        internal static int DrawGaleforceModeIndex = -1;
        internal static int DrawGaleforcePriorityIndex = -1;
        internal static int DrawGaleforceColorIndex = -1;
        internal static int DrawGaleforceAlphaIndex = -1;
        #endregion

        #region MenuSetup&Props
        internal void SetupMenu()
        {
            TabIndex = Menu.Init();
            Group group = new Group("More Drawings");
            GroupIndex = Menu.tab.AddGroup(group);
            EnabledIndex = group.AddItem(new Switch() { Title = "Enabled", IsOn = true });
            DrawCollectorExecuteIndex = group.AddItem(new Switch() { Title = "Draw Collector Execute", IsOn = true });
            DrawCollectorModeIndex = group.AddItem(new ModeDisplay() { Title = "Draw Mode Collector", ModeNames = new() { "Line", "Box" }, SelectedModeName = "Box" });
            DrawCollectorColorIndex = group.AddItem(new ModeDisplay() { Title = "Collector Color", ModeNames = ColorConverter.GetColors(), SelectedModeName = "Red" });
            DrawCollectorYOffsetIndex = group.AddItem(new Counter() { Title = "Y Axis Offset", Value = 0, MinValue = -50000, MaxValue = 50000, ValueFrequency = 1 });
            DrawGaleforceIndex = group.AddItem(new Switch() { Title = "Draw Galeforce Damage", IsOn = true });
            DrawGaleforceModeIndex = group.AddItem(new ModeDisplay() { Title = "Draw Mode Galeforce", ModeNames = new() { "AboveHPBar", "AboveHPBarWithName" }, SelectedModeName = "AboveHPBar" });
            DrawGaleforcePriorityIndex = group.AddItem(new Counter() { Title = "Galeforce Priority", MinValue = 1, MaxValue = 10, ValueFrequency = 1 });
            DrawGaleforceColorIndex = group.AddItem(new ModeDisplay() { Title = "Galeforce Color", ModeNames = ColorConverter.GetColors(), SelectedModeName = "Blue" });
            DrawGaleforceAlphaIndex = group.AddItem(new Counter() { Title = "Galeforce Alpha", Value = 200, ValueFrequency = 5, MinValue = 0, MaxValue = 255 });
        }

        internal bool Enabled => Menu.tab.GetGroup(GroupIndex).GetItem<Switch>(EnabledIndex).IsOn;

        internal bool DrawCollectorExecute => Menu.tab.GetGroup(GroupIndex).GetItem<Switch>(DrawCollectorExecuteIndex).IsOn;

        internal string DrawCollectorMode => Menu.tab.GetGroup(GroupIndex).GetItem<ModeDisplay>(DrawCollectorModeIndex).SelectedModeName;

        internal string DrawCollectorColorName => Menu.tab.GetGroup(GroupIndex).GetItem<ModeDisplay>(DrawCollectorColorIndex).SelectedModeName;

        internal Color DrawCollectorColor => ColorConverter.GetColor(DrawCollectorColorName);

        internal int DrawCollectorYOffset => Menu.tab.GetGroup(GroupIndex).GetItem<Counter>(DrawCollectorYOffsetIndex).Value;

        internal bool DrawGaleforceDamage => Menu.tab.GetGroup(GroupIndex).GetItem<Switch>(DrawGaleforceIndex).IsOn;

        internal string DrawGaleforceMode => Menu.tab.GetGroup(GroupIndex).GetItem<ModeDisplay>(DrawGaleforceModeIndex).SelectedModeName;

        internal string DrawGaleforceColorName => Menu.tab.GetGroup(GroupIndex).GetItem<ModeDisplay>(DrawGaleforceColorIndex).SelectedModeName;

        internal int DrawGaleforcePrio => Menu.tab.GetGroup(GroupIndex).GetItem<Counter>(DrawGaleforcePriorityIndex).Value;

        internal Color DrawGaleforceColor => ColorConverter.GetColor(DrawGaleforceColorName);

        internal int DrawGaleforceAlpha => Menu.tab.GetGroup(GroupIndex).GetItem<Counter>(DrawGaleforceAlphaIndex).Value;

        #endregion

        internal Damage? GaleforceDamage;

        internal void Init()
        {
            SetupMenu();
            CoreEvents.OnCoreRender += Draw;
            CoreEvents.OnCoreMainTick += MainTick;
            Render.Init();
            GaleforceDamage = new Damage(Menu.tab, Menu.tab.GetGroup(GroupIndex), "GF", 3, new GaleforceCalc(), ColorConverter.GetColorWithAlpha(DrawGaleforceColor, DrawGaleforceAlpha), (x => x.IsAlive && HasGaleforce()));
            Render.AddDamage(GaleforceDamage);
        }

        internal Task MainTick()
        {
            return Task.CompletedTask;
        }

        internal bool HasCollector()
        {
            foreach (Item item in Env.Me().Inventory.GetItemList())
            {
                if (item.ID == Oasys.Common.Enums.GameEnums.ItemID.The_Collector)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool HasGaleforce()
        {
            foreach (Item item in Env.Me().Inventory.GetItemList())
            {
                if (item.ID == Oasys.Common.Enums.GameEnums.ItemID.Galeforce)
                {
                    return true;
                }
            }
            return false;
        }

        internal void Draw()
        {
            if (!DrawCollectorExecute && !DrawGaleforceDamage || !Enabled)
                return;

            foreach (GameObjectBase target in UnitManager.EnemyChampions)
            {
                if (!target.IsAlive || !target.IsVisible || !target.IsEnemy || !(target.Distance <= 2000))
                    continue;
                if (HasCollector() && DrawCollectorExecute)
                {
                    Drawings.DrawExecuteBar(target, (target.MaxHealth * 0.0495F), DrawCollectorColor, DrawCollectorMode == "Line", DrawCollectorYOffset);
                }
            }
        }
    }
}
