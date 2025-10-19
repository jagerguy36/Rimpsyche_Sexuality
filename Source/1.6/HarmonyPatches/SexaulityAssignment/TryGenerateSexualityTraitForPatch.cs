using HarmonyLib;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.TryGenerateSexualityTraitFor))]
    public static class TryGenerateSexualityTraitForPatch
    {
        public static bool Prefix(Pawn pawn, bool allowGay)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche != null)
            {
                Log.Message($"intercepted trygen sexuality for {pawn.Name}. generating.");
                compPsyche.SexualitySetup(true, allowGay);
                return false;
            }
            return true;
        }
    }
}
