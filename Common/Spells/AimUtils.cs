using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Logic;
using Oasys.SDK;
using SharpDX;
using SyncWave.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Spells
{
    internal static class AimUtils
    {
        internal static List<GameObjectBase> enemiesInRange(int range) => UnitManager.EnemyChampions.Where(x => x.Distance < range && x.Position.IsOnScreen()).ToList<GameObjectBase>();

        internal static Vector2 MousePosOnScreen => new(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);

        internal static List<GameObjectBase> enemiesNearToMouseSorted(int range)
        {
            List<GameObjectBase> targets = enemiesInRange(range);
            return targets.OrderBy(x => x.Position.ToW2S().Distance(MousePosOnScreen)).ToList();
        }

        internal static GameObjectBase? NearestTargetToMouse(int range)
        {
            return enemiesNearToMouseSorted(range).FirstOrDefault();
        }

        internal static List<GameObjectBase> TargetEnemies(int range)
        {
            return Oasys.Common.Logic.TargetSelector.GetMixedTargets(UnitManager.EnemyChampions.deepCopy(), x => x.Distance < range && x.Position.IsOnScreen()).ToList();
        }

        internal static GameObjectBase BestHeroTarget(int range)
        {
            return Oasys.Common.Logic.TargetSelector
                .GetBestHeroTarget(null, x => x.Distance < range);
        }
    }
}
