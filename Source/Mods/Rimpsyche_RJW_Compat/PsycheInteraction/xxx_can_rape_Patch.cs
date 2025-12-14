using HarmonyLib;
using rjw;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(xxx), nameof(xxx.can_rape))]
    public static class xxx_can_rape_Patch
    {
        public static bool Prefix(ref bool __result, Pawn pawn, bool forced)
        {
            if (forced)
                return true;
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled == true)
            {
                var personality = compPsyche.Personality;
                //Pawns who are more than somewhat compassionate will never violate others
                if (personality.GetPersonality(PersonalityDefOf.Rimpsyche_Compassion) >= 0.25f)
                {
                    __result = false;
                    return false;
                }
                //Pawns who are more than somewhat moral will never violate others
                if (personality.GetPersonality(PersonalityDefOf.Rimpsyche_Morality) >= 0.25f)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}
