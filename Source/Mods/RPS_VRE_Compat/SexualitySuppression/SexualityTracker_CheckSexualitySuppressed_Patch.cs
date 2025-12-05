using HarmonyLib;
using Verse;
using VREAndroids;

namespace Maux36.RimPsyche.Sexuality.RPS_VRE_Compat
{
    [HarmonyPatch(typeof(Pawn_SexualityTracker), "CheckSexualitySuppressed")]
    public static class SexualityTracker_CheckSexualitySuppressed_Patch
    {
        public static bool Prefix(Pawn ___pawn, ref bool __result)
        {
            Log.Message($"{___pawn.Name} patched CheckSexualitySuppressed call");
            if (!HarmonyInit.androidPawnkindShorthash.Contains(___pawn.kindDef.shortHash)) return true;
            if (___pawn.Emotionless())
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_SexualityTracker), nameof(Pawn_SexualityTracker.DirtyGeneCache))]
    public static class SexualityTracker_DirtyGeneCache_Patch
    {
        public static void Postfix(Pawn_SexualityTracker __instance)
        {
            __instance.DirtySuppressedCheck();
        }
    }
}
