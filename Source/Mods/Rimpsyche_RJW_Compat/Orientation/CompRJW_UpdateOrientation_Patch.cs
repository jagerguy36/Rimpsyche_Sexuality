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
            //Log.Message($"{pawn.LabelShort} | Update orientation prefix called");
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
            //Log.Message($"{pawn.LabelShort} | Comp null check passed");
            var o = pawnPsyche.Sexuality.orientationCategory;
            //Log.Message($"{pawn.LabelShort} | RPS category: {o}");
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
            //RJW asexuality denotes they are not willing to / unable to have sex
            //RPS asexuality denotes they don't find any gender to be appealing
            //The two is treated differently for both implementation purpose and conceptual purpose.
            if (!Genital_Helper.has_genitals(pawn) && (pawn.kindDef.race.defName.ToLower().Contains("droid") || pawn.kindDef.race.defName.ToLower().Contains("drone")))
            {
                //Log.Message($"{pawn.LabelShort} | Droid case");
                compRJW.orientation = Orientation.Asexual;
                return false;
            }

            //IF the pawn is sexually active, then respect Psyche sexuality.
            int k = pawnPsyche.Sexuality.GetKinseyReport();
            //Log.Message($"{pawn.LabelShort} | Sexually active pawn. Giving orientaiton with kinsey: {k}");
            switch (k)
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
