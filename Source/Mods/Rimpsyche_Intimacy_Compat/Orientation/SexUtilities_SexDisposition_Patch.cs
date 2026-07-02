using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_Intimacy_Compat
{
    [HarmonyPatch(typeof(SexUtilities), nameof(SexUtilities.SexDisposition))]
    public static class SexUtilities_SexDisposition_Patch
    {
        public static void Postfix(ref float __result, Pawn initiator, Pawn recipient)
        {
            var compPsyche = initiator.compPsyche();
            if (compPsyche?.Enabled != true) return;

            float attraction = compPsyche.Sexuality.GetAdjustedAttraction(recipient);
            if (RimpsycheSexualitySettings.usePreferenceSystem)
                attraction *= Sexuality_Utility.EvaluateSexPreference(initiator, recipient, attraction);
            __result *= attraction;
        }
    }
}
