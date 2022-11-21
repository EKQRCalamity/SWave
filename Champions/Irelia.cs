using Microsoft.VisualBasic;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Events;
using Oasys.SDK.Menu;
using Oasys.SDK.Rendering;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Base;
using SyncWave.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    internal class Irelia : Base.Champion
    {

        internal int TickCycles = 0;
        internal Common.Helper.Selectors.TargetSelector TS = new(600);

        #region EObject
        internal GameObjectBase? EObj => ObjectCheck();

        internal GameObjectBase? __EObj = null;

        internal Vector3 originalEPos = new();

        internal List<GameObjectBase> ireliaObjects = new();

        internal GameObjectBase? GetObjectFromList()
        {
            GameObjectBase? retobj = null;
            foreach (GameObjectBase obj in UnitManager.AllNativeObjects)
            {
                if (__EObj.Name.Contains("Base_E_Blades", StringComparison.OrdinalIgnoreCase))
                {
                    retobj = obj;
                    break;
                }
            }
            return retobj;
        }

        internal GameObjectBase? ObjectCheck()
        {
            if (__EObj is null)
                return GetObjectFromList();
            if (__EObj.Name.Contains("Base_E_Blades", StringComparison.OrdinalIgnoreCase))
            {
                return __EObj;
            }
            else
            {
                //Logger.Log(__EObj.Name);
                __EObj = null;
                ireliaObjects = new();
                return null;
            }
        }
        #endregion

        #region Stats
        internal static float[] PassiveBonusDamageOnHit = new float[] { 10, 13, 16, 19, 22, 25, 28, 31, 34, 37, 40, 43, 46, 49, 52, 55, 58, 61 };
        internal static float PassiveADScaling = 0.2F;

        internal static int[] QManaCost = new int[] { 0, 20, 20, 20, 20, 20 };
        internal static int[] QDamage = new int[] { 0, 5, 25, 45, 65, 85 };
        internal static int[] QExtraMinionDamage = new int[] { 55, 67, 79, 91, 103, 115, 127, 139, 151, 163, 175, 187, 199, 211, 223, 235, 247, 259 };
        internal static float[] QHealPercent = new float[] { 0, 0.09F, 0.10F, 0.11F, 0.12F, 0.13F };
        internal static float QADScaling = 0.6F;
        internal static int QTargetRange = 600;
        internal static float QSpeed = 1400 + Env.Me().UnitStats.MoveSpeed;

        internal static int[] WManaCost = new int[] { 0, 70, 75, 80, 85, 90 };
        internal static int[] WDamage = new int[] { 0, 10, 25, 40, 55, 70 };
        internal static float WADScaling = 0.4F;
        internal static float WAPScaling = 0.4F;
        internal static int WMissileRange = 775;
        internal static int WRange = 895;
        internal static int WEffectRadiusPB = 300;
        internal static int WEffectRadiusLR = 120;
        internal static int WWidth = 240;

        internal static int[] EManaCost = new int[] { 0, 50, 50, 50, 50, 50 };
        internal static int[] EDamage = new int[] { 0, 80, 125, 170, 215, 260 };
        internal static float EAPScaling = 0.8F;
        internal static float EDelay = 0.15F;
        internal static int ESpeed = 2000;
        internal static int EWidth = 80;
        internal static int ERange = 775;

        internal static int[] RManaCost = new int[] { 0, 100, 100, 100 };
        internal static int[] RDamage = new int[] { 0, 125, 250, 375 };
        internal static float RAPScaling = 0.7F;
        internal static float RCastTime = 0.4F;
        internal static int RWidth = 320;
        internal static int RSpeed = 2000;
        internal static int RRange = 1000;
        #endregion

        #region Menu
        internal static int TabIndex = -1;
        internal static int EnabledIndex = -1;
        internal static int AbilityQGroupIndex = -1;
        internal static int AbilityQIndex = -1;
        internal static int AbilityQSkipCanResetIndex = -1;
        internal static int AbilityQLaneClearIndex = -1;
        internal static int AbilityQMaxStacksIndex = -1;
        internal static int AbilityQUnderTowerIndex = -1;
        internal static int AbilityEGroupIndex = -1;
        internal static int AbilityEIndex = -1;
        internal static int AbilityECastModeIndex = -1;
        internal static int AbilityEHitChanceIndex = -1;
        internal static int AbilityRGroupIndex = -1;
        internal static int AbilityRIndex = -1;
        internal static int AbilityRCastModeIndex = -1;
        internal static int AbilityRHitChanceIndex = -1;
        internal static int AbilityRHPPercentIndex = -1;
        internal static int AbilityRMinHitsIndex = -1;

        internal static bool Enabled => MenuManager.GetTab(TabIndex).GetItem<Switch>(EnabledIndex).IsOn;
        
        internal static Prediction.MenuSelected.HitChance GetHitchanceFromName(string name)
        {
            return name.ToLower() switch {
                "immobile" => Prediction.MenuSelected.HitChance.Immobile,
                "veryhigh" => Prediction.MenuSelected.HitChance.VeryHigh,
                "high" => Prediction.MenuSelected.HitChance.High,
                "medium" => Prediction.MenuSelected.HitChance.Medium,
                "low" => Prediction.MenuSelected.HitChance.Low,
                "dashing" => Prediction.MenuSelected.HitChance.Dashing,
                "outofrange" => Prediction.MenuSelected.HitChance.OutOfRange,
                "unknown" => Prediction.MenuSelected.HitChance.Unknown,
                _ => Prediction.MenuSelected.HitChance.Impossible
            };
        }

        internal static bool QEnabled => MenuManager.GetTab(TabIndex).GetGroup(AbilityQGroupIndex).GetItem<Switch>(AbilityQIndex).IsOn;
        internal static bool QSkipCheck => MenuManager.GetTab(TabIndex).GetGroup(AbilityQGroupIndex).GetItem<Switch>(AbilityQSkipCanResetIndex).IsOn;
        internal static bool QLaneClear => MenuManager.GetTab(TabIndex).GetGroup(AbilityQGroupIndex).GetItem<Switch>(AbilityQLaneClearIndex).IsOn;
        internal static bool QMaxStacks => MenuManager.GetTab(TabIndex).GetGroup(AbilityQGroupIndex).GetItem<Switch>(AbilityQMaxStacksIndex).IsOn;
        internal static bool QUnderTower => MenuManager.GetTab(TabIndex).GetGroup(AbilityQGroupIndex).GetItem<Switch>(AbilityQUnderTowerIndex).IsOn;
        internal static bool EEnabled => MenuManager.GetTab(TabIndex).GetGroup(AbilityEGroupIndex).GetItem<Switch>(AbilityEIndex).IsOn;
        internal static string ECastMode => MenuManager.GetTab(TabIndex).GetGroup(AbilityEGroupIndex).GetItem<ModeDisplay>(AbilityECastModeIndex).SelectedModeName;
        internal static Prediction.MenuSelected.HitChance EHitChance => GetHitchanceFromName(MenuManager.GetTab(TabIndex).GetGroup(AbilityEGroupIndex).GetItem<ModeDisplay>(AbilityEHitChanceIndex).SelectedModeName);
        internal static bool REnabled => MenuManager.GetTab(TabIndex).GetGroup(AbilityRGroupIndex).GetItem<Switch>(AbilityRIndex).IsOn;
        internal static string RCastMode => MenuManager.GetTab(TabIndex).GetGroup(AbilityRGroupIndex).GetItem<ModeDisplay>(AbilityRCastModeIndex).SelectedModeName;
        internal static Prediction.MenuSelected.HitChance RHitChance => GetHitchanceFromName(MenuManager.GetTab(TabIndex).GetGroup(AbilityRGroupIndex).GetItem<ModeDisplay>(AbilityRHitChanceIndex).SelectedModeName);
        internal static int RHPPercent => MenuManager.GetTab(TabIndex).GetGroup(AbilityRGroupIndex).GetItem<Counter>(AbilityRHPPercentIndex).Value;
        internal static int RMinChamps => MenuManager.GetTab(TabIndex).GetGroup(AbilityRGroupIndex).GetItem<Counter>(AbilityRMinHitsIndex).Value;


        internal static void SetupMenu() 
        {
            Tab tab = new Tab("SyncWave - Irelia");
            EnabledIndex = tab.AddItem(new Switch() { IsOn = true, Title = "Enabled"});
            TabIndex = MenuManagerProvider.AddTab(tab);
            Group AbilityQ = new Group("Q");
            AbilityQGroupIndex = tab.AddGroup(AbilityQ);
            AbilityQIndex = AbilityQ.AddItem(new Switch() {IsOn = true, Title = "Q Enabled"});
            AbilityQSkipCanResetIndex = AbilityQ.AddItem(new Switch() { IsOn = false, Title = "Q Skip Reset Check" });
            AbilityQLaneClearIndex = AbilityQ.AddItem(new Switch() { IsOn = true, Title = "Q Use Laneclear"});
            AbilityQMaxStacksIndex = AbilityQ.AddItem(new Switch() { IsOn = true, Title = "Q In Combo w/ MaxStacks"});
            AbilityQUnderTowerIndex = AbilityQ.AddItem(new Switch() { IsOn = false, Title = "Q Under Tower" });
            Group AbilityE = new Group("E");
            AbilityEGroupIndex = tab.AddGroup(AbilityE);
            AbilityEIndex = AbilityE.AddItem(new Switch() {IsOn = true, Title = "E Enabled"});
            AbilityECastModeIndex = AbilityE.AddItem(new ModeDisplay() { Title = "E Cast Mode", SelectedModeName = "SemiAuto", ModeNames = new() { "SemiAuto" } });
            AbilityEHitChanceIndex = AbilityE.AddItem(new ModeDisplay() { Title = "E HitChance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" });
            Group AbilityR = new Group("R");
            AbilityRGroupIndex = tab.AddGroup(AbilityR);
            AbilityRIndex = AbilityR.AddItem(new Switch() {IsOn = true, Title = "R Enabled"});
            AbilityRCastModeIndex = AbilityR.AddItem(new ModeDisplay() { Title = "R Cast Mode", SelectedModeName = "Mixed", ModeNames = new() { "HP%", "EnemiesHit", "Mixed" } });
            AbilityRHitChanceIndex = AbilityR.AddItem(new ModeDisplay() {Title = "R Hitchance", SelectedModeName = "VeryHigh", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" } });
            AbilityRHPPercentIndex = AbilityR.AddItem(new Counter() { Title = "HP% Value", Value = 20, MaxValue = 100, MinValue = 0, ValueFrequency = 1 });
            AbilityRMinHitsIndex = AbilityR.AddItem(new Counter() { Title = "Min Champs Hit", Value = 2, MaxValue = 5, MinValue = 1, ValueFrequency = 1 });
        }

        #endregion

        #region Damage
        internal static float PassiveExtraDamage => PassiveBonusDamageOnHit[Env.Me().Level] + (Env.Me().UnitStats.BonusAttackDamage * PassiveADScaling);
        
        internal float GetQDamage(GameObjectBase target)
        {
            if (!Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q).IsSpellReady || Env.QLevel < 1)
                return 0;
            float physicalDamage = QDamage[Env.QLevel] + (Env.Me().UnitStats.TotalAttackDamage * QADScaling);
            physicalDamage += QExtraMinionDamage[Env.Me().Level - 1];
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, physicalDamage);
        }

        internal float GetEDamage(GameObjectBase target)
        {
            if (!Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.E).IsSpellReady && Env.ELevel < 1)
                return 0;
            float magicalDamage = EDamage[Env.ELevel] + (Env.Me().UnitStats.TotalAbilityPower * EAPScaling);
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, magicalDamage, 0);
        }

        internal float GetRDamage(GameObjectBase target)
        {

            if (!Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.R).IsSpellReady && Env.RLevel < 1)
                return 0;
            float magicalDamage = RDamage[Env.RLevel] + (Env.Me().UnitStats.TotalAbilityPower * RAPScaling);
            return DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, magicalDamage, 0);
        }

        internal bool QCanKill(GameObjectBase target)
        {
            return target.Health - GetQDamage(target) <= 0;
        }

        internal bool QCanReset(GameObjectBase target)
        {
            if (target == null || !target.IsAlive || target.Distance > QTargetRange || !TargetSelector.IsAttackable(target))
                return false;
            if (Env.Me().Mana < ((Env.QLevel >= 1) ? 20 : 0))
                return false;
            if (QSkipCheck && UnitManager.EnemyChampions.deepCopy().Any(x => target.Name == x.Name))
                return true;
            return HasMark(target) || QCanKill(target);
        }
        #endregion 

        #region Passive/Buffs
        internal bool HasMaxStacks()
        {
            return (PassiveStacks() >= 4) ? true : false;
        }

        internal float PassiveStacks()
        {
            var buff = UnitManager.MyChampion.BuffManager.GetBuffByName("ireliapassivestacks", false, true);
            return (buff == null) ? 0 : buff.Stacks;
        }

        internal bool HasMark(GameObjectBase target)
        {
            var buff = target.BuffManager.GetBuffByName("ireliamark", false, true);
            return buff != null && buff.IsActive && buff.Stacks >= 1;
        }

        #endregion

        #region Spells

        internal GameObjectBase? GetQTarget(bool combo)
        {

            if (combo)
            {
                GameObjectBase? champ = new Common.Helper.Selectors.TargetSelector(QTargetRange).GetVisibleTargetsInRange(Common.Helper.Selectors.Modes.Enemy).FirstOrDefault(x => ShouldCastQ(x, combo));
                if (champ != null)
                    return champ;
            }
            if (!QLaneClear && combo)
                return null;
            GameObjectBase? resetMinion = UnitManager.EnemyMinions.FirstOrDefault(x => x.IsAlive && x.Health > 1 && x.Distance <= QTargetRange && ShouldCastQ(x, combo));
            if (resetMinion != null)
            {
                return resetMinion;
            }
            GameObjectBase? resetMob = UnitManager.EnemyJungleMobs.FirstOrDefault(x => x.IsAlive && x.Health > 1 && !x.Name.Contains("Ward") && TargetSelector.IsAttackable(x) && x.Distance <= QTargetRange && ShouldCastQ(x, combo));
            if (resetMob != null)
            {
                return resetMob;
            }
            GameObjectBase? resetChamp = new Common.Helper.Selectors.TargetSelector(QTargetRange).GetVisibleTargetsInRange(Common.Helper.Selectors.Modes.Enemy).FirstOrDefault(x => ShouldCastQ(x, combo));
            if (resetChamp != null)
                return resetChamp;
            return null;
        }

        internal bool ShouldCastQ(GameObjectBase target, bool combo) {
            if (target.UnitComponentInfo.SkinName.Contains("Minion") && combo && HasMaxStacks() && !QMaxStacks)
            {
                return false;
            }
            if (!QUnderTower && TS.InNexusRange(target))
                return false;
            if (!QUnderTower)
            {
                if (TS.InTowerRange(target) || TS.InNexusRange(target))
                    return false;
            }
            if (Env.Me().Mana < ((Env.QLevel >= 1) ? 20 : 0))
                return false;
            if (target.Distance >= QTargetRange)
                return false;
            return QCanReset(target)? true : QCanKill(target);
        }

        internal bool QCast(GameObjectBase target, bool combo) 
        {
            if (!QEnabled || !Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q).IsSpellReady) 
                return false;
            if (ShouldCastQ(target, combo))
            {
                SpellCastProvider.CastSpell(CastSlot.Q, target.Position);
                return true;
            }
            return false;
        }

        internal bool CanUseE() 
        {
            if (Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.E).IsSpellReady && EEnabled)
                return true;
            return false;
        }

        internal Prediction.MenuSelected.PredictionOutput GetPredE(GameObjectBase target, bool col)
        {
            Vector3 sourcePos = new();
            if (EObj == null)
                sourcePos = Env.Me().Position;
            else
                sourcePos = EObj.Position;
            return Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, ERange, EWidth, EDelay, ESpeed, sourcePos, col);
        }

        internal float GetEExtend(Vector3 castPos)
        {
            return ERange - Env.Me().DistanceTo(castPos);
        }

        internal bool PredictAndCastE(GameObjectBase target)
        {
            if (EObj == null)
            {
                if (ECastMode == "Auto")
                {
                    var t = target.Position.Extend(Env.Me(), -(target.Distance / 2));
                    SpellCastProvider.CastSpell(CastSlot.E, t, 0.15F);
                    return true;
                } 
                 if (ECastMode == "SemiAuto")
                    return false;
            }
            Prediction.MenuSelected.PredictionOutput pred = GetPredE(target, false);
            if (pred.HitChance >= EHitChance)
            {
                Vector3 pos = pred.CastPosition.Extend(EObj.Position, -GetEExtend(pred.CastPosition));
                SpellCastProvider.CastSpell(CastSlot.E, pos);
                __EObj = null;
                ireliaObjects = new();
                return true;
            }
            return false;
        }

        

        internal bool ECast(GameObjectBase target)
        {
            if (!CanUseE())
                return false;
            if (TargetSelector.IsAttackable(target) && target.IsValidTarget())
                return PredictAndCastE(target);
            return false;
        }

        internal bool RReady()
        {
            return Env.Spells.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.R).IsSpellReady;
        }

        internal bool CanUseR()
        {
            if (RReady() && REnabled)
                return true;
            return false;
        }

        internal bool HPCheck(GameObjectBase target)
        {
            //Logger.Log($"{target.HealthPercent} {RHPPercent}");
            if (target.HealthPercent <= RHPPercent && (RCastMode == "Mixed"? true : RCastMode == "HP%"? true : false))
                return true;
            return false;
        }

        internal bool HitCheck(GameObjectBase target)
        {
            if (target.Distance <= RRange - 20)
            {
                Prediction.MenuSelected.PredictionOutput pred = GetPredR(target, true);
                int collidingchamps = pred.CollisionObjects.OfType<Hero>().ToList().Count;
                if (collidingchamps + 1 >= RMinChamps)
                    return true;
            }
            return false;
        }

        internal bool ShouldCastR(GameObjectBase target)
        {
            if (RCastMode == "EnemiesHit" || RCastMode == "Mixed")
            {
                if (HitCheck(target))
                    return true;
            } else
            {
                if (HPCheck(target))
                    return true;
            }
            return false;
        }

        internal GameObjectBase? GetBestCollisionTargetR()
        {
            GameObjectBase? ret = null;
            int n = 0;
            foreach (GameObjectBase enemy in new Common.Helper.Selectors.TargetSelector(RRange).GetVisibleTargetsInRange().deepCopy())
            {
                Prediction.MenuSelected.PredictionOutput pred = GetPredR(enemy, true);
                int collidingchamps = pred.CollisionObjects.OfType<Hero>().ToList().Count;
                if (n == 0)
                {
                    ret = enemy;
                    n = collidingchamps;
                }
                if (collidingchamps > n)
                {
                    ret = enemy;
                    n = collidingchamps;
                }
            }
            return ret;
        }

        internal GameObjectBase? GetBestCollisionTargetE()
        {
            GameObjectBase? ret = null;
            int n = 0;
            foreach (GameObjectBase enemy in new Common.Helper.Selectors.TargetSelector(ERange).GetVisibleTargetsInRange(Env.Me().Position, ERange, Common.Helper.Selectors.Modes.Enemy).deepCopy())
            {
                Prediction.MenuSelected.PredictionOutput pred = GetPredE(enemy, true);
                if (n == 0)
                {
                    ret = enemy;
                    n = pred.CollisionObjects.OfType<Hero>().ToList().Count;
                }
                if (pred.CollisionObjects.OfType<Hero>().ToList().Count > n)
                {
                    ret = enemy;
                    n = pred.CollisionObjects.Count;
                }
            }
            return ret;
        }

        internal Prediction.MenuSelected.PredictionOutput GetPredR(GameObjectBase target, bool collisioncheck=false)
        {
            return Prediction.MenuSelected.GetPrediction(
                    Prediction.MenuSelected.PredictionType.Line,
                    target,
                    RRange,
                    RWidth,
                    0,
                    RSpeed,
                    collisioncheck
                    );
        }

        internal bool RCast(GameObjectBase target)
        {
            if (!CanUseR())
                return false;
            if (ShouldCastR(target) && TargetSelector.IsAttackable(target) && new Common.Helper.Selectors.TargetSelector(ERange).IsValidTarget(target))
            {
                Prediction.MenuSelected.PredictionOutput pred = GetPredR(target, false);
                if (pred.HitChance >= RHitChance)
                {
                    SpellCastProvider.CastSpell(CastSlot.R, pred.CastPosition);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Events
        internal override void Init()
        {
            SetupMenu();
            CoreEvents.OnCoreMainInputAsync += MainTick;
            CoreEvents.OnCoreLaneclearInputAsync += LCI;
            CoreEvents.OnCoreRender += OnCoreRenderTick;
            GameEvents.OnCreateObject += OnCreateObject;
        }

        private void OnCoreRenderTick()
        {
            if (EObj != null)
            {
                foreach (GameObjectBase obj in ireliaObjects)
                {
                    RenderFactory.DrawNativeCircle(EObj.Position, 60, Color.Red, 1);
                }
            }
        }

        private Task LCI()
        {
            GameObjectBase? QTarget = GetQTarget(false);
            if (QTarget != null)
                QCast(QTarget, false);
            return Task.CompletedTask;
        }

        private Task MainTick()
        {
            GameObjectBase? QTarget = GetQTarget(true);
            if (QTarget != null)
                QCast(QTarget, true);
            GameObjectBase? ETarget = GetBestCollisionTargetE();
            if (ETarget != null)
                ECast(ETarget);
            GameObjectBase? RTarget = GetBestCollisionTargetR();
            if (RTarget != null)
                RCast(RTarget);
            return Task.CompletedTask;
        }

        private Task OnCreateObject(List<AIBaseClient> callbackObjectList, AIBaseClient callbackObject, float callbackGameTime)
        {
            if (callbackObject.Name.Contains("Irelia", StringComparison.OrdinalIgnoreCase))
            {
                //Logger.Log($"Added: {callbackObject}");
                if (callbackObject.Name.Contains("Base_E_Blades", StringComparison.OrdinalIgnoreCase))
                {
                    if (ireliaObjects.Count <= 0)
                        __EObj = callbackObject;
                    if (ireliaObjects.Count >= 0)
                        ireliaObjects = new();
                    ireliaObjects.Add(callbackObject);
                }
            }
            return Task.CompletedTask;
        }
        #endregion
    }
}
