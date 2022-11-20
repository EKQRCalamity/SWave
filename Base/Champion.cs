using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.SDK.Tools;
using SyncWave.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Base
{
    internal abstract class Champion
    {
        internal abstract List<Combo> Combos { get; }
        // Thanks to Six!!! https://github.com/SiXZZZ/SixAIO/blob/341d50e497d561242cbf8003c50ad12aa9830117/src/SixAIO.NET/Models/Champion.cs#L214
        internal static Champion? GetFromName(string name)
        {
            if (!Env.SupportedChamps.Contains(name))
                return (Champion?)Activator.CreateInstance(Type.GetType("SyncWave.Champions.None"));
            var type = Type.GetType($"SyncWave.Champions.{char.ToUpper(name[0]) + name.ToLower().Substring(1)}");
            Champion? c = (Champion?)Activator.CreateInstance(type);
            return (c == null)? (Champion?)Activator.CreateInstance(Type.GetType("SyncWave.Champions.None")) : c;
        }

        internal abstract void Init();
    }
}
