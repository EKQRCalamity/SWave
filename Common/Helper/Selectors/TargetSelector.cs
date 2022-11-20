using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Menu;
using SharpDX;
using SyncWave.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncWave.Common.Helper.Selectors
{
    internal enum Modes
    {
        None = 0,
        Enemy = 1,
        Ally = 2,
        Both = 3
    }

    internal class TargetSelector
    {
        internal Modes Mode = Modes.None;
        internal int range;
        internal bool Allies = false;
        internal bool Enemies = true;

        internal static int TabIndex = -1;
        internal static int EnemiesGroupIndex = -1;
        internal static int PrioritizeModeIndex = -1;
        internal static int EnemyLowHPCounterIndex = -1;
        internal static Dictionary<string, int> EnemyIndexes = new();
        internal static int AlliesGroupIndex = -1;
        internal static int AllyPrioritizeModeIndex = -1;
        internal static int AllyLowHPCounterIndex = -1;
        internal static Dictionary<string, int> AllyIndexes = new();

        internal static void MenuInit()
        {
            Tab tab = new Tab("SyncWave - TargetSelector");
            TabIndex = MenuManager.AddTab(tab);
            Group EnemyGroup = new Group("Enemies");
            EnemiesGroupIndex = tab.AddGroup(EnemyGroup);
            PrioritizeModeIndex = EnemyGroup.AddItem(new ModeDisplay() { SelectedModeName = "Mixed", Title = "Prioritize Mode", ModeNames = new() { "Low HP", "Prio", "Mixed" } });
            EnemyLowHPCounterIndex = EnemyGroup.AddItem(new Counter() { Value = 20, Title = "Enemy Low HP%", MaxValue = 100, MinValue = 0, ValueFrequency = 1 });
            List<Hero> enemies = UnitManager.EnemyChampions.deepCopy();
            foreach (GameObjectBase hero in enemies)
            {
                try
                {
                    EnemyGroup.GetItem<Counter>($"{hero.Name}-{hero.ModelName}");
                    EnemyIndexes.Add($"{hero.Name}-{hero.ModelName}", EnemyGroup.AddItem(new Counter() { Title = $"{hero.ModelName} Prio", MinValue = 0, MaxValue = 5, Value = 1, ValueFrequency = 1 }));
                }
                catch (Exception ex)
                {
                    EnemyIndexes.Add($"{hero.Name}-{hero.ModelName}-{hero.EncryptedID}", EnemyGroup.AddItem(new Counter() { Title = $"{hero.ModelName}-{hero.EncryptedID} Prio", MinValue = 0, MaxValue = 5, Value = 1, ValueFrequency = 1 }));
                }
            }
            Group AllyGroup = new Group("Allies");
            AlliesGroupIndex = tab.AddGroup(AllyGroup);
            AllyPrioritizeModeIndex = AllyGroup.AddItem(new ModeDisplay() { SelectedModeName = "Mixed", Title = "Prioritize Mode", ModeNames = new() { "Low HP", "Prio", "Mixed" } });
            AllyLowHPCounterIndex = AllyGroup.AddItem(new Counter() { Value = 20, Title = "Ally Low HP%", MaxValue = 100, MinValue = 0, ValueFrequency = 1 });
            List<Hero> allies = UnitManager.AllyChampions.deepCopy();
            foreach (GameObjectBase hero in allies)
            {
                try
                {
                    AllyGroup.GetItem<Counter>($"{hero.Name}-{hero.ModelName}");
                    AllyIndexes.Add($"{hero.Name}-{hero.ModelName}", AllyGroup.AddItem(new Counter() { Title = $"{hero.ModelName} Prio", MinValue = 0, MaxValue = 5, Value = 1, ValueFrequency = 1 }));
                }
                catch (Exception ex)
                {
                    AllyIndexes.Add($"{hero.Name}-{hero.ModelName}-{hero.EncryptedID}", AllyGroup.AddItem(new Counter() { Title = $"{hero.ModelName}-{hero.EncryptedID} Prio", MinValue = 0, MaxValue = 5, Value = 1, ValueFrequency = 1 }));

                }
            }
        }

        internal TargetSelector(int Range, Modes mode = Modes.Enemy)
        {
            range = Range;
            Mode = mode;
            if (mode.Equals(Modes.Enemy) || mode.Equals(Modes.Both))
            {
                Enemies = true;
            }
            if (mode.Equals(Modes.Ally) || mode.Equals(Modes.Both))
            {
                Allies = true;
            }
        }

        internal int GetLowHealthPercentAlly()
        {
            return MenuManager.GetTab(TabIndex).GetGroup(AlliesGroupIndex).GetItem<Counter>(AllyLowHPCounterIndex).Value;
        }

        internal int GetLowHealthPercentEnemy()
        {
            return MenuManager.GetTab(TabIndex).GetGroup(EnemiesGroupIndex).GetItem<Counter>(EnemyLowHPCounterIndex).Value;
        }

        internal bool IsLowHealth(GameObjectBase target)
        {
            if (target.IsAlly)
                return target.HealthPercent <= GetLowHealthPercentAlly();
            else
                return target.HealthPercent <= GetLowHealthPercentEnemy();
        }

        internal bool IsValidTarget(GameObjectBase target)
        {
            if (target.IsAlive && target.IsTargetable && target.IsVisible)
                return true;
            return false;
        }

        internal bool TargetInRange(GameObjectBase target)
        {
            return TargetInRange(target, range, Env.Me().Position);
        }

        internal bool TargetInRange(GameObjectBase target, Vector3 sourcePos)
        {
            if (target.DistanceTo(sourcePos) <= range)
                return true;
            return false;
        }

        internal bool TargetInRange(GameObjectBase target, int Range, Vector3 sourcePos)
        {
            if (target.DistanceTo(sourcePos) <= Range)
                return true;
            return false;
        }

        internal List<GameObjectBase> GetTargetsInRange(Vector3 sourcePos, int range, Modes mode = Modes.Enemy)
        {
            List<GameObjectBase> targets = new();
            if (mode.Equals(Modes.Enemy) || mode.Equals(Modes.Both))
            {
                List<Hero> enemies = UnitManager.EnemyChampions.deepCopy();
                foreach (GameObjectBase target in enemies)
                {
                    if (TargetInRange(target, range, sourcePos) && IsValidTarget(target))
                        targets.Add(target);
                }
            }
            if (mode.Equals(Modes.Ally) || mode.Equals(Modes.Both))
            {
                List<Hero> allies = UnitManager.AllyChampions.deepCopy();
                foreach (GameObjectBase target in allies)
                {
                    if (TargetInRange(target, range, sourcePos) && IsValidTarget(target))
                        targets.Add(target);
                }
            }
            return targets;
        }

        internal List<GameObjectBase> GetTargetsInRange(Modes mode = Modes.Enemy)
        {
            return GetTargetsInRange(Env.Me().Position, range, mode);
        }
        internal List<GameObjectBase> GetTargetsInRange(Vector3 sourcePos, Modes mode = Modes.Enemy)
        {
            return GetTargetsInRange(sourcePos, range, mode);
        }

        internal List<GameObjectBase> GetTargetsInRange(int range, Modes mode = Modes.Enemy)
        {
            return GetTargetsInRange(Env.Me().Position, range, mode);
        }

        internal int GetPrio(GameObjectBase target)
        {
            if (target.IsAlly)
                return MenuManager.GetTab(TabIndex).GetGroup(AlliesGroupIndex).GetItem<Counter>(AllyIndexes[$"{target.Name}-{target.ModelName}"]).Value;
            else
                return MenuManager.GetTab(TabIndex).GetGroup(EnemiesGroupIndex).GetItem<Counter>(EnemyIndexes[$"{target.Name}-{target.ModelName}"]).Value;
        }

        internal GameObjectBase? GetLowestHealthPrioTarget(Modes mode = Modes.Enemy)
        {
            return GetLowestHealthPrioTarget(Env.Me().Position, range, mode);
        }

        internal GameObjectBase? GetLowestHealthPrioTarget(int range, Modes mode = Modes.Enemy)
        {
            return GetLowestHealthPrioTarget(Env.Me().Position, range, mode);
        }
        internal GameObjectBase? GetLowestHealthPrioTarget(Vector3 sourcePos, Modes mode = Modes.Enemy)
        {
            return GetLowestHealthPrioTarget(sourcePos, range, mode);
        }
        internal GameObjectBase? GetLowestHealthPrioTarget(Vector3 sourcePos, int range,  Modes mode = Modes.Enemy)
        {
            List<GameObjectBase> targets = GetTargetsInRange(sourcePos, mode);
            GameObjectBase? reTarget = null;
            foreach (GameObjectBase target in targets)
            {
                if (reTarget == null)
                    reTarget = target;
                else
                {
                    if (target.HealthPercent < reTarget.HealthPercent)
                        reTarget = target;
                }
            }
            return reTarget;
        }

        internal GameObjectBase? GetLowestHealthTarget( Modes mode = Modes.Enemy)
        {
            return GetLowestHealthPrioTarget(Env.Me().Position, mode);
        }

        internal GameObjectBase? GetClosestTarget(Vector3 sourcePos, int range,  Modes mode = Modes.Enemy)
        {
            List<GameObjectBase> targets = GetTargetsInRange(sourcePos, mode);
            GameObjectBase? reTarget = null;
            foreach (GameObjectBase target in targets)
            {
                if (reTarget == null)
                    reTarget = target;
                else
                {
                    if (target.DistanceTo(sourcePos) < reTarget.DistanceTo(sourcePos))
                        reTarget = target;
                }
            }
            return reTarget;
        }

        internal GameObjectBase? GetClosestTarget(Modes mode = Modes.Enemy)
        {
            return GetClosestTarget(Env.Me().Position, range, mode);
        }
        internal GameObjectBase? GetClosestTarget(Vector3 sourcePos,Modes mode = Modes.Enemy)
        {
            return GetClosestTarget(sourcePos, range, mode);
        }
        internal GameObjectBase? GetClosestTarget(int range, Modes mode = Modes.Enemy)
        {
            return GetClosestTarget(Env.Me().Position, range, mode);
        }

        internal GameObjectBase? GetMostPrioTarget(Vector3 sourcePos, int range, Modes mode = Modes.Enemy)
        {
            List<GameObjectBase> targets = GetTargetsInRange(sourcePos, mode);
            GameObjectBase? reTarget = null;
            foreach (GameObjectBase target in targets)
            {
                if (reTarget == null)
                    reTarget = target;
                else
                {
                    if (GetPrio(target) > GetPrio(reTarget))
                        reTarget = target;
                }
            }
            return reTarget;
        }

        internal GameObjectBase? GetMostPrioTarget( Modes mode = Modes.Enemy)
        {
            return GetMostPrioTarget(Env.Me().Position, range, mode);
        }
        internal GameObjectBase? GetMostPrioTarget(Vector3 sourcePos, Modes mode = Modes.Enemy)
        {
            return GetMostPrioTarget(sourcePos, range, mode);
        }
        internal GameObjectBase? GetMostPrioTarget(int range, Modes mode = Modes.Enemy)
        {
            return GetMostPrioTarget(Env.Me().Position, range, mode);
        }

        internal bool XTargetsInRange(int X, Vector3 sourcePos, int range, Modes mode = Modes.Enemy)
        {
            List<GameObjectBase> enemiesInRange = GetTargetsInRange(sourcePos, mode);
            if (enemiesInRange.Count >= X)
                return true;
            return false;
        }

        internal bool XTargetsInRange(int X,  Modes mode = Modes.Enemy)
        {
            return XTargetsInRange(X, Env.Me().Position, range, mode);
        }
        internal bool XTargetsInRange(int X, int range, Modes mode = Modes.Enemy)
        {
            return XTargetsInRange(X, Env.Me().Position, range, mode);
        }
        internal bool XTargetsInRange(int X, Vector3 sourcePos, Modes mode = Modes.Enemy)
        {
            return XTargetsInRange(X, sourcePos, range, mode);
        }

        internal List<GameObjectBase> GetVisibleTargetsInRange(Vector3 sourcePos, int range, Modes mode = Modes.Enemy)
        {
            List<GameObjectBase> targetsInRange = GetTargetsInRange(sourcePos, range, mode);
            List<GameObjectBase> list = new();
            foreach (GameObjectBase target in targetsInRange)
            {
                if (target.IsVisible)
                    list.Add(target);
            }
            return list;
        }

        internal List<GameObjectBase> GetVisibleTargetsInRange(Modes mode = Modes.Enemy)
        {
            return GetVisibleTargetsInRange(Env.Me().Position, range, mode);
        }

        internal List<GameObjectBase> GetVisibleTargetsInRange(int range, Modes mode = Modes.Enemy)
        {
            return GetVisibleTargetsInRange(Env.Me().Position, range, mode);
        }
        internal List<GameObjectBase> GetVisibleTargetsInRange(Vector3 sourcePos, Modes mode = Modes.Enemy)
        {
            return GetVisibleTargetsInRange(sourcePos, range, mode);
        }

        internal bool InNexusRange(GameObjectBase target)
        {
            return InNexusRange(target.Position);
        }
        internal bool InNexusRange(Vector3 pos)
        {
            Vector3 BlueNexus = new Vector3(405, 95, 425);
            Vector3 RedNexus = new Vector3(14300, 90, 14400);
            return pos.Distance(BlueNexus) <= 1000 || pos.Distance(RedNexus) <= 1000;
        }

        internal bool InTowerRange(GameObjectBase target) 
        {
            return InTowerRange(target.Position);
        }
        internal bool InTowerRange(Vector3 pos)
        {
            return UnitManager.EnemyTowers.Any(x => x.IsAlive && x.Health >= 1 && x.DistanceTo(pos) < 750);
        }
    }
}
