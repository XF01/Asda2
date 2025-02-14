﻿using System.Collections;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Items
{
    /// <summary>
    /// General interface for anything that can hold items (BaseInventory and PartialInventory mostly)
    /// </summary>
    public interface IInventory : IList<Item>, ICollection<Item>, IEnumerable<Item>, IEnumerable
    {
        /// <summary>The actual owner of this srcCont.</summary>
        Character Owner { get; }

        /// <summary>
        /// The array of items that backup this inventory (don't modify from outside)
        /// </summary>
        Item[] Items { get; }

        /// <summary>
        /// The maximum amount of items, supported by this inventory
        /// </summary>
        int MaxCount { get; }

        /// <summary>Whether Count == 0</summary>
        bool IsEmpty { get; }

        /// <summary>Whether Count == MaxCount</summary>
        bool IsFull { get; }

        /// <summary>Returns the next empty available slot</summary>
        int FindFreeSlot();

        /// <summary>
        /// Returns whether the given slot exists in this inventory
        /// </summary>
        bool IsValidSlot(int slot);

        /// <summary>
        /// Tries to add the given item to the given slot in this srcCont
        /// </summary>
        /// <returns>Whether the item could be added</returns>
        InventoryError TryAdd(int slot, Item item, bool isNew, ItemReceptionType reception);

        /// <summary>Tries to add the item to a free slot in this srcCont</summary>
        /// <returns>Whether the item could be added</returns>
        InventoryError TryAdd(Item item, bool isNew, ItemReceptionType reception);

        /// <summary>
        /// Removes the item at the given slot. If you intend to enable the user continuing to use that item, do not use this method
        /// but use PlayerInventory.TrySwap instead.
        /// </summary>
        /// <returns>Whether there was an item to be removed and removal was successful</returns>
        Item Remove(int slot, bool ownerChange);

        /// <summary>
        /// Deletes the item in the given slot (item cannot be re-used afterwards)
        /// </summary>
        /// <returns>Whether the given item could be deleted</returns>
        bool Destroy(int slot);
    }
}