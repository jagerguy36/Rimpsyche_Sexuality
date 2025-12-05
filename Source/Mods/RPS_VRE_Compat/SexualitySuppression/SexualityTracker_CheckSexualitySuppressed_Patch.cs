using HarmonyLib;
using RimWorld;
using Verse;
using VREAndroids;

namespace Maux36.RimPsyche.Sexuality.RPS_VRE_Compat
{
    [HarmonyPatch(typeof(Pawn_SexualityTracker), "CheckSexualitySuppressed")]
    public static class SexualityTracker_CheckSexualitySuppressed_Patch
    {
        public static bool Prefix(Pawn ___pawn, ref bool __result)
        {
            if (!VRE_HarmonyInit.androidPawnkindShorthash.Contains(___pawn.kindDef.shortHash)) return true;
            if (___pawn.Emotionless())
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_GeneTracker), "Notify_GenesChanged")]
    public static class GeneTracker_Notify_GeneChanged
    {
        public static void Postfix(Pawn_GeneTracker __instance, GeneDef addedOrRemovedGene, Pawn ___pawn)
        {
            if (ModsConfig.BiotechActive && VRE_HarmonyInit.androidSuppressionGene.Contains(addedOrRemovedGene.shortHash))
            {
                var compPsyche = ___pawn.compPsyche();
                if (compPsyche != null)
                {
                    compPsyche.Sexuality.DirtySuppressedCheck();
                }
            }
        }
    }
}
