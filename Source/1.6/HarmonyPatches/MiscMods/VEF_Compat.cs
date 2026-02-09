using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class VEF_Compat
    {
        [HarmonyPatch]
        public static class VEF_ApplyGeneEffects_Patch
        {
            public static bool Prepare()
            {
                if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core"))
                {
                    return true;
                }
                return false;
            }
            static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("VEF.Genes.GeneUtils");
                return AccessTools.Method(type, "ApplyGeneEffects");
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var geneExtensionType = AccessTools.TypeByName("VEF.Genes.GeneExtension");
                var forceFemaleField = AccessTools.Field(geneExtensionType, "forceFemale");
                var forceMaleField = AccessTools.Field(geneExtensionType, "forceMale");

                var methodForceGender = AccessTools.Method(typeof(VEF_ApplyGeneEffects_Patch), nameof(ForceGender));
                if (geneExtensionType == null || forceFemaleField == null || forceMaleField==null)
                {
                    Log.Error($"[Rimpsyche - Sexuality] Rimpsyche failed to patch VRE.Genes.GeneUtils.ApplyGeneEffect");
                    foreach (var code in instructions) yield return code;
                    yield break;
                }
                var codes = instructions.ToList();
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i>5 && codes[i-5].opcode == OpCodes.Ldfld)
                    {
                        if ((codes[i - 5].OperandIs(forceFemaleField) || codes[i - 5].OperandIs(forceMaleField)) && codes[i].opcode == OpCodes.Stfld)
                        {
                            yield return new CodeInstruction(OpCodes.Call, methodForceGender);
                            continue;
                        }
                    }
                    yield return codes[i];
                }
            }
            private static void ForceGender(Pawn pawn, Gender futureGender)
            {
                if (pawn.gender == futureGender)
                    return;
                pawn.gender = futureGender;
                var compPsyche = pawn.compPsyche();
                if (compPsyche == null)
                    return;
                if (!PawnGenerator.IsBeingGenerated(pawn))
                {
                    compPsyche.Sexuality.Notify_Sexchange();
                }
                else
                {
                    //Pawn gender was reversed during generation. Reverse Direction of the Sexual Orientation
                    compPsyche.Sexuality.SetmKinsey(1f - compPsyche.Sexuality.MKinsey);
                }
            }
        }
    }
}
