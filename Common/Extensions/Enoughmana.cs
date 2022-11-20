
using Oasys.Common.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Extensions
{
    internal static class EnoughMana
    {
        internal static bool enoughMana(this GameObjectBase target,float manaNeeded) => target.Mana >= manaNeeded;
    }
}
