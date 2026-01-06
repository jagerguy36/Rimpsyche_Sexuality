using HarmonyLib;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    
    [HarmonyPatch(typeof(Pawn_AgeTracker), "RecalculateLifeStageIndex")]
    public static class Pawn_AgeTracker_RecalculateLifeStageIndex_Patch
    {
        public static void Postfix(Pawn ___pawn, bool ___lifeStageChange)
        {
            if (!___lifeStageChange) return;
            var compPsyche = ___pawn.compPsyche();
            if (compPsyche != null)
            {
                compPsyche.Sexuality.Notify_LifestageChanged();
            }
        }
    }
}
