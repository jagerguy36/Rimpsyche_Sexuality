using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class PsychePreferenceWorker : PreferenceWorker
    {
        private static readonly List<PersonalityDef> SourceDefs = DefDatabase<PersonalityDef>.AllDefsListForReading;
        private static readonly int nodeCount = SourceDefs.Count;
        private static readonly int[] IndexPool = Enumerable.Range(0, nodeCount).ToArray();
        private const int fullCount = 5;
        private const int reportCount = 3;
        public PsychePreferenceWorker()
        {
            EditorHeight = 200f;
        }

        public static List<PersonalityDef> RandomFiveNodes()
        {
            for (int i = 0; i < fullCount; i++)
            {
                int randIndex = Rand.Range(i, nodeCount);
                (IndexPool[i], IndexPool[randIndex]) = (IndexPool[randIndex], IndexPool[i]);
            }
            List<PersonalityDef> result = new List<PersonalityDef>(fullCount);
            for (int i = 0; i < fullCount; i++)
            {
                result.Add(SourceDefs[IndexPool[i]]);
            }
            return result;
        }
        public override bool TryGenerate(Pawn pawn, out List<PrefEntry> pref)
        {
            pref = new List<PrefEntry>(5);
            var relevantNodes = RandomFiveNodes();
            for (int i = 0; i < relevantNodes.Count; i++)
            {
                float importance = Rand.Range(0f, 1f);
                float target = Rand.Range(-1f, 1f);
                pref.Add(new PrefEntry(relevantNodes[i].defName, relevantNodes[i].shortHash, target, importance));
            }
            return true;
        }

        // Top 3 only for report.
        public override string Report(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true) return "RPS_NoPreference";
            var psychePreference = compPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_PsychePreference);
            var parts = new StringBuilder();
            for (int i = 0; i < reportCount; i++)
            {
                if (string.IsNullOrEmpty(psychePref[i].stringKey)) continue;
                if (parts.Length > 0) parts.Append(" | ");
                parts.Append($"{pawn.Name}'s preference || {psychePref[i].stringKey}: {psychePref[i].target:F2} ({psychePref[i].importance:F2})");
            }
            return parts.ToString();
        }

        public override float Evaluate(Pawn obesrver, Pawn target)
        {
            var observerPsyche = obesrver.compPsyche();
            if (observerPsyche?.Enabled != true) return 0f;
            var targetPsyche = target.compPsyche();
            if (targetPsyche?.Enabled != true) return 0f;
            var psychePreference = observerPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_PsychePreference);
            float value = 0f;
            for (int i = 0; i < psychePreference.Count; i++)
            {
                var pf = psychePreference[i];
                PersonalityDef personality = DefDatabase<PersonalityDef>.GetNamed(pf.stringKey, false);
                if (personality == null)
                {
                    Log.Warning($"Psyche Preference unable to load Personality def {pf.stringKey}");
                    //Logic to fix it.
                }
                var pv = targetPsyche.Personality.GetPersonality(personality);
                value += (2 - Mathf.Abs(pv - pf.target))* pf.importance;
            }
            return value * 0.1f;
        }

        private static List<PersonalityDef> GetRelevantDefs(List<PrefEntry> psychePrefs)
        {
            List<PersonalityDef> defs = new();
            for(int i = 0; i < psychePref.Count; i++)
            {
                var pf = psychePreference[i];
                PersonalityDef personality = DefDatabase<PersonalityDef>.GetNamed(pf.stringKey, false);
                defs.Add(personality);
            }
            return defs;
        }

        public static readonly float innerPadding = 5f;
        public static readonly float titleHeight = 35f;
        public static readonly float titleContentSpacing = 5f;
        private static float personalityLabelWidth => RimpsycheDatabase.maxPersonalityLabelWidth;
        private static float personalityWidthDiff => 2f * (personalityLabelWidth - 130f);
        private static readonly float personalityRowHeight = 32f;
        private static readonly float personalityLabelPadding = 2f;
        private static readonly float personalityBarWidth = 100f;
        private static readonly float personalityBarHeight = 4f;
        public override void DrawEditor(Rect rect, Pawn pawn, bool EditEnabled)
        {
            Rect innerRect = rect.ContractedBy(innerPadding);
            Rect titleRect = new Rect(innerRect.x, innerRect.y, innerRect.width, titleHeight);
            Text.Anchor = TextAnchor.MiddleCenter;
            string titleString = "RPC_Personality".Translate();
            Widgets.Label(titleRect, titleString);
            Vector2 titleTextSize = Text.CalcSize(titleString);
            Text.Anchor = TextAnchor.UpperLeft;

            Rect viewRect = new Rect(innerRect.x, titleRect.yMax + titleContentSpacing, innerRect.width, innerRect.height - (titleRect.height + titleContentSpacing));
            var y = 0;
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true) return;
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            var psychePreference = observerPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_PsychePreference);
            var personalityDefList = GetRelevantDefs(psychePreference);
            for(int i = 0; i < psychePref.Count; i++)
            {
                var targetValue = psychePreference[i].target;
                var def = personalityDefList[i];
                var (leftLabel, rightLabel, leftColor, rightColor) = (def.low.CapitalizeFirst(), def.high.CapitalizeFirst(), Color.red, Color.green);

                // rowRect and its sub-rects are correctly relative to 'y' which is inside viewRect
                Rect rowRect = new Rect(0f, y, viewRect.width, personalityRowHeight);

                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    string tooltipString = $"{def.label.CapitalizeFirst()}: {(targetValue * 100f).ToString("F1")}";
                    TooltipHandler.TipRegion(rowRect, tooltipString);
                }
                float centerY = rowRect.y + rowRect.height / 2f;
                // Left label
                Rect leftRect = new Rect(rowRect.x + personalityLabelPadding, centerY - Text.LineHeight / 2f, personalityLabelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(leftRect, leftLabel);

                // Right label
                Rect rightRect = new Rect(rowRect.xMax - personalityLabelWidth - personalityLabelPadding, centerY - Text.LineHeight / 2f, personalityLabelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(rightRect, rightLabel);
                if (EditEnabled)
                {
                    float highend = 1f;
                    float lowend = -1f;
                    if (!scope.NullOrEmpty())
                    {
                        if (scope.TryGetValue(def.shortHash, out var range))
                        {
                            (lowend, highend) = range;
                        }
                    }
                    //Rect sliderRect = new Rect(barCenterX + barWidth / 2f * lowend , centerY - barHeight / 2f, barWidth*(highend-lowend)*0.5f, 24f);?
                    Rect sliderRect = new Rect(barCenterX - personalityBarWidth / 2f, centerY - personalityBarHeight / 2f, personalityBarWidth, personalityRowHeight);
                    float newValue = Widgets.HorizontalSlider(sliderRect, targetValue, lowend, highend);
                    //newValue = Mathf.Clamp(newValue, lowend, highend);
                    if (newValue != targetValue)
                    {
                        psychePreference[i].target = newValue;
                    }
                }
                else
                {
                    // Bar background
                    Rect barRect = new Rect(barCenterX - personalityBarWidth / 2f, centerY - personalityBarHeight / 2f, personalityBarWidth, personalityBarHeight);
                    Widgets.DrawBoxSolid(barRect, barBackgroundColor);

                    // Value bar
                    float clamped = Mathf.Clamp(targetValue, -1f, 1f);
                    float halfBar = Mathf.Abs(clamped) * (personalityBarWidth) / 2f;
                    Rect valueRect = clamped >= 0
                        ? new Rect(barCenterX, barRect.y, halfBar, personalityBarHeight)
                        : new Rect(barCenterX - halfBar, barRect.y, halfBar, personalityBarHeight);

                    // Color based on intensity (small = yellow, strong = green)
                    float intensity = Mathf.Abs(clamped) * 2f;
                    Color barColor = Color.Lerp(Color.yellow, Color.green, intensity);
                    Widgets.DrawBoxSolid(valueRect, barColor);
                }

                y += personalityRowHeight;
            }
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
            return;
        }

        public p
    }
}
