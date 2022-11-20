using SyncWave.Base;
using SyncWave.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Helper.Selectors.Orianna
{
    internal class ComboSelector : Base.ComboSelector
    {
        internal override List<Combo> Combos => new Champions.Orianna().Combos;
        internal override Combo? preferedCombo => Combos[0];
        internal override Combo Select(ComboModes mode = ComboModes.Mixed)
        {
            return DefaultSelect(mode);
        }
    }
}
