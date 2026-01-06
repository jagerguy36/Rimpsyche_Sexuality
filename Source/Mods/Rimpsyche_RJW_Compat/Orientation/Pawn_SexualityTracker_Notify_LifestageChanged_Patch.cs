using HarmonyLib;
using rjw;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(Pawn_SexualityTracker), "Notify_LifestageChanged")]
    public static class Pawn_SexualityTracker_Notify_LifestageChanged_Patch
    {
        public static void Postfix(Pawn_SexualityTracker __instance)
        {
            var pawn = __instance.pawn;
            CompRJW.UpdateOrientation(pawn);
        }
    }
}
