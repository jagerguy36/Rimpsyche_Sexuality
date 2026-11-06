using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_Intimacy_Compat
{
    [HarmonyPatch(typeof(SexUtilities), nameof(SexUtilities.ApplyLovinThoughts))]
    public static class SexUtilities_ApplyLovinThoughts_Patch
    {
        public static void Postfix(Pawn pawn, Pawn partner)
        {
            //Both pawns' JobDriver_Sex call this, so each adds its own side.
            var pawnPsyche = pawn.compPsyche();
            if (pawnPsyche?.Enabled == true && partner.compPsyche()?.Enabled == true)
                pawnPsyche.Sexuality.IncrementRelationshipWith(partner, 0.01f);
        }
    }
}