using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.PrettinessFactor))]
    public static class Pawn_RelationsTracker_PrettinessFactor_Patch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ref float __result, Pawn ___pawn, Pawn otherPawn)
        {
            __result = Sexuality_Utility.EvaluatePhysicalAttractiveness(___pawn, otherPawn);
            return false;
        }
    }
}
