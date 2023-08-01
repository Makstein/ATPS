using System;
using System.Linq;
using GamePlay.Data;
using UI.Inventory;
using UnityEngine;
using UnityEngine.Rendering;

namespace GamePlay.Managers
{
    public class PlayerInventoryManager : MonoBehaviour
    {
        // Scriptable Object: Backpack
        public ItemList_SO InventoryData;

        public static PlayerInventoryManager Instance;

        public Canvas DragCanvas;

        public DragData currentDrag;

        public ItemToolTip ToolTip;

        [Header("Containers")] public ContainerUI InventoryUI;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InventoryUI.RefreshUI();
        }

        /// <summary>
        /// 遍历判断结束拖拽时当前鼠标位置是否在背包格子中
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool CheckInInventoryUI(Vector3 position)
        {
            return InventoryUI.SlotHolders.Select(t1 => t1.transform as RectTransform)
                .Any(t => RectTransformUtility.RectangleContainsScreenPoint(t, position));
        }

        public void SwapItem(int current, int target)
        {
            if (current == target)
            {
                return;
            }
            var targetItem = InventoryData.itemList[target];
            var currentItem = InventoryData.itemList[current];
            var isSameItem = targetItem.ItemDataSo == currentItem.ItemDataSo;
            if (isSameItem)
            {
                targetItem.amount += InventoryData.itemList[current].amount;
                currentItem.ItemDataSo = null;
                currentItem.amount = 0;
            }
            else
            {
                InventoryData.itemList[current] = targetItem;
                InventoryData.itemList[target] = currentItem;
            }
        }
    }

    public class DragData
    {
        public SlotHolder OriginSlotHolder;
        public RectTransform originalParent;
    }
}