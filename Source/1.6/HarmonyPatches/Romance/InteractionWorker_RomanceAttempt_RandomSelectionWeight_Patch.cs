using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    //1. Replace inhumanized check with compPsyche.Enabled check (faster inhumanized check and comp will be used later)
    //2. Modify SLC hard limit of 0.15f based on pawn personality
    //3. Modify num4 line so that loyalty dictates the lerp value
    //4. Remove num5 line. Postfix will implement similar feature.
    //5. Remove num8 line. Postfix will implement similar feature.
    //Using transpiler so that postfix/prefix from other mods survive.
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight))]
    public static class InteractionWorker_RomanceAttempt_RandomSelectionWeight_Patch
    {
        //Remove vanilla orientation block.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);

            //1. CompPsyche
            var method_compPsyche = AccessTools.Method(typeof(PawnExtensions), nameof(PawnExtensions.compPsyche));
            var property_compPsyche_Enabled = AccessTools.PropertyGetter(typeof(CompPsyche), nameof(CompPsyche.Enabled));
            var method_inhumanized = AccessTools.Method(typeof(AnomalyUtility), nameof(AnomalyUtility.Inhumanized));
            var psycheLocal = generator.DeclareLocal(typeof(CompPsyche)).LocalIndex;
            bool gotPsyche = false;
            
            //2. SLC
            var method_slc = AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryRomanceChanceFactor));
            var method_opinionOf = AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.OpinionOf));
            var method_GetSLClimit = AccessTools.Method(typeof(InteractionWorker_RomanceAttempt_RandomSelectionWeight_Patch), nameof(GetSLClimit));
            int slcIndex = -1;
            bool gotSLCindex = false;
            bool modifiedSLClimit = false;

            //3. Loyalty offset
            var method_GetOffset = AccessTools.Method(typeof(InteractionWorker_RomanceAttempt_RandomSelectionWeight_Patch), nameof(GetLoyaltyOffset));
            var offsetLocal = generator.DeclareLocal(typeof(float)).LocalIndex;
            bool foundFirst = false;
            bool foundSecond = false;

            //4. Gender Based Willingness
            bool willingnessblockReached = false;
            var storyField = AccessTools.Field(typeof(Pawn), nameof(Pawn.story));
            bool skipping1 = false;

            //5. Sexuality block skip
            var genderField = AccessTools.Field(typeof(Pawn), nameof(Pawn.gender));
            bool genderBlockReached = false;
            bool skipping2 = false;

            List<Label> savedLabels = new List<Label>();
            var newCodes = new List<CodeInstruction>();

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                //1. CompPsyche
                if (!gotPsyche &&
                    code.opcode == OpCodes.Call && Equals(code.operand, method_inhumanized))
                {
                    //Instead of Inumanized, call compPsyche and place it in the psycheLocal
                    newCodes.Add(new CodeInstruction(OpCodes.Call, method_compPsyche));
                    newCodes.Add(new CodeInstruction(OpCodes.Stloc, psycheLocal));

                    //If psycheLocal is null, return 0f. Otherwise, move onto calling Enabled.
                    newCodes.Add(new CodeInstruction(OpCodes.Ldloc, psycheLocal));
                    var compPsyhce_not_null_label = generator.DefineLabel();
                    newCodes.Add(new CodeInstruction(OpCodes.Brtrue_S, compPsyhce_not_null_label));
                    newCodes.Add(new CodeInstruction(OpCodes.Ldc_R4, 0f));
                    newCodes.Add(new CodeInstruction(OpCodes.Ret));
                    newCodes.Add(new CodeInstruction(OpCodes.Ldloc, psycheLocal).WithLabels(compPsyhce_not_null_label));
                    newCodes.Add(new CodeInstruction(OpCodes.Callvirt, property_compPsyche_Enabled));
                    //If Enabled is true, then continue the code to the next ldarg.1 If not, then the following codes will return 0f
                    codes[i + 1].opcode = OpCodes.Brtrue_S;
                    gotPsyche = true;
                    continue;
                }

                //2. Modify SLC hard limit
                if (!gotSLCindex &&
                    i - 1 > 0 &&
                    codes[i - 1].opcode == OpCodes.Call && Equals(code.operand, method_slc) &&
                    code.IsStloc())
                {
                    slcIndex = code.LocalIndex();
                    newCodes.Add(code);
                    //skip (num < 0.15f) check
                    i += 5;
                    continue;
                }
                if (gotSLCindex &&
                    !modifiedSLClimit &&
                    i - 2 > 0 &&
                    codes[i - 2].opcode == OpCodes.Call && Equals(code.operand, method_opinionOf) &&
                    code.IsLdloc()
                    )
                {
                    newCodes.Add(new CodeInstruction(OpCodes.Ldloc, gotSLCindex)); //load SLC
                    newCodes.Add(new CodeInstruction(code.opcode, code.operand)); //load opinion
                    newCodes.Add(new CodeInstruction(OpCodes.Ldloc, psycheLocal)); // load compPsyche
                    newCodes.Add(new CodeInstruction(OpCodes.Ldarg_2)); //load recipient
                    newCodes.Add(new CodeInstruction(OpCodes.Callvirt, method_GetSLClimit)); //get limit value
                    var passed_SLC_label = generator.DefineLabel();
                    newCodes.Add(new CodeInstruction(OpCodes.Bge_Un_S, passed_SLC_label));//if SLC >= limit, go to passed label
                    newCodes.Add(new CodeInstruction(OpCodes.Ldc_R4, 0f)); //if not, then get 0
                    newCodes.Add(new CodeInstruction(OpCodes.Ret)); // return it
                    newCodes.Add(code.WithLabels(passed_SLC_label)); //if passed, go onto do the original code.
                }

                //3. Modify num4
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

                //4. Remove num5
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

                //5. Skip num8 block. 
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
                    //Skip num8 block until 1.15f is seen.
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
                    //Remove * num8
                    i += 2;
                    continue;
                }
                newCodes.Add(code);
            }

            return newCodes;
        }
        static float GetSLClimit(int initOpinion, CompPsyche initComp, Pawn recipient)
        {
            return 0.15f;
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

            //Consider the other's orientation
            var pReciAttraction;
            if (initPsyche.Sexuality.KnowsOrientationOf(recipient))
                pReciAttraction = reciPsyche.Sexuality.GetAdjustedAttraction(initiator);
            else
                pReciAttraction = 0.5f;
            float otherOrientationConsideration = 4f * (pReciAttraction + initPsyche.Evaluate(OrientationSensitivityOffset));
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
                var confidenceFactor = 0.1f * confidence; //-0.1~0.1
                var hopefulnessFactor = 0.1f * Mathf.Max(0f, openness) * Mathf.Max(0f, optimism - 0.5f);//-0.15~0.05
                var entitlementFactor = 0.1f * Mathf.Max(0f, -compassion) * Mathf.Max(0f, selfInterest);//-0.1~0.1
                return confidenceFactor + hopefulnessFactor + entitlementFactor - 0.25f;//-0.6~0
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
    }
}
