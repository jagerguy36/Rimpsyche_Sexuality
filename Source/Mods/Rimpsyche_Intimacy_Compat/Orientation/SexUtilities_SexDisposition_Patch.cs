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
            if (compPsyche.Enabled != true) return;
            __result *= compPsyche.Sexuality.GetAdjustedAttraction(recipient);
        }
    }
}
