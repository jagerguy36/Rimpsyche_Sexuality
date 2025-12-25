using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_Intimacy_Compat
{
    [HarmonyPatch(typeof(InteractionWorker_Seduce), nameof(InteractionWorker_Seduce.RandomSelectionWeight))]
    public static class InteractionWorker_Seduce_Patch
    {
        public static void Postfix(ref float __result, Pawn initiator, Pawn recipient)
        {
            if (__result == 0) return;
            var compPsyche = initiator.compPsyche();
            if (compPsyche?.Enabled != true) return;
            __result *= compPsyche.Sexuality.GetAdjustedSexdrive();
        }
    }
}
