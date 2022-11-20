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
    internal class QCast : Base.Combo
    {
        internal override string Name => "QCast";
        internal override int MinMana => Champions.Orianna.QManaCost[Env.QLevel];

        internal override int MinRange => Champions.Orianna.QRange - 50;
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new(MinRange);
        internal override float Damage => GetFullDamageRaw();

        internal override float GetFullDamageRaw()
        {
            float Damage = 0;
            if (Env.Spells.GetSpellClass(SpellSlot.Q).IsSpellReady)
            {
                Damage = Champions.Orianna.QDamage[Env.QLevel] + (Env.Me().UnitStats.TotalAbilityPower * Champions.Orianna.QDamageScaling);
            }
            return Damage;
        }

        internal override float GetDamage(GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
        }

        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Orianna.TabIndex).GetGroup(Champions.Orianna.AbilityGroupIndex).GetItem<Switch>(Champions.Orianna.AbilityQIndex).IsOn)
                return true;
            return false;
        }

        internal override bool CanKill(GameObjectBase target)
        {
            if (target.DistanceToPlayer() >= MinRange)
                return false;
            float actualDamage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
            //Logger.Log($"Q can kill: {target.ModelName} | {(target.Health - actualDamage) <= 0}");
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
            if (Env.Spells.GetSpellClass(SpellSlot.Q).IsSpellReady)
                return true;
            return false;
        }

        internal override bool Run()
        {
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthPrioTarget();
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
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, Champions.Orianna.QRange, Champions.Orianna.QWidth, 0.1F, Champions.Orianna.QSpeed, true);
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;
            if (Enabled())
            {
                if (TargetSelector.TargetInRange(target) && Env.Spells.GetSpellClass(SpellSlot.Q).IsSpellReady && EnoughMana() && target.IsVisible && target.IsAlive && target.IsTargetable)
                {
                    Prediction.MenuSelected.PredictionOutput QLinePrediction = Predict(target);
                    if (QLinePrediction.HitChance >= Prediction.MenuSelected.HitChance.High)
                    {
                        SpellCastProvider.CastSpell(CastSlot.Q, QLinePrediction.CastPosition.Extend(Champions.Orianna.QPosition(), -50));
                        spellCasted = true;
                    }
                }
            }
            return spellCasted;
        }
    }
}