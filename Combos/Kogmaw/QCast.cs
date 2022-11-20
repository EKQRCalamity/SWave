using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using System.Collections.Generic;

namespace SyncWave.Combos.Kogmaw
{
    internal class QCast : Base.Combo
    {
        internal override string Name => "QCast";
        internal override int MinMana => Champions.Kogmaw.QManaCost[Env.QLevel];
        internal override int MinRange => Champions.Kogmaw.QRange;
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);
        internal override float Damage => GetFullDamageRaw();
        internal override float GetFullDamageRaw()
        {
            if (Env.QLevel >= 1)
                return (Champions.Kogmaw.QDamage[Env.QLevel] + (Env.Me().UnitStats.TotalAbilityPower * Champions.Kogmaw.QScaling));
            return 0;
        }
        internal override float GetDamage(GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
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
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthTarget();
            if (Enemy != null)
            {
                return CanKill(Enemy);
            }
            return false;
        }

        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Kogmaw.TabIndex).GetGroup(Champions.Kogmaw.AbilityGroupIndex).GetItem<Switch>(Champions.Kogmaw.AbilityQIndex).IsOn)
                return true;
            return false;
        }


        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.Q).IsSpellReady && Enabled() && EnoughMana())
                return true;
            return false;
        }

        internal override bool Run()
        {
            List<GameObjectBase> enemiesInRange = TargetSelector.GetTargetsInRange();
            foreach (GameObjectBase enemy in enemiesInRange)
            {
                if (!enemy.IsAlive)
                    continue;
                if (CanHit(enemy) && enemy.IsVisible)
                {
                    Champions.Kogmaw.currentCombo = this;
                    Champions.Kogmaw.currentTarget = enemy;
                    return Run(enemy);
                }
                else
                    continue;
            }
            Champions.Kogmaw.currentTarget = null;
            Champions.Kogmaw.currentCombo = null;
            return false;
        }

        internal bool CanHit(GameObjectBase target)
        {
            Prediction.MenuSelected.PredictionOutput prediction = Predict(target);
            if (prediction.Collision)
                return false;
            return true;
        }

        internal Prediction.MenuSelected.PredictionOutput Predict(GameObjectBase target)
        {
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, Champions.Kogmaw.QRange, Champions.Kogmaw.QWidth, Champions.Kogmaw.QCastTime, Champions.Kogmaw.QSpeed, true);
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;
            if (SpellsReady() && target.IsVisible && target.IsTargetable)
            {
                Prediction.MenuSelected.PredictionOutput pred = Predict(target);
                if (!pred.Collision && pred.HitChance >= Prediction.MenuSelected.HitChance.High)
                {
                    SpellCastProvider.CastSpell(CastSlot.Q, pred.CastPosition, Champions.Kogmaw.QCastTime);
                    spellCasted = true;
                }
            }
            return spellCasted;
        }
    }
}
