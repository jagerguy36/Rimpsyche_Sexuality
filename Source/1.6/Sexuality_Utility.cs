using LudeonTK;
using System.Text;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public static class Sexuality_Utility
    {

        [DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void LogPawnPsychePreference(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche != null)
            {
                var psychePref = compPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_PsychePreference);
                if (psychePref == null || psychePref.Count == 0)
                {
                    Log.Message("None");
                    return;
                }
                var parts = new StringBuilder();
                for (int i = 0; i < psychePref.Count; i++)
                {
                    if (string.IsNullOrEmpty(psychePref[i].stringKey)) continue;
                    if (parts.Length > 0) parts.Append(" | ");
                    parts.Append($"{pawn.Name}'s preference || {psychePref[i].stringKey}: {psychePref[i].target:F2} ({psychePref[i].importance:F2})");
                }
                Log.Message(parts.ToString());
            }
        }
    }
}
