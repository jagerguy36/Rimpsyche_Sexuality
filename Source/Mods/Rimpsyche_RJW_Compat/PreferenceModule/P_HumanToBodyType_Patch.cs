using HarmonyLib;
using rjw;
using rjw.Modules.Attraction;
using rjw.Modules.Attraction.StandardPreferences;
using System.Collections.Generic;
using static rjw.GenderHelper;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    [HarmonyPatch(typeof(P_HumanToBodyType), "SelectFromBody")]
    public static class P_HumanToBodyType_Patch
    {
        public static bool Prepare()
        {
            if (RimpsycheSexualitySettings.usePreferenceSystem)
            {
                return true;
            }
            return false;
        }
        public static Dictionary<int, StandardizedBody> mBodyTypeDict = new Dictionary<int, StandardizedBody>
        {
            { 0, StandardizedBody.Unknown },
            { 1, StandardizedBody.Thin },
            { 2, StandardizedBody.Male },
            { 3, StandardizedBody.Hulk },
            { 4, StandardizedBody.Fat },
        };
        public static Dictionary<int, StandardizedBody> fBodyTypeDict = new Dictionary<int, StandardizedBody>
        {
            { 0, StandardizedBody.Unknown },
            { 1, StandardizedBody.Thin },
            { 2, StandardizedBody.Female },
            { 3, StandardizedBody.Hulk },
            { 4, StandardizedBody.Fat },
        };
        public static bool Prefix(ref float __result, BodyTypeData data, ref AttractionRequest request)
        {
            if (!RimpsycheDatabase.activePreferenceHashSet.Contains(DefOfRimpsycheSexuality.Rimpsyche_BodyTypePreference.shortHash))
                return true;
            var observer = request.Pawn;
            var target = request.Target;
            float result = 1f;

            // Don't do anything with unknown bodies.
            if (data.TargetBody == StandardizedBody.Unknown)
            {
                result = 1f;
            }
            // Depress strongly for animals (zoophiles will override this).
            else if (data.TargetBody == StandardizedBody.Animal || data.TargetBody == StandardizedBody.Insectoid)
            {
                if (!xxx.is_zoophile(request.Pawn))
                {
                    result = 0.45f;
                }
                else
                {
                    result = 1f;
                }
            }
            // Depress strongly for the abdomen bursting machines.
            else if (data.TargetBody == StandardizedBody.Mechanoid)
            {
                result = 0.45f;
            }
            else if (request.Pawn.compPsyche() is { Enabled: true } observerPsyche)
            {
                var bodyPreference = observerPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_BodyTypePreference);
                if (bodyPreference != null)
                {
                    var targetGender = GenderHelper.GetSex(target);
                    var prefValue = 0f;
                    if (targetGender == Sex.Male)
                    {
                        var mPref = bodyPreference[0].intKey;
                        var mDis = bodyPreference[1].intKey;
                        if (mBodyTypeDict[mPref] == data.TargetBody)
                            prefValue = 0.25f;
                        else if (mBodyTypeDict[mDis] == data.TargetBody)
                            prefValue = -0.25f;
                    }
                    else if (targetGender == Sex.Female)
                    {
                        var fPref = bodyPreference[2].intKey;
                        var fDis = bodyPreference[3].intKey;
                        if (fBodyTypeDict[fPref] == data.TargetBody)
                            prefValue = 0.25f;
                        else if (fBodyTypeDict[fDis] == data.TargetBody)
                            prefValue = -0.25f;
                    }
                    result += prefValue * observerPsyche.Evaluate(SexualityFormula.PhysicalPrefAuthSway);// 0.2 ~ 1.8
                }
            }
            // No boost in attraction for child-like bodies.
            else if (data.TargetBody == StandardizedBody.Baby || data.TargetBody == StandardizedBody.Child)
            {
                result = 1f;
            }
            // Sexualities that don't care about fat pawns.
            else if (data.PawnOri == Orientation.Asexual)
            {
                result = 1f;
            }
            else if (data.PawnOri == Orientation.Pansexual)
            {
                result = 1.25f;
            }
            // Fat pawns get no bonus (teratophiles will override this).
            else if (data.TargetBody == StandardizedBody.Fat)
            {
                result = 1f;
            }
            // Bisexuals are into both.
            else if (data.PawnOri == Orientation.Bisexual &&
                     (data.TargetBody == StandardizedBody.Male || data.TargetBody == StandardizedBody.Female))
            {
                result = 1.25f;
            }
            // Treat futa as effectively bisexual.
            else if (data.PawnSex == Sex.Futa &&
                     (data.TargetBody == StandardizedBody.Male || data.TargetBody == StandardizedBody.Female))
            {
                result = 1.25f;
            }
            // Males with a certain orientation.
            else if (data.PawnOri == Orientation.Heterosexual &&
                     (data.PawnSex == Sex.Male || data.PawnSex == Sex.Trap) &&
                     data.TargetBody == StandardizedBody.Female)
            {
                result = 1.25f;
            }
            else if (data.PawnOri == Orientation.Homosexual &&
                     (data.PawnSex == Sex.Male || data.PawnSex == Sex.Trap) &&
                     data.TargetBody == StandardizedBody.Male)
            {
                result = 1.25f;
            }
            // Females with a certain orientation.
            else if (data.PawnOri == Orientation.Heterosexual &&
                     data.PawnSex == Sex.Female &&
                     data.TargetBody == StandardizedBody.Male)
            {
                result = 1.25f;
            }
            else if (data.PawnOri == Orientation.Homosexual &&
                     data.PawnSex == Sex.Female &&
                     data.TargetBody == StandardizedBody.Female)
            {
                result = 1.25f;
            }
            // The default is already covered by the initial assignment (1f or 1.1f depending on your intention)
            __result = result;
            return false;
        }
    }
}
