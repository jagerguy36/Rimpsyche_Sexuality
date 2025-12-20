using HarmonyLib;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(Pawn_SexualityTracker), nameof(Pawn_SexualityTracker.DirtyTraitCache))]
    public static class SexualityTracker_DirtyTraitCache_Patch
    {
        public static void Postfix(Pawn_SexualityTracker __instance, bool __result)
        {
            if (!__result) return;
            var pawn = __instance.pawn;
            if (PawnGenerator.IsBeingGenerated(pawn))
            {
                return;
            }
            __instance.Validate();
        }
    }
}
