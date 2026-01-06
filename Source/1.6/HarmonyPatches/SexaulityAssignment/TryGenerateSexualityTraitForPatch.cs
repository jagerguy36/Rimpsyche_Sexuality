using HarmonyLib;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.TryGenerateSexualityTraitFor))]
    public static class TryGenerateSexualityTraitForPatch
    {
        public static bool Prefix(Pawn pawn, bool allowGay)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche != null)
            {
                //Log.Message($"intercepted trygen sexuality for {pawn.Name}. generating.");
                compPsyche.SexualitySetup(true, allowGay);
                return false;
            }
            return true;
        }
    }
    // [HarmonyPatch(typeof(Pawn_AgeTracker), "RecalculateLifeStageIndex")]
    // public static class Pawn_AgeTracker_RecalculateLifeStageIndex_Patch
    // {
    //     public static void Postfix(Pawn ___pawn, bool ___lifestageChange)
    //     {
    //         if (!___lifestageChange) return;
    //         var compPsyche = ___pawn.compPsyche();
    //         if (compPsyche != null)
    //         {
    //             compPsyche.Sexuality.Notify_LifestageChanged();
    //         }
    //     }
    // }
}
