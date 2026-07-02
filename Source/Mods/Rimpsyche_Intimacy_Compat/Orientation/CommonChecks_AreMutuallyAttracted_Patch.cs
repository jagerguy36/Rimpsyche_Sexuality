using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_Intimacy_Compat
{
    [HarmonyPatch(typeof(CommonChecks), nameof(CommonChecks.AreMutuallyAttracted))]
    public static class CommonChecks_AreMutuallyAttracted_Patch
    {
        public static bool Prefix(ref bool __result, Pawn asker, Pawn candidate)
        {
            var askerPsyche = asker.compPsyche();
            var candidatePsyche = candidate.compPsyche();
            if (askerPsyche?.Enabled != true || candidatePsyche?.Enabled != true) return true;
            __result = askerPsyche.Sexuality.GetAdjustedAttraction(candidate) > 0f && candidatePsyche.Sexuality.GetAdjustedAttraction(asker) > 0f;
            return false;
        }
    }
}
