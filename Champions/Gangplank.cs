using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.SDK;
using SyncWave.Base;
using SyncWave.Common.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncWave.Common.Helper;
using System.Windows.Forms;
using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject.Clients;
using Oasys.SDK.Tools;
using SharpDX;
using Oasys.Common.Enums.GameEnums;
using Oasys.SDK.SpellCasting;
using SyncWave.Common.Extensions;
using Oasys.SDK.Rendering;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.Menu.ItemComponents;
using System.Threading;

namespace SyncWave.Champions
{ 
    internal class QPQDamage : DamageCalculation
    {
        internal int[] QBaseDamage = new int[] { 0, 10, 40, 70, 100, 130 };
        internal float ADScaling = 1.0f;
        internal float Speed = 2600.0f;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (target.IsValidTarget() && target.IsAlive && target.IsTargetable)
            {
                damage = QBaseDamage[Env.QLevel];
                damage = damage + (Env.Me().UnitStats.TotalAttackDamage * ADScaling);
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, damage);
            }
            return damage;
        }
    }

    internal class _RDamage : DamageCalculation {
        internal int[] RBaseDamage = new int[] { 0, 480, 840, 1200 };
        internal float APScaling = 1.2f;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (target.IsValidTarget() && target.IsAlive && target.IsTargetable)
            {
                damage = RBaseDamage[Env.RLevel];
                damage = damage + (Env.Me().UnitStats.TotalAbilityPower * APScaling);
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal class GPBarrelPos {
        internal AIBaseClient SelectedBarrel { get; set; }
        internal GameObjectBase SelectedTarget { get; set; }
        internal Vector3 SelectedPosition { get; set; }
        internal bool ValidPosition => SelectedPosition != Vector3.Zero;
        internal bool ValidTarget => SelectedTarget != null && SelectedTarget.IsValidTarget();
        internal bool ValidBarrel => SelectedBarrel != null && SelectedBarrel.IsAlive && SelectedBarrel.IsTargetable;

        public GPBarrelPos(AIBaseClient selectedBarrel, GameObjectBase selectedTarget, Vector3 selectedPosition) {
            SelectedBarrel = selectedBarrel;
            SelectedTarget = selectedTarget;
            SelectedPosition = selectedPosition;
        }
    }

    internal class AdvancedGangplankBarrel
    {
        internal AIBaseClient OriginalBarrel { get; set; }

        public AdvancedGangplankBarrel(AIBaseClient originalBarrel)
        {
            OriginalBarrel = originalBarrel;
        }

        internal bool HasConnectedBarrels()
        {
            foreach (AIBaseClient barrel in Gangplank.GetGangplankBarrels().deepCopy())
            {
                if (barrel.DistanceTo(OriginalBarrel.Position) < Gangplank.EBindRange)
                    return true;
            }
            return false;
        }

        internal List<AIBaseClient> ConnectedBarrels()
        {
            List<AIBaseClient> ConnectedBarrels = new List<AIBaseClient>();
            foreach (AIBaseClient barrel in Gangplank.GetGangplankBarrels().deepCopy())
            {
                if (barrel.DistanceTo(OriginalBarrel.Position) < Gangplank.EBindRange)
                {
                    ConnectedBarrels.Add(barrel);
                }
            }
            return ConnectedBarrels;
        }

        internal List<AdvancedGangplankBarrel> ConnectedBarrelsAsAGB()
        {
            List<AIBaseClient> barrels = ConnectedBarrels();
            List<AdvancedGangplankBarrel> ActualBarrels = new();
            foreach (AIBaseClient barrel in barrels)
            {
                ActualBarrels.Add(new AdvancedGangplankBarrel(barrel));
            }
            return ActualBarrels;
        }

        internal AdvancedGangplankBarrel? NearestBarrelAsAGB()
        {
            AIBaseClient? aiBaseBarrel = null;
            foreach (AIBaseClient barrel in Gangplank.GetGangplankBarrels().deepCopy())
            {
                if (barrel.DistanceTo(OriginalBarrel.Position) < Gangplank.EBindRange && aiBaseBarrel == null || barrel.DistanceTo(OriginalBarrel.Position) < aiBaseBarrel?.DistanceTo(OriginalBarrel.Position))
                {
                    aiBaseBarrel = barrel;
                }
            }
            AdvancedGangplankBarrel? Barrel = (aiBaseBarrel == null) ? null : new AdvancedGangplankBarrel(aiBaseBarrel);
            return Barrel;
        }

        internal AdvancedGangplankBarrel? NearestBarrelTo(GameObjectBase aiBaseClient)
        {
            AIBaseClient? aiBaseBarrel = null;
            foreach (AIBaseClient barrel in Gangplank.GetGangplankBarrels().deepCopy())
            {
                if (barrel.DistanceTo(aiBaseClient.Position) < Gangplank.EBindRange && aiBaseBarrel == null || barrel.DistanceTo(aiBaseClient.Position) < aiBaseBarrel?.DistanceTo(aiBaseClient.Position))
                {
                    aiBaseBarrel = barrel;
                }
            }
            AdvancedGangplankBarrel? Barrel = (aiBaseBarrel == null) ? null : new AdvancedGangplankBarrel(aiBaseBarrel);
            return Barrel;
        }

        internal bool CanHit(GameObjectBase aiBaseClient)
        {
            if (aiBaseClient.DistanceTo(OriginalBarrel.Position) <= Gangplank.EExplosionRange) return true;
            if (HasConnectedBarrels())
            {
                AdvancedGangplankBarrel? gpBarrel = NearestBarrelTo(aiBaseClient);
                if (gpBarrel != null && gpBarrel.OriginalBarrel.DistanceTo(aiBaseClient.Position) < OriginalBarrel.DistanceTo(aiBaseClient.Position) && gpBarrel.CanHit(aiBaseClient))
                {
                    return true;
                }
            }
            return false;
        }

        internal Vector3 GetNextPlacementPosition(GameObjectBase aiBaseClient)
        {
            if (aiBaseClient.DistanceTo(OriginalBarrel.Position) <= Gangplank.EExplosionRange)
            {
                return new Vector3(1,1,1);
            }
            else if (aiBaseClient.Distance < Gangplank.ERange)
            {
                if (aiBaseClient.DistanceTo(OriginalBarrel.Position) < (Gangplank.EBindRange * 2.9) && NearestBarrelTo(aiBaseClient) == null)
                {
                    Vector3 barrelPos;
                    if (aiBaseClient.DistanceTo(OriginalBarrel.Position) > (Gangplank.EBindRange * 2) && aiBaseClient.DistanceTo(OriginalBarrel.Position) < (Gangplank.EBindRange * 3))
                    {
                        barrelPos = aiBaseClient.Position.Extend(OriginalBarrel.Position, Gangplank.EBindRange);
                    }
                    else
                    {
                        barrelPos = aiBaseClient.Position.Extend(OriginalBarrel.Position, (Gangplank.randomGen.Next(50, Gangplank.EBindRange / 4)));
                    }
                    return barrelPos;
                }
                else
                {
                    AdvancedGangplankBarrel? barrel = NearestBarrelTo(aiBaseClient);
                    if (barrel != null)
                        return barrel.GetNextPlacementPosition(aiBaseClient);
                }
            }
            return Vector3.Zero;
        }
    }

    internal class Gangplank : Module
    {
        internal Tab MainTab = new Tab("SyncWave - GP");
        internal Group QGroup = new Group("Q Settings");
        internal Group WGroup = new Group("W Settings");
        internal Switch WEnabled = new Switch("W Enabled", true);
        internal Group EGroup = new Group("E Settings");
        internal Switch EEnabled = new Switch("E Enabled", true);
        internal Switch DrawBarrels = new Switch("Draw Barrels", false);
        internal Group RGroup = new Group("R Settings");
        internal List<AIBaseClient> Barrels => GetGangplankBarrels();
        internal static int QRange = 650;
        internal static int ERange = 1000;
        internal static int EExplosionRange = 360;
        internal static int EBindRange = 345;
        internal static Random randomGen = new Random();

        internal static List<AIBaseClient> GetGangplankBarrels()
        {
            List<AIBaseClient> barrels = new List<AIBaseClient>();
            foreach (AIBaseClient barrel in UnitManager.GetEnemies(new ObjectTypeFlag[] { ObjectTypeFlag.AIMinionClient }).Where(x => x.ModelName.Contains("Barrel", StringComparison.OrdinalIgnoreCase)))
            {
                barrels.Add(barrel);
            }
            return barrels ;
        }

        internal static bool IsCrowdControllButCanCleanse(BuffEntry buff, bool slowIsCC)
        {
            return buff.IsActive && buff.Stacks >= 1 &&
                   ((slowIsCC && buff.EntryType == BuffType.Slow) ||
                   buff.EntryType == BuffType.Stun || buff.EntryType == BuffType.Taunt ||
                   buff.EntryType == BuffType.Snare || buff.EntryType == BuffType.Charm ||
                   buff.EntryType == BuffType.Silence || buff.EntryType == BuffType.Blind ||
                   buff.EntryType == BuffType.Fear || buff.EntryType == BuffType.Polymorph ||
                   buff.EntryType == BuffType.Flee || buff.EntryType == BuffType.Sleep) &&
                   !buff.Name.Equals("yonerstun", System.StringComparison.OrdinalIgnoreCase) &&
                   !buff.Name.Equals("landslidedebuff", System.StringComparison.OrdinalIgnoreCase) &&
                   !buff.Name.Equals("CassiopeiaWSlow", System.StringComparison.OrdinalIgnoreCase) &&
                   !buff.Name.Equals("megaadhesiveslow", System.StringComparison.OrdinalIgnoreCase) &&
                   !buff.Name.Equals("UnknownBuff", System.StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsCrowdControlledButCanCleanse<T>(T obj) where T : GameObjectBase
        {
            if (Env.ModuleVersion >= Common.Enums.V.Development)
            {
                BuffEntry? buff = obj.BuffManager.GetBuffList().FirstOrDefault(x => IsCrowdControllButCanCleanse(x, false));
                if (buff != null)
                {
                    Logger.Log($"{obj.ModelName} - {buff.EntryType} - {buff.Name}");
                }
            }

            return obj.BuffManager.GetBuffList().Any(x => IsCrowdControllButCanCleanse(x, false));
        }

        private GPBarrelPos ExpGetBarrelPosition(AIBaseClient selectedBarrel=null, AIBaseClient selectedTarget=null)
        {
            if (selectedBarrel == null)
            {
                List<AIBaseClient> barrelList = Barrels.deepCopy();
                GameObjectBase target;
                if (selectedTarget == null)
                {
                    target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, (x => x.Distance < 3000 && x.IsTargetable && x.IsAlive));
                } else
                {
                    target = selectedTarget;
                }
                if (target == null)
                    return new GPBarrelPos(null, null, Vector3.Zero);
                foreach (AIBaseClient Barrel in barrelList.OrderByDescending(x=>(GameEngine.GameTime - x.BuffManager.ActiveBuffs.First().StartTime)).ToList())
                {
                    AdvancedGangplankBarrel AGB = new AdvancedGangplankBarrel(Barrel);
                    if (AGB.CanHit(target))
                    {
                        return new GPBarrelPos(Barrel, target, Vector3.Zero);
                    }
                    if (target.DistanceTo(Barrel.Position) < (EBindRange * 3) && target.Distance < 3000 && Env.Me().DistanceTo(Barrel.Position) < QRange)
                    {
                        if (target.DistanceTo(Barrel.Position) < EExplosionRange)
                        {
                            return new GPBarrelPos(Barrel, target, Vector3.Zero);
                        } else
                        {
                            if (AGB.HasConnectedBarrels())
                            {
                                Vector3 nextPlacementPos = AGB.GetNextPlacementPosition(target);
                                
                                if (nextPlacementPos != Vector3.Zero)
                                {
                                    if (nextPlacementPos.X == 1 && nextPlacementPos.Y == 1 && nextPlacementPos.Z == 1)
                                    {
                                        if (HealthThresholdReached(Barrel))
                                        {
                                            return new GPBarrelPos(Barrel, target, Vector3.Zero);
                                        }
                                    } else
                                    {
                                        return new GPBarrelPos(Barrel, target, nextPlacementPos);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return new GPBarrelPos(null, null, Vector3.Zero);
        }

        private GPBarrelPos GetBarrelPlacementPosition(AIBaseClient selectedBarrel=null, AIBaseClient selectedTarget=null) 
        {
            if (selectedBarrel == null)
            {
                List<AIBaseClient> barrels = Barrels.deepCopy();
                GameObjectBase target;
                if (selectedTarget == null) {
                    target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, (x=>x.Distance < ERange && x.IsTargetable && x.IsAlive));
                } else {
                    target = selectedTarget;
                }
                if (target == null)
                    return new GPBarrelPos(null, null, Vector3.Zero);
                foreach (AIBaseClient barrel in barrels) 
                {
                    if (target.DistanceTo(barrel.Position) < (EBindRange * 3F) && target.Distance < ERange && Env.Me().DistanceTo(barrel.Position) < QRange) 
                    {
                        if (!(target.DistanceTo(barrel.Position) < EExplosionRange) && Env.EReady) {
                            Vector3 barrelPos;
                            if (target.DistanceTo(barrel.Position) > EBindRange * 2F)
                            {
                                barrelPos = target.Position.Extend(barrel.Position, EBindRange);
                            } else
                            {
                                barrelPos = target.Position.Extend(barrel.Position, (randomGen.Next(10, (int)Math.Floor(target.DistanceTo(barrel.Position) / 5))));
                            }
                            return new(barrel, target, barrelPos);
                        } else if (target.DistanceTo(barrel.Position) < EExplosionRange)
                            return new(barrel, target, Vector3.Zero);
                    }
                }                
            }
            return new(null, null, Vector3.Zero);
        }

        QPQDamage QDamage = new QPQDamage();
        _RDamage RDamage = new _RDamage();
        TargetedSpell qTargeted;
        CircleSpell rCircle;

        internal override void Init()
        {
            MenuManagerProvider
                .AddTab(MainTab);
            MainTab.AddGroup(QGroup);
            MainTab.AddGroup(WGroup);
            MainTab.AddGroup(EGroup);
            MainTab.AddGroup(RGroup);
            WGroup.AddItem(WEnabled);
            EGroup.AddItem(EEnabled);
            EGroup.AddItem(DrawBarrels);
            qTargeted = new TargetedSpell(MainTab, QGroup, Oasys.SDK.SpellCasting.CastSlot.Q, Oasys.Common.Enums.GameEnums.SpellSlot.Q, true, QDamage, (x => x.IsAlive && x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient)), 625, 0.25F, 55);
            rCircle = new CircleSpell(MainTab, RGroup, CastSlot.R, SpellSlot.R, true, RDamage, (x => x.IsAlive && x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient)), 20000, 580, 0.25F, 100, true);
            Damage qDamage = new Damage("Q", (uint)6, QDamage, SharpDX.Color.AliceBlue);
            Damage rDamage = new Damage("R", (uint)4, RDamage, SharpDX.Color.Red);
            Render.AddDamage(qDamage);
            Render.AddDamage(rDamage);
            qDamage.IsOn = true;
            rDamage.IsOn = true;
            GameEvents.OnCreateObject += ObjectInspector;
            CoreEvents.OnCoreMainInputAsync += MainInput;
            CoreEvents.OnCoreRender += BarrelDrawer;
            rCircle.IsOn.IsOn = true;
        }

        private void BarrelDrawer()
        {
            if (!DrawBarrels.IsOn) return;
            foreach (AIBaseClient barrel in Barrels.deepCopy())
            {
                if (barrel.Position.IsOnScreen() && barrel.IsAlive)
                {
                    RenderFactory.DrawNativeCircle(barrel.Position, 60, Color.Blue, 1);
                    
                }
            }
            List<AIBaseClient> barrelList = Barrels.OrderByDescending(x => (GameEngine.GameTime - x.BuffManager.ActiveBuffs.First().StartTime)).ToList();
            for (int i = 0; i < barrelList.Count; i++)
            {
                if (barrelList[i].Position.IsOnScreen())
                {
                    RenderFactory.DrawText($"{i+1}", barrelList[i].Position.ToW2S(), Color.Green);
                }
            }
        }

        private Task MainInput() 
        {
            bool origIsOn = qTargeted.IsOn.IsOn;
            qTargeted.IsOn.IsOn = false;
            GPBarrelPos BarrelObj = ExpGetBarrelPosition(null, null);
            if (BarrelObj.ValidBarrel && BarrelObj.ValidTarget && EEnabled
                .IsOn)
            {
                if (Env.EReady && Env.ELevel >= 1 && Env.QReady) 
                {
                    if (!BarrelObj.ValidPosition && HealthThresholdReached(BarrelObj.SelectedBarrel)) {
                        SpellCastProvider.CastSpell(CastSlot.Q, BarrelObj.SelectedBarrel.Position, 0.25F);
                    } else if (Env.EReady && Env.Spells.GetSpellClass(SpellSlot.E).Cooldown <= 0 && HealthThresholdReached(BarrelObj.SelectedBarrel, true)) {
                        SpellCastProvider.CastSpell(CastSlot.E, BarrelObj.SelectedPosition, 0.25F);
                        bool targetChampsOnlyOrig = Orbwalker.TargetChampionsOnly;
                        Orbwalker.TargetChampionsOnly = false;
                        SpellCastProvider.CastSpell(CastSlot.Q, BarrelObj.SelectedBarrel.Position, 0.25F);
                        Orbwalker.TargetChampionsOnly = targetChampsOnlyOrig;
                    }
                }
            }
            if (Env.WReady && Env.WLevel > 0 && WEnabled.IsOn)
            {
                if (IsCrowdControlledButCanCleanse(Env.Me()))
                {
                    SpellCastProvider.CastSpell(CastSlot.W, 0.25F);
                }
            }
            qTargeted.IsOn.IsOn = origIsOn;
            return Task.CompletedTask;
        }

        private bool HealthThresholdReached(AIBaseClient barrel, bool eplace=false)
        {
            float[] times = new float[] { 2F, 1F, 0.5F };
            float HealthLossPerS = ((Env.Me().Level >= 13) ? times[2] : (Env.Me().Level >= 7) ? times[1] : times[0]);
            float timeTillImpact = barrel.Distance / 2600;
            if (eplace)
                timeTillImpact += 0.2F;
            //Logger.Log($"Time till impact: {timeTillImpact}");
            float activebarreltime = GameEngine.GameTime - barrel.BuffManager.ActiveBuffs.First().StartTime;
            //Logger.Log($"Time since barrel placement: {activebarreltime}");
            float HealthLossSincePlacement = activebarreltime / HealthLossPerS;
            //Logger.Log($"Health Loss Since Placement: {HealthLossSincePlacement}");
            float ActualHealthLoss = HealthLossSincePlacement + ((float)Math.Pow(HealthLossPerS, -1) * timeTillImpact);
            //Logger.Log($"Actual Health Loss: {ActualHealthLoss}");
            if (ActualHealthLoss >= 2)
                return true;
            return false;

        }

        private Task ObjectInspector(List<AIBaseClient> callbackObjectList, AIBaseClient callbackObject, float callbackGameTime)
        {
            if (callbackObject.ModelName.Contains("Barrel", StringComparison.OrdinalIgnoreCase) && callbackObject.Health == 3)
                Logger.Log($"{callbackObject} - {callbackObject.BaseObjectTypesAssociated[2]} - {callbackObject.CombatType}");
            return Task.CompletedTask;
        }
    }
}
