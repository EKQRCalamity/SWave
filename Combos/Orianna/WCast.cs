using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace SyncWave.Combos.Orianna
{
    internal class WCast : Base.Combo
    {
        internal override string Name => "WCast";
        internal override int MinMana => Champions.Orianna.WManaCost[Env.WLevel];
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);

        internal override int MinRange => 1290;

        internal override float Damage => GetFullDamageRaw();

        internal override float GetFullDamageRaw()
        {
            float Damage = 0;
            if (Env.Spells.GetSpellClass(SpellSlot.W).IsSpellReady)
            {
                Damage = Champions.Orianna.WDamage[Env.WLevel] + (Env.Me().UnitStats.TotalAbilityPower * Champions.Orianna.WDamageScaling);
            }
            return Damage;
        }
        internal override float GetDamage(GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
        }
        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Orianna.TabIndex).GetGroup(Champions.Orianna.GroupCombosIndex).GetItem<Switch>(Champions.Orianna.GeneralComboIndex).IsOn)
                return true;
            return false;
        }

        internal override bool CanKill(GameObjectBase target)
        {
            if (target.DistanceToPlayer() >= MinRange)
                return false;
            float actualDamage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
            //Logger.Log($"W can kill: {target.ModelName} | {(target.Health - actualDamage) <= 0}");
            return (target.Health - actualDamage) <= 0;
        }

        internal override bool CanKill()
        {
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthPrioTarget(Champions.Orianna.QPosition(), Champions.Orianna.WRadius);
            if (Enemy != null)
            {
                return CanKill(Enemy);
            }
            return false;
        }

        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.W).IsSpellReady)
                return true;
            return false;
        }

        internal override bool Run()
        {
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthPrioTarget(Champions.Orianna.QPosition(), Champions.Orianna.WRadius);
            if (Enemy != null)
            {
                Champions.Orianna.currentCombo = this;
                Champions.Orianna.currentTarget = Enemy;
                return Run(Enemy);
            }
            Champions.Orianna.currentTarget = null;
            Champions.Orianna.currentCombo = null;
            return false;
        }

        internal Prediction.MenuSelected.PredictionOutput Predict(GameObjectBase target)
        {
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Circle, target, 0, Champions.Orianna.WRadius, 0F, 999999);
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;
            if (Enabled() && target.DistanceTo(Champions.Orianna.QPosition()) <= 225 && Env.Spells.GetSpellClass(SpellSlot.W).IsSpellReady && EnoughMana() && target.IsVisible && target.IsAlive && target.IsTargetable)
            {
                Prediction.MenuSelected.PredictionOutput WPrediction = Predict(target);

                if (WPrediction.HitChance >= Prediction.MenuSelected.HitChance.High)
                {
                    SpellCastProvider.CastSpell(CastSlot.W);
                    spellCasted = true;
                }
            }
            return spellCasted;
        }
    }
}