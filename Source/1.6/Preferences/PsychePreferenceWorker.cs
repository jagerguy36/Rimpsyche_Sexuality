using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        //TODO: use descriptor instead of numbers
        public override string Report(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true) return "RPS_NoPreference";
            var psychePreference = compPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_PsychePreference);
            var sortedPreferences = psychePreference.OrderByDescending(p => p.importance).ToList();
            var parts = new StringBuilder();
            parts.Append("RPS_AttractionReport".Translate(pawn.Name.ToStringShort)+"\n");
            for (int i = 0; i < sortedPreferences.Count; i++)
            {
                if (string.IsNullOrEmpty(sortedPreferences[i].stringKey)) continue;
                var def = DefDatabase<PersonalityDef>.GetNamed(sortedPreferences[i].stringKey);
                parts.Append($"      {Rimpsyche_Utility.GetPersonalityDesc(def, sortedPreferences[i].target)}\n");
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
                value += (1 - Mathf.Abs(pv - pf.target))* pf.importance; //-5f~5f
            }
            float auth = observerPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Authenticity);
            float sway = 0.3f + 0.2f * auth; // 0.1~0.5
            float preferenceFactor =  1f + value * sway; // 0.9~1.1 || 0.5~1.5
            return preferenceFactor;
        }
        
        //Static values
        public static readonly float innerPadding = 5f;
        public static readonly float titleHeight = 35f;
        public static readonly float titleContentSpacing = 5f;
        private static float personalityLabelWidth => RimpsycheDatabase.maxPersonalityLabelWidth;
        //private static float personalityWidthDiff => 2f * (personalityLabelWidth - 130f);
        private static readonly float personalityRowHeight = 20f;
        private static readonly float personalityLabelPadding = 2f;
        private static readonly float personalityBarHeight = 4f;
        private static readonly Color barBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        private static readonly float verticalWidth = 20f;
        private static readonly float verticalPadding = 5f;

        //Internal Cache
        private static List<PersonalityDef> relevantDefsCache = null;
        private static HashSet<int> relevantDefsHash = null;
        private static List<PersonalityDef> GetRelevantDefs(List<PrefEntry> psychePrefs)
        {
            if(relevantDefsCache != null) return relevantDefsCache;
            relevantDefsCache = [];
            relevantDefsHash = new();
            for(int i = 0; i < psychePrefs.Count; i++)
            {
                var pf = psychePrefs[i];
                PersonalityDef personality = DefDatabase<PersonalityDef>.GetNamed(pf.stringKey, false);
                relevantDefsCache.Add(personality);
                relevantDefsHash.Add(personality.shortHash);
            }
            return relevantDefsCache;
        }
        private static List<PersonalityDef> potentialDefsCache = null;
        private static List<PersonalityDef> GetPotentialDefs(List<PrefEntry> psychePrefs)
        {
            if(potentialDefsCache != null) return potentialDefsCache;

            var relevantDefs = GetRelevantDefs(psychePrefs);
            potentialDefsCache = [];
            foreach(var personalityDef in  DefDatabase<PersonalityDef>.AllDefs)
            {
                if (relevantDefsHash.Contains(personalityDef.shortHash)) continue;
                potentialDefsCache.Add(personalityDef);
            }
            return potentialDefsCache;
        }

        public override void DrawEditor(Rect rect, Pawn pawn, bool EditEnabled)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            //Rect innerRect = rect.ContractedBy(innerPadding);
            Text.Anchor = TextAnchor.MiddleCenter;
            string titleString = "RPC_Personality".Translate();
            Vector2 titleTextSize = Text.CalcSize(titleString);
            Rect titleRect = new Rect(rect.x, rect.y, rect.width, titleTextSize.y);
            Widgets.Label(titleRect, titleString);
            Text.Anchor = TextAnchor.UpperLeft;

            Rect viewRect = new Rect(rect.x, titleRect.yMax + titleContentSpacing, rect.width, rect.height - (titleRect.height + titleContentSpacing));
            float y = viewRect.y;

            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true) return;
            var psychePreference = compPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_PsychePreference);
            var personalityDefList = GetRelevantDefs(psychePreference);
            Text.Font = GameFont.Tiny;

            float rowWidth = viewRect.width - verticalWidth;
            float leftRectX = rect.x + personalityLabelPadding;
            float rightRectX = rect.x + rowWidth - personalityLabelPadding - personalityLabelWidth;
            float vertRectX = rect.x + rowWidth;
            float vertBarX = vertRectX + (verticalWidth - personalityBarHeight) / 2f;
            float vertBarHeight = (personalityRowHeight - verticalPadding) * 2f;

            for (int i = 0; i < psychePreference.Count; i++)
            {
                var targetValue = psychePreference[i].target;
                var importanceValue = psychePreference[i].importance;
                var def = personalityDefList[i];
                var (leftLabel, rightLabel, leftColor, rightColor) = (def.low.CapitalizeFirst(), def.high.CapitalizeFirst(), Color.red, Color.green);

                Rect rowRect = new Rect(rect.x, y, rowWidth, personalityRowHeight * 2f);
                Rect vertRect = new Rect(vertRectX, y, verticalWidth, personalityRowHeight * 2f);

                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    string tooltipString = $"{def.label.CapitalizeFirst()}: {(targetValue * 100f).ToString("F1")}";
                    TooltipHandler.TipRegion(rowRect, tooltipString);
                    if (EditEnabled && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        int capturedIndex = i;
                        foreach (PersonalityDef potentialDef in GetPotentialDefs(psychePreference))
                        {
                            PersonalityDef capturedDef = potentialDef;
                            Action action = delegate
                            {
                                psychePreference[capturedIndex].stringKey = capturedDef.defName;
                                relevantDefsCache = null;
                                relevantDefsHash = null;
                                potentialDefsCache = null;
                            };
                            options.Add(new FloatMenuOption(capturedDef.label.CapitalizeFirst(), action));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                        Event.current.Use();
                    }

                }
                if (Mouse.IsOver(vertRect))
                {
                    Widgets.DrawHighlight(vertRect);
                    string tooltipString = "RPC_Importance".Translate() + $": {(importanceValue * 100f).ToString("F1")}";
                    TooltipHandler.TipRegion(vertRect, tooltipString);
                }
                float uppercenterY = rowRect.y + rowRect.height * 0.25f;
                float lowercenterY = rowRect.y + rowRect.height * 0.75f;
                // Left label
                Rect leftRect = new Rect(leftRectX, uppercenterY - Text.LineHeight / 2f, personalityLabelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(leftRect, leftLabel);

                // Right label
                Rect rightRect = new Rect(rightRectX, uppercenterY - Text.LineHeight / 2f, personalityLabelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(rightRect, rightLabel);
                if (EditEnabled)
                {
                    Rect sliderRect = new Rect(0, lowercenterY - personalityBarHeight / 2f, rowRect.width, personalityRowHeight);
                    float newValue = Widgets.HorizontalSlider(sliderRect, targetValue, -1f, 1f);
                    if (newValue != targetValue)
                    {
                        psychePreference[i].target = newValue;
                    }
                    //Importance
                    psychePreference[i].importance = GUI.VerticalSlider(vertRect, psychePreference[i].importance, 1f, 0f);
                }
                else
                {
                    // Bar background
                    Rect barRect = new Rect(0, lowercenterY - personalityBarHeight / 2f, rowRect.width, personalityBarHeight);
                    Widgets.DrawBoxSolid(barRect, barBackgroundColor);

                    // Value bar
                    float clamped = Mathf.Clamp(targetValue, -1f, 1f);
                    float halfBar = Mathf.Abs(clamped) * (rowRect.width) / 2f;
                    Rect valueRect = clamped >= 0
                        ? new Rect(rowRect.width * 0.5f, barRect.y, halfBar, personalityBarHeight)
                        : new Rect(rowRect.width * 0.5f - halfBar, barRect.y, halfBar, personalityBarHeight);

                    // Color based on intensity (small = yellow, strong = green)
                    float intensity = Mathf.Abs(clamped) * 2f;
                    Color barColor = Color.Lerp(Color.yellow, Color.green, intensity);
                    Widgets.DrawBoxSolid(valueRect, barColor);

                    //Importance
                    Rect vertBarRect = new Rect(vertBarX, vertRect.y + verticalPadding, personalityBarHeight, vertBarHeight);
                    Widgets.DrawBoxSolid(vertBarRect, barBackgroundColor);
                    Rect vertValueRect = new Rect(vertBarX, vertBarRect.y + vertBarHeight * (1- psychePreference[i].importance), personalityBarHeight, vertBarHeight * psychePreference[i].importance);
                    Color vertBarColor = Color.Lerp(Color.yellow, Color.green, psychePreference[i].importance);
                    Widgets.DrawBoxSolid(vertValueRect, vertBarColor);
                }

                y += 2f * personalityRowHeight;
            }
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
            return;
        }
        public override void ClearEditorCache()
        {
            relevantDefsCache = null;
            relevantDefsHash = null;
            potentialDefsCache = null;
        }
    }
}
