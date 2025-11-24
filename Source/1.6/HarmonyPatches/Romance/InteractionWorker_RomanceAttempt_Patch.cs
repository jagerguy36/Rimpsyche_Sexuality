using HarmonyLib;
using Maux36.RimPsyche;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "PartnerFactor")]
public static class InteractionWorker_RomanceAttempt_Patch
{
    //Remove vanilla orientation block.
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codes = new List<CodeInstruction>(instructions);

        //Loyalty offset
        var method_GetOffset = AccessTools.Method(typeof(InteractionWorker_RomanceAttempt_Patch), nameof(GetLoyaltyOffset));
        var offsetLocal = generator.DeclareLocal(typeof(float)).LocalIndex;
        bool foundFirst = false;

        List<Label> savedLabels = new List<Label>();

        for (int i = 0; i < codes.Count; i++)
        {
            var code = codes[i];
            //Loyalty Offset
            if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 100f)
            {
                foundFirst = true;
                yield return code;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, method_GetOffset);
                yield return new CodeInstruction(OpCodes.Stloc, offsetLocal);
                yield return new CodeInstruction(OpCodes.Ldloc, offsetLocal);
                yield return new CodeInstruction(OpCodes.Add);
                continue;
            }
            if (!foundFirst && code.opcode == OpCodes.Ldc_R4 && (float)code.operand == -50f)
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
            offset = initPsyche.Evaluate(PartnerFactorLoyaltyOffset);
        }
        return offset;
    }
    public static RimpsycheFormula PartnerFactorLoyaltyOffset = new(
        "PartnerFactorLoyaltyOffset",
        (tracker) =>
        {
            float loyalty = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Loyalty);
            if (loyalty > 0) return -200f * loyalty;
            else return -100f * loyalty;
        },
        RimpsycheFormulaManager.FormulaIdDict
    );
}
