using System;
using GamePlay.Data;
using GamePlay.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Inventory
{
    public enum SlotType
    {
        BAG,
        WEAPON,
        ARMOR,
        ACTION,
    }
    
    public class SlotHolder : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public SlotType SlotType;
        public ItemUI ItemUI;

        private void OnDisable()
        {
            PlayerInventoryManager.Instance.ToolTip.gameObject.SetActive(false);
        }

        public void UpdateItem()
        {
            switch (SlotType)
            {
                case SlotType.BAG:
                    ItemUI.Bag = PlayerInventoryManager.Instance.InventoryData;
                    break;
                case SlotType.WEAPON:
                    break;
                case SlotType.ARMOR:
                    break;
                case SlotType.ACTION:
                    break;
            }

            var item = ItemUI.Bag.itemList[ItemUI.Index];
            ItemUI.SetupItemUI(item.ItemDataSo, item.amount);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount != 2) return;
            
            // Use Item
            if (ItemUI.GetItem() != null && ItemUI.GetItem().ItemType == ItemType.Usable)
            {
                GameManager.Instance.PlayerHealth.Heal(ItemUI.GetItem().HealthItemData.HealthAmount);
                ItemUI.Bag.itemList[ItemUI.Index].amount--;
            }
            UpdateItem();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ItemUI != null && ItemUI.GetItem())
            {
                PlayerInventoryManager.Instance.ToolTip.SetupToolTip(ItemUI.GetItem());
                PlayerInventoryManager.Instance.ToolTip.gameObject.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PlayerInventoryManager.Instance.ToolTip.gameObject.SetActive(false);
        }
    }
}