using LudeonTK;
using System.Text;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public static class Sexuality_Utility
    {
        public static float GetOrientationAttemptFactor(Pawn initiator, Pawn recipient)
        {
            var initPsyche = initiator.compPsyche();
            var reciPsyche = recipient.compPsyche();
            //Return vanilla for non psyche things
            if (initPsyche?.Enabled != true || reciPsyche?.Enabled != true)
            {
                return ((initiator.gender == recipient.gender) ? ((!initiator.story.traits.HasTrait(TraitDefOf.Gay) || !recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 0.15f : 1f) : ((initiator.story.traits.HasTrait(TraitDefOf.Gay) || recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 0.15f : 1f));
            }
            float initOrientationFactor = initPsyche.Sexuality.GetAdjustedAttraction(recipient.gender);
            //TODO
            float reciOrientationFactor = 1; //if initiator knows reci orientation, they should consider this info. How much they care about should depend on confidence, aggressiveness, etc
            return initOrientationFactor * reciOrientationFactor;
        }

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
