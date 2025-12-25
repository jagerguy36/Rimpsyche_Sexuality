using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(CompAbilityEffect_WordOfLove), "ValidateTarget")]
    public static class CompAbilityEffect_WordOfLove_ValidateTarget_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);
            var method_CanCastWordOfLove = AccessTools.Method(typeof(CompAbilityEffect_WordOfLove_ValidateTarget_Patch), nameof(CanCastWordOfLove));
            FieldInfo storyField = AccessTools.Field(typeof(Pawn), "story");
            FieldInfo field_Gender = AccessTools.Field(typeof(Pawn), "gender");
            bool skipping = false;

            var maxCount = codes.Count;
            for (int i = 0; i < maxCount; i++)
            {
                var code = codes[i];
                if (!skipping &&
                    i > 1 &&
                    i + 1 < maxCount &&
                    codes[i - 1].opcode == OpCodes.Brfalse &&
                    codes[i].opcode == OpCodes.Ldloc_0 &&
                    codes[i + 1].opcode == OpCodes.Ldfld && Equals(codes[i + 1].operand, storyField))
                {
                    skipping = true;
                    yield return code;
                    continue;
                }
                if (skipping)
                {
                    if (i - 2 > 0 &&
                        codes[i - 2].opcode == OpCodes.Ldfld && Equals(codes[i - 2].operand, field_Gender) &&
                        code.opcode == OpCodes.Beq_S
                    )
                    {
                        skipping = false;
                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                        yield return new CodeInstruction(OpCodes.Call, method_CanCastWordOfLove);
                        var nCode = new CodeInstruction(OpCodes.Brtrue, code.operand);
                        yield return nCode;
                    }
                    continue;
                }
                yield return code;
            }
        }

        public static bool CanCastWordOfLove(Pawn initiator, Pawn target)
        {
            var initPsyche = initiator.compPsyche();
            if (initPsyche?.Enabled != true)
            {
                //Vanilla logic
                if (initiator.story.traits.HasTrait(TraitDefOf.Bisexual)) return true;
                Gender gender = initiator.gender;
                Gender gender2 = (initiator.story.traits.HasTrait(TraitDefOf.Gay) ? gender : gender.Opposite());
                if (target.gender != gender2) return false;
                return true;
            }
            return initPsyche.Sexuality.CanFeelAttractionToGender(target.gender);
        }
    }
}
