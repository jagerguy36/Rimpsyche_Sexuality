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
                //compPsyche check is redundant because this is convo result
                initiator.compPsyche().Sexuality.LearnOrientationOf(recipient);
                recipient.compPsyche().Sexuality.LearnOrientationOf(initiator);
            }
        }
    }
}
