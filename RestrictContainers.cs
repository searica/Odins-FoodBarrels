﻿using System.Collections.Generic;
using HarmonyLib;
using static OdinsFoodBarrels.OdinsFoodBarrelsPlugin;

namespace OdinsFoodBarrels
{
    /// <summary>
    ///     Class to patch Valheim inventory methods so that the specified
    ///     containers can only contain items of a single type.
    /// </summary>
    [HarmonyPatch(typeof(Inventory))]
    internal class RestrictContainers
    {
        private static string? _targetContainer;
        private static HashSet<string>? _allowedItems;
        private static Dictionary<string, HashSet<string>> _allowedItemsByContainer = new();

        private static RestrictContainers? _instance;

        /// <summary>
        ///     Singleton instance
        /// </summary>
        public static RestrictContainers Instance => _instance ??= new RestrictContainers();

        /// <summary>
        ///     Hide .ctor
        /// </summary>
        private RestrictContainers()
        { }

        /// <summary>
        ///     Set which containers to restrict and which item is allowed to be placed in each one.
        /// </summary>
        /// <param name="allowedItemByContainer"></param>
        public void SetContainerRestrictions(Dictionary<string, HashSet<string>> allowedItemByContainer)
        {
            _allowedItemsByContainer = allowedItemByContainer;
        }

        /// <summary>
        ///     Check if container should be restricted and get the allowed item type for it.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="allowedItem"></param>
        /// <returns></returns>
        public static bool IsRestrictedContainer(string containerName, out HashSet<string> allowedItems)
        {
            return _allowedItemsByContainer.TryGetValue(containerName, out allowedItems);
        }

        /// <summary>
        ///     Checks if adding item to inventory should be blocked based on
        ///     if the container and item names are restricted. Also notifies
        ///     the player of why adding the item is not allowed.
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool CanAddItem(Inventory inventory, ItemDrop.ItemData item)
        {
            if (inventory == null || item == null) { return false; }

            if (IsRestrictedContainer(inventory.m_name, out HashSet<string> allowedItems))
            {
                var result = allowedItems.Contains(item.PrefabName());
                if (!result)
                {
                    // Message player that item cannot be placed in container.
                    var msg = $"{item.m_shared.m_name} cannnot be placed in {inventory.m_name}";
                    Player.m_localPlayer?.Message(MessageHud.MessageType.Center, msg);
                }
                return result;
            }
            return true;
        }

        /// <summary>
        ///    Patch to trigger CanAddItem check when attempting to add item to inventory.
        ///    If CanAddItem is false then the AddItem method in Valheim will be skipped
        ///    and return false to indicate the item was not added.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="item"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData) })]
        [HarmonyPriority(Priority.First)]
        private static bool AddItemPrefix_1(Inventory __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (!CanAddItem(__instance, item))
            {
                __result = false;
                return __result;
            }
            return true;
        }

        /// <summary>
        ///    Patch to trigger CanAddItem check when attempting to add item to inventory.
        ///    If CanAddItem is false then the AddItem method in Valheim will be skipped
        ///    and return false to indicate the item was not added.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="item"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPriority(Priority.First)]
        private static bool AddItemPrefix_2(Inventory __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (!CanAddItem(__instance, item))
            {
                __result = false;
                return __result;
            }
            return true;
        }

        /// <summary>
        ///     Patch to prevent MoveItemToThis from running if CanAddItem check is false. MoveItemToThis
        ///     will call RemoveItem from the source inventory even in cases where AddItem returns false,
        ///     so the entire method needs to be prevented from running to avoid items being lost when players
        ///     try to place items in containers that do not allow that type of item.
        /// </summary>
        /// <param name="__0"></param>
        /// <param name="__1"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Inventory.MoveItemToThis), new[] { typeof(Inventory), typeof(ItemDrop.ItemData) })]
        [HarmonyPriority(Priority.First)]
        private static bool MoveItemToThisPrefix_1(Inventory __0, ItemDrop.ItemData __1, Inventory __instance)
        {
            var fromInventory = __0;
            var item = __1;
            if (__instance == null || fromInventory == null || item == null) { return false; }

            return CanAddItem(__instance, item);
        }

        /// <summary>
        ///     Patch to prevent MoveItemToThis from running if CanAddItem check is false. MoveItemToThis
        ///     will call RemoveItem from the source inventory even in cases where AddItem returns false,
        ///     so the entire method needs to be prevented from running to avoid items being lost when players
        ///     try to place items in containers that do not allow that type of item.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveItemToThis), new[] { typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPriority(Priority.First)]
        private static bool MoveItemToThisPrefix_2(Inventory __0, ItemDrop.ItemData __1, Inventory __instance)
        {
            var fromInventory = __0;
            var item = __1;
            if (__instance == null || fromInventory == null || item == null) { return false; }
            Log.LogDebug("MoveItemToThisPrefix");
            Log.LogDebug($"Add to: {__instance.m_name}");
            Log.LogDebug($"Item: {item.PrefabName()}");
            return CanAddItem(__instance, item);
        }

        // Dev Note: I don't actually think the MoveAll and RemoveItem patches are necessary
        // but I've kept them just in case IronGate decides to change MoveAll to function
        // like MoveToThis or other mods do stuff during MoveAll.

        /// <summary>
        ///     Patch to check if MoveAll is being used to dump inventory into a restricted
        ///     container and record the container name and allowed item type if it is so that
        ///     removal of items from the source inventory can be prevented if they were not added to the container.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="fromInventory"></param>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveAll), new[] { typeof(Inventory) })]
        [HarmonyPriority(Priority.First)]
        private static void MoveAllPrefix(Inventory __instance, Inventory fromInventory)
        {
            if (__instance == null || fromInventory == null) { return; }
            Log.LogDebug("MoveAllPrefix");
            Log.LogDebug($"Move to: {__instance.m_name}");
            if (IsRestrictedContainer(__instance.m_name, out HashSet<string> allowedItems))
            {
                _targetContainer = __instance.m_name;
                _allowedItems = allowedItems;
            }
        }

        /// <summary>
        ///     Reset the _targetContainer and _allowedItems values since MoveAll has finished.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveAll), new[] { typeof(Inventory) })]
        [HarmonyPriority(Priority.First)]
        private static void MoveAllPostfix()
        {
            _targetContainer = null;
            _allowedItems = null;
        }

        /// <summary>
        ///     Patch to check trigger ShouldRemoveItem check and prevent RemoveItem from running if
        ///     the item was moved during a call to MoveAll and failed to be added to the target
        ///     container because it was restricted and not the allowed item type.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), new[] { typeof(ItemDrop.ItemData) })]
        [HarmonyPriority(Priority.First)]
        private static bool RemoveItemPrefix_1(Inventory __instance, ItemDrop.ItemData item)
        {
            return ShouldRemoveItem(__instance, item);
        }

        /// <summary>
        ///     Patch to check trigger ShouldRemoveItem check and prevent RemoveItem from running if
        ///     the item was moved during a call to MoveAll and failed to be added to the target
        ///     container because it was restricted and not the allowed item type.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Inventory.RemoveItem), new[] { typeof(ItemDrop.ItemData), typeof(int) })]
        [HarmonyPriority(Priority.First)]
        private static bool RemoveItemPrefix_2(Inventory __instance, ItemDrop.ItemData item)
        {
            return ShouldRemoveItem(__instance, item);
        }

        /// <summary>
        ///     Patch to check trigger ShouldRemoveItem check and prevent RemoveItem from running if
        ///     the item was moved during a call to MoveAll and failed to be added to the target
        ///     container because it was restricted and not the allowed item type.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Inventory.RemoveOneItem), new[] { typeof(ItemDrop.ItemData) })]
        [HarmonyPriority(Priority.First)]
        private static bool RemoveOneItemPrefix(Inventory __instance, ItemDrop.ItemData item)
        {
            return ShouldRemoveItem(__instance, item);
        }

        /// <summary>
        ///    Checks if the values for _targetContainer and _allowedItems were set during a
        ///    call to MoveAll and checks it the item matches _allowableItem and therefore
        ///    should be removed since it was added to _targetContainerr
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool ShouldRemoveItem(Inventory __instance, ItemDrop.ItemData item)
        {
            // early return and block removal since it's null
            if (__instance == null || item == null)
            {
                return false;
            }

            // Check if item is being removed because it was moved to a dynamic container
            bool wasAddedToDynamicPile = !string.IsNullOrEmpty(_targetContainer);

            Log.LogDebug("RemoveItemPrefix");
            Log.LogDebug($"Remove from: {__instance.m_name}");
            Log.LogDebug($"Item: {item.PrefabName()}");
            if (wasAddedToDynamicPile && _allowedItems != null)
            {
                return _allowedItems.Contains(item.PrefabName());
            }
            return true;
        }
    }

    internal static class InventoryHelper
    {
        public static string PrefabName(this ItemDrop.ItemData item)
        {
            if (item.m_dropPrefab)
            {
                return item.m_dropPrefab.name;
            }

            Log.LogWarning("Item has missing prefab " + item.m_shared.m_name);
            return item.m_shared.m_name;
        }
    }
}