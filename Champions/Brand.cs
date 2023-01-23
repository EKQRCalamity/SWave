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
using System.Text;
using System.Threading.Tasks;

namespace SyncWave.Champions
{
    internal class BrandQCalc : DamageCalculation
    {
        internal int[] QBaseDamage = new int[] { 0, 80, 110, 140, 170, 200 };
        internal float QAPScaling = 0.55F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.QLevel >= 1) 
            {
                damage = QBaseDamage[Env.QLevel];
                damage += QAPScaling * Env.Me().UnitStats.TotalAbilityPower;
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal class BrandWCalc : DamageCalculation
    {
        internal int[] WBaseDamage = new int[] { 0, 75, 120, 165, 210, 255 };
        internal float WAPScaling = 0.6F;

        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.WLevel>= 1)
            {
                damage = WBaseDamage[Env.WLevel];
                damage += WAPScaling * Env.Me().UnitStats.TotalAbilityPower;
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal class BrandECalc : DamageCalculation
    {
        internal int[] EBaseDamage = new int[] { 0, 70, 95, 120, 145, 170 };
        internal float EAPScaling = 0.45F;
        internal override float CalculateDamage(GameObjectBase target)
        {
            float damage = 0;
            if (Env.ELevel >= 1)
            {
                damage = EBaseDamage[Env.WLevel];
                damage += EAPScaling * Env.Me().UnitStats.TotalAbilityPower;
                damage = DamageCalculator.CalculateActualDamage(Env.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal class Brand : Module
    {
        internal Tab MainTab = new Tab("SyncWave - Brand");
        internal Group QGroup = new Group("Q Settings");
        internal Group WGroup = new Group("W Settings");
        internal Group EGroup = new Group("E Settings");

        internal static int QRange = 1040;
        internal static int QWidth = 120;
        internal static float QCastTime = 0.25F;

        internal static int WRange = 900;
        internal static int WRadius = 260;
        internal static float WCastTime = 0.25F;

        internal static int ERange = 675;
        internal static float ECastTime = 0.25F;

        BrandQCalc QCalc = new BrandQCalc();
        BrandWCalc WCalc = new BrandWCalc();
        BrandECalc ECalc = new BrandECalc();

        LineSpell qLine;
        CircleSpell wCircle;
        TargetedSpell eTargeted;

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            MainTab.AddGroup(QGroup);
            MainTab.AddGroup(WGroup);
            MainTab.AddGroup(EGroup);
            qLine = new LineSpell(MainTab, QGroup, Oasys.SDK.SpellCasting.CastSlot.Q, Oasys.Common.Enums.GameEnums.SpellSlot.Q, true, QCalc, (x => x.IsAlive && x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient) && x.Distance < QRange), QRange, QWidth, QCastTime, 50, false, true, false, false, true, 0);
            wCircle = new CircleSpell(MainTab, WGroup, Oasys.SDK.SpellCasting.CastSlot.W, Oasys.Common.Enums.GameEnums.SpellSlot.W, true, WCalc, (x => x.IsAlive && x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient) && x.Distance < WRange), WRange, WRadius, WCastTime, 100, false, true);
            eTargeted = new TargetedSpell(MainTab, EGroup, Oasys.SDK.SpellCasting.CastSlot.E, Oasys.Common.Enums.GameEnums.SpellSlot.E, true, ECalc, (x => x.IsAlive && x.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient) && x.Distance < ERange), ERange, ECastTime, 90);
            qLine.SetPrediction(QCastTime, 1600, true);
            wCircle.SetPrediction(0.627F, 10000, false);
            Damage qDamage = new Damage(MainTab, QGroup, "Q", (uint)4, QCalc, SharpDX.Color.AliceBlue);
            Damage wDamage = new Damage(MainTab, WGroup, "W", (uint)5, WCalc, SharpDX.Color.Aquamarine);
            Damage eDamage = new Damage(MainTab, EGroup, "E", (uint)6, ECalc, SharpDX.Color.Blue);
            Render.AddDamage(qDamage);
            Render.AddDamage(wDamage);
            Render.AddDamage(eDamage);
            CoreEvents.OnCoreMainTick += MainTick;
        }

        private Task MainTick()
        {
            qLine.SetPrediction(QCastTime, 1600, true);
            wCircle.SetPrediction(0.627F, 10000, false);
            return Task.CompletedTask;
        }
    }
}
