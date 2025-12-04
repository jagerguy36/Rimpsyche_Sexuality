using HarmonyLib;
using rjw;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(OrientationUtility), nameof(OrientationUtility.PreferenceFactor))]
    public static class OrientationUtility_PreferenceFactor_Patch
    {
        public static bool Prefix(ref float __result, Pawn pawn, Pawn partner)
        {
            var observerPsyche = pawn.compPsyche();
            if (observerPsyche?.Enabled == true)
            {
                __result = observerPsyche.Sexuality.GetAdjustedAttraction(partner);
                return false;
            }
            return true;
        }
    }
}
