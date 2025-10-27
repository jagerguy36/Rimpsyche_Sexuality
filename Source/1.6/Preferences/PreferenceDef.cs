using System;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class PreferenceDef: Def
    {
        public RimpsychePrefCategory category;
        public RimpsycheCacheType cacheType;
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
    public abstract class PreferenceWorker
    {
        public float EditorHeight;
        public PreferenceDef def;

        public abstract void SetUp(Pawn pawn);
        
        public abstract string Report(Pawn pawn);

        public abstract float Evaluate(Pawn obesrver, Pawn target);

        public abstract void DrawEditor(Rect rect, Pawn pawn);
    }
}
