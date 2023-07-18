using GamePlay.Managers;
using UnityEngine;

namespace UI.Inventory
{
    public enum SlotType
    {
        BAG,
        WEAPON,
        ARMOR,
        ACTION,
    }
    
    public class SlotHolder : MonoBehaviour
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
            if (item.ItemDataSo == null) return;
            ItemUI.SetupItemUI(item.ItemDataSo, item.amount);
        }
    }
}