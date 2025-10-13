using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class SexualityHelper
    {
    //TODO: Look up distribution researches
    public List<float> Distribution = new List<float> { 0.6f, 0.15f, 0.1f, 0.05f, 0.05f, 0.01f, 0.04f };
    public static readonly SimpleCurve SexualityCurve = CreateSexualityCurve();
    private static SimpleCurve CreateSexualityCurve()
    {
        List<CurvePoint> curvePoints = new List<CurvePoint>();
        List<float> yValues = new List<float> { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f };
        float cumulativeX = 0f;
        int yIndex = 0;

        for (int i = 0; i < Distribution.Count-1; i++)
        {
            cumulativeX += Distribution[i];
            curvePoints.Add(new CurvePoint(cumulativeX, yValues[i]));
        }
        return new SimpleCurve(curvePoints.ToArray());
    }

        public static readonly SimpleCurve StraightCurve = new SimpleCurve
        {
            new CurvePoint(0.8f, 0f),
            new CurvePoint(1f, 0.2f)
        };
        public static readonly SimpleCurve BiCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0.2f),
            new CurvePoint(0.5, 0.4f)
            new CurvePoint(0.75f, 0.6f)
            new CurvePoint(1f, 0.8f)
        };
        public static readonly SimpleCurve GayCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0.8f),
            new CurvePoint(0.2f, 1f)
        };

        public static float GenerateKinseyFor(SexualOrientation orientation)
        {
            float kinsey = -1f;
            int safetyMaxtries = 10;
            int tryCounter = 0;
            switch (orientation)
            {
                case SexualOrientation.Heterosexual:
                    kinsey = Rand.Range(0f, 0.15f);
                    break;
                case SexualOrientation.Bisexual:
                    kinsey = Rand.Range(0.15f, 0.85f);
                    while ((kinsey == 0.15f || kinsey == 0.85f) && tryCounter < safetyMaxtries)
                    {
                        kinsey = Rand.Range(0.15f, 0.85f);
                        tryCounter++;
                    }
                    break;
                case SexualOrientation.Homosexual:
                    kinsey = Rand.Range(0.85f, 1f);
                    break;
                case SexualOrientation.Asexual:
                    kinsey = Rand.Range(0, 1f);
                    break;
            }
            return kinsey;
        }
        public static float GenerateSexDriveFor(SexualOrientation orientation)
        {
            if (orientation == SexualOrientation.Asexual) return Rand.Range(0, 0.05f);
            else return Rand.Range(0.05f, 1f);
        }
        public static float GenerateAttractionFor(SexualOrientation orientation)
        {
            if (orientation == SexualOrientation.Asexual) return Rand.Range(0, 0.05f);
            else return Rand.Range(0.05f, 1f);
        }
    }
}
