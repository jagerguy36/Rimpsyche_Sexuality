using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight))]
    public static class InteractionWorker_RomanceAttempt_RandomSelectionWeight_Patch
    {
        //Remove vanilla orientation block.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);

            //Loyalty offset
            var method_GetOffset = AccessTools.Method(typeof(InteractionWorker_RomanceAttempt_RandomSelectionWeight_Patch), nameof(GetLoyaltyOffset));
            var offsetLocal = generator.DeclareLocal(typeof(float)).LocalIndex;
            bool foundFirst = false;
            bool foundSecond = false;

            //Gender block skip
            var genderField = AccessTools.Field(typeof(Pawn), nameof(Pawn.gender));
            bool genderBlockReached = false;
            bool skipping = false;

            var newCodes = new List<CodeInstruction>();

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (!foundFirst && code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 50f)
                {
                    foundFirst = true;
                    newCodes.Add(code);
                    newCodes.Add(new CodeInstruction(OpCodes.Ldarg_1));
                    newCodes.Add(new CodeInstruction(OpCodes.Call, method_GetOffset));
                    newCodes.Add(new CodeInstruction(OpCodes.Stloc, offsetLocal));
                    newCodes.Add(new CodeInstruction(OpCodes.Ldloc, offsetLocal));
                    newCodes.Add(new CodeInstruction(OpCodes.Add));
                    continue;
                }
                if (!foundSecond && code.opcode == OpCodes.Ldc_R4 && (float)code.operand == -50f)
                {
                    foundSecond = true;
                    newCodes.Add(code);
                    newCodes.Add(new CodeInstruction(OpCodes.Ldloc, offsetLocal));
                    newCodes.Add(new CodeInstruction(OpCodes.Add));
                    continue;
                }

                //Skip Gender block
                if (!genderBlockReached &&
                    code.opcode == OpCodes.Ldarg_1 &&
                    codes[i + 1].opcode == OpCodes.Ldfld &&
                    codes[i + 1].operand is FieldInfo fi &&
                    fi == genderField)
                {
                    if (i > 1 && codes[i - 1].opcode == OpCodes.Stloc_S)
                    {
                        genderBlockReached = true;
                        skipping = true;
                        continue;
                    }
                }
                
                if (skipping)
                {
                    if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 1.15f)
                    {
                        skipping = false;
                        newCodes.Add(code);
                    }
                    continue;
                }
                if (genderBlockReached &&
                    code.opcode == OpCodes.Ldloc_S &&
                    i + 2 < codes.Count &&
                    codes[i + 1].opcode == OpCodes.Mul &&
                    codes[i + 2].opcode == OpCodes.Ret)
                {
                    i += 2;
                    continue;
                }

                newCodes.Add(code);
            }

            return newCodes;
        }
        static float GetLoyaltyOffset(Pawn initiator)
        {
            float offset = 0f;
            var initPsyche = initiator.compPsyche();
            if (initPsyche?.Enabled==true)
            {
                offset = initPsyche.Evaluate(LoyaltyRomanceOffset);
            }
            return offset;
        }
        public static RimpsycheFormula LoyaltyRomanceOffset = new(
            "LoyaltyRomanceOffset",
            (tracker) =>
            {
                float loyalty = -150f * tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Loyalty);
                return loyalty;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );

        //TODO: Fix double call on randomselection weight.?????
        //Prevent Less stupid romance attempt
        private static bool Prefix(Pawn initiator, Pawn recipient)
        {
            Log.Message($"PREF InteractionWorker_RomanceAttempt called on {initiator.Name} -> {recipient.Name}");
            return true;
        }

        //Orientation knowledge respect
        private static void Postfix(ref float __result, Pawn initiator, Pawn recipient)
        {
            if (__result == 0f) return;
            var initPsyche = initiator.compPsyche();
            if (initPsyche.Enabled != true) return;

            //Rebuffed thought should deter approach
            //
            //

            //confidence etc factor
            //
            //

            //known orientation should affect factors
            if (!initPsyche.Sexuality.knownOrientation.Contains(recipient.thingIDNumber)) return;
            var reciPsyche = recipient.compPsyche();
            if (reciPsyche.Enabled != true) return;
            var knownReciAttraction = reciPsyche.Sexuality.GetAdjustedAttraction(initiator.gender);
            //Case possible
            if (knownReciAttraction > 0f)
            {

            }
            //Case impossible
            else
            {

            }
        }
    }
}
