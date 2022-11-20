using Oasys.Common.Extensions;
using Oasys.Common.GameObject.Clients;
using Oasys.SDK;
using SyncWave.Common.Helper.Selectors;
using SharpDX;
using System.Collections.Generic;
using Oasys.Common.GameObject;

namespace SyncWave.Base
{
    internal abstract class Combo
    {
        internal abstract string Name { get; }

        internal abstract int MinMana { get; }

        internal abstract int MinRange { get; }

        internal abstract SyncWave.Common.Helper.Selectors.TargetSelector TargetSelector { get; }

        internal abstract float Damage { get; }
        internal float Efficiency => Damage / MinMana;

        internal bool EnoughMana()
        {
            return Env.Me().Mana >= MinMana;
        }

        internal abstract bool Enabled();

        internal List<GameObjectBase> KillableEnemiesInRange()
        {
            List<GameObjectBase> ret = new();
            List<GameObjectBase> targets = TargetSelector.GetTargetsInRange();
            foreach (GameObjectBase target in targets)
            {
                if (!target.IsAlive)
                    continue;
                if (!target.IsVisible)
                    continue;
                if (TargetSelector.TargetInRange(target) && CanKill(target))
                    ret.Add(target);
            }
            return ret;
        }

        internal abstract bool SpellsReady();

        internal bool CheckAvailability()
        {
            // Calculate Enough Mana, Enemy in Range, ...
            return EnoughMana() && TargetSelector.XTargetsInRange(1) && Enabled();
        }

        internal abstract bool CanKill(GameObjectBase target);

        internal abstract bool CanKill();

        internal abstract float GetFullDamageRaw();

        internal abstract float GetDamage(GameObjectBase target);

        internal abstract bool Run();

        internal abstract bool Run(GameObjectBase target);
    }
}
