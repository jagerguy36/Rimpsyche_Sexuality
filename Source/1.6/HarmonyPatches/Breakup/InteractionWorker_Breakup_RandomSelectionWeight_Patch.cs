using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(InteractionWorker_Breakup), "RandomSelectionWeight")]
    public static class InteractionWorker_Breakup_RandomSelectionWeight_Patch
    {
        //Remove vanilla orientation block.
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);

            //Loyalty offset
            var method_GetOffset = AccessTools.Method(typeof(InteractionWorker_Breakup_RandomSelectionWeight_Patch), nameof(GetLoyaltyOffset));
            var offsetLocal = generator.DeclareLocal(typeof(float)).LocalIndex;
            bool foundFirst = false;

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                //Loyalty Offset
                if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 100f)
                {
                    foundFirst = true;
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, method_GetOffset);
                    yield return new CodeInstruction(OpCodes.Stloc, offsetLocal);
                    yield return new CodeInstruction(OpCodes.Ldloc, offsetLocal);
                    yield return new CodeInstruction(OpCodes.Add);
                    continue;
                }
                if (foundFirst && code.opcode == OpCodes.Ldc_R4 && (float)code.operand == -100f)
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldloc, offsetLocal);
                    yield return new CodeInstruction(OpCodes.Add);
                    continue;
                }
                yield return code;
            }
        }
        public static float GetLoyaltyOffset(Pawn initiator)
        {
            float offset = 0f;
            var initPsyche = initiator.compPsyche();
            if (initPsyche?.Enabled == true)
            {
                offset = initPsyche.Evaluate(BreakupLoyaltyOffset);
            }
            return offset;
        }
        //float num = Mathf.InverseLerp(100f, -100f, initiator.relations.OpinionOf(recipient));
        //with loyalty 1 ->  Mathf.InverseLerp(-100f, -300f, value) | no matter how bad the opinion for their current lover is, they will not break up.
        //with loyalty -1 ->  Mathf.InverseLerp(200, 0f, value) | no matter how good the opinion is, it will only have some mitigating effect.
        public static RimpsycheFormula BreakupLoyaltyOffset = new(
            "BreakupLoyaltyOffset",
            (tracker) =>
            {
                float loyalty = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Loyalty);
                if (loyalty > 0) return -200f * loyalty;
                else return -100f * loyalty;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
    }
}