using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_Intimacy_Compat.Orientation
{
    [HarmonyPatch(typeof(CommonChecks), nameof(CommonChecks.AttractedToGender))]
    public static class CommonChecks_AttractedToGender_Patch
    {
        public static void Postfix(ref bool __result, Pawn pawn, Gender gender)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche.Enabled != true) return;
            __result = compPsyche.Sexuality.CanFeelAttractionToGender(gender);
        }
    }
}
