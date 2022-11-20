
using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Menu;
using SharpDX;
using SyncWave.Base;
using SyncWave.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Misc
{
    internal class TestCalc : DamageCalculation
    {
        internal override float CalculateDamage(GameObjectBase target)
        {
            return TestDamage.Damage;
        }
    }

    internal static class TestDamage
    {
        internal static int TabIndex = -1;
        internal static int OnIndex = -1;
        internal static int ColorIndex = -1;
        internal static int AlphaIndex = -1;
        internal static int PriorityIndex = -1;
        internal static int DamageIndex = -1;

        internal static void SetupMenu()
        {
            Tab Tab = new Tab("SyncWave - Test");
            TabIndex = MenuManagerProvider.AddTab(Tab);
            OnIndex = Tab.AddItem(new Switch() { Title = "Enabled", IsOn = true });
            ColorIndex = Tab.AddItem(new ModeDisplay() { Title = "Test Color", ModeNames = ColorConverter.GetColors(), SelectedModeName = "Red" });
            AlphaIndex = Tab.AddItem(new Counter() { Title = "Test Alpha", Value = 200, ValueFrequency = 5, MinValue = 0, MaxValue = 255 });
            PriorityIndex = Tab.AddItem(new Counter() { Title = "Test Prio", MaxValue = 10, MinValue = 1, Value = 2 });
            DamageIndex = Tab.AddItem(new FloatCounter() { Title = "Damage", MaxValue = 20000, MinValue = 0, ValueFrequency = 10 });
        }
        internal static bool On => MenuManager.GetTab(TabIndex).GetItem<Switch>(OnIndex).IsOn;
        internal static uint Prio => (uint)MenuManager.GetTab(TabIndex).GetItem<Counter>(PriorityIndex).Value;
        internal static float Damage => MenuManager.GetTab(TabIndex).GetItem<FloatCounter>(DamageIndex).Value;
        internal static int Alpha => MenuManager.GetTab(TabIndex).GetItem<Counter>(AlphaIndex).Value;
        internal static Color Color => ColorConverter.GetColor(MenuManager.GetTab(TabIndex).GetItem<ModeDisplay>(ColorIndex).SelectedModeName);

        internal static Damage? Test;

        internal static void Init()
        {
            SetupMenu();
            CoreEvents.OnCoreRender += Draw;
            Render.Init();
            Test = new Damage("T", Prio, new TestCalc(), ColorConverter.GetColorWithAlpha(Color, Alpha));
            Render.AddDamage(Test);
        }

        internal static void Draw()
        {
            Test.IsOn = On;
            Test.UpdateColor(ColorConverter.GetColorWithAlpha(Color, Alpha));
            Test.UpdatePriority(Prio);
        }
    }
}
