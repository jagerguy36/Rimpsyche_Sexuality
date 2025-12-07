using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    //TODO: 0.15f hard limit should be adjsted
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
                if (foundFirst && !foundSecond && code.opcode == OpCodes.Ldc_R4 && (float)code.operand == -50f)
                {
                    foundSecond = true;
                    newCodes.Add(code);
                    newCodes.Add(new CodeInstruction(OpCodes.Ldloc, offsetLocal));
                    newCodes.Add(new CodeInstruction(OpCodes.Add));
                    continue;
                }

                //Remove gender based willingness. Make num5 = 1f;
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

                //Skip Gender block (whole num8 block). 
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
                //Skip Gender block until 1.15f is seen.
                if (skipping2)
                {
                    if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 1.15f)
                    {
                        skipping2 = false;
                        newCodes.Add(code);
                    }
                    continue;
                }
                //Remove * num8
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
        //num4 = Mathf.InverseLerp(50f, -50f, value);
        //with loyalty 1 ->  Mathf.InverseLerp(-100f, -200f, value) | no matter how bad the opinion for the current lover is, they will not attempt to seduce others.
        //with loyalty -1 ->  Mathf.InverseLerp(200f, 100f, value) | no matter how good the opinion is, it will have no mitigating effect.
        public static RimpsycheFormula LoyaltyRomanceOffset = new(
            "LoyaltyRomanceOffset",
            (tracker) =>
            {
                float loyalty = -150f * tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Loyalty);
                return loyalty;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );

        private static void Postfix(ref float __result, Pawn initiator, Pawn recipient)
        {
            if (__result == 0f) return;
            var initPsyche = initiator.compPsyche();
            var reciPsyche = recipient.compPsyche();
            //Return vanilla for non psyche things
            if (initPsyche?.Enabled != true || reciPsyche?.Enabled != true)
            {
                __result *= ((initiator.gender == recipient.gender) ? ((!initiator.story.traits.HasTrait(TraitDefOf.Gay) || !recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 0.15f : 1f) : ((initiator.story.traits.HasTrait(TraitDefOf.Gay) || recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 0.15f : 1f));
                return;
            }

            //Prevent stupid romance attempt
            //Relationship Moudle will implement its own method to block interaction.
            if (!Rimpsyche.RelationshipModuleLoaded && initPsyche.Sexuality.GetLatestRebuffImpact(recipient) < initPsyche.Evaluate(CanOvercomeRebuffValue))
            {
                __result = 0f;
                return;
            }

            //Apply Gender Difference
            if (RimpsycheSettings.romanceAttemptGenderDiff)
            {
                if (initiator.gender == Gender.Female && recipient.gender == Gender.Male)
                {
                    __result *= 0.15f;
                }
            }

            //Flat confidence factor
            __result *= initPsyche.Evaluate(FlatConfidenceAttemptFactor);

            //Recipient Partner Consideration
            if (!new HistoryEvent(recipient.GetHistoryEventForLoveRelationCountPlusOne(), recipient.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
            {
                __result *= initPsyche.Evaluate(CompeteForLoveFactor);
            }

            //If orientation unknown, then just return
            if (!initPsyche.Sexuality.KnowsOrientationOf(recipient)) return;
            //Case Other's Orientation Known
            var knownReciAttraction = reciPsyche.Sexuality.GetAdjustedAttraction(initiator);
            float otherOrientationConsideration = 4f * (knownReciAttraction + initPsyche.Evaluate(OrientationSensitivityOffset));
            __result *= Mathf.Clamp01(otherOrientationConsideration);
        }
        public static RimpsycheFormula CanOvercomeRebuffValue = new(
            "CanOvercomeRebuffValue",
            (tracker) =>
            {
                var confidence = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Confidence);
                return -3f - 3f * confidence;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
        public static RimpsycheFormula CompeteForLoveFactor = new(
            "CompeteForLoveFactor",
            (tracker) =>
            {
                var cooperative = Mathf.Min(tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Competitiveness), 0f);
                return 1f + 0.8f * cooperative;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
        public static RimpsycheFormula FlatConfidenceAttemptFactor = new(
            "FlatConfidenceAttemptFactor",
            (tracker) =>
            {
                var confidence = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Confidence);
                return 1f + 0.1f * confidence;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
        public static RimpsycheFormula OrientationSensitivityOffset = new(
            "OrientationSensitivityOffset",
            (tracker) =>
            {
                var confidence = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Confidence);
                var openness = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Openness);
                var optimism = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Optimism);
                var compassion = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Compassion);
                var selfInterest = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_SelfInterest);
                var confidenceFactor = 0.1f * confidence;
                var hopefulnessFactor = 0.1f * Mathf.Max(0f, openness) * Mathf.Max(0f, optimism - 0.5f);
                var entitlementFactor = 0.1f * Mathf.Max(0f, -compassion) * Mathf.Max(0f, selfInterest);
                return confidenceFactor + hopefulnessFactor + entitlementFactor - 0.25f;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
    }
}
