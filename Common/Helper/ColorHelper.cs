using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Helper
{
    internal static class ColorHelper
    {
        private static int LinearInterpolation(int start, int end, float percentage)
        {
            return start + (int)Math.Round(percentage * (end - start));
        }

        internal static Color ColorInterpolation(Color start, Color end, float percentage)
        {
            return new Color(LinearInterpolation(start.R, end.R, percentage),
                              LinearInterpolation(start.G, end.G, percentage),
                              LinearInterpolation(start.B, end.B, percentage),
                              LinearInterpolation(start.A, end.A, percentage));
        }
    }
}
