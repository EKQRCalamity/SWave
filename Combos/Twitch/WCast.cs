using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Settings;
using Oasys.Common.Tools;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;

namespace SyncWave.Combos.Twitch
{
    internal class WCast : Base.Combo
    {
        internal override string Name => "WCast";
        internal override int MinMana => Champions.Twitch.WManaCost[Env.WLevel];

        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);
        internal override int MinRange => Champions.Twitch.WRange;

        internal override float Damage => GetFullDamageRaw();

        internal override float GetFullDamageRaw()
        {
            return 0F;
        }

        internal override float GetDamage(GameObjectBase target)
        {
            return 0F;
        }

        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Twitch.TabIndex).GetGroup(Champions.Twitch.AbilityGroupIndex).GetItem<Switch>(Champions.Twitch.AbilityWIndex).IsOn)
                return true;
            return false;
        }

        internal override bool CanKill(GameObjectBase target)
        {
            if (target.DistanceToPlayer() >= MinRange)
                return false;
            float actualDamage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
            return (target.Health - actualDamage) <= 0;
        }

        internal override bool CanKill()
        {
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthPrioTarget();
            if (Enemy != null)
            {
                return CanKill(Enemy);
            }
            return false;
        }

        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.W).IsSpellReady && Enabled())
                return true;
            return false;
        }

        internal override bool Run()
        {
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthPrioTarget();
            if (Enemy != null)
            {
                return Run(Enemy);
            }
            return false;
        }

        internal override bool Run(GameObjectBase target)
        {
            if (SpellsReady() && target.IsAlive && target.IsVisible && Enabled() && target.IsTargetable)
            {
                Prediction.MenuSelected.PredictionOutput WPred = Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, Champions.Twitch.WRange, Champions.Twitch.WRadius, 0, Champions.Twitch.WSpeed, true);
                
                if (WPred.HitChance >= Prediction.MenuSelected.HitChance.High)
                {
                    SpellCastProvider.CastSpell(CastSlot.W, WPred.CastPosition);
                }
            }
            return false;
        }
    }
}