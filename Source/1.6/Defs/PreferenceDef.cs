using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public enum RimpsychePrefCategory
    {
        Physical,
        Social,
        Romantic,
        Misc
    }
    public enum RimpsycheCacheType
    {
        Once,
        Never,
        Psyche
    }
    public class PreferenceDef: Def
    {
        public RimpsychePrefCategory category
        public RimpsycheCacheType cacheType
        public Type workerClass = typeof(PreferenceWorker);

        [Unsaved(false)]
        private PreferenceWorker workerInt;

        public PreferenceWorker Worker
        {
            get
            {
                if (workerInt == null)
                {
                    workerInt = (PreferenceWorker)Activator.CreateInstance(workerClass);
                    workerInt.def = this;
                }
                return workerInt;
            }
        }
    }

    public abstract class PreferenceWorker
    {
        public abstract void SetUp(Pawn pawn);

        public abstract float Evaluate(Pawn obesrver, Pawn target);
    }
}
