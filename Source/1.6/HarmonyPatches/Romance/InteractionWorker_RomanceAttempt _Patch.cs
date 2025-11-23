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

            //Gender Based Willingness
            bool willingnessblockReached = false;
            var storyField = AccessTools.Field(typeof(Pawn), nameof(Pawn.story));
            bool skipping1 = false;

            //Gender block skip
            var genderField = AccessTools.Field(typeof(Pawn), nameof(Pawn.gender));
            bool genderBlockReached = false;
            bool skipping2 = false;

            List<Label> savedLabels = new List<Label>();
            var newCodes = new List<CodeInstruction>();

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                //Loyalty Offset
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

                if (!willingnessblockReached &&
                    code.opcode == OpCodes.Ldarg_1 &&
                    codes[i + 1].opcode == OpCodes.Ldfld &&
                    codes[i + 1].operand is FieldInfo fi1 &&
                    fi1 == storyField)
                {
                    willingnessblockReached = true;
                    skipping1 = true;
                    if (code.labels != null && code.labels.Count > 0)
                    {
                        savedLabels.AddRange(code.labels);
                    }
                    continue;
                }
                if (skipping1)
                {
                    if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 1f &&
                        codes[i - 2].opcode == OpCodes.Ldc_R4 && (float)codes[i - 2].operand == 0.15f)
                    {
                        skipping1 = false;
                        if (savedLabels.Count > 0)
                        {
                            if (code.labels == null)
                            {
                                code.labels = savedLabels;
                            }
                            else
                            {
                                code.labels.AddRange(savedLabels);
                                savedLabels.Clear();
                            }
                        }
                        newCodes.Add(code);
                    }
                    continue;
                }

                //Skip Gender block
                if (!genderBlockReached &&
                    code.opcode == OpCodes.Ldarg_1 &&
                    codes[i + 1].opcode == OpCodes.Ldfld &&
                    codes[i + 1].operand is FieldInfo fi2 &&
                    fi2 == genderField)
                {
                    if (i > 1 && codes[i - 1].opcode == OpCodes.Stloc_S)
                    {
                        genderBlockReached = true;
                        skipping2 = true;
                        continue;
                    }
                }
                
                if (skipping2)
                {
                    if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 1.15f)
                    {
                        skipping2 = false;
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

        //Prevent Less stupid romance attempt
        private static bool Prefix(float __result, Pawn initiator, Pawn recipient)
        {

            var initPsyche = initiator.compPsyche();
            if (initPsyche.Enabled != true) return true;
            if (initPsyche.Sexuality.GetLatestRebuffImpact(recipient) < initPsyche.Evaluate(CanOvercomeRebuffValue))
            {
                __result = 0f;
                return false;
            }
            return true;
        }
        //TODO: Implement Rebuff overcome value
        public static RimpsycheFormula CanOvercomeRebuffValue = new(
            "CanOvercomeRebuffValue",
            (tracker) =>
            {
                return 0f;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );

        private static void Postfix(ref float __result, Pawn initiator, Pawn recipient)
        {
            if (__result == 0f) return;
            //Apply Gender Difference
            if(RimpsycheSettings.romanceAttemptGenderDiff)
            {
                if(initiator.gender==Gender.Female && recipient.gender==Gender.Male)
                {
                    __result *= 0.15f;
                }
            }
            var initPsyche = initiator.compPsyche();
            if (initPsyche.Enabled != true) return;

            //confidence etc factor
            //
            //

            //known orientation should affect factors
            if (!initPsyche.Sexuality.knownOrientation.Contains(recipient.thingIDNumber)) return;
            //Case Other's Orientation Known
            var reciPsyche = recipient.compPsyche();
            if (reciPsyche.Enabled != true) return;
            var knownReciAttraction = reciPsyche.Sexuality.GetAdjustedAttraction(initiator.gender);
            if (knownReciAttraction > 0f)
            {
                float otherOrientationConsideration = 1f;
                //Use knownReciAttraction to calculate how likely they are to initiate
                otherOrientationConsideration *= initPsyche.Evaluate(OrientationSensitivity);
                //
                __result *= otherOrientationConsideration;
            }


        }
        //TODO: Implement values
        public static RimpsycheFormula OrientationSensitivity = new(
            "OrientationSensitivity",
            (tracker) =>
            {
                return 1f;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
    }
}
