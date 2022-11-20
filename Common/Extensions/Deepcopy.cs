using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Extensions
{
    internal static class Deepcopy
    {
        internal static List<T> deepCopy<T>(this IEnumerable<T> enumerable)
        {
            List<T> list = new List<T>(enumerable.ToArray());
            return list;
        }
    }
}
