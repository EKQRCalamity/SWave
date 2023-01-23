using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject;
using Oasys.SDK.SpellCasting;
using Oasys.SDK;
using SyncWave.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oasys.Common.Extensions;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.EventsProvider;
using Oasys.Common.Menu;
using SharpDX;
using Oasys.SDK.Tools;

namespace SyncWave.Common.Spells
{

    internal sealed class LineSpell : SpellCastBase
    {
        internal int Width { get; set; }
        internal Prediction? Prediction;
        internal bool CollisionCheck;
        internal int HeroCollisions;
        internal ModeDisplay HitChance { get; set; }
        internal float castTime;
        public LineSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, DamageCalculation effectCalculator, int range, int width, float CastTime = 0, int minMana = 0, bool canKill = false, bool harass = false, bool laneclear = false, bool lasthit = false, bool collisionCheck = false, int heroCollisions = 0)
        {
            castTime = CastTime;
            EffectCalculator = effectCalculator;
            MainTab = mainTab;
            SpellGroup = group;
            SpellSlot = spellSlot;
            CastSlot = castSlot;
            isOn = new Oasys.Common.Menu.ItemComponents.Switch("Enabled", enabled);
            IsOn = x => x.IsAlive;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            HitChance = new ModeDisplay() { Title = "Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };
            HarassIsOn = new Switch("Harass", false);
            LaneclearIsOn = new Switch("Laneclear", false);
            LasthitIsOn = new Switch("Lasthit", false);
            CollisionCheck = collisionCheck;
            HeroCollisions = heroCollisions;
            CanKill = canKill;
            Range = range;
            Width = width;
            TargetSelector = x => x.IsAlive && x.Distance < Range;
            group.AddItem(isOn);
            group.AddItem(MinMana);
            group.AddItem(HitChance);
            if (harass)
            {
                group.AddItem(HarassIsOn);
                Harass = true;
            }
            if (lasthit)
            {
                group.AddItem(LasthitIsOn);
                LastHit = true;
            }
            if (laneclear)
            {
                group.AddItem(LaneclearIsOn);
                Push = true;
            }
            CoreEvents.OnCoreMainTick += MainTick;

        }

        public LineSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, DamageCalculation effectCalculator, Func<GameObjectBase, bool> targetSelector, int range, int width, float CastTime = 0, int minMana = 0, bool canKill = false, bool harass = false, bool laneclear = false, bool lasthit = false, bool collisionCheck = false, int heroCollisions = 0)
        {
            castTime = CastTime;
            EffectCalculator = effectCalculator;
            MainTab = mainTab;
            SpellGroup = group;
            SpellSlot = spellSlot;
            CastSlot = castSlot;
            isOn = new Oasys.Common.Menu.ItemComponents.Switch("Enabled", enabled);
            IsOn = x => x.IsAlive;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            HitChance = new ModeDisplay() { Title = "Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };
            HarassIsOn = new Switch("Harass", false);
            LaneclearIsOn = new Switch("Laneclear", false);
            LasthitIsOn = new Switch("Lasthit", false);
            CollisionCheck = collisionCheck;
            HeroCollisions = heroCollisions;
            CanKill = canKill;
            Range = range;
            Width = width;
            TargetSelector = targetSelector;
            group.AddItem(isOn);
            group.AddItem(MinMana);
            group.AddItem(HitChance);
            if (harass)
            {
                group.AddItem(HarassIsOn);
                Harass = true;
            }
            if (lasthit)
            {
                group.AddItem(LasthitIsOn);
                LastHit = true;
            }
            if (laneclear)
            {
                group.AddItem(LaneclearIsOn);
                Push = true;
            }
            CoreEvents.OnCoreMainTick += MainTick;

        }

        public LineSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, DamageCalculation effectCalculator, Func<GameObjectBase, bool> targetSelector, int range, int width, Func<GameObjectBase, bool> isOnFunc, float CastTime = 0, int minMana = 0, bool canKill = false, bool harass = false, bool laneclear = false, bool lasthit = false, bool collisionCheck = false, int heroCollisions = 0)
        {
            castTime = CastTime;
            EffectCalculator = effectCalculator;
            MainTab = mainTab;
            SpellGroup = group;
            SpellSlot = spellSlot;
            CastSlot = castSlot;
            isOn = new Oasys.Common.Menu.ItemComponents.Switch("Enabled", enabled);
            IsOn = isOnFunc;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            HitChance = new ModeDisplay() { Title = "Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = "VeryHigh" };
            HarassIsOn = new Switch("Harass", false);
            LaneclearIsOn = new Switch("Laneclear", false);
            LasthitIsOn = new Switch("Lasthit", false);
            CollisionCheck = collisionCheck;
            HeroCollisions = heroCollisions;
            CanKill = canKill;
            Range = range;
            Width = width;
            TargetSelector = targetSelector;
            group.AddItem(isOn);
            group.AddItem(MinMana);
            group.AddItem(HitChance);
            if (harass)
            {
                group.AddItem(HarassIsOn);
                Harass = true;
            }
            if (lasthit)
            {
                group.AddItem(LasthitIsOn);
                LastHit = true;
            }
            if (laneclear)
            {
                group.AddItem(LaneclearIsOn);
                Push = true;
            }
            CoreEvents.OnCoreMainTick += MainTick;

        }

        internal void SetPrediction(float delay, float speed, bool collisioncheck=false, Vector3? origPos = null)
        {
            if (origPos == null)
                origPos = Env.Me().Position;
            Prediction = new(Oasys.SDK.Prediction.MenuSelected.PredictionType.Line, Range, Width, delay, speed, collisioncheck, origPos);
        }

        internal void SetPrediction(float delay, float speed, bool collisioncheck)
        {
            Prediction = new(Oasys.SDK.Prediction.MenuSelected.PredictionType.Line, Range, Width, delay, speed, collisioncheck);
        }

        internal Task MainTick()
        {
            if (MainInput && isOn.IsOn && !Initialized)
            {
                CoreEvents.OnCoreMainInputAsync += MainInputFunction;
                Initialized = true;
            }
            if (Harass && HarassIsOn.IsOn && !HarassInitialized)
            {
                CoreEvents.OnCoreHarassInputAsync += HarassInputFunction;
                HarassInitialized = true;
            }
            if (Push && LaneclearIsOn.IsOn && !LaneclearInitialized)
            {
                CoreEvents.OnCoreLaneclearInputAsync += PushInputFunction;
                LaneclearInitialized = true;
            }
            if (LastHit && LasthitIsOn.IsOn && !LasthitInitialized)
            {
                CoreEvents.OnCoreLasthitInputAsync += LastHitInputFunction;
                LasthitInitialized = true;
            }
            return Task.CompletedTask;
        }

        private Task LastHitInputFunction()
        {
            if (!LasthitIsOn.IsOn || !isOn.IsOn || !IsOn(Env.Me()))
                return Task.CompletedTask;
            foreach (GameObjectBase enemy in UnitManager.EnemyMinions)
            {
                if (enemy.IsAlive && enemy.IsTargetable && enemy.IsValidTarget() && enemy.Distance < Range && this.SpellIsReady())
                {
                    if (Env.Me().Mana > MinMana.Value)
                    {
                        SpellCastProvider.CastSpell(CastSlot, enemy.Position, castTime);
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task PushInputFunction()
        {
            if (!LaneclearIsOn.IsOn || !isOn.IsOn || !IsOn(Env.Me()))
                return Task.CompletedTask;
            foreach (GameObjectBase enemy in UnitManager.EnemyMinions)
            {
                if (enemy.IsAlive && enemy.IsTargetable && enemy.IsValidTarget() && enemy.Distance < Range && this.SpellIsReady())
                {
                    if (Env.Me().Mana > MinMana.Value)
                    {
                        SpellCastProvider.CastSpell(CastSlot, enemy.Position, castTime);
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task HarassInputFunction()
        {
            GameObjectBase? target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, (x => x.Distance < Range));
            if (target != null&& isOn.IsOn && HarassIsOn.IsOn && IsOn(Env.Me()))
            {
                if (Prediction == null)
                {
                    if ((CanKill) ? target.Health - EffectCalculator.CalculateDamage(target) < 0 : true)
                    {
                        Vector3 tpos = target.Position;
                        Vector2 pos = tpos.ToW2S();
                        if (!tpos.IsOnScreen())
                            pos = tpos.ToWorldToMap();
                        SpellCastProvider.CastSpell(CastSlot, pos, castTime);
                    }
                }
                else
                {
                    Oasys.SDK.Prediction.MenuSelected.PredictionOutput pred = Prediction.Predict(target);
                    if (pred.Collision && CollisionCheck)
                    {
                        if (pred.CollisionObjects.Where(x => !x.IsObject(ObjectTypeFlag.AIHeroClient) && x.ModelName != target.ModelName).Count() >= HeroCollisions)
                            return Task.CompletedTask;
                    }
                    if (pred.HitChance >= GetHitchanceFromName(HitChance.SelectedModeName) && SpellIsReady() && Env.Me().Mana > MinMana.Value)
                    {
                        if ((CanKill) ? target.Health - EffectCalculator.CalculateDamage(target) < 0 : true)
                        {
                            Vector3 tpos = pred.CastPosition;
                            Vector2 pos = tpos.ToW2S();
                            if (!tpos.IsOnScreen())
                                pos = tpos.ToWorldToMap();
                            SpellCastProvider.CastSpell(CastSlot, pos, castTime);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task MainInputFunction()
        {
            GameObjectBase? target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, (x => x.Distance < Range));
            if (target != null && isOn.IsOn && IsOn(Env.Me()))
            {
                if (Prediction == null)
                {
                    if ((CanKill) ? target.Health - EffectCalculator.CalculateDamage(target) < 0 : true)
                    {
                        Vector3 tpos = target.Position;
                        Vector2 pos = tpos.ToW2S();
                        if (!tpos.IsOnScreen())
                            pos = tpos.ToWorldToMap();
                        SpellCastProvider.CastSpell(CastSlot, pos, castTime);
                    }
                }
                else
                {
                    Oasys.SDK.Prediction.MenuSelected.PredictionOutput pred = Prediction.Predict(target);
                    if (pred.Collision && CollisionCheck)
                    {
                        if (pred.CollisionObjects.Where(x => !x.IsObject(ObjectTypeFlag.AIHeroClient)).Count() >= HeroCollisions)
                            return Task.CompletedTask;
                    }
                    if (pred.HitChance >= GetHitchanceFromName(HitChance.SelectedModeName) && SpellIsReady() && Env.Me().Mana > MinMana.Value)
                    {
                        if ((CanKill) ? target.Health - EffectCalculator.CalculateDamage(target) < 0 : true)
                        {
                            Vector3 tpos = pred.CastPosition;
                            Vector2 pos = tpos.ToW2S();
                            if (!tpos.IsOnScreen())
                                pos = tpos.ToWorldToMap();
                            SpellCastProvider.CastSpell(CastSlot, pos, castTime);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
