using HarmonyLib;
using Maux36.RimPsyche;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

[HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryLovinChanceFactor")]
public static class Patch_SecondaryLovinChanceFactor
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        FieldInfo pawnField = AccessTools.Field(typeof(Pawn_RelationsTracker), "pawn");
        FieldInfo storyField = AccessTools.Field(typeof(Pawn), "story");
        MethodInfo prettinessMethod = AccessTools.Method(typeof(Pawn_RelationsTracker), "PrettinessFactor");
        MethodInfo multiplierMethod = AccessTools.Method(typeof(Patch_SecondaryLovinChanceFactor), nameof(Patch_SecondaryLovinChanceFactor.KinseyBasedLovinChance));

        bool chancePatched = false;

        //Skip Trait Sexuality Check
        Label? skipLabel = null;
        int insertIndex = -1;
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
                codes.InsertRange(i+2, newInstrs);
                chancePatched = true;
                break;
            }
        }

        if (!chancePatched)
        {
            Log.Error("[Rimpsyche - Sexuality] Could not patch SecondaryLovinChanceFactor. Pawn sexuality will not work correctly");
        }

        return codes;
    }

    public static float KinseyBasedLovinChance(Pawn initiator, Pawn otherPawn)
    {
        var initPsyche = initiator.compPsyche();
        var reciPsyche = otherPawn.compPsyche();
        if (initPsyche == null || reciPsyche == null) return 0;

        return 1f;
    }
}
