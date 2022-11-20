using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.SDK;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Base;
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
        internal string Name { get; set; }
        internal Color Color { get; private set; }
        internal bool IsOn { get; set; }
        internal uint Priority { get; private set; }

        internal DamageCalculation Calculator { get; }

        internal float GetDamage(GameObjectBase target)
        {
            return Calculator.CalculateDamage(target);
        }

        internal void UpdatePriority(uint priority)
        {
            this.Priority = priority;
            return;
        }

        internal void UpdateColor(Color color)
        {
            this.Color = color;
            return;
        }

        internal void UpdateName(string name)
        {
            this.Name = name;
            return;
        }

        public Damage(string name, uint priority, DamageCalculation calculator, Color color)
        {
            this.Name = name;
            this.Priority = priority;
            this.Calculator = calculator;
            this.Color = color;
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

        internal static void Init()
        {
            if (Initialized) return;
            CoreEvents.OnCoreRender += Draw;
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
