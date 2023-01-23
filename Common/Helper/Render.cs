using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Base;
using SyncWave.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Helper
{

    internal class Damage
    {
        internal string OrigName { get; set; }
        internal string Name => GetName();
        internal Color Color => GetColor();
        internal bool IsOn => IsOnSwitch.IsOn && IsOnFunc(Env.Me());
        internal Func<GameObjectBase, bool> IsOnFunc { get; set; }
        internal uint Priority => (uint)PriorityCounter.Value;
        internal bool ShowOnHUD { get; set; } 
        internal Tab MainTab { get; set; }
        internal Group MainGroup { get; set; }
        internal Switch IsOnSwitch { get; set; } = new Switch("Draw Damage", true);
        internal ModeDisplay ColorDisplay { get; set; }
        internal Counter PriorityCounter { get; set; }
        internal Switch ShowName { get; set; } = new Switch("Show Name", true);

        internal DamageCalculation Calculator { get; }

        internal string GetName()
        {
            if (ShowName.IsOn)
            {
                return OrigName;
            }
            return "";
        }

        internal Color GetColor()
        {
            return ColorConverter.GetColor(ColorDisplay.SelectedModeName);
        }

        internal float GetDamage(GameObjectBase target)
        {
            return Calculator.CalculateDamage(target);
        }

        public Damage(Tab tab, Group group, string name, uint priority, DamageCalculation calculator, Color color, bool showOnHUD = false)
        {
            OrigName = name;
            this.ShowOnHUD = showOnHUD;
            this.MainGroup = group;
            this.MainTab = tab;
            IsOnFunc = (x => x.IsAlive);
            ColorDisplay = new ModeDisplay("Color", color);
            PriorityCounter = new Counter("Priority", (int)priority, 1, 10);
            Calculator= calculator;
            MainGroup.AddItem(IsOnSwitch);
            MainGroup.AddItem(ColorDisplay);
            MainGroup.AddItem(PriorityCounter);
            MainGroup.AddItem(ShowName);
        }

        public Damage(Tab tab, Group group, string name, uint priority, DamageCalculation calculator, Color color, Func<GameObjectBase, bool> isOnFunc,bool showOnHUD = false)
        {
            OrigName = name;
            this.ShowOnHUD = showOnHUD;
            this.MainGroup = group;
            this.MainTab = tab;
            IsOnFunc = isOnFunc;
            ColorDisplay = new ModeDisplay("Color", color);
            PriorityCounter = new Counter("Priority", (int)priority, 1, 10);
            Calculator = calculator;
            MainGroup.AddItem(IsOnSwitch);
            MainGroup.AddItem(ColorDisplay);
            MainGroup.AddItem(PriorityCounter);
            MainGroup.AddItem(ShowName);
        }
    }

    internal static class Render
    {
        internal static List<Damage> Damages = new List<Damage>();
        internal static bool Initialized = false;

        internal static void AddDamage(Damage damage)
        {
            Damages.Add(damage);
        }

        internal static void ClearDamage()
        {
            Damages.Clear();
        }

        internal static void RemoveDamage(Damage damage)
        {
            Damages.Remove(damage);
        }

        internal static bool HasDamage(Damage damage)
        {
            return Damages.Contains(damage);
        }

        internal static int Threshold => Menu.tab.GetItem<Counter>("Remove name threshold (HP%)").Value;

        internal static void Init()
        {
            if (Initialized) return;
            CoreEvents.OnCoreRender += Draw;
            int MiscTabIndex = Menu.Init();
            Menu.tab.AddItem(new Counter("Remove name threshold (HP%)", 7, 0, 100));
            Initialized = true;
        }

        internal static void Draw()
        {
            List<Hero> targets = UnitManager.EnemyChampions.Where(x => x != null && x.IsAlive && x.IsVisible && x.Position.IsOnScreen() && x.Distance <= 3000).ToList();
            List<Damage> damages = Damages.Where(x => x.IsOn).ToList();
            foreach (Hero target in targets)
            {
                Drawings.DrawMultiDamage(target, damages);
            }

        }

    }
}
