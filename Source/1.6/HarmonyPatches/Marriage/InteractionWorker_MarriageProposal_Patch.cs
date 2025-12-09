using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(InteractionWorker_MarriageProposal), nameof(InteractionWorker_MarriageProposal.RandomSelectionWeight))]
    public static class InteractionWorker_MarriageProposal_RandomSelectionWeight_Patch
    {
        public static void Postfix(ref float __result, Pawn initiator, Pawn recipient)
        {
            if (__result == 0f) return;
            var initPsyche = initiator.compPsyche();
            if (initPsyche?.Enabled != true) return;
            __result *= initPsyche.Evaluate(MarriageProposalChanceFactor);
        }

        public static RimpsycheFormula MarriageProposalChanceFactor = new(
            "MarriageProposalChanceFactor",
            (tracker) =>
            {
                float loyalty = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Loyalty);
                float confidence = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Confidence);
                float passion = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float social = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
                float lovedFactor = 1f + loyalty * 0.2f; //0.8~1.2
                float confidenceFactor = 1f + confidence * 0.15f; //0.85~1.15
                float passionFactor = 1f + passion * 0.05f; //0.95~1.05
                float socialFactor = 1f + social * 0.05f; //0.95~1.05
                return lovedFactor * passionFactor * socialFactor * confidenceFactor; //0.6137 ~ 1.52145
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
    }

    [HarmonyPatch(typeof(InteractionWorker_MarriageProposal), nameof(InteractionWorker_MarriageProposal.AcceptanceChance))]
    public static class InteractionWorker_MarriageProposal_AcceptanceChance_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var MarriageAcceptanceBase = AccessTools.Method(typeof(InteractionWorker_MarriageProposal_AcceptanceChance_Patch), nameof(InteractionWorker_MarriageProposal_AcceptanceChance_Patch.MarriageAcceptanceBase));
            var codes = new List<CodeInstruction>(instructions);

            //Loyalty offset
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                //Loyalty Offset
                if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 0.9f)
                {
                    var ldarg_line = new CodeInstruction(OpCodes.Ldarg_1);
                    if (code.labels != null && code.labels.Count > 0)
                    {
                        ldarg_line.labels.AddRange(code.labels);
                    }
                    yield return ldarg_line;
                    yield return new CodeInstruction(OpCodes.Call, MarriageAcceptanceBase);
                    continue;
                }
                yield return code;
            }
        }

        public static float MarriageAcceptanceBase(Pawn recipient)
        {
            var reciPsyche = recipient.compPsyche();
            if (reciPsyche?.Enabled != true) return 0.9f;
            return reciPsyche.Evaluate(MarriageAcceptChanceFactor);
        }

        public static RimpsycheFormula MarriageAcceptChanceFactor = new(
            "MarriageAcceptChanceFactor",
            (tracker) =>
            {
                float baseValue = 0.9f;
                float loyalty = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Loyalty);
                float passion = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float social = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
                float lovedFactor = 1f + loyalty * 0.2f; //0.8~1.2
                float socialFactor = 1f + social * 0.05f; //0.95~1.05
                return baseValue * lovedFactor * socialFactor; //0.9 * (0.76~1.26)
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
    }
}