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
            var pawnPsyche = pawn.compPsyche();
            var partnerPsyche = partner.compPsyche();
            if (pawnPsyche?.Enabled == true && partnerPsyche?.Enabled == true)
            {
                pawnPsyche.Sexuality.IncrementRelationshipWith(partner, 0.01f);
                partnerPsyche.Sexuality.IncrementRelationshipWith(pawn, 0.01f);
            }
        }
    }
}