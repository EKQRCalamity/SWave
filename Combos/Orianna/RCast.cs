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
    internal class RCast : Base.Combo
    {
        internal override string Name => "RCast";
        internal override int MinMana => Champions.Orianna.RManaCost[Env.RLevel];

        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new(MinRange);
        internal override int MinRange => 1290;

        internal override float Damage => GetFullDamageRaw();

        internal override float GetFullDamageRaw()
        {
            float Damage = 0;
            if (Env.Spells.GetSpellClass(SpellSlot.R).IsSpellReady)
            {
                Damage = Champions.Orianna.RDamage[Env.RLevel] + (Env.Me().UnitStats.TotalAbilityPower * Champions.Orianna.RDamageScaling);
            }
            return Damage;
        }

        internal override float GetDamage(GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
        }

        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Orianna.TabIndex).GetGroup(Champions.Orianna.AbilityGroupIndex).GetItem<Switch>(Champions.Orianna.AbilityRIndex).IsOn)
                return true;
            return false;
        }

        internal override bool CanKill(GameObjectBase target)
        {
            if (target.DistanceToPlayer() >= MinRange)
                return false;
            float actualDamage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
            //Logger.Log($"R can kill: {target.ModelName} | {(target.Health - actualDamage) <= 0}");
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

        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.R).IsSpellReady)
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
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Circle, target, 0, Champions.Orianna.RRadius, 0.5F, 999999, true);
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;
            if (Enabled() && target.DistanceTo(Champions.Orianna.QPosition()) <= 400 && Env.Spells.GetSpellClass(SpellSlot.R).IsSpellReady && EnoughMana() && target.IsVisible && target.IsAlive && target.IsTargetable)
            {
                Prediction.MenuSelected.PredictionOutput RPrediction = Predict(target);

                int n = MenuManagerProvider.GetTab(Champions.Orianna.TabIndex).GetGroup(Champions.Orianna.AbilityGroupIndex).GetItem<Counter>(Champions.Orianna.AbilityRMinEnemiesHit).Value;
                bool canKill = CanKill(target);
                if (canKill)
                {
                    //Logger.Log("R can kill! => cast");
                    SpellCastProvider.CastSpell(CastSlot.R);
                    spellCasted = true;
                } else if (RPrediction.CollisionObjects.Count >= n || TargetSelector.XTargetsInRange(n, Champions.Orianna.QPosition(), Champions.Orianna.RRadius))
                {
                    SpellCastProvider.CastSpell(CastSlot.R);
                    spellCasted = true;
                }
            }
            return spellCasted;
        }
    }
}
