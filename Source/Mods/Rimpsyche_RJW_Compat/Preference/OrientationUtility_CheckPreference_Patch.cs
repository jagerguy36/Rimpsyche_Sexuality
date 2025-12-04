using HarmonyLib;
using rjw;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(OrientationUtility), nameof(OrientationUtility.CheckPreference))]
    public static class OrientationUtility_CheckPreference_Patch
    {
        public static bool Prefix(ref bool __result, Pawn pawn, Pawn partner)
        {
            var observerPsyche = pawn.compPsyche();
            if (observerPsyche?.Enabled == true)
            {
                __result = observerPsyche.Sexuality.GetAdjustedAttraction(partner) > 0f;
                return false;
            }
            return true;
        }
    }
}
