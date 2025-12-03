using HarmonyLib;
using Maux36.RimPsyche;
using rjw;
using Verse;

namespace Rimpsyche_RJW_Compat
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
