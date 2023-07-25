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
    
    public class SlotHolder : MonoBehaviour, IPointerClickHandler
    {
        public SlotType SlotType;
        public ItemUI ItemUI;

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
    }
}