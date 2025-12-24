using HarmonyLib;
using rjw;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(Pawn_SexualityTracker), nameof(Pawn_SexualityTracker.InjectData))]
    public static class SexualityTracker_InjectData_Patch
    {
        public static void Postfix(Pawn_SexualityTracker __instance, Pawn ___pawn)
        {
            CompRJW.UpdateOrientation(___pawn);
        }
    }
}
