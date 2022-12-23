using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Tools.Devices;
using Oasys.SDK;
using Oasys.SDK.InputProviders;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SyncWave.Common.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Oasys.SDK.Prediction.MenuSelected;

namespace SyncWave.Common.SpellAim
{
    internal class Prediction
    {
        float Range;
        float Radius;
        float Delay;
        float Speed;
        Vector3 OrigPosition;
        bool CollisionCheck;
        PredictionType Type;

        internal Prediction(PredictionType type, float range, float radius, float delay, float speed, bool collisionCheck, Vector3? origPosition = null)
        {
            this.Type = type;
            this.Range = range;
            this.Radius = radius;
            this.Delay = delay;
            this.Speed = speed;
            this.CollisionCheck = collisionCheck;
            if (origPosition == null)
                origPosition = Env.Me().Position;
            this.OrigPosition = (SharpDX.Vector3)origPosition;
        }

        internal PredictionOutput Predict(GameObjectBase target)
        {
            return GetPrediction(Type, target, Range, Radius, Delay, Speed, OrigPosition, CollisionCheck);
        }
    }
    internal class AimSpell
    {
        private Tab MainTab;
        private Group SpellGroup;

        internal Switch IsOnSwitch = new Switch("Enabled", false);
        internal ModeDisplay TargetSelectMode = new ModeDisplay() { Title = "Target Selector", ModeNames = new() { "NearToMouse", "HeroTarget" }};
        internal KeyBinding SpellKey;
        internal CastSlot SpellCastSlot;
        internal SpellSlot Slot;
        internal Prediction? prediction;

        internal bool IsOn { get; set; } = true;

        private Keys SpellSlotToKey()
        {
            if (Slot == SpellSlot.Q)
            {
                return Keys.Q;
            }
            else if (Slot == SpellSlot.W)
            {
                return Keys.W;
            } 
            else if (Slot == SpellSlot.E)
            {
                return Keys.E;
            }
            else
            {
                return Keys.R;
            }
        }

        private string SpellSlotToString()
        {
            if (Slot == SpellSlot.Q)
            {
                return "Q";
            }
            else if (Slot == SpellSlot.W)
            {
                return "W";
            }
            else if (Slot == SpellSlot.E)
            {
                return "E";
            }
            else
            {
                return "R";
            }
        }

        internal bool isOn => IsOnSwitch.IsOn;

        int Range;

        public AimSpell(int range, Tab tab, CastSlot castSlot, SpellSlot spellSlot)
        {
            AimMenu.Init(tab);
            this.Range = range;
            this.MainTab = tab;
            this.SpellCastSlot = castSlot;
            this.Slot = spellSlot;
            SpellKey = new KeyBinding("Spell Key", SpellSlotToKey());
            SpellGroup = new Group($"{SpellSlotToString()} Settings");
            AimMenu.AimGroup.AddItem(SpellGroup);
            SpellGroup.AddItem(IsOnSwitch);
            SpellGroup.AddItem(TargetSelectMode);
            SpellGroup.AddItem(SpellKey);
            KeyboardProvider.OnKeyPress += KeyPressHandler;
        }

        internal void SetPrediction(PredictionType predictionType, float range, float radius, float delay, float speed, bool collisionCheck, Vector3? origPosition = null)
        {
            prediction = new(predictionType, range, radius, delay, speed, collisionCheck, origPosition);
        }

        internal void KeyPressHandler(Keys keyBeingPressed, Keyboard.KeyPressState pressState)
        {
            if (!isOn || !IsOn)
                return;
            if (AimMenu.UseToggle.IsOn && !AimMenu.Toggled)
                return;
            if (keyBeingPressed == SpellKey.SelectedKey && AimUtils.NearestTargetToMouse(Range) != null && Env.Spells.GetSpellClass(Slot).IsSpellReady)
            {
                GameObjectBase? target;
                if (TargetSelectMode.SelectedModeName == "NearToMouse")
                {
                    target = AimUtils.NearestTargetToMouse(Range);
                    if (prediction != null && target != null)
                    {
                        PredictionOutput pred = prediction.Predict(target);
                        if (pred.HitChance >= HitChance.VeryHigh)
                        {
                            SpellCastProvider.CastSpell(SpellCastSlot, pred.CastPosition.ToW2S());
                            return;
                        }
                    } else if (target != null)
                    {
                        SpellCastProvider.CastSpell(SpellCastSlot, target.Position.ToW2S());
                        return;
                    }
                } else
                {
                    target = AimUtils.BestHeroTarget(Range);
                    if (prediction != null && target != null)
                    {
                        PredictionOutput pred = prediction.Predict(target);
                        if (pred.HitChance >= HitChance.VeryHigh)
                        {
                            SpellCastProvider.CastSpell(SpellCastSlot, pred.CastPosition.ToW2S());
                            return;
                        }
                    }
                    else if (target != null)
                    {
                        SpellCastProvider.CastSpell(SpellCastSlot, target.Position.ToW2S());
                        return;
                    }
                }
                foreach (GameObjectBase enemy in UnitManager.EnemyChampions)
                {
                    if (enemy.Distance < Range)
                    {
                        if (prediction != null && target != null)
                        {
                            PredictionOutput pred = prediction.Predict(target);
                            if (pred.HitChance >= HitChance.VeryHigh)
                            {
                                SpellCastProvider.CastSpell(SpellCastSlot, pred.CastPosition);
                                return;
                            }
                        }
                        else if (target != null)
                        {
                            SpellCastProvider.CastSpell(SpellCastSlot, target.Position.ToW2S());
                            return;
                        }
                    }
                }
            }
        }
    }
}
