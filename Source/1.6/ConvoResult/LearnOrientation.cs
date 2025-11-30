using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class LearnOrientation: ConvoResultBase
    {
        public float learningChance = 1f;
        public override void ApplyEffect(Pawn initiator, Pawn recipient, float alignment, float initOpinionOffset, float reciOpinionOffset)
        {
            if (Rand.Value < learningChance)
            {
                initiator.compPsyche().Sexuality.LearnOrientationOf(recipient);
            }
        }
    }
}
