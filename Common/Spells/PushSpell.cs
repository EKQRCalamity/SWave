

using Oasys.Common;
using Oasys.Common.Enums.GameEnums;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Rendering;
using Oasys.SDK.SpellCasting;
using SharpDX;
using SyncWave.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Common.Spells
{
    internal class PushSpell : SpellCastBase
    {
        internal float PushRange;
        internal bool WallCollision;
        internal float castTime;
        internal Switch PushAway;
        internal Counter PushAwayRange;
        internal Switch ShowPushAway;
        internal bool DislocateFriendly;

        public PushSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, int range, float dislocationRange, float _castTime = 0, int minMana = 0, bool wallColiision = false, bool usePushaway = false, int pushAwayRange = 0, bool dislocateFriendly = false, bool harass=false)
        {
            PushRange = dislocationRange;
            DislocateFriendly = dislocateFriendly;
            WallCollision= wallColiision;
            castTime = _castTime;
            MainTab = mainTab;
            SpellGroup = group;
            CastSlot = castSlot;
            SpellSlot = spellSlot;
            isOn = new Switch("Enabled", enabled);
            IsOn = x => x.IsAlive;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            PushAway = new Switch("Pushaway", usePushaway);
            PushAwayRange = new Counter("Pushaway Range", pushAwayRange, 0, 1500);
            HarassIsOn = new Switch("Harass", false);
            ShowPushAway = new Switch("Show Pushaway", false);
            Range = range;
            group.AddItem(isOn);
            group.AddItem(MinMana);
            TargetSelector = x => x.IsAlive && x.Distance < Range;
            if (usePushaway)
            {
                group.AddItem(PushAway);
                group.AddItem(PushAwayRange);
                group.AddItem(ShowPushAway);
            }
            if (harass)
            {
                group.AddItem(HarassIsOn);
                Harass = true;
            }
            CoreEvents.OnCoreMainTick += MainTick;
            CoreEvents.OnCoreRender += Render;
        }

        public PushSpell(Tab mainTab, Group group, CastSlot castSlot, SpellSlot spellSlot, bool enabled, int range, float dislocationRange, Func<GameObjectBase, bool> targetSelector, float _castTime = 0, int minMana = 0, bool wallColiision = false, bool usePushaway = false, int pushAwayRange = 0, bool dislocateFriendly = false, bool harass = false)
        {
            PushRange = dislocationRange;
            DislocateFriendly = dislocateFriendly;
            castTime = _castTime;
            MainTab = mainTab;
            SpellGroup = group;
            CastSlot = castSlot;
            SpellSlot = spellSlot;
            isOn = new Switch("Enabled", enabled);
            IsOn = x => x.IsAlive;
            MinMana = new Counter("Min Mana", minMana, 0, 1000);
            PushAway = new Switch("Pushaway", usePushaway);
            PushAwayRange = new Counter("Pushaway Range", pushAwayRange, 0, 1500);
            ShowPushAway = new Switch("Show Pushaway", false);
            HarassIsOn = new Switch("Harass", false);
            Range = range;
            TargetSelector = targetSelector;
            group.AddItem(isOn);
            group.AddItem(MinMana);
            if (usePushaway)
            {
                group.AddItem(PushAway);
                group.AddItem(PushAwayRange);
                group.AddItem(ShowPushAway);
            }
            if (harass)
            {
                group.AddItem(HarassIsOn);
                Harass = true;
            }
            CoreEvents.OnCoreMainTick += MainTick;
            CoreEvents.OnCoreRender += Render;
        }

        private void Render()
        {
            if (ShowPushAway.IsOn)
            {
                foreach (GameObjectBase target in UnitManager.EnemyChampions)
                {
                    if (target.IsAlive && target.IsValidTarget() && target.Position.IsOnScreen())
                    {
                        Vector2 endpos = CalculateHitPoint(Env.Me(), target, PushRange).ToW2S();
                        Vector2 startpos = target.Position.ToW2S();
                        if (!endpos.IsZero && !startpos.IsZero)
                            RenderFactory.DrawLine(startpos.X, startpos.Y, endpos.X, endpos.Y, 2, Color.Black);
                    }
                }
            }
             
        }

        private Task MainTick()
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
            return Task.CompletedTask;
        }

        private Task HarassInputFunction()
        {
            GameObjectBase? target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, TargetSelector);

            if (target != null && HarassIsOn.IsOn && IsOn(Env.Me()) && Env.Me().Mana > MinMana.Value && target.Distance < Range)
            {
                if (target.IsAlive && target.IsTargetable && target.IsValidTarget() && target.IsObject(ObjectTypeFlag.AIHeroClient) && SpellIsReady())
                {
                    if (WallCollision)
                    {
                        float range1 = PushRange * 0.1F;
                        float range2 = PushRange * 0.2F;
                        float range3 = PushRange * 0.3F;
                        float range4 = PushRange * 0.4F;
                        float range5 = PushRange * 0.5F;
                        float range6 = PushRange * 0.6F;
                        float range7 = PushRange * 0.7F;
                        float range8 = PushRange * 0.8F;
                        float range9 = PushRange * 0.9F;
                        float range10 = PushRange;
                        bool collision = false;
                        List<Vector3> positions = new List<Vector3>() { CalculateHitPoint(Env.Me(), target, range10), CalculateHitPoint(Env.Me(), target, range9), CalculateHitPoint(Env.Me(), target, range8), CalculateHitPoint(Env.Me(), target, range7), CalculateHitPoint(Env.Me(), target, range6), CalculateHitPoint(Env.Me(), target, range5), CalculateHitPoint(Env.Me(), target, range4), CalculateHitPoint(Env.Me(), target, range3), CalculateHitPoint(Env.Me(), target, range2), CalculateHitPoint(Env.Me(), target, range1) };
                        foreach (Vector3 position in positions)
                        {
                            if (EngineManager.IsWall(position))
                            {
                                collision = true;
                                break;
                            }
                        }
                        if (collision)
                        {
                            SpellCastProvider.CastSpell(CastSlot, target.Position, castTime);
                            return Task.CompletedTask;
                        }
                    }
                    if (PushAway.IsOn)
                    {
                        List<GameObjectBase> targets = UnitManager.EnemyChampions.Where(TargetSelector).OrderByDescending(x => x.Distance).ToList().deepCopy();
                        foreach (GameObjectBase _target in targets)
                        {
                            if (target.Distance < PushAwayRange.Value)
                            {
                                SpellCastProvider.CastSpell(CastSlot, _target.Position, castTime);
                                break;
                            }
                        }
                        return Task.CompletedTask;
                    }
                }
            }

            return Task.CompletedTask;
        }

        internal Vector3 CalculateHitPoint(GameObjectBase source, GameObjectBase target, float range)
        {
            return target.Position.Extend(source.Position, -range);
        }

        private Task MainInputFunction()
        {
            GameObjectBase? target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, TargetSelector);

            if (target != null && isOn.IsOn && IsOn(Env.Me()) &&Env.Me().Mana > MinMana.Value && target.Distance < Range)
            {
                if (target.IsAlive && target.IsTargetable && target.IsValidTarget() && target.IsObject(ObjectTypeFlag.AIHeroClient) && SpellIsReady())
                {
                    if (WallCollision)
                    {
                        float range1 = PushRange * 0.1F;
                        float range2 = PushRange * 0.2F;
                        float range3 = PushRange * 0.3F;
                        float range4 = PushRange * 0.4F;
                        float range5 = PushRange * 0.5F;
                        float range6 = PushRange * 0.6F;
                        float range7 = PushRange * 0.7F;
                        float range8 = PushRange * 0.8F;
                        float range9 = PushRange * 0.9F;
                        float range10 = PushRange;
                        bool collision = false;
                        List<Vector3> positions = new List<Vector3>() { CalculateHitPoint(Env.Me(), target, range10), CalculateHitPoint(Env.Me(), target, range9), CalculateHitPoint(Env.Me(), target, range8), CalculateHitPoint(Env.Me(), target, range7), CalculateHitPoint(Env.Me(), target, range6), CalculateHitPoint(Env.Me(), target, range5), CalculateHitPoint(Env.Me(), target, range4), CalculateHitPoint(Env.Me(), target, range3), CalculateHitPoint(Env.Me(), target, range2), CalculateHitPoint(Env.Me(), target, range1) };
                        foreach (Vector3 position in positions)
                        {
                            if (EngineManager.IsWall(position))
                            {
                                collision = true;
                                break;
                            }
                        }
                        if (collision)
                        {
                            SpellCastProvider.CastSpell(CastSlot, target.Position, castTime);
                            return Task.CompletedTask;
                        }
                    }
                    if (PushAway.IsOn)
                    {
                        List<GameObjectBase> targets = UnitManager.EnemyChampions.Where(TargetSelector).OrderByDescending(x => x.Distance).ToList().deepCopy();
                        foreach (GameObjectBase _target in targets)
                        {
                            if (target.Distance < PushAwayRange.Value)
                            {
                                SpellCastProvider.CastSpell(CastSlot, _target.Position, castTime);
                                break;
                            }
                        }
                        return Task.CompletedTask;
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
