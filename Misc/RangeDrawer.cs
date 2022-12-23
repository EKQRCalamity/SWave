using Oasys.Common.EventsProvider;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Tools;
using Oasys.SDK.Rendering;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Misc
{
    internal static class RangeDrawer
    {

        internal static float QCastRange => Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q).SpellData.CastRange;
        internal static float WCastRange => Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.W).SpellData.CastRange;
        internal static float ECastRange => Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.E).SpellData.CastRange;
        internal static float RCastRange => Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.R).SpellData.CastRange;

        internal static float QSpellRange => Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q).SpellData.SpellRange;
        internal static float WSpellRange => Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.W).SpellData.SpellRange;
        internal static float ESpellRange => Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.E).SpellData.SpellRange;
        internal static float RSpellRange => Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.R).SpellData.SpellRange;

        internal static float QRange => (QCastRange >= QSpellRange) ? QCastRange : QSpellRange;
        internal static float WRange => (WCastRange >= WSpellRange) ? WCastRange : WSpellRange;
        internal static float ERange => (ECastRange >= ESpellRange) ? ECastRange : ESpellRange;
        internal static float RRange => (RCastRange >= RSpellRange)? RCastRange : RSpellRange;

        internal static Group DrawerGroup = new Group("Range Drawer");
        internal static Switch ShowQRange = new Switch("Show Q Range", true);
        internal static ModeDisplay QColor = new ModeDisplay() { Title = "Q Color", SelectedModeName = "Blue", ModeNames = ColorConverter.GetColors() };
        internal static Switch ShowWRange = new Switch("Show W Range", true);
        internal static ModeDisplay WColor = new ModeDisplay() { Title = "W Color", SelectedModeName = "Red", ModeNames = ColorConverter.GetColors() };
        internal static Switch ShowERange = new Switch("Show E Range", true);
        internal static ModeDisplay EColor = new ModeDisplay() { Title = "E Color", SelectedModeName = "Green", ModeNames = ColorConverter.GetColors() };
        internal static Switch ShowRRange = new Switch("Show R Range", true);
        internal static ModeDisplay RColor = new ModeDisplay() { Title = "R Color", SelectedModeName = "Blue", ModeNames = ColorConverter.GetColors() };
        internal static Counter Alpha = new Counter("Alpha", 200, 0, 255);
        internal static FloatCounter Thickness = new FloatCounter() { Title = "Thickness", Value = 1, MaxValue = 20, MinValue = 1};

        internal static bool QDisabled { get; set; } = false;
        internal static bool WDisabled { get; set; } = false;
        internal static bool EDisabled { get; set; } = false;
        internal static bool RDisabled { get; set; } = false;
        
        internal static void Init()
        {
            Menu.Init();
            Menu.tab.AddGroup(DrawerGroup);
            DrawerGroup.AddItem(ShowQRange);
            DrawerGroup.AddItem(QColor);
            DrawerGroup.AddItem(ShowWRange);
            DrawerGroup.AddItem(WColor);
            DrawerGroup.AddItem(ShowERange);
            DrawerGroup.AddItem(EColor);
            DrawerGroup.AddItem(ShowRRange);
            DrawerGroup.AddItem(RColor);
            DrawerGroup.AddItem(Alpha);
            DrawerGroup.AddItem(Thickness);
            CoreEvents.OnCoreRender += Draw;
        }

        private static void Draw()
        {
            // Q Drawings
            if (!QDisabled && ShowQRange.IsOn && QRange > 75 && Env.QLevel >= 1) {
                Color color = ColorConverter.GetColorWithAlpha(ColorConverter.GetColor(QColor.SelectedModeName), Alpha.Value);
                RenderFactory.DrawNativeCircle(Env.Me().Position, QRange, color, Thickness.Value);
            }
            if (!WDisabled && ShowWRange.IsOn && WRange > 75 && Env.WLevel >= 1)
            {
                Color color = ColorConverter.GetColorWithAlpha(ColorConverter.GetColor(WColor.SelectedModeName), Alpha.Value);
                RenderFactory.DrawNativeCircle(Env.Me().Position, WRange, color, Thickness.Value);
            }
            if (!EDisabled && ShowERange.IsOn && ERange > 75 && Env.ELevel >= 1)
            {
                Color color = ColorConverter.GetColorWithAlpha(ColorConverter.GetColor(EColor.SelectedModeName), Alpha.Value);
                RenderFactory.DrawNativeCircle(Env.Me().Position, ERange, color, Thickness.Value);
            }
            if (!RDisabled && ShowRRange.IsOn && RRange > 75 && Env.RLevel >= 1)
            {
                Color color = ColorConverter.GetColorWithAlpha(ColorConverter.GetColor(RColor.SelectedModeName), Alpha.Value);
                RenderFactory.DrawNativeCircle(Env.Me().Position, RRange, color, Thickness.Value);
            }

        }
    }
}
