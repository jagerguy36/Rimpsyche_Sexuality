using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        private static Harmony harmony;
        static HarmonyInit()
        {
            Log.Message("[Rimpsyche - Sexuality] Patching RJW");
            harmony = new Harmony("rimworld.mod.Maux.RimPsyche.Sexuality.RJW");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            LongEventHandler.QueueLongEvent(DelayedUnpatch, "RPS_RJW_Unpatching", false, null);
        }
        static void DelayedUnpatch()
        {
            //Unpatch RJW
            Log.Message("[Rimpsyche - Sexuality] Running delayed RJW patches");
            MethodInfo RJW_TraitSet_GainTrait_Patch_fixes = typeof(TraitSet).GetMethod("GainTrait", new Type[] { typeof(Trait), typeof(bool) });
            MethodInfo RJW_TraitSet_RemoveTrait_Patch_fixes = typeof(TraitSet).GetMethod("RemoveTrait", new Type[] { typeof(Trait), typeof(bool) });
            MethodInfo RJW_CompAbilityEffect_WordOfLove_ValidateTarget_Patch_fixes = typeof(CompAbilityEffect_WordOfLove).GetMethod("ValidateTarget");
            harmony.Unpatch(RJW_TraitSet_GainTrait_Patch_fixes, HarmonyPatchType.Postfix, "rjw");
            harmony.Unpatch(RJW_TraitSet_RemoveTrait_Patch_fixes, HarmonyPatchType.Postfix, "rjw");
            harmony.Unpatch(RJW_CompAbilityEffect_WordOfLove_ValidateTarget_Patch_fixes, HarmonyPatchType.Prefix, "rjw");
        }
    }
}
