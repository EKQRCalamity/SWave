using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.Rendering;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncWave.Misc
{
    internal class Ward
    {
        internal string Name { get; }
        internal Vector3 MovePosition { get; }
        internal Vector3 ClickPosition { get; }
        internal Vector3 WardPosition { get; }

        public Ward(string name, Vector3 movePos, Vector3 clickPos, Vector3 wardPos)
        {
            this.Name = name;
            this.MovePosition = movePos;
            this.ClickPosition = clickPos;
            this.WardPosition = wardPos;
        }
        
        public bool IsOnWard(GameObjectBase target)
        {
            if (WardIsOn(target) != null)
            {
                return true;
            }
            return false;
        }

        public bool MeOnWard => IsOnWard(Env.Me());
        public Ward? WardIsOn(GameObjectBase target)
        {
            if (target.DistanceTo(MovePosition) <= 25)
            {
                return this;
            }
            return null;
        }
        public Ward? WardIAmOn => WardIsOn(Env.Me());
    }

    internal static class IngeniousWards
    {
        internal static Vector2 MousePosOnScreen => new Vector2(Cursor.Position.X, Cursor.Position.Y);
        public static Ward? WardIAmOn()
        {
            foreach (Ward ward in SummonersRiftLoc)
            {
                if (Env.Me().DistanceTo(ward.MovePosition) <= 25)
                {
                    return ward;
                }
            }
            return null;
        }

        internal static List<Ward> SummonersRiftLoc = new List<Ward>()
        {
             new Ward(
                "Sidebrush Blue",
                new Vector3(1774, 52.84F, 10856),
                new Vector3(2380.09F, -71.24F, 11004.69F),
                new Vector3(2826.47F, -71.02F, 11221.34F)
                ),
            new Ward(
                "Wolvesbush Mid Blue",
                new Vector3(5749.25F, 51.65F, 7282.75F),
                new Vector3(5174.83F, 50.57F, 7119.81F),
                new Vector3(4909.10F, 50.65F, 7110.90F)
                ),
            new Ward(
                "Wolvesbush Mid Red",
                new Vector3(9122, 52.60F, 7606),
                new Vector3(9647.62F, 51.31F, 7889.96F),
                new Vector3(9874.42F, 51.50F, 7969.29F)
                ),
            new Ward(
                "Tower-Wolvesbush Blue",
                new Vector3(5574F, 51.74F, 6458F),
                new Vector3(5239.21F, 50.67F, 6944.90F),
                new Vector3(4909.10F, 50.65F, 7110.90F)
                ),
            new Ward(
                "Tower-Wolvesbush Red",
                new Vector3(9122, 53.74F, 8356),
                new Vector3(9586.57F, 59.62F, 8020.29F),
                new Vector3(9871.77F, 51.47F, 8014.44F)
                ),
            new Ward(
                "Redbush Blue",
                new Vector3(8022F, 53.72F, 4258F),
                new Vector3(8463.64F, 50.60F, 4658.71F),
                new Vector3(8512.29F, 51.30F, 4745.90F)
                ),
            new Ward(
                "Redbush Red",
                new Vector3(6824, 56, 10656),
                new Vector3(6360.12F, 52.61F, 10362.71F),
                new Vector3(6269.35F, 53.72F, 10306.69F)
                ),
            new Ward(
                "Riverbush Top Blue",
                new Vector3(1774F, 52.84F, 10856F),
                new Vector3(2380.09F, -71.24F, 11004.69F),
                new Vector3(2826.47F, -71.02F, 11221.34F)
                ),
            new Ward(
                "Riverbush Bot Red",
                new Vector3(13022F, 51.37F, 3808F),
                new Vector3(12427.00F, -35.46F, 3984.26F),
                new Vector3(11975.34F, 66.37F, 3927.68F)
                ),
            new Ward(
                "Dragon-Tribush",
                new Vector3(10072, -71.24F, 3908),
                new Vector3(10301.03F, 49.03F, 3333.20F),
                new Vector3(10322.94F, 49.03F, 3244.38F)
                ),
            new Ward(
                "Baron-Tribush",
                new Vector3(4824, -71.24F, 10906),
                new Vector3(4633.83F, 50.51F, 11354.40F),
                new Vector3(4524.69F, 53.25F, 11515.21F))
        };



        internal static void Init()
        {
            MenuInit();
            CoreEvents.OnCoreRender += Render;
        }

        private static void Render()
        {
            if (Enabled.IsOn)
            {
                foreach (Ward ward in SummonersRiftLoc)
                {
                    if (ward.MovePosition.IsOnScreen() && group.GetItem<Switch>($"{ward.Name} Ward Enabled").IsOn)
                    {
                        if (ward.MeOnWard)
                        {
                            RenderFactory.DrawNativeCircle(ward.MovePosition, 50, Color.Red, 2);
                            if (ward.ClickPosition.IsOnScreen())
                            {
                                if (MousePosOnScreen.Distance(ward.ClickPosition.ToW2S()) <= 15)
                                {
                                    RenderFactory.DrawNativeCircle(ward.ClickPosition, 15, Color.Red, 2);
                                    if (ward.WardPosition.IsOnScreen())
                                    {
                                        RenderFactory.DrawNativeCircle(ward.WardPosition, 50, Color.White, 2);
                                    }
                                } else
                                {
                                    RenderFactory.DrawNativeCircle(ward.ClickPosition, 15, Color.White, 2);
                                }
                            }
                           
                        } else
                        {
                            RenderFactory.DrawNativeCircle(ward.MovePosition, 50, Color.White, 2);
                        }
                    }
                }
            }
        }

        internal static Group group = new Group("Ward Helper");
        internal static Switch Enabled = new Switch("Enabled", true);
        internal static InfoDisplay Info = new InfoDisplay() { Information = "Shows ward positions for difficult wards." };

        private static void MenuInit()
        {
            Menu.Init();
            Menu.tab.AddGroup(group);
            group.AddItem(Enabled);
            group.AddItem(Info);
            foreach (Ward ward in SummonersRiftLoc)
            {
                group.AddItem(new Switch($"{ward.Name} Ward Enabled", true));
            }
        }
    }
}
