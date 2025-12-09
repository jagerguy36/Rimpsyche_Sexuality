using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class LearnOrientation: ConvoResultBase
    {
        public float learningChance = 1f;
        public override void ApplyEffect(Pawn initiator, Pawn recipient, float alignment, float initOpinionOffset, float reciOpinionOffset)
        {
            //compPsyche check is redundant because this is convo result
            if (Rand.Value < learningChance)
                initiator.compPsyche().Sexuality.LearnOrientationOf(recipient);
            if (Rand.Value < learningChance)
                recipient.compPsyche().Sexuality.LearnOrientationOf(initiator);
        }
    }
}
