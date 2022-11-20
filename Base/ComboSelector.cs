using Oasys.Common.GameObject.Clients;
using SharpDX;
using SyncWave.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Base
{
    internal abstract class ComboSelector
    {
        internal abstract Combo? preferedCombo { get; }
        internal abstract List<Combo> Combos { get; }
        internal Combo None = new Combos.None();

        internal abstract Combo Select(ComboModes mode = ComboModes.Mixed);

        internal Combo DefaultSelect(ComboModes mode = ComboModes.Mixed)
        {
            Combo returnCombo = None;
            foreach (Combo combo in Combos)
            {
                if (!combo.CheckAvailability())
                    continue;
                if (Env.QLevel == 0 || Env.WLevel == 0 || Env.ELevel == 0)
                {
                    if (preferedCombo != null)
                    {
                        returnCombo = preferedCombo;
                    }
                    break;
                }
                if (mode == ComboModes.CanKill)
                {
                    if (returnCombo.GetFullDamageRaw() <= combo.GetFullDamageRaw())
                        returnCombo = combo;
                }
                else if (mode == ComboModes.Efficency)
                {
                    if (returnCombo.Efficiency <= combo.Efficiency)
                        returnCombo = combo;
                }
                else
                {
                    if (returnCombo.GetFullDamageRaw() <= combo.GetFullDamageRaw() || returnCombo.Efficiency <= combo.Efficiency)
                        returnCombo = combo;
                }
            }
            return returnCombo;
        }

    }
}
