using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class SexualityHelper
    {
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
    }
}
