using HarmonyLib;
using rjw;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
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
            var pawnPsyche = pawn.compPsyche();
            if (pawnPsyche?.Enabled != true)
            {
                return true;
            }
            var o = pawnPsyche.Sexuality.orientationCategory;
            if (o == SexualOrientation.Asexual || o == SexualOrientation.Developing)
            {
                compRJW.orientation = Orientation.Asexual;
                return false;
            }
            else if(o == SexualOrientation.None)
            {
                compRJW.orientation = Orientation.Asexual;
                return false;
            }

            //Let RJW logic decide coital asexuality, since their separate logic might depend on it.
            if (!Genital_Helper.has_genitals(pawn) && (pawn.kindDef.race.defName.ToLower().Contains("droid") || pawn.kindDef.race.defName.ToLower().Contains("drone")))
            {
                compRJW.orientation = Orientation.Asexual;
                return false;
            }

            //IF the pawn is sexually active, then respect Psyche sexuality.
            int k = pawnPsyche.Sexuality.GetKinseyReport();
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
