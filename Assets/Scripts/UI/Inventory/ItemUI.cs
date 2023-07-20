using GamePlay.Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class ItemUI : MonoBehaviour
    {
        public Image Icon;
        public TMPro.TMP_Text Amount;
        public ItemList_SO Bag { get; set; }
        public int Index { get; set; } = -1;

        public void SetupItemUI(ItemData_SO item, int amount)
        {
            if (item != null && item.ItemIcon != null)
            {
                Icon.sprite = item.ItemIcon;
                Amount.text = amount.ToString();
                Icon.gameObject.SetActive(true);
            }
            else
            {
                Icon.gameObject.SetActive(false);
            }
        }
    }
}