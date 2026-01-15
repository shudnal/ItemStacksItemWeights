using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static ItemStacksItemWeights.ItemStacksItemWeights;

namespace ItemStacksItemWeights
{
    internal static class ItemStackText
    {
        private static readonly Dictionary<string, string> compactTextCache = new(256);
        private const int CompactCacheLimit = 1024;

        internal static void ClearCache() => compactTextCache.Clear();

        private static bool TryFindNextNumber(
            string text,
            int start,
            out int value,
            out int numStart,
            out int numLength)
        {
            value = 0;
            numStart = numLength = 0;

            bool inTag = false;

            for (int i = start; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '<') { inTag = true; continue; }
                if (c == '>') { inTag = false; continue; }
                if (inTag) continue;

                if (!char.IsDigit(c))
                    continue;

                numStart = i;
                int v = 0;
                int j = i;

                while (j < text.Length && char.IsDigit(text[j]))
                {
                    v = v * 10 + (text[j] - '0');
                    j++;
                }

                value = v;
                numLength = j - i;
                return true;
            }

            return false;
        }

        private static string GetCompactText(string original)
        {
            if (compactTextCache.TryGetValue(original, out var cached))
                return cached;

            string result = BuildCompactText(original);

            if (compactTextCache.Count > CompactCacheLimit)
                compactTextCache.Clear();

            compactTextCache[original] = result;
            return result;
        }

        private static string TrimAfterIndexPreserveTags(string text, int cutIndex)
        {
            int lastTagClose = text.LastIndexOf('>');
            if (lastTagClose >= cutIndex)
                return text.Substring(0, cutIndex) + text.Substring(lastTagClose + 1);

            return text.Substring(0, cutIndex);
        }

        private static string BuildCompactText(string text)
        {
            if (!TryFindNextNumber(text, 0, out int amount, out int aStart, out int aLen))
                return text;

            string compactAmount = compactAmountSize.Value
                ? FormatCompact(amount)
                : text.Substring(aStart, aLen);

            text = text.Substring(0, aStart) + compactAmount + text.Substring(aStart + aLen);

            int cutIndex = aStart + compactAmount.Length;

            if (hideStackSize.Value)
                return TrimAfterIndexPreserveTags(text, cutIndex);

            if (compactStackSize.Value &&
                TryFindNextNumber(text, cutIndex, out int max, out int mStart, out int mLen))
            {
                string compactMax = FormatCompact(max);
                text = text.Substring(0, mStart) + compactMax + text.Substring(mStart + mLen);
            }

            return text;
        }

        private static string FormatCompact(int value)
        {
            if (value < 1000)
                return value.ToFastString();

            if (value < 1_000_000)
            {
                float v = Mathf.Floor(value / 100f) / 10f;
                return v.ToString("0.#") + "k";
            }

            float m = Mathf.Floor(value / 100_000f) / 10f;
            return m.ToString("0.#") + "M";
        }

        [HarmonyPatch(typeof(HotkeyBar), nameof(HotkeyBar.UpdateIcons))]
        public static class HotkeyBar_UpdateIcons_StackSizeFormatting
        {
            [HarmonyPriority(Priority.Low)]
            public static void Postfix(HotkeyBar __instance)
            {
                if (showFullStackSize.Value.IsPressed() ||
                    (!hideStackSize.Value && !compactStackSize.Value && !compactAmountSize.Value))
                    return;

                foreach (var element in __instance.m_elements)
                {
                    if (!element.m_amount.gameObject.activeInHierarchy)
                        continue;

                    string text = element.m_amount.text;
                    if (string.IsNullOrEmpty(text))
                        continue;

                    element.m_amount.SetText(GetCompactText(text));
                }
            }
        }

        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
        public static class InventoryGrid_UpdateGui_StackSizeFormatting
        {
            [HarmonyPriority(Priority.Low)]
            public static void Postfix(InventoryGrid __instance)
            {
                if (showFullStackSize.Value.IsPressed() ||
                    (!hideStackSize.Value && !compactStackSize.Value && !compactAmountSize.Value))
                    return;

                foreach (var element in __instance.m_elements)
                {
                    if (!element.m_amount.gameObject.activeInHierarchy)
                        continue;

                    string text = element.m_amount.text;
                    if (string.IsNullOrEmpty(text))
                        continue;

                    element.m_amount.SetText(GetCompactText(text));
                }
            }
        }
    }
}
