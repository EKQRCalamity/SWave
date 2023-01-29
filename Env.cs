using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
using Oasys.SDK;
using SyncWave.Common.Enums;
using System.Collections.Generic;

namespace SyncWave
{
    internal class Env
    {
        internal static AIHeroClient Me() => UnitManager.MyChampion;
        internal static int Level => Env.Me().Level;
        internal static SpellBook Spells => Env.Me().GetSpellBook();
        internal static int QLevel => Env.Spells.GetSpellClass(SpellSlot.Q).Level;
        internal static int WLevel => Env.Spells.GetSpellClass(SpellSlot.W).Level;
        internal static int ELevel => Env.Spells.GetSpellClass(SpellSlot.E).Level;
        internal static int RLevel => Env.Spells.GetSpellClass(SpellSlot.R).Level;
        internal static bool QReady => Env.Spells.GetSpellClass(SpellSlot.Q).IsSpellReady;
        internal static bool WReady => Env.Spells.GetSpellClass(SpellSlot.W).IsSpellReady;
        internal static bool EReady => Env.Spells.GetSpellClass(SpellSlot.E).IsSpellReady;
        internal static bool RReady => Env.Spells.GetSpellClass(SpellSlot.R).IsSpellReady;


        internal static List<string> SupportedChamps = new List<string>() { "Orianna", "Twitch", "KogMaw", "TahmKench", "Kalista", "Irelia", "Graves", "Nidalee", "LeeSin", "Blitzcrank", "Gangplank", "Brand", "Cassiopeia", "Vayne", "Veigar" };
        internal static V ModuleVersion => V.Stable;
    }
}
