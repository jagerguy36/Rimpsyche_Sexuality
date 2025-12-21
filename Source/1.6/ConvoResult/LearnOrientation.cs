using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class LearnOrientation: ConvoResultBase
    {
        public float learningChance = 1f;
        public override void ApplyEffect(Pawn initiator, Pawn recipient, float alignment, float initOpinionOffset, float reciOpinionOffset)
        {
            //compPsyche check is redundant because this is convo result
            var initSexuality = initiator.compPsyche().Sexuality;
            var reciSexuality = recipient.compPsyche().Sexuality;
            if (Rand.Value < learningChance && reciSexuality.SexualityExpressed())
                initSexuality.LearnOrientationOf(recipient);
            if (Rand.Value < learningChance && initSexuality.SexualityExpressed())
                reciSexuality.LearnOrientationOf(initiator);
        }
    }
}
