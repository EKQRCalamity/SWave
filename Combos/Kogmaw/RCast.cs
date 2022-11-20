using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using Oasys.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Extensions;
using SyncWave.Common.Extensions;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.GameObject;

namespace SyncWave.Combos.Kogmaw
{
    internal class RCast : Base.Combo
    {
        internal override string Name => "RCast";
        internal override int MinMana => Champions.Kogmaw.RManaCost[int.Parse($"{RStacks}")];
        internal override Common.Helper.Selectors.TargetSelector TargetSelector => new Common.Helper.Selectors.TargetSelector(MinRange);
        internal float RStacks => rStacks();
                                
        internal float rStacks()
        {
            List<BuffEntry> ActiveBuffs = UnitManager.MyChampion.BuffManager.ActiveBuffs.deepCopy();
            float buff = ActiveBuffs.FirstOrDefault(x => x.IsActive && x.Name.Equals("kogmawlivingartillerycost", StringComparison.OrdinalIgnoreCase))?.Stacks ?? 0;
            return buff;
        }
        internal override int MinRange => Champions.Kogmaw.RRange[Env.RLevel];

        internal override float Damage => GetFullDamageRaw();

        internal override float GetFullDamageRaw()
        {
            float Damage = 0;
            return Damage;
        }

        internal override float GetDamage(GameObjectBase target)
        {
            float damage = 0;
            if (TargetSelector.TargetInRange(target) && Env.RLevel >= 1)
            {

                if (target.HealthPercent < 40)
                {
                    float magicDamage = Champions.Kogmaw.RDamageEnhanced[Env.RLevel] + (Env.Me().UnitStats.BonusAttackDamage * Champions.Kogmaw.RADScalingEnhanced) + (Env.Me().UnitStats.TotalAbilityPower * Champions.Kogmaw.RAPScalingEnhanced);
                    damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, magicDamage, 0);
                }
                else
                {
                    float magicDamage = Champions.Kogmaw.RDamage[Env.RLevel] + (Env.Me().UnitStats.BonusAttackDamage * Champions.Kogmaw.RADScaling) + (Env.Me().UnitStats.TotalAbilityPower * Champions.Kogmaw.RAPScaling);
                    damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, magicDamage, 0);
                }
            }
            return damage;
        }

        internal override bool Enabled()
        {
            if (MenuManagerProvider.GetTab(Champions.Kogmaw.TabIndex).GetGroup(Champions.Kogmaw.AbilityGroupIndex).GetItem<Switch>(Champions.Kogmaw.AbilityRIndex).IsOn)
                return true;
            return false;
        }

        internal override bool CanKill(GameObjectBase target)
        {
            if (target.DistanceToPlayer() >= MinRange)
                return false;
            float actualDamage = GetDamage(target);
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
            if (Env.Spells.GetSpellClass(SpellSlot.R).IsSpellReady && Enabled() && EnoughMana())
                return true;
            return false;
        }

        internal override bool Run()
        {
            Group group = MenuManagerProvider.GetTab(Champions.Kogmaw.TabIndex).GetGroup(Champions.Kogmaw.AbilityGroupIndex);
            string selectedMode = group.GetItem<ModeDisplay>(Champions.Kogmaw.AbilityRCastModeIndex).SelectedModeName;
            if (selectedMode == "Under X Health")
            {
                GameObjectBase? LowestHealthEnemyInRange = TargetSelector.GetLowestHealthTarget();
                if (LowestHealthEnemyInRange != null)
                {
                    if (LowestHealthEnemyInRange.Health <= group.GetItem<Counter>(Champions.Kogmaw.XHealthRIndex).Value)
                    {
                        return Run(LowestHealthEnemyInRange);
                    }
                }
            } else if (selectedMode == "Under X Health%")
            {
                GameObjectBase? LowestHealthEnemyInRange = TargetSelector.GetLowestHealthTarget();
                if (LowestHealthEnemyInRange != null)
                {
                    if (LowestHealthEnemyInRange.HealthPercent <= group.GetItem<Counter>(Champions.Kogmaw.XHealthRIndex).Value)
                    {
                        return Run(LowestHealthEnemyInRange);
                    }
                }
            } else if (selectedMode == "Can Kill")
            {
                GameObjectBase? LowestHealthEnemyInRange = TargetSelector.GetLowestHealthTarget();
                if (LowestHealthEnemyInRange != null)
                {
                    if (CanKill(LowestHealthEnemyInRange))
                    {
                        return Run(LowestHealthEnemyInRange);
                    }
                }
            } 
            else if (selectedMode == "Mixed")
            {
                GameObjectBase? LowestHealthEnemyInRange = TargetSelector.GetLowestHealthTarget();
                if (LowestHealthEnemyInRange != null)
                {
                    if (CanKill(LowestHealthEnemyInRange))
                    {
                        return Run(LowestHealthEnemyInRange);
                    } else if (LowestHealthEnemyInRange.HealthPercent <= group.GetItem<Counter>(Champions.Kogmaw.XHealthRIndex).Value)
                    {
                        return Run(LowestHealthEnemyInRange);
                    }
                }
            }
            
            return false;
        }

        internal Prediction.MenuSelected.PredictionOutput Predict(GameObjectBase target)
        {
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Circle, target, MinRange, Champions.Kogmaw.RRadius, Champions.Kogmaw.RCastTime + 0.6F, 1500, false);
        }

        internal static bool UnderXHealth(float n)
        {
            return n <= MenuManagerProvider.GetTab(Champions.Kogmaw.TabIndex).GetGroup(Champions.Kogmaw.AbilityGroupIndex).GetItem<Counter>(Champions.Kogmaw.XHealthRIndex).Value;
        }

        internal override bool Run(GameObjectBase target)
        {
            bool spellCasted = false;
            Group group = MenuManagerProvider.GetTab(Champions.Kogmaw.TabIndex).GetGroup(Champions.Kogmaw.AbilityGroupIndex);
            string selectedMode = group.GetItem<ModeDisplay>(Champions.Kogmaw.AbilityRCastModeIndex).SelectedModeName;
            bool cast = (selectedMode == "Can Kill") ? CanKill(target) :
                        (selectedMode == "Under X Health%") ? UnderXHealth(target.HealthPercent) :
                        (selectedMode == "Under X Health") ? UnderXHealth(target.Health) :
                        (selectedMode == "Mixed") ? (CanKill(target) || UnderXHealth(target.HealthPercent)) : false;
            if (cast && Enabled() && target.DistanceToPlayer() <= MinRange && Env.Spells.GetSpellClass(SpellSlot.R).IsSpellReady && EnoughMana() && target.IsVisible && target.IsAlive && target.IsTargetable)
            {
                Prediction.MenuSelected.PredictionOutput RPrediction = Predict(target);
                if (RPrediction.HitChance >= Prediction.MenuSelected.HitChance.High)
                {
                    SpellCastProvider.CastSpell(CastSlot.R, RPrediction.CastPosition, Champions.Kogmaw.RCastTime);
                    spellCasted = true;
                }
            }
            return spellCasted;
        }
    }
}
