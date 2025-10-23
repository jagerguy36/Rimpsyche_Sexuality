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
        public float EditorHeight;

        public abstract void SetUp(Pawn pawn);
        
        public abstract string Report(Pawn pawn);

        public abstract float Evaluate(Pawn obesrver, Pawn target);

        public abstract void DrawEditor(Rect rect, Pawn pawn);
    }

    public static class PsychePerferenceWorker: PreferenceWorker
    {
        private static readonly List<PersonalityDef> SourceDefs = DefDatabase<PersonalityDef>.AllDefsListForReading;
        private const int nodeCount = SourceDefs.Count;
        private static readonly int[] IndexPool = Enumerable.Range(0, nodeCount).ToArray();

        public static List<PersonalityDef> RandomFiveNodes()
        {
            const int selectCount = 5;
            for (int i = 0; i < selectCount; i++)
            {
                int randIndex = Rand.Range(i, nodeCount);
                (IndexPool[i], IndexPool[randIndex]) = (IndexPool[randIndex], IndexPool[i]);
            }
            List<PersonalityDef> result = new List<PersonalityDef>(selectCount);
            for (int i = 0; i < selectCount; i++)
            {
                result.Add(SourceDefs[IndexPool[i]]);
            }
            return result;
        }

        public override float EditorHeight = 100f;
        public override void SetUp(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true) return;
            var relevantNodes = RandomFiveNodes();
            for (int i = 0; i<relevantNodes.Count; i++)
            {

            }

        }
    }
}
