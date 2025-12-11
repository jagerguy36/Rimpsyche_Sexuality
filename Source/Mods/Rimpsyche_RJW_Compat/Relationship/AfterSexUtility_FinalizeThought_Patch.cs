using HarmonyLib;
using RimWorld;
using rjw;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(AfterSexUtility), nameof(AfterSexUtility.FinalizeThought))]
    public static class AfterSexUtility_FinalizeThought_Patch
    {
        public static void Postfix(ThoughtDef thoughtgain, Pawn pawn, Pawn partner)
        {
            if (thoughtgain == ThoughtDefOf.GotSomeLovin)
            {
                var pawnPsyche = pawn.compPsyche();
                if (pawnPsyche?.Enabled == true)
                {
                    pawnPsyche.Sexuality.IncrementRelationshipWith(partner, 0.01f);
                }
            }
        }
    }
}