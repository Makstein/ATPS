using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GamePlay.Data
{
    [CreateAssetMenu(fileName = "Item List", menuName = "Inventory/Item List", order = 0)]
    public class ItemList_SO : ScriptableObject
    {
        public List<InventoryItem> itemList = new();

        public void AddItem(ItemData_SO item, int amount)
        {
            if (!item.Stackable || itemList.Count(x => x.ItemDataSo == item) == 0) // 当前已拥有此物品
            {
                foreach (var t in itemList.Where(t => t.ItemDataSo == null))
                {
                    t.ItemDataSo = item;
                    t.amount = amount;
                    break;
                }
            }
            else // 已拥有此物品且可堆叠
            {
                foreach (var inventoryItem in itemList.Where(inventoryItem => inventoryItem.ItemDataSo == item))
                {
                    inventoryItem.amount += amount;
                    break;
                }
            }
        }
    }

    [Serializable]
    public class InventoryItem
    {
        public ItemData_SO ItemDataSo;
        public int amount;
    }
}