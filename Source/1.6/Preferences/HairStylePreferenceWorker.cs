using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class HairStylePreferenceWorker : PreferenceWorker
    {
        public class HairStylePrefBias
        {
            public string targetTag; 
            public bool dislike = false;//dislike == true means negative preference for this tag.
            public float toMaleProb; //The probability that a pawn will have this preference towards male
            public float toFemaleProb; //The probability that a pawn will have this preference towards female
        }

        public static List<HairStylePrefBias> hairPrefBias;
        public static List<HairStylePrefBias> beardPrefBias;

        public HairStylePreferenceWorker()
        {
            EditorHeight = 200f;
        }

        public override bool TryGenerate(Pawn pawn, out List<PrefEntry> pref)
        {
            pref = null;
            return false;
        }

        public override string Report(Pawn pawn)
        {
            return "";
        }

        public override float Evaluate(Pawn observer, Pawn target, float result, bool isRomantic)
        {
            return result;
        }


        public override void DrawEditor(Rect rect, Pawn pawn, bool EditEnabled)
        {
            return;
        }
        public override void ClearEditorCache()
        {
            return;
        }
    }
}
