using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Combos
{
    internal class None : SyncWave.Base.Combo
    {
        internal override string Name => "None";
        internal override int MinMana => 99999;

        internal override int MinRange => 99999;

        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(0);

        internal override float Damage => 1F;

        internal override bool SpellsReady()
        {
            return false;
        }

        internal override bool Enabled()
        {
            return false;
        }

        internal override float GetFullDamageRaw()
        {
            return 1F;
        }
        internal override float GetDamage(GameObjectBase target)
        {
            return 1F;
        }

        internal override bool CanKill(GameObjectBase target)
        {
            return false;
        }

        internal override bool CanKill()
        {
            return false;
        }

        internal override bool Run()
        {
            return false;
        }

        internal override bool Run(GameObjectBase target)
        {
            return false;
        }
    }
}
