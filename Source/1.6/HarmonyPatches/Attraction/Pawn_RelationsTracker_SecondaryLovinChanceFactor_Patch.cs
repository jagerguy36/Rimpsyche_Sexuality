using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor))]
    public static class Pawn_RelationsTracker_SecondaryLovinChanceFactor_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            FieldInfo pawnField = AccessTools.Field(typeof(Pawn_RelationsTracker), "pawn");
            FieldInfo storyField = AccessTools.Field(typeof(Pawn), "story");
            MethodInfo prettinessMethod = AccessTools.Method(typeof(Pawn_RelationsTracker), "PrettinessFactor");
            MethodInfo multiplierMethod = AccessTools.Method(typeof(Pawn_RelationsTracker_SecondaryLovinChanceFactor_Patch), nameof(Pawn_RelationsTracker_SecondaryLovinChanceFactor_Patch.PsycheBasedLovinChance));

            bool chancePatched = false;

            //Skip Trait Sexuality Check
            Label? skipLabel = null;
            int insertIndex = -1;
            //Determine where the skip label should go
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (codes[i].opcode == OpCodes.Ldarg_0 &&
                    i + 3 < codes.Count &&
                    codes[i + 1].opcode == OpCodes.Ldfld && Equals(codes[i + 1].operand, pawnField) &&
                    codes[i + 2].opcode == OpCodes.Ldfld && Equals(codes[i + 2].operand, storyField) &&
                    codes[i + 3].opcode == OpCodes.Brfalse)
                {
                    skipLabel = (Label)codes[i + 3].operand;
                    insertIndex = i;
                    break;
                }
            }
            //If appropriate skip landing position if found insert unconditional skip at insertIndex to skip trait check for sexuality
            if (skipLabel.HasValue && insertIndex >= 0)
            {
                var newInstruction = new CodeInstruction(OpCodes.Br, skipLabel.Value);
                foreach (var lbl in codes[insertIndex].labels) newInstruction.labels.Add(lbl);
                codes[insertIndex].labels.Clear();
                codes.Insert(insertIndex, newInstruction);
                chancePatched = true;
            }

            //Apply Kinsey Sexuality
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && Equals(codes[i].operand, prettinessMethod)
                    && i + 2 < codes.Count
                    && codes[i + 1].opcode == OpCodes.Mul
                    && codes[i + 2].opcode == OpCodes.Ret)
                {
                    var newInstrs = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, pawnField),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, multiplierMethod),
                        new CodeInstruction(OpCodes.Mul)
                    };
                    codes.InsertRange(i + 2, newInstrs);
                    chancePatched = chancePatched && true;
                    break;
                }
            }

            if (!chancePatched)
            {
                Log.Error("[Rimpsyche - Sexuality] Could not patch SecondaryLovinChanceFactor. Pawn sexuality will not work correctly");
            }

            return codes;
        }

        public static float PsycheBasedLovinChance(Pawn pawn, Pawn otherPawn)
        {
            var pawnPsyche = pawn.compPsyche();
            if (pawnPsyche?.Enabled == true)
            {
                float attraction = pawnPsyche.Sexuality.GetAdjustedAttraction(otherPawn);
                if (RimpsycheSexualitySettings.usePreferenceSystem) return attraction;
                return SexualityHelper.EvaluateSexPreference(pawn, otherPawn, attraction);
            }
            //Vanilla logic if psyche not available for some reason.
            if (pawn.story != null && pawn.story.traits != null)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Asexual))
                {
                    return 0f;
                }
                if (!pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
                {
                    if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        if (otherPawn.gender != pawn.gender)
                        {
                            return 0f;
                        }
                    }
                    else if (otherPawn.gender == pawn.gender)
                    {
                        return 0f;
                    }
                }
            }
            return 1f;
        }
    }
}