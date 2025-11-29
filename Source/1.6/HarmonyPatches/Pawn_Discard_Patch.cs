using HarmonyLib;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Discard))]
    public static class Pawn_Discard_Patch
    {
        public static void Postfix(Pawn __instance)
        {
            VersionManager.DiscardedPawnThingIDnumber.Add(__instance.thingIDNumber);
        }
    }
}
