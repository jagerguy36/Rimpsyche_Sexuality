using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche.Sexuality.RPS_VRE_Compat
{
    [HarmonyPatch(typeof(Pawn_SexualityTracker), nameof(Pawn_SexualityTracker.Initialize))]
    public static class SexualityTracker_Initialzie_Patch
    {
        public static bool Prefix(Pawn ___pawn, ref bool forceAdult)
        {
            if (HarmonyInit.androidPawnkindShorthash.Contains(___pawn.kindDef.shortHash)) forceAdult = true;
            return true;
        }
    }
}
