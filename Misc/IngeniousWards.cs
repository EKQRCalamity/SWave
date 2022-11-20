using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Misc
{
    internal class Ward
    {
        internal Vector3 ClickPosition { get; }
        internal Vector3 WardPosition { get; }
        internal Vector3 MovePosition { get; }

        public Ward(Vector3 movePos, Vector3 clickPos, Vector3 wardPos)
        {
            this.MovePosition = movePos;
            this.ClickPosition = clickPos;
            this.WardPosition = wardPos;
        }
    }

    internal class IngeniousWards
    {
        internal List<Ward> SummonersRiftLoc = new List<Ward>()
        {
            new Ward(
                new Vector3(1774F, 52.84F, 10856F),
                new Vector3(2380.09F, -71.24F, 11004.69F),
                new Vector3(2826.47F, -71.02F, 11221.34F)
                ),
            new Ward(
                new Vector3(5749.25F, 51.65F, 7282.75F),
                new Vector3(5174.83F, 50.57F, 7119.81F),
                new Vector3(4909.10F, 50.65F, 7110.90F)
                ),
            new Ward(
                new Vector3(8022F, 53.72F, 4258F),
                new Vector3(8463.64F, 50.60F, 4658.71F),
                new Vector3(8512.29F, 51.30F, 4745.90F)
                ),
            new Ward(
                new Vector3(10072F, -71.24F, 3908F),
                new Vector3(10301.03F, 49.03F, 3333.20F),
                new Vector3(10322.94F, 49.03F, 3244.38F)
                )
        };
    }
}
