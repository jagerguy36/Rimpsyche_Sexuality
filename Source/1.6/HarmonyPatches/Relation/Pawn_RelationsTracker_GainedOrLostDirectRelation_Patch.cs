using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "GainedOrLostDirectRelation")]
    public static class Pawn_RelationsTracker_GainedOrLostDirectRelation_Patch
    {
        public static void Postfix(Pawn ___pawn)
        {
            var pawnPsyche = ___pawn.compPsyche();
            if (pawnPsyche?.Enabled != true) return;
            pawnPsyche.Sexuality.DirtyLoversCache();
        }
    }
}
