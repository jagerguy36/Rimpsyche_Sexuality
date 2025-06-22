using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{

    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.TryGenerateSexualityTraitFor))]
    public static class TryGenerateSexualityTraitForPatch
    {
        public static void Postfix(Pawn pawn, bool allowGay)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche != null)
            {
                pawn.compPsyche().Sexuality.Initialize(pawn);
            }
        }
    }
}
