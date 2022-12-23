using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Tools;
using Oasys.Common.Tools.Devices;
using Oasys.SDK;
using Oasys.SDK.InputProviders;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncWave.Misc
{
    internal static class SpellBuffer
    {
        internal static Vector2 MousePosOnScreen => new(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
        internal static void Init()
        {
            Oasys.Common.Tools.Devices.Keyboard.OnKeyPress += KeyPressHandler;
        }

        private static void KeyPressHandler(Keys keyBeingPressed, Keyboard.KeyPressState pressState)
        {
            if (keyBeingPressed == Keys.Q)
            {
                GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance < 1115);
                if (target != null)
                {
                    Vector2 originalPos = MousePosOnScreen;
                    Prediction.MenuSelected.PredictionOutput pred = Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, 1115, 160, 0.25F, 1800);
                    if (pred.CollisionObjects.All(x => x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient)))
                    {
                        Vector2 pos = pred.CastPosition.ToW2S();
                        Logger.Log($"{pred.CastPosition} is {pos}");
                        MouseProvider.SetCursor((int)pos.X, (int)pos.Y);
                        Thread.Sleep(25);
                        MouseProvider.SetCursor((int)originalPos.X, (int)originalPos.Y);
                    }
                }
            }
        }
    }
}
