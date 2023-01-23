using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.SDK.Rendering;
using Oasys.SDK.Tools;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Oasys.Common.Logic.EB.Prediction;

namespace SyncWave.Common.Helper
{
    internal static class Drawings
    {
        internal static void DrawBox(float x, float y, float w, float h, float lw, Color color)
        {
            Vector2[] vertices =
            {
                new Vector2(x, y),
                new Vector2(x + w, y),
                new Vector2(x + w, y + h),
                new Vector2(x, y + h),
                new Vector2(x, y)
            };

            RenderFactory.D3D9BoxLine.Width = lw;
            RenderFactory.D3D9BoxLine.Begin();
            RenderFactory.D3D9BoxLine.Draw(vertices, color);
            RenderFactory.D3D9BoxLine.End();
        }

        internal static void DrawHPBar(GameObjectBase target, Color color, float lineWidth)
        {
            var resolution = Screen.PrimaryScreen.Bounds;
            var isHero = target.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient);
            var isJungle = false;
            var isJungleBuff = false;
            var isCrab = false;
            var isDragon = false;
            var isBaron = false;
            var isHerald = false;
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Krug", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("SRU_Gromp", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Equals("SRU_Murkwolf", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("Super", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Equals("SRU_Razorbeak", StringComparison.OrdinalIgnoreCase))
            {
                isJungle = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Red", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("SRU_Blue", StringComparison.OrdinalIgnoreCase))
            {
                isJungleBuff = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Crab", StringComparison.OrdinalIgnoreCase))
            {
                isCrab = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Baron", StringComparison.OrdinalIgnoreCase))
            {
                isBaron = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Dragon", StringComparison.OrdinalIgnoreCase))
            {
                isDragon = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_RiftHerald", StringComparison.OrdinalIgnoreCase))
            {
                isHerald = true;
            }

            var barWidth = resolution.Width switch
            {
                >= 2560 => isHero ? 125 :
                           isBaron ? 200 :
                           isDragon ? 170 :
                           isHerald ? 170 :
                           isCrab ? 170 :
                           isJungleBuff ? 170 :
                           isJungle ? 110 :
                           75,
                >= 1920 => isHero ? 105 :
                           isBaron ? 175 :
                           isDragon ? 150 :
                           isHerald ? 150 :
                           isCrab ? 150 :
                           isJungleBuff ? 145 :
                           isJungle ? 95 :
                           60,
                _ => isHero ? 105 :
                           isBaron ? 175 :
                           isDragon ? 150 :
                           isHerald ? 150 :
                           isCrab ? 150 :
                           isJungleBuff ? 145 :
                           isJungle ? 95 :
                           60,
            };
            var barHeight = resolution.Height switch
            {
                >= 1440 => isHero ? 13 :
                           isBaron ? 13 :
                           isDragon ? 13 :
                           isHerald ? 13 :
                           isCrab ? 13 :
                           isJungleBuff ? 13 :
                           isJungle ? 4 :
                           6,
                >= 1080 => isHero ? 10 :
                           isBaron ? 10 :
                           isDragon ? 10 :
                           isHerald ? 10 :
                           isCrab ? 10 :
                           isJungleBuff ? 10 :
                           isJungle ? 3 :
                           5,
                _ => isHero ? 10 :
                           isBaron ? 10 :
                           isDragon ? 10 :
                           isHerald ? 10 :
                           isCrab ? 10 :
                           isJungleBuff ? 10 :
                           isJungle ? 3 :
                           5,
            };
            var healthBarOffset = resolution.Width switch
            {
                >= 2560 => isHero ? new Vector2(-14, -22) :
                           isBaron ? new Vector2(-60, -28) :
                           isDragon ? new Vector2(-32, -70) :
                           isHerald ? new Vector2(-45, -22) :
                           isCrab ? new Vector2(-45, -22) :
                           isJungleBuff ? new Vector2(-45, -22) :
                           isJungle ? new Vector2(-14, -4) :
                           new Vector2(3, -6),
                >= 1920 => isHero ? new Vector2(-6, -18) :
                           isBaron ? new Vector2(-50, -22) :
                           isDragon ? new Vector2(-32, -55) :
                           isHerald ? new Vector2(-35, -18) :
                           isCrab ? new Vector2(-35, -18) :
                           isJungleBuff ? new Vector2(-35, -18) :
                           isJungle ? new Vector2(-6, -4) :
                           new Vector2(8, -6),
                _ => isHero ? new Vector2(-6, -18) :
                           isBaron ? new Vector2(-50, -22) :
                           isDragon ? new Vector2(-32, -55) :
                           isHerald ? new Vector2(-35, -18) :
                           isCrab ? new Vector2(-35, -18) :
                           isJungleBuff ? new Vector2(-35, -18) :
                           isJungle ? new Vector2(-6, -4) :
                           new Vector2(8, -6),
            };

            var pos = target.HealthBarScreenPosition;
            pos += healthBarOffset;
            DrawBox(pos.X, pos.Y, barWidth, barHeight, lineWidth, color);
        }

        internal static void DrawMultiDamage(GameObjectBase target, List<Damage> damageList)
        {
            if (target != null)
                Logger.Log($"Target given: {target.ModelName}");
            if (target == null)
                throw new Exception("Null target given.");
            if (damageList.Count == 0) return;
            damageList = damageList.OrderByDescending(x => x.Priority).ToList();

            float tempHealth = target.Health;
            List<Damage> actualDmgList = new();
            List<float> damages = new();
            List<Color> colors = new();
            List<string> names = new();
            for (int i = 0; i < damageList.Count; i++)
            {
                Damage damage = damageList[i];
                Logger.Log(damage.Name);
                float prioDamage = damage.GetDamage(target);
                float tempPrioDamage = prioDamage;
                tempHealth -= prioDamage;
                float tempFullDamage = (target.Health - tempHealth);
                if (tempFullDamage > target.Health)
                    tempPrioDamage = prioDamage - (tempFullDamage - target.Health);
                if ((tempHealth) >= -(prioDamage))
                {
                    actualDmgList.Add(damageList[i]);
                    
                    damages.Add(damage.GetDamage(target));
                    colors.Add(damage.Color);
                    if (tempPrioDamage / target.MaxHealth * 100 <= Render.Threshold && Env.ModuleVersion == Enums.V.InTesting)
                        Logger.Log($"Remove name for : {damage.Name}");
                    names.Add((tempPrioDamage / target.MaxHealth * 100 <= Render.Threshold) ? "" : damage.Name);
                    if (Env.ModuleVersion == Enums.V.InTesting)
                    {
                        Logger.Log($"Added: {damageList[i].Name}.");
                    }
                }
                else
                {
                    if (Env.ModuleVersion == Enums.V.InTesting)
                    {
                        Logger.Log($"Skipped: {damageList[i].Name}.");
                    }
                    break;
                }
            }
            actualDmgList = actualDmgList.OrderByDescending(x => x.Priority).ToList();
            if (damages.Count != colors.Count || damages.Count != names.Count)
                return;

            float fullDamage = 0;
            foreach (Damage damage in actualDmgList)
            {
                fullDamage += damage.GetDamage(target);
            }

            for (int i = 0; i <= damages.Count - 1; i++)
            {
                int index = damages.Count - (i + 1);
                DrawDamageAboveHP(target, fullDamage, colors[index], 1, false, names[index]);
                fullDamage -= damages[index];
            }
            
        }

        internal static void DrawDamageAboveHP(GameObjectBase target, float damage, Color color, uint position = 1, bool line = false, string name = "")
        {
            if (target is null || !target.W2S.IsValid() || !target.IsVisible || damage <= 1)
            {
                return;
            }
            if (line)
                DrawDamageLine(target, damage, 2, color, (position == 0)? 1 : position, 0, name);
            var resolution = Screen.PrimaryScreen.Bounds;
            var isHero = target.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient);
            var isJungle = false;
            var isJungleBuff = false;
            var isCrab = false;
            var isDragon = false;
            var isBaron = false;
            var isHerald = false;
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Krug", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("SRU_Gromp", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Equals("SRU_Murkwolf", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("Super", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Equals("SRU_Razorbeak", StringComparison.OrdinalIgnoreCase))
            {
                isJungle = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Red", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("SRU_Blue", StringComparison.OrdinalIgnoreCase))
            {
                isJungleBuff = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Crab", StringComparison.OrdinalIgnoreCase))
            {
                isCrab = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Baron", StringComparison.OrdinalIgnoreCase))
            {
                isBaron = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Dragon", StringComparison.OrdinalIgnoreCase))
            {
                isDragon = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_RiftHerald", StringComparison.OrdinalIgnoreCase))
            {
                isHerald = true;
            }
            
            var barHeight = resolution.Height switch
            {
                >= 1440 => isHero ? 13 :
                           isBaron ? 13 :
                           isDragon ? 13 :
                           isHerald ? 13 :
                           isCrab ? 13 :
                           isJungleBuff ? 13 :
                           isJungle ? 4 :
                           4,
                >= 1080 => isHero ? 10 :
                           isBaron ? 10 :
                           isDragon ? 10 :
                           isHerald ? 10 :
                           isCrab ? 10 :
                           isJungleBuff ? 10 :
                           isJungle ? 3 :
                           3,
                _ => isHero ? 10 :
                           isBaron ? 10 :
                           isDragon ? 10 :
                           isHerald ? 10 :
                           isCrab ? 10 :
                           isJungleBuff ? 10 :
                           isJungle ? 3 :
                           3,
            };
            DrawDamageLine(target, damage, barHeight, color, (position == 0) ? 1 : position, 0, name);
        }

        internal static void DrawDamageLine(GameObjectBase target, float damage, float thickness, Color color, uint position, int yOffset = 0, string name = "")
        {
            if (target is null || !target.W2S.IsValid() || !target.IsVisible || damage <= 1)
            {
                return;
            }

            var resolution = Screen.PrimaryScreen.Bounds;
            var isHero = target.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient);
            var isJungle = false;
            var isJungleBuff = false;
            var isCrab = false;
            var isDragon = false;
            var isBaron = false;
            var isHerald = false;
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Krug", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("SRU_Gromp", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Equals("SRU_Murkwolf", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("Super", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Equals("SRU_Razorbeak", StringComparison.OrdinalIgnoreCase))
            {
                isJungle = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Red", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("SRU_Blue", StringComparison.OrdinalIgnoreCase))
            {
                isJungleBuff = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Crab", StringComparison.OrdinalIgnoreCase))
            {
                isCrab = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Baron", StringComparison.OrdinalIgnoreCase))
            {
                isBaron = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Dragon", StringComparison.OrdinalIgnoreCase))
            {
                isDragon = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_RiftHerald", StringComparison.OrdinalIgnoreCase))
            {
                isHerald = true;
            }

            var healthBarOffset = resolution.Width switch
            {
                >= 2560 => isHero ? new Vector2(-14, -22) :
                           isBaron ? new Vector2(-60, -28) :
                           isDragon ? new Vector2(-32, -70) :
                           isHerald ? new Vector2(-45, -22) :
                           isCrab ? new Vector2(-45, -22) :
                           isJungleBuff ? new Vector2(-45, -22) :
                           isJungle ? new Vector2(-14, -4) :
                           new Vector2(3, -4),
                >= 1920 => isHero ? new Vector2(-6, -18) :
                           isBaron ? new Vector2(-50, -22) :
                           isDragon ? new Vector2(-32, -55) :
                           isHerald ? new Vector2(-35, -18) :
                           isCrab ? new Vector2(-35, -18) :
                           isJungleBuff ? new Vector2(-35, -18) :
                           isJungle ? new Vector2(-6, -4) :
                           new Vector2(8, -4),
                _ => isHero ? new Vector2(-6, -18) :
                           isBaron ? new Vector2(-50, -22) :
                           isDragon ? new Vector2(-32, -55) :
                           isHerald ? new Vector2(-35, -18) :
                           isCrab ? new Vector2(-35, -18) :
                           isJungleBuff ? new Vector2(-35, -18) :
                           isJungle ? new Vector2(-6, -4) :
                           new Vector2(8, -4),
            };

            var barWidth = resolution.Width switch
            {
                >= 2560 => isHero ? 125 :
                           isBaron ? 200 :
                           isDragon ? 170 :
                           isHerald ? 170 :
                           isCrab ? 170 :
                           isJungleBuff ? 170 :
                           isJungle ? 110 :
                           75,
                >= 1920 => isHero ? 105 :
                           isBaron ? 175 :
                           isDragon ? 150 :
                           isHerald ? 150 :
                           isCrab ? 150 :
                           isJungleBuff ? 145 :
                           isJungle ? 95 :
                           60,
                _ => isHero ? 105 :
                           isBaron ? 175 :
                           isDragon ? 150 :
                           isHerald ? 150 :
                           isCrab ? 150 :
                           isJungleBuff ? 145 :
                           isJungle ? 95 :
                           60,
            };

            var pos = target.HealthBarScreenPosition;
            pos += healthBarOffset;
            pos.Y -= 16 * position;
            pos.Y += yOffset;
            var end = pos;
            end.X += barWidth * Math.Max(float.Epsilon, target.Health / target.MaxHealth);

            var start = pos;
            var dmgPercent = Math.Max(float.Epsilon, target.Health - damage) / target.MaxHealth;
            start.X += barWidth * dmgPercent;

            RenderFactory.DrawLine(start.X, start.Y, end.X, end.Y, thickness, color);
            if (name != "" || name != String.Empty)
                RenderFactory.DrawText(name, new Vector2(start.X + 3, start.Y - 22), Color.Black, false);
        }

        internal static void DrawExecuteBar(GameObjectBase target, float damage, Color color, bool line = false, int yOffset = 0, int xOffset = 0)
        {
            if (target is null || !target.W2S.IsValid() || !target.IsVisible || damage <= 1)
            {
                return;
            }

            var resolution = Screen.PrimaryScreen.Bounds;
            var isHero = target.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient);
            var isJungle = false;
            var isJungleBuff = false;
            var isCrab = false;
            var isDragon = false;
            var isBaron = false;
            var isHerald = false;
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Krug", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("SRU_Gromp", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Equals("SRU_Murkwolf", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("Super", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Equals("SRU_Razorbeak", StringComparison.OrdinalIgnoreCase))
            {
                isJungle = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Red", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("SRU_Blue", StringComparison.OrdinalIgnoreCase))
            {
                isJungleBuff = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Crab", StringComparison.OrdinalIgnoreCase))
            {
                isCrab = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Baron", StringComparison.OrdinalIgnoreCase))
            {
                isBaron = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Dragon", StringComparison.OrdinalIgnoreCase))
            {
                isDragon = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_RiftHerald", StringComparison.OrdinalIgnoreCase))
            {
                isHerald = true;
            }

            var healthBarOffset = resolution.Width switch
            {
                >= 2560 => isHero ? new Vector2(-14, -22) :
                           isBaron ? new Vector2(-60, -28) :
                           isDragon ? new Vector2(-32, -70) :
                           isHerald ? new Vector2(-45, -22) :
                           isCrab ? new Vector2(-45, -22) :
                           isJungleBuff ? new Vector2(-45, -22) :
                           isJungle ? new Vector2(-14, -4) :
                           new Vector2(3, -4),
                >= 1920 => isHero ? new Vector2(-6, -18) :
                           isBaron ? new Vector2(-50, -22) :
                           isDragon ? new Vector2(-32, -55) :
                           isHerald ? new Vector2(-35, -18) :
                           isCrab ? new Vector2(-35, -18) :
                           isJungleBuff ? new Vector2(-35, -18) :
                           isJungle ? new Vector2(-6, -4) :
                           new Vector2(8, -4),
                _ => isHero ? new Vector2(-6, -18) :
                           isBaron ? new Vector2(-50, -22) :
                           isDragon ? new Vector2(-32, -55) :
                           isHerald ? new Vector2(-35, -18) :
                           isCrab ? new Vector2(-35, -18) :
                           isJungleBuff ? new Vector2(-35, -18) :
                           isJungle ? new Vector2(-6, -4) :
                           new Vector2(8, -4),
            };
            var barHeight = resolution.Height switch
            {
                >= 1440 => isHero ? 13 :
                           isBaron ? 13 :
                           isDragon ? 13 :
                           isHerald ? 13 :
                           isCrab ? 13 :
                           isJungleBuff ? 13 :
                           isJungle ? 4 :
                           6,
                >= 1080 => isHero ? 10 :
                           isBaron ? 10 :
                           isDragon ? 10 :
                           isHerald ? 10 :
                           isCrab ? 10 :
                           isJungleBuff ? 10 :
                           isJungle ? 3 :
                           5,
                _ => isHero ? 10 :
                           isBaron ? 10 :
                           isDragon ? 10 :
                           isHerald ? 10 :
                           isCrab ? 10 :
                           isJungleBuff ? 10 :
                           isJungle ? 3 :
                           5,
            };
            var barWidth = resolution.Width switch
            {
                >= 2560 => isHero ? 125 :
                           isBaron ? 200 :
                           isDragon ? 170 :
                           isHerald ? 170 :
                           isCrab ? 170 :
                           isJungleBuff ? 170 :
                           isJungle ? 110 :
                           75,
                >= 1920 => isHero ? 105 :
                           isBaron ? 175 :
                           isDragon ? 150 :
                           isHerald ? 150 :
                           isCrab ? 150 :
                           isJungleBuff ? 145 :
                           isJungle ? 95 :
                           60,
                _ => isHero ? 105 :
                           isBaron ? 175 :
                           isDragon ? 150 :
                           isHerald ? 150 :
                           isCrab ? 150 :
                           isJungleBuff ? 145 :
                           isJungle ? 95 :
                           60,
            };

            var pos = target.HealthBarScreenPosition;
            pos += healthBarOffset;
            pos.Y += yOffset;
            pos.X += xOffset;

            if (!line)
                DrawBox(pos.X, pos.Y, barWidth * Math.Max(float.Epsilon, damage / target.MaxHealth), barHeight, 2, color);
            else
            {
                pos.X += barWidth * (damage / target.MaxHealth);
                RenderFactory.DrawLine(pos.X, pos.Y, pos.X, pos.Y + barHeight, 2, color);
            }
        }
    }
}
