using Oasys.Common.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Base
{
    internal abstract class DamageCalculation
    {
        internal abstract float CalculateDamage(GameObjectBase target);
    }
}
