using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemStacksItemWeights.ItemStacksItemWeights;

namespace ItemStacksItemWeights
{
    public static class ItemProcessing
    {
        public static ItemConfigurations itemConfig = new();
        public static bool containerUpdateRequired = false;
        public static readonly List<Inventory> containerInventoryUpdated = new();

        public static void OnConfigFileChange()
        {
            LogInfo($"Item config updated");

            itemConfig.Load(configurationFile.Value);

            PatchObjectDB();

            if (Player.m_localPlayer)
            {
                if (PatchPlayerInventory(Player.m_localPlayer))
                    LogInfo("Patched local player inventory");

                // Value updated while playing, container needs to be also updated
                containerUpdateRequired = true;
                containerInventoryUpdated.Clear();
            }
        }

        public static void PatchObjectDB()
        {
            itemConfig.FillItemNames();

            if (!ObjectDB.instance)
                return;

            foreach (GameObject item in ObjectDB.instance.m_items)
            {
                if (item == null || item.GetComponent<ItemDrop>() is not ItemDrop itemDrop)
                    continue;

                if (itemDrop.m_itemData is not ItemDrop.ItemData itemData || itemData.m_shared is not ItemDrop.ItemData.SharedData shared)
                    continue;

                PatchShared(shared);
            }

            LogInfo("Patched ObjectDB items");
        }

        public static bool PatchShared(ItemDrop.ItemData.SharedData shared)
        {
            if (shared == null)
                return false;

            int originalStack = shared.m_maxStackSize;
            float originalWeight = shared.m_weight;

            int stackSize = shared.GetDefaultItemStack();
            if (stackSize > 1)
            {
                shared.m_maxStackSize = stackSize;

                if (itemConfig.stackSize.TryGetValue(shared.m_name, out int itemStack))
                    shared.m_maxStackSize = itemStack;
                else if (itemConfig.stackSize.TryGetValue(ItemConfigurations.global, out int globalStack))
                    shared.m_maxStackSize = globalStack;
                else if (itemConfig.stackMultiplier.TryGetValue(shared.m_name, out float itemStackMultiplier))
                    shared.m_maxStackSize = Mathf.CeilToInt(shared.m_maxStackSize * itemStackMultiplier);
                else if (itemConfig.stackMultiplier.TryGetValue(ItemConfigurations.global, out float globalStackMultiplier))
                    shared.m_maxStackSize = Mathf.CeilToInt(shared.m_maxStackSize * globalStackMultiplier);
            }

            float weight = shared.GetDefaultItemWeight();
            if (weight >= 0)
            {
                shared.m_weight = weight;

                if (itemConfig.weightAmount.TryGetValue(shared.m_name, out float itemWeight))
                    shared.m_weight = itemWeight;
                else if (itemConfig.weightAmount.TryGetValue(ItemConfigurations.global, out float globalWeight))
                    shared.m_weight = globalWeight;
                else if (itemConfig.weightMultiplier.TryGetValue(shared.m_name, out float itemWeightMultiplier))
                    shared.m_weight *= itemWeightMultiplier;
                else if (itemConfig.weightMultiplier.TryGetValue(ItemConfigurations.global, out float globalWeightMultiplier))
                    shared.m_weight *= globalWeightMultiplier;
            }

            return originalStack != shared.m_maxStackSize || originalWeight != shared.m_weight;
        }

        public static int PatchItem(ItemDrop.ItemData item) => PatchShared(item?.m_shared) ? 1 : 0;

        public static bool PatchInventory(Inventory inventory)
        {
            if (inventory == null)
                return false;

            if (inventory.m_inventory?.Sum(PatchItem) > 0)
            {
                inventory.Changed();
                return true;
            }

            return false;
        }

        public static bool PatchPlayerInventory(Player player) => PatchInventory(player?.GetInventory());

        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake))]
        public static class ItemDrop_Awake_PatchItems
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(ItemDrop __instance)
            {
                if (FejdStartup.instance != null)
                    return;

                PatchItem(__instance.m_itemData);
            }
        }

        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.OnCreateNew), typeof(ItemDrop))]
        public static class ItemDrop_OnCreateNew_PatchItems
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(ItemDrop item)
            {
                if (FejdStartup.instance != null)
                    return;
                
                PatchItem(item.m_itemData);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Load))]
        public static class Player_Load_PatchItems
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(Player __instance)
            {
                if (FejdStartup.instance != null)
                    return;

                if (PatchPlayerInventory(__instance))
                    LogInfo("Patched player inventory");

                containerInventoryUpdated.Clear();
                containerUpdateRequired = false;
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Show))]
        public static class InventoryGui_Show_PatchItems
        {
            private static void Postfix() => PatchPlayerInventory(Player.m_localPlayer);
        }

        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateInventory))]
        public static class InventoryGrid_UpdateInventory_PatchContainerItems
        {
            private static void Postfix(InventoryGrid __instance)
            {
                if (__instance == InventoryGui.instance.ContainerGrid && containerUpdateRequired && !containerInventoryUpdated.Contains(__instance.m_inventory))
                {
                    if (PatchInventory(__instance.m_inventory))
                        LogInfo($"Inventory {__instance.m_inventory} patched");

                    containerInventoryUpdated.Add(__instance.m_inventory);
                }
            }
        }
    }
}
