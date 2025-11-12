using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.PrettinessFactor))]
    public static class Pawn_RelationsTracker_PrettinessFactor_Patch
    {
        private static void Postfix(ref float __result, Pawn ___pawn, Pawn otherPawn)
        {
            var pawnPsyche = ___pawn.compPsyche();
            if (pawnPsyche?.Enabled != true) return;
            if (__result == 1f) return;
            if (__result < 1f)
            {
                __result *= pawnPsyche.Evaluate(AuthenticUglinessMultiplier);
                return;
            }
            else
            {
                __result *= pawnPsyche.Evaluate(AuthenticBeautyMultiplier);
                return;
            }
        }
        public static RimpsycheFormula AuthenticUglinessMultiplier = new(
            "AuthenticUglinessMultiplier",
            (tracker) =>
            {
                float authenticityFactor = tracker.GetPersonalityAsMult(PersonalityDefOf.Rimpsyche_Authenticity, 2f);
                return authenticityFactor;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
        public static RimpsycheFormula AuthenticBeautyMultiplier = new(
            "AuthenticBeautyMultiplier",
            (tracker) =>
            {
                float authenticityFactor = 1f - 0.5f * tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Authenticity);
                return authenticityFactor;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
    }
}
