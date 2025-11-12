using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(RelationsUtility), nameof(RelationsUtility.AttractedToGender))]
    public static class RelationsUtility_AttractedToGender_Patch
    {
        private static bool Prefix(ref bool __result, Pawn pawn, Gender gender)
        {
            var initPsyche = pawn.compPsyche();
            if (initPsyche?.Enabled != true) return true;
            if (initPsyche.Sexuality.GetAdjustedAttraction(gender) > 0f)
            {
                __result = true;
                return false;
            }
            else
            {
                __result = false;
                return false;
            }
        }
    }
}
