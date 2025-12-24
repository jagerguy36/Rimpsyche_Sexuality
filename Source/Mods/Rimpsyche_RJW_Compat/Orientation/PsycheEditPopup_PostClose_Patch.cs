using HarmonyLib;
using rjw;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(PsycheEditPopup), nameof(PsycheEditPopup.PostClose))]
    public static class PsycheEditPopup_PostClose_Patch
    {
        public static void Prefix(PsycheEditPopup __instance, Pawn ___editFor)
        {
            if (___editFor == null) return;
            CompRJW.UpdateOrientation(___editFor);
        }
    }
}
