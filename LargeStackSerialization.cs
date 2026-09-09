using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using static ItemDrop;

namespace ItemStacksItemWeights
{
    // Valheim 1.0.7 stores the built-in stack field as UInt16. Keep larger counts in the existing custom-data format.
    internal static class LargeStackSerialization
    {
        private const string StackKey = "shudnal.ItemStacksItemWeights.FullStack";

        [HarmonyPatch(typeof(ItemData), nameof(ItemData.Save), typeof(ZPackage))]
        private static class ItemData_Save_PreserveFullStack
        {
            private struct SaveState
            {
                internal bool Changed;
                internal int Stack;
                internal Dictionary<string, string> CustomData;
            }

            [HarmonyPriority(Priority.First)]
            private static void Prefix(ItemData __instance, out SaveState __state)
            {
                __state = default;
                Dictionary<string, string> originalData = __instance.m_customData;
                bool oversized = __instance.m_stack > ushort.MaxValue;
                if (!oversized && originalData != null && !originalData.ContainsKey(StackKey))
                    return;

                Dictionary<string, string> customData = originalData == null
                    ? new Dictionary<string, string>()
                    : new Dictionary<string, string>(originalData);
                // Never copy stale transport counts into split or cloned items.
                customData.Remove(StackKey);
                if (oversized)
                    customData[StackKey] = __instance.m_stack.ToString(CultureInfo.InvariantCulture);

                __state = new SaveState { Changed = true, Stack = __instance.m_stack, CustomData = originalData };
                __instance.m_customData = customData;
                if (oversized)
                {
                    // A reader without this mod sees a valid saturated count rather than a wrapped or empty stack.
                    __instance.m_stack = ushort.MaxValue;
                }
            }

            [HarmonyPriority(Priority.Last)]
            private static void Finalizer(ItemData __instance, SaveState __state)
            {
                if (!__state.Changed)
                    return;

                __instance.m_stack = __state.Stack;
                __instance.m_customData = __state.CustomData;
            }
        }

        [HarmonyPatch(typeof(ItemData), nameof(ItemData.Load), typeof(ZPackage), typeof(ItemData), typeof(global::Version.Item))]
        private static class ItemData_Load_RestoreFullStack
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(ItemData itemData)
            {
                Dictionary<string, string> customData = itemData.m_customData;
                if (customData == null)
                    return;

                bool hasCurrent = customData.TryGetValue(StackKey, out string currentValue);
                // Consume transport key so future splits and clones cannot reuse an outdated count.
                customData.Remove(StackKey);

                if (hasCurrent && itemData.m_stack == ushort.MaxValue && TryReadCount(currentValue, out int currentCount))
                {
                    itemData.m_stack = currentCount;
                    return;
                }
            }
        }

        private static bool TryReadCount(string value, out int stack)
        {
            return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out stack) && stack > ushort.MaxValue;
        }
    }
}
