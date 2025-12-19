using HarmonyLib;
using rjw.Modules.Attraction.StandardPreferences;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(R_Relationship), "WhenNotInRelationship")]
    public static class R_Relationship_WhenNotInRelationship_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var method_LerpDouble = AccessTools.Method(typeof(GenMath), nameof(GenMath.LerpDouble));
            var WillingToCheatMultiplier = AccessTools.Method(typeof(R_Relationship_WhenNotInRelationship_Patch), nameof(R_Relationship_WhenNotInRelationship_Patch.WillingToCheatMult));
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Call && Equals(code.operand, method_LerpDouble))
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, WillingToCheatMultiplier);
                    yield return new CodeInstruction(OpCodes.Mul);
                    continue;
                }
                if (i == codes.Count-1)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, WillingToCheatMultiplier);
                    yield return new CodeInstruction(OpCodes.Mul);
                }

                yield return code;
            }
        }

        public static float WillingToCheatMult(Pawn pawn)
        {
            var observerPsyche = pawn.compPsyche();
            if (observerPsyche?.Enabled == true)
            {
                float cheatFactor = observerPsyche.Evaluate(R_Homewrecking_WhenHomewrecking_Patch.WillingToCheatFactor);
                return cheatFactor;
            }
            return 1f;
        }
    }
}
