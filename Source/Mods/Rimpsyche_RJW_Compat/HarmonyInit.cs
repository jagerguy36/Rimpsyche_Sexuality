using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            Log.Message("[Rimpsyche - Sexuality] Patching RJW");
            MethodInfo RJW_TraitSet_GainTrait_Patch_fixes = typeof(TraitSet).GetMethod("GainTrait");
            MethodInfo RJW_TraitSet_RemoveTrait_Patch_fixes = typeof(TraitSet).GetMethod("RemoveTrait");
            MethodInfo RJW_CompAbilityEffect_WordOfLove_ValidateTarget_Patch_fixes = typeof(CompAbilityEffect_WordOfLove).GetMethod("ValidateTarget");
            var harmony = new Harmony("rimworld.mod.Maux.RimPsyche.Sexuality.RJW");
            harmony.Unpatch(RJW_TraitSet_GainTrait_Patch_fixes, HarmonyPatchType.Postfix, "rjw");
            harmony.Unpatch(RJW_TraitSet_RemoveTrait_Patch_fixes, HarmonyPatchType.Postfix, "rjw");
            harmony.Unpatch(RJW_CompAbilityEffect_WordOfLove_ValidateTarget_Patch_fixes, HarmonyPatchType.Prefix, "rjw");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
