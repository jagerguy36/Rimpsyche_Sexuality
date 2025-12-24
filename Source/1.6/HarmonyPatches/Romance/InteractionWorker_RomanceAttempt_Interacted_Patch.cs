using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.Interacted))]
    public static class InteractionWorker_RomanceAttempt_Interacted_Patch
    {
        public static void Postfix(Pawn initiator, Pawn recipient)
        {
            var initPsyche = initiator.compPsyche();
            var reciPsyche = recipient.compPsyche();
            if (reciPsyche?.Enabled != true || initPsyche?.Enabled != true) return;
            
            var initSexuality = initPsyche.Sexuality;

            //Learn orientation
            reciPsyche.Sexuality.LearnOrientationOf(initiator);

            //Unless forced: Remember that initiator is attracted to recipient as well as learning orientation.
            //learning orientation is included in increment
            if (initiator.CurJob?.def != JobDefOf.TryRomance)
                initSexuality.IncrementRelationshipWith(recipient, 0f);
            else
                initSexuality.LearnOrientationOf(recipient);
        }


        //Increment Relationship on success
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);

            var method_PostSuccess = AccessTools.Method(typeof(InteractionWorker_RomanceAttempt_Interacted_Patch), nameof(PostSuccessIncrement));
            FieldInfo field_BecameLover = AccessTools.Field(typeof(TaleDefOf), nameof(TaleDefOf.BecameLover));
            bool patchedSuccess = false;

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                //Loyalty Offset
                if (code.opcode == OpCodes.Ldsfld && Equals(code.operand, field_BecameLover))
                {
                    patchedSuccess = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, method_PostSuccess);
                    yield return code;
                    continue;
                }
                yield return code;
            }
            if (!patchedSuccess)
                Log.Error("RomanceAttempt_Interacted patch failed");
        }

        public static void PostSuccessIncrement(Pawn initiator, Pawn recipient)
        {
            var initPsyche = initiator.compPsyche();
            var reciPsyche = recipient.compPsyche();
            if (reciPsyche?.Enabled != true || initPsyche?.Enabled != true) return;
            initPsyche.Sexuality.IncrementRelationshipWith(recipient, 0f);
            reciPsyche.Sexuality.IncrementRelationshipWith(initiator, 0f);
        }
    }
}