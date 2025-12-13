using HarmonyLib;
using rjw;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(GenderHelper), nameof(GenderHelper.ChangeSex), [typeof(Pawn), typeof(GenderHelper.Sex), typeof(GenderHelper.Sex)])]
    public static class GenderHelper_ChangeSex_Patch
    {
        public static void Postfix(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true) return;
            compPsyche.Sexuality.Notify_Sexchange();
        }
    }
}
