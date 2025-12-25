using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_Intimacy_Compat
{
    [HarmonyPatch(typeof(CommonChecks), nameof(CommonChecks.AreMutuallyAttracted))]
    public static class CommonChecks_AreMutuallyAttracted_Patch
    {
        public static bool Prefix(ref bool __result, Pawn a, Pawn b)
        {
            var aPsyche = a.compPsyche();
            var bPsyche = b.compPsyche();
            if (aPsyche?.Enabled != true || bPsyche?.Enabled != true) return true;
            __result = aPsyche.Sexuality.GetAdjustedAttraction(b) > 0f && bPsyche.Sexuality.GetAdjustedAttraction(a) > 0f;
            return false;
        }
    }
}
