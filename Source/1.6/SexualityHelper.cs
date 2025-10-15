using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class SexualityHelper
    {
        //TODO: Look up distribution researches
        public static List<float> Distribution = [0.6f, 0.15f, 0.1f, 0.05f, 0.05f, 0.01f, 0.04f];
        public static readonly List<float> steps = [0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f];
        private static readonly float StraightSum = Distribution[0]+Distribution[1];
        private static readonly float BiSum = Distribution[2]+Distribution[3]+Distribution[4];
        private static readonly float GaySum = Distribution[5]+Distribution[6];
        public static readonly SimpleCurve SexualityCurve = CreateSexualityCurve();
        private static SimpleCurve CreateSexualityCurve()
        {
            List<CurvePoint> curvePoints = new List<CurvePoint>();
            float cumulativeX = 0f;
            for (int i = 0; i < Distribution.Count-1; i++)
            {
                cumulativeX += Distribution[i];
                curvePoints.Add(new CurvePoint(cumulativeX, steps[i]));
            }
            return new SimpleCurve(curvePoints.ToArray());
        }
        public static readonly SimpleCurve StraightCurve = new SimpleCurve
        {
            new CurvePoint(Distribution[0]/StraightSum, steps[0]),
            new CurvePoint(1f, steps[1])
        };
        public static readonly SimpleCurve BiCurve = new SimpleCurve
        {
            new CurvePoint(0f, steps[1]),
            new CurvePoint(Distribution[2]/BiSum, steps[2]),
            new CurvePoint((Distribution[2]+Distribution[3])/BiSum, steps[3]),
            new CurvePoint(1f, steps[4])
        };
        public static readonly SimpleCurve GayCurve = new SimpleCurve
        {
            new CurvePoint(0f, steps[4]),
            new CurvePoint(Distribution[5]/GaySum, steps[5])
        };

        public static float GenerateKinseyFor(SexualOrientation orientation)
        {
            float kinsey = -1f;
            float flatRatio = Rand.Range(0f, 1f);
            switch (orientation)
            {
                case SexualOrientation.Heterosexual:
                    kinsey = StraightCurve.Evaluate(flatRatio);
                    break;
                case SexualOrientation.Bisexual:
                    kinsey = BiCurve.Evaluate(flatRatio);
                    break;
                case SexualOrientation.Homosexual:
                    kinsey = GayCurve.Evaluate(flatRatio);
                    break;
                case SexualOrientation.Asexual:
                    kinsey = SexualityCurve.Evaluate(flatRatio);
                    break;
            }
            return kinsey;
        }
        public static float ReEvaluateKinsey(float sameAttraction, float diffAttraction)
        {
            return sameAttraction / (sameAttraction + diffAttraction);
        }

        public static float GenerateSexDriveFor(SexualOrientation orientation)
        {
            if (orientation == SexualOrientation.Asexual) return Rand.Range(0, 0.05f);
            else return GetNormalDistribution(0.05f, 1f);
        }
        public static float GenerateAttractionFor(SexualOrientation orientation)
        {
            if (orientation == SexualOrientation.Asexual) return Rand.Range(0, 0.05f);
            else return GetNormalDistribution(0.05f, 1f);
        }
        public static float GetNormalDistribution(float lowBracket = 0f, float highBracket = 1f, int maxAttempts = 4)
        {
            float result;
            int attempts = 0;
            do
            {
                result = Rand.Gaussian(0.5f, 0.2f);
                attempts++;
            }
            while ((result < lowBracket || result > highBracket) && attempts < maxAttempts); 

            if (result < lowBracket || result > highBracket)result = Mathf.Clamp(result, lowBracket, highBracket);
            return result;
        }
    }
}
