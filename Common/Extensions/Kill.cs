using Oasys.Common.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Extensions
{
    internal static class Kill
    {
        internal static bool CanKill(this GameObjectBase target, float damage)
        {
            return target.Health - damage < 0;
        }
    }
}
