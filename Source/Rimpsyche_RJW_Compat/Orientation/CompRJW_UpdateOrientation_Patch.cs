using HarmonyLib;
using Maux36.RimPsyche;
using rjw;
using Verse;

namespace Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(CompRJW), nameof(CompRJW.UpdateOrientation))]
    public static class CompRJW_UpdateOrientation_Patch
    {
        public static bool Prefix(Pawn pawn)
        {
            CompRJW compRJW = pawn.GetCompRJW();
            if (compRJW == null)
            {
                return true;
            }
            var observerPsyche = pawn.compPsyche();
            if (observerPsyche?.Enabled != true)
            {
                return true;
            }
            var o = observerPsyche.Sexuality.orientationCategory;
            if (o == SexualOrientation.Asexual || o == SexualOrientation.Developing)
            {
                compRJW.orientation = Orientation.Asexual;
                return false;
            }
            else if(o == SexualOrientation.None)
            {
                compRJW.orientation = Orientation.None;
                return false;
            }
            int k = observerPsyche.Sexuality.GetKinseyReport();
            switch(k)
            {
                case 0:
                    compRJW.orientation = Orientation.Heterosexual; break;
                case 1:
                    compRJW.orientation = Orientation.MostlyHeterosexual; break;
                case 2:
                    compRJW.orientation = Orientation.LeaningHeterosexual; break;
                case 3:
                    compRJW.orientation = Orientation.Bisexual; break;
                case 4:
                    compRJW.orientation = Orientation.LeaningHomosexual ; break;
                case 5:
                    compRJW.orientation = Orientation.MostlyHomosexual; break;
                case 6:
                    compRJW.orientation = Orientation.Homosexual; break;
            }

            return false;
        }
    }
}
