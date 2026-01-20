using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.PrettinessFactor))]
    public static class Pawn_RelationsTracker_PrettinessFactor_Patch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ref float __result, Pawn ___pawn, Pawn otherPawn)
        {
            float num = 0f;
            if (otherPawn.RaceProps.Humanlike)
            {
                num = otherPawn.GetStatValue(StatDefOf.PawnBeauty);
            }
            if (num < 0f)
            {
                num = Mathf.Max(-2.5f, num);
                __result = 1/(1-num);
                var pawnPsyche = ___pawn.compPsyche();
                if (pawnPsyche?.Enabled != true) return false;
                __result *= pawnPsyche.Evaluate(AuthenticUglinessMultiplier);
                return false;
            }
            if (num > 0f)
            {
                num = Mathf.Min(2.5f, num);
                __result = num + 1f;
                var pawnPsyche = ___pawn.compPsyche();
                if (pawnPsyche?.Enabled != true) return false;
                __result *= pawnPsyche.Evaluate(AuthenticBeautyMultiplier);
                return false;
            }
            __result = 1f;
            return false;
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
