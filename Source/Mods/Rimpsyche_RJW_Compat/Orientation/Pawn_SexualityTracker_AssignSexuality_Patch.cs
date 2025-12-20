using HarmonyLib;
using rjw;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(Pawn_SexualityTracker), "AssignSexuality")]
    public static class Pawn_SexualityTracker_AssignSexuality_Patch
    {
        public static void Postfix(Pawn_SexualityTracker __instance)
        {
            var pawn = __instance.pawn;
            if (PawnGenerator.IsBeingGenerated(pawn))
            {
                return;
            }
            CompRJW.UpdateOrientation(pawn);
        }
    }
}
