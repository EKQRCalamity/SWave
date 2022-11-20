using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Menu;
using Oasys.SDK.SpellCasting;
using SyncWave.Common.Extensions;

namespace SyncWave.Combos.Tahmkench
{
    internal class WCast : Base.Combo
    {
        internal override string Name => "WCast";
        internal override int MinMana => Champions.Tahmkench.WManaCost[Env.WLevel];
        internal override int MinRange => Champions.Tahmkench.WRange[Env.WLevel];

        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);
        internal override float Damage => GetFullDamageRaw();
        internal override float GetFullDamageRaw()
        {
            return Champions.Tahmkench.WDamage[Env.WLevel] + Champions.Tahmkench.PassiveExtraDamage[Env.Level] + (Env.Me().UnitStats.TotalAbilityPower * Champions.Tahmkench.WScaling);
        }
        internal override float GetDamage(GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
        }
        internal override bool CanKill(GameObjectBase target)
        {
            if (target.DistanceToPlayer() >= MinRange)
                return false;
            float actualDamage = GetDamage(target);
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

        internal override bool Enabled()
        {
            if (MenuManager.GetTab(Champions.Tahmkench.TabIndex).GetGroup(Champions.Tahmkench.AbilityGroupIndex).GetItem<Switch>(Champions.Tahmkench.AbilityWIndex).IsOn)
                return true;
            return false;
        }
        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.W).IsSpellReady && Enabled() && EnoughMana())
                return true;
            return false;
        }
        internal override bool Run()
        {
            GameObjectBase? target = TargetSelector.GetLowestHealthPrioTarget();
            if (target != null)
            {
                return Run(target);
            }
            return false;
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;
            if (Champions.Tahmkench.KeyPressed && SpellsReady() && target.IsAlive && target.IsVisible && target.IsValidTarget(MinRange, true) && target.IsTargetable)
            {
                Prediction.MenuSelected.PredictionOutput pred = Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Circle, target, MinRange, Champions.Tahmkench.WWidth, Champions.Tahmkench.WCastTime + Champions.Tahmkench.WExtraDelay, 99999, false);
                if (pred.HitChance >= Prediction.MenuSelected.HitChance.High)
                {
                    SpellCastProvider.CastSpell(CastSlot.W, pred.CastPosition, Champions.Tahmkench.WCastTime + Champions.Tahmkench.WExtraDelay);
                    spellCasted = true;
                }
            }
            return spellCasted;
        }
    }
}
