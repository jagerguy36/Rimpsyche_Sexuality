using HarmonyLib;
using UnityEngine;
using rjw.Modules.Attraction;
using rjw.Modules.Attraction.StandardPreferences;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(R_Homewrecking), "WhenHomewrecking")]
    public static class R_Homewrecking_WhenHomewrecking_Patch
    {
        public static void Postfix(ref float __result, AttractionRequest request)
        {
            if (__result == 0) return;
            var pawn = request.Pawn;
            var observerPsyche = pawn.compPsyche();
            if (observerPsyche?.Enabled == true)
            {
                float trySeduce = observerPsyche.Evaluate(HomewreckSeduceFactor);
                float react = observerPsyche.Evaluate(HomewreckReactFactor);
                __result = trySeduce * react;
            }
        }
        public static RimpsycheFormula HomewreckSeduceFactor = new(
            "HomewreckSeduceFactor",
            (tracker) =>
            {
                var competitive = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Competitiveness);
                var morality = Mathf.Max(tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Morality), 0f);
                return Mathf.Max(1f + 0.25f * competitive - 0.75f * morality, 0f);
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
        public static RimpsycheFormula HomewreckReactFactor = new(
            "HomewreckReactFactor",
            (tracker) =>
            {
                var loyal = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Loyalty);
                var morality = Mathf.Max(tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Morality), 0f);
                return Mathf.Max(1f - 0.2f * loyal - morality, 0f);
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
    }
}
