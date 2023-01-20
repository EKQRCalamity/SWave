using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.SpellCasting;
using SyncWave.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Spells
{
    internal abstract class SpellCastBase
    {
        internal CastSlot CastSlot { get; set; }
        internal SpellSlot SpellSlot { get; set; }
        internal bool MainInput { get; set; } = true;
        internal bool Harass { get; set; } = false;
        internal bool LastHit { get; set; } = false;
        internal bool Push { get; set; } = false;
        internal Tab MainTab { get; set; }
        internal Group SpellGroup { get; set; }
        internal Switch IsOn { get; set; }
        internal Counter MinMana { get; set; }
        internal int Range { get; set; }
        internal DamageCalculation EffectCalculator { get; set; }
        internal Func<GameObjectBase, bool> TargetSelector { get; set; }
        internal bool TargetInRange(GameObjectBase target)
        {
            return target.Distance < Range;
        }
        internal bool SpellIsReady()
        {
            if (Env.Me().GetSpellBook().GetSpellClass(SpellSlot).IsSpellReady)
            {
                return true;
            }
            return false;
        }
        internal Oasys.SDK.Prediction.MenuSelected.HitChance GetHitchanceFromName(string name)
        {
            return name.ToLower() switch
            {
                "immobile" => Oasys.SDK.Prediction.MenuSelected.HitChance.Immobile,
                "veryhigh" => Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                "high" => Oasys.SDK.Prediction.MenuSelected.HitChance.High,
                "medium" => Oasys.SDK.Prediction.MenuSelected.HitChance.Medium,
                "low" => Oasys.SDK.Prediction.MenuSelected.HitChance.Low,
                "dashing" => Oasys.SDK.Prediction.MenuSelected.HitChance.Dashing,
                "outofrange" => Oasys.SDK.Prediction.MenuSelected.HitChance.OutOfRange,
                "unknown" => Oasys.SDK.Prediction.MenuSelected.HitChance.Unknown,
                _ => Oasys.SDK.Prediction.MenuSelected.HitChance.Impossible
            };
        }
    }
}
