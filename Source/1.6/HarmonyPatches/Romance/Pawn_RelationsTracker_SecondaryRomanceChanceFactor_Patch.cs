using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryRomanceChanceFactor))]
    public static class Pawn_RelationsTracker_SecondaryRomanceChanceFactor_Patch
    {
        public static void Postfix(ref float __result, Pawn ___pawn, Pawn otherPawn)
        {
            if(__result==0f) return;
            __result = SexualityHelper.EvaluateRomPreference(___pawn, otherPawn, __result);
        }
    }
}