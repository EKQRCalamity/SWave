using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using SyncWave.Base;
using SyncWave.Champions;
using SyncWave.Common.Enums;
using SyncWave.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Helper.Selectors.Kogmaw
{
    internal class ComboSelector : Base.ComboSelector
    {
        internal override Combo? preferedCombo => Combos[0];
        internal override List<Combo> Combos => new Champions.Kogmaw().Combos;
        internal override Combo Select(ComboModes mode = ComboModes.Mixed)
        {
            Combo retCombo = new Combos.None();
            List<GameObjectBase> enemiesR = Champions.Kogmaw.R.TargetSelector.GetTargetsInRange().deepCopy();
            foreach (GameObjectBase enemy in enemiesR)
            {
                if (Combos[1].CanKill(enemy))
                {
                    retCombo = Combos[1];
                }
            }
            List<GameObjectBase> enemiesGeneral = Combos[0].TargetSelector.GetTargetsInRange().deepCopy();
            foreach (GameObjectBase enemy in enemiesGeneral)
            {
                if (retCombo.GetType() == new Combos.None().GetType() && enemy.DistanceToPlayer() <= Champions.Kogmaw.E.MinRange)
                {
                    retCombo = Combos[0];
                }
            }
            return retCombo;
        }
    }
}
