using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class BodyTypePreferenceDef : PreferenceDef
    {
        public class BodyPrefBias
        {
            public float mean;
            public float variance;
        }

        public BodyPrefBias maleFatPrefBias;
        public BodyPrefBias maleMusclePrefBias;

        public BodyPrefBias femaleFatPrefBias;
        public BodyPrefBias femaleMusclePrefBias;
    }

    public class BodyTypePreferenceWorker : PreferenceWorker
    {
        public static Dictionary<int, string> KeyDict = new Dictionary<int, string>
        {
            { 0, "MuscleM" },
            { 1, "FatM" },
            { 2, "MuscleF" },
            { 3, "FatF" },
        };
        public static Dictionary<int, string> LabelDict = new Dictionary<int, string>
        {
            { 0, "RPS_MuscleM".Translate() },
            { 1, "RPS_FatM".Translate() },
            { 2, "RPS_MuscleF".Translate() },
            { 3, "RPS_FatF".Translate() },
        };
        public BodyTypePreferenceWorker()
        {
            EditorHeight = 50f;
        }

        public override void PostInit(){}

        public override bool TryGenerate(Pawn pawn, out List<PrefEntry> pref)
        {
            pref = new List<PrefEntry>(4);
            //Log.Message($"generating bodytype preference for {pawn.Name}");
            for (int i = 0; i < pref.Count; i++)
            {
                target = 25f;
                importance = 1f;
                pref.Add(new PrefEntry(KeyDict[i], i, target, importance));
            }
            return true;
        }

        public override float Evaluate(Pawn observer, Pawn target, float result, bool isRomantic)
        {
            if (RimpsycheSexualitySettings.usePreferenceSystem != true) return result;
            if (isRomantic) return result;
            var observerPsyche = observer.compPsyche();
            if (observerPsyche?.Enabled != true) return 0f;
            var targetPsyche = target.compPsyche();
            if (targetPsyche?.Enabled != true) return 0f;
            var bodyPreference = observerPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_BodyTypePreference);
            if (bodyPreference == null)
                return 0f;
            float value = 0f;
            return value;
        }
        
        //Static values
        public static readonly float innerPadding = 5f;
        public static readonly float titleHeight = 35f;
        public static readonly float titleContentSpacing = 5f;
        private static readonly Color barBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        private static readonly float verticalWidth = 20f;
        private static readonly float verticalPadding = 5f;

        public override float GetViewerHeight(Pawn pawn)
        {
            return 50f;
        }

        private static List<string> drawerCache = null;
        
        public override void DrawViewer(Rect rect, Pawn pawn)
        {
        }

        public override void DrawEditor(Rect rect, Pawn pawn, bool EditEnabled)
        {
        }
        public override void PostLoadAdjustment(Dictionary<string, List<PrefEntry>> _preference)
        {
        }
        public override void ClearViewerCache()
        {
            drawerCache = null;
        }
        public override void ClearEditorCache()
        {
        }
    }
}
