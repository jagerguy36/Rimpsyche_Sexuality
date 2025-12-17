using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_Intimacy_Compat
{
    [HarmonyPatch(typeof(CommonChecks), nameof(CommonChecks.AttractedToGender))]
    public static class CommonChecks_AttractedToGender_Patch
    {
        public static bool Prefix(ref bool __result, Pawn pawn, Gender gender)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche.Enabled != true) return true;
            __result = compPsyche.Sexuality.CanFeelAttractionToGender(gender);
            return false;
        }
    }
}
