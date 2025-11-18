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
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var genderField = AccessTools.Field(typeof(Pawn), nameof(Pawn.gender));
            var newCodes = new List<CodeInstruction>();

            bool genderBlockReached = false;
            bool skipping = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (!genderBlockReached &&
                    codes[i].opcode == OpCodes.Ldarg_1 &&
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
                    if (codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 1.15f)
                    {
                        skipping = false;
                        newCodes.Add(codes[i]);
                    }
                    continue;
                }
                if (genderBlockReached &&
                    codes[i].opcode == OpCodes.Ldloc_S &&
                    i + 2 < codes.Count &&
                    codes[i + 1].opcode == OpCodes.Mul &&
                    codes[i + 2].opcode == OpCodes.Ret)
                {
                    i += 2;
                    continue;
                }

                newCodes.Add(codes[i]);
            }

            return newCodes;
        }

        //Prevent Less stupid romance attempt
        private static bool Prefix(Pawn initiator, Pawn recipient)
        {
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
