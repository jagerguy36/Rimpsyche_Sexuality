using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(RelationsUtility), nameof(RelationsUtility.AttractedToGender))]
    public static class RelationsUtility_AttractedToGender_Patch
    {
        private static bool Prefix(ref bool __result, Pawn pawn, Gender gender)
        {
            var initPsyche = pawn.compPsyche();
            if (initPsyche?.Enabled != true) return true;
            if (initPsyche.Sexuality.GetAdjustedAttractionToGender(gender) > 0f)
            {
                __result = true;
            }
            else
            {
                __result = false;
            }
            return false;
        }
    }
}
