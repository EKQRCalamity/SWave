using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.SDK;
using SyncWave.Base;
using SyncWave.Common.Helper;
using SyncWave.Common.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    internal sealed class AmumuQCalc : DamageCalculation
    {
        internal int[] BaseDamage = new int[] { 0, 70, 95, 120, 145, 170 };
        internal float APScaling = 0.85F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.QLevel >= 1)
            {
                damage = BaseDamage[Env.QLevel];
                damage += APScaling * Env.Me().UnitStats.TotalAbilityPower;
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;

        }
    }
    internal sealed class AmumuECalc : DamageCalculation
    {
        internal int[] BaseDamage = new int[] { 0, 80, 110, 140, 170, 200 };
        internal float APScaling = 0.5F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.ELevel >= 1)
            {
                damage = BaseDamage[Env.ELevel];
                damage += APScaling * Env.Me().UnitStats.TotalAbilityPower;
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;

        }

    }
    internal sealed class AmumuRCalc : DamageCalculation
    {
        internal int[] BaseDamage = new int[] { 0, 200, 300, 400 };
        internal float APScaling = 0.8F;
        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.ELevel >= 1)
            {
                damage = BaseDamage[Env.ELevel];
                damage += APScaling * Env.Me().UnitStats.TotalAbilityPower;
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;

        }
    }
    internal class Amumu : Module
    {
        internal Tab MainTab = new Tab("SyncWave - Amumu");
        internal Group QGroup = new Group("Q Settings");
        internal Group EGroup = new Group("E Settings");
        internal Group RGroup = new Group("R Settings");

        AmumuQCalc QCalc= new AmumuQCalc();
        AmumuECalc ECalc= new AmumuECalc();
        AmumuRCalc RCalc= new AmumuRCalc();

        LineSpell qLine;
        SelfcontainingMidPointCircleSpell eCircle;
        SelfcontainingMidPointCircleSpell rCircle;

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            MainTab.AddGroup(QGroup);
            MainTab.AddGroup(EGroup);
            MainTab.AddGroup(RGroup);
            qLine = new LineSpell(MainTab, QGroup, Oasys.SDK.SpellCasting.CastSlot.Q, Oasys.Common.Enums.GameEnums.SpellSlot.Q, true, QCalc, 1050, 160, 0.25F, 60, false, false, false, false, true, 0);
            eCircle = new SelfcontainingMidPointCircleSpell(MainTab, EGroup, Oasys.SDK.SpellCasting.CastSlot.E, Oasys.Common.Enums.GameEnums.SpellSlot.E, true, ECalc, 350, 0.25F, 35, false, false);
            rCircle = new SelfcontainingMidPointCircleSpell(MainTab, RGroup, Oasys.SDK.SpellCasting.CastSlot.R, Oasys.Common.Enums.GameEnums.SpellSlot.R, true, RCalc, 550, 0.25F, 200, true, false);
            Damage qDamage = new Damage(MainTab, QGroup, "Q", (uint)6, QCalc, SharpDX.Color.Purple);
            Damage eDamage = new Damage(MainTab, EGroup, "E", (uint)8, ECalc, SharpDX.Color.ForestGreen);
            Damage rDamage = new Damage(MainTab, RGroup, "R", (uint)7, RCalc, SharpDX.Color.AliceBlue);
            Render.AddDamage(qDamage);
            Render.AddDamage(eDamage);
            Render.AddDamage(rDamage);
            CoreEvents.OnCoreMainTick += Maintick;
        }

        private Task Maintick()
        {
            qLine.SetPrediction(0.25F, 1100, true);
            return Task.CompletedTask;
        }
    }
}
