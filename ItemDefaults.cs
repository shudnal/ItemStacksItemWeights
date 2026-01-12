using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ItemStacksItemWeights
{
    public static class ItemDefaults
    {
        public static readonly Dictionary<string, string> itemNames = new(StringComparer.OrdinalIgnoreCase);
        public static readonly Dictionary<string, float> itemWeight = new(StringComparer.OrdinalIgnoreCase);
        public static readonly Dictionary<string, int> itemStack = new(StringComparer.OrdinalIgnoreCase);

        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
        private static class ZoneSystem_Start_UpdateRegisters
        {
            private static void Postfix()
            {
                UpdateRegisters();
            }
        }

        public static void UpdateRegisters()
        {
            if (!ObjectDB.instance)
                return;

            foreach (GameObject item in ObjectDB.instance.m_items)
            {
                if (item == null || item.GetComponent<ItemDrop>() is not ItemDrop itemDrop)
                    continue;

                if (itemDrop.m_itemData is not ItemDrop.ItemData itemData || itemData.m_shared is not ItemDrop.ItemData.SharedData shared)
                    continue;

                if (shared.m_name.StartsWith("$"))
                {
                    itemNames[item.name] = shared.m_name;
                    itemNames[shared.m_name] = shared.m_name;
                }

                if (!itemWeight.ContainsKey(item.name))
                    itemWeight[item.name] = shared.m_weight;

                if (!itemWeight.ContainsKey(shared.m_name))
                    itemWeight[shared.m_name] = shared.m_weight;

                if (!itemStack.ContainsKey(item.name))
                    itemStack[item.name] = shared.m_maxStackSize;
                
                if (!itemStack.ContainsKey(shared.m_name))
                    itemStack[shared.m_name] = shared.m_maxStackSize;
            }

            ItemStacksItemWeights.LogInfo("Items defaults updated");

            DocGen.GenerateDocumentationFile();

            ItemProcessing.PatchObjectDB();
        }

        public static string GetItemName(this string input) => itemNames.GetValueOrDefault(input.Trim(), input);

        public static int GetDefaultItemStack(this ItemDrop.ItemData.SharedData shared) => itemStack.GetValueOrDefault(shared.m_name, -1);
        
        public static float GetDefaultItemWeight(this ItemDrop.ItemData.SharedData shared) => itemWeight.GetValueOrDefault(shared.m_name, -1f);

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            if (!dictionary.TryGetValue(key, out var value))
            {
                return defaultValue;
            }

            return value;
        }
    }
}
