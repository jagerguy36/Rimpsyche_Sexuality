using HarmonyLib;
using rjw;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(Pawn_SexualityTracker), "AssignSexuality")]
    public static class Pawn_SexualityTracker_AssignSexuality_Patch
    {
        public static void Postfix(Pawn_SexualityTracker __instance)
        {
            var pawn = __instance.pawn;
            CompRJW.UpdateOrientation(pawn);
        }
    }
}
