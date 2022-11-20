using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Tools;
using Oasys.SDK;
using Oasys.SDK.InputProviders;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Common.Extensions;

namespace SyncWave.Combos.Orianna
{
    internal class ECast : Base.Combo
    {
        internal override string Name => "ECast";
        internal override int MinMana => Champions.Orianna.EManaCost[Env.ELevel];
        internal override int MinRange => Champions.Orianna.LeashRange;
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);
        internal override float Damage => GetFullDamageRaw();
        internal override float GetFullDamageRaw()
        {
            float Damage = 0;
            if (Env.Spells.GetSpellClass(SpellSlot.E).IsSpellReady)
            {
                Damage = Champions.Orianna.EDamage[Env.ELevel] + (Env.Me().UnitStats.TotalAbilityPower * Champions.Orianna.EDamageScaling);
            }
            return Damage;
        }

        internal override float GetDamage(GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, GetFullDamageRaw(), 0);
        }

        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Orianna.TabIndex).GetGroup(Champions.Orianna.AbilityGroupIndex).GetItem<Switch>(Champions.Orianna.AbilityEIndex).IsOn)
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
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthTarget();
            if (Enemy != null)
            {
                return CanKill(Enemy);
            }
            return false;
        }

        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.E).IsSpellReady)
                return true;
            return false;
        }

        internal override bool Run()
        {
            List<GameObjectBase> enemies = TargetSelector.GetTargetsInRange().deepCopy();
            bool hasRun = false;
            foreach (GameObjectBase enemy in enemies)
            {
                Champions.Orianna.currentTarget = enemy;
                Champions.Orianna.currentCombo = this;
                if (Run(enemy))
                    break;
            }
            Champions.Orianna.currentTarget = null;
            Champions.Orianna.currentCombo = null;
            return false;
        }

        internal Prediction.MenuSelected.PredictionOutput Predict(GameObjectBase target)
        {
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, Env.Me(), Champions.Orianna.ERange, Champions.Orianna.EWidth, 0.0F, Champions.Orianna.ESpeed, Champions.Orianna.QPosition(), true);
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;
            if (Enabled() && TargetSelector.TargetInRange(target) && Env.Spells.GetSpellClass(SpellSlot.E).IsSpellReady && EnoughMana() && target.IsVisible && target.IsAlive && target.IsTargetable)
            {
                Vector2 in1 = Vector2.Zero;
                Vector2 in2 = Vector2.Zero;
                int intersections = Oasys.Common.Logic.Geometry.LineCircleIntersection(target.Position.ToW2S().X, target.Position.ToW2S().Y, target.UnitComponentInfo.UnitBoundingRadius, Champions.Orianna.QPosition().ToW2S(), Env.Me().Position.ToW2S(), out in1, out in2);
                bool isLineCollision = (intersections >= 1);
                if (isLineCollision)
                {
                    SpellCastProvider.CastSpell(CastSlot.E, Env.Me().Position.ToW2S());
                    spellCasted = true;
                }
            }
            return spellCasted;
        }
    }
}
