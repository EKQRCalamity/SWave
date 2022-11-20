using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace SyncWave.Combos.Kogmaw
{
    internal class ECast : Base.Combo
    {
        internal override string Name => "ECast";
        internal override int MinMana => Champions.Kogmaw.EManaCost[Env.ELevel];
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);
        internal override int MinRange => Champions.Kogmaw.ERange;
        internal override float Damage => GetFullDamageRaw();
        internal override float GetFullDamageRaw()
        {
            if (Env.ELevel >= 1)
                return (Champions.Kogmaw.EDamage[Env.ELevel] + (Env.Me().UnitStats.TotalAbilityPower * Champions.Kogmaw.EScaling));
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
            GameObjectBase? Enemy = TargetSelector.GetLowestHealthPrioTarget();
            if (Enemy != null)
            {
                return CanKill(Enemy);
            }
            return false;
        }

        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Kogmaw.TabIndex).GetGroup(Champions.Kogmaw.AbilityGroupIndex).GetItem<Switch>(Champions.Kogmaw.AbilityEIndex).IsOn)
                return true;
            return false;
        }


        internal override bool SpellsReady()
        {
            if (Env.Spells.GetSpellClass(SpellSlot.E).IsSpellReady && Enabled())
                return true;
            return false;
        }

        internal override bool Run()
        {
            GameObjectBase? enemy = TargetSelector.GetLowestHealthPrioTarget();
            if (enemy != null)
            {
                Champions.Kogmaw.currentTarget = enemy;
                Run(enemy);
            }
            Champions.Kogmaw.currentTarget = null;
            return false;
        }


        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;
            if (SpellsReady())
            {
                if (TargetSelector.TargetInRange(target) && target.IsVisible && target.IsTargetable && target.IsAlive)
                {
                    Prediction.MenuSelected.PredictionOutput pred = Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, Champions.Kogmaw.ERange, Champions.Kogmaw.EWidth, Champions.Kogmaw.ECastTime, Champions.Kogmaw.ESpeed, false);
                    if (pred.HitChance >= Prediction.MenuSelected.HitChance.High)
                    {
                        SpellCastProvider.CastSpell(CastSlot.E, pred.CastPosition, Champions.Kogmaw.ECastTime);
                        spellCasted = true;
                    }
                }
            }
            return spellCasted;
        }
    }
}
