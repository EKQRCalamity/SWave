using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Menu;
using Oasys.SDK.SpellCasting;
using SyncWave.Champions;
using SyncWave.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Combos.Tahmkench
{
    internal class QCast : Base.Combo
    {
        internal override string Name => "QCast";
        internal override int MinMana => Champions.Tahmkench.QManaCost[Env.QLevel];
        internal override int MinRange => Champions.Tahmkench.QRange;
        internal override float Damage => GetFullDamageRaw();
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);

        internal float Healing => GetFullHealing();
        internal override float GetFullDamageRaw()
        {
            if (Env.QLevel >= 1)
                return Champions.Tahmkench.QDamage[Env.QLevel] + Champions.Tahmkench.PassiveExtraDamage[Env.Level] + (Env.Me().UnitStats.TotalAbilityPower * Champions.Tahmkench.QScaling);
            return 0;
        }
        internal float GetFullHealing()
        {
            if (!SpellsReady())
                return 0;
            float healing = Champions.Tahmkench.QHeal[Env.QLevel] + ((Env.Me().MaxHealth - Env.Me().Health) * Champions.Tahmkench.QHealMissingHealth[Env.QLevel]);
            if (Env.Me().Health + healing > Env.Me().MaxHealth)
                return Env.Me().MaxHealth - Env.Me().Health;
            else
                return healing;
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
            if (MenuManager.GetTab(Champions.Tahmkench.TabIndex).GetGroup(Champions.Tahmkench.AbilityGroupIndex).GetItem<Switch>(Champions.Tahmkench.AbilityQIndex).IsOn)
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
            GameObjectBase? target = TargetSelector.GetLowestHealthPrioTarget();
            if (target != null)
            {
                return Run(target);
            }
            return false;
        }

        internal bool EnoughStacks(GameObjectBase enemy)
        {
            Champions.Tahmkench tahm = new();
            return (tahm.GetPStack(enemy) == 0)? true : (tahm.GetPStack(enemy) >= 3)? true : false;
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;
            if (EnoughStacks(target) && SpellsReady() && target.IsAlive && target.IsVisible && target.IsValidTarget(MinRange, true) && target.IsTargetable)
            {
                Prediction.MenuSelected.PredictionOutput pred = Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, MinRange, Champions.Tahmkench.QWidth, Champions.Tahmkench.QCastTime, Champions.Tahmkench.QSpeed, true);
                if (!pred.Collision && pred.HitChance >= Prediction.MenuSelected.HitChance.High)
                {
                    SpellCastProvider.CastSpell(CastSlot.Q, pred.CastPosition, Champions.Tahmkench.QCastTime);
                    spellCasted = true;
                }
            }
            return spellCasted;
        }
    }
}
