using GamePlay.Data;
using GamePlay.Managers;
using UnityEngine;

namespace GamePlay
{
    public class WeaponPickup : Pickup
    {
        public ItemData_SO ItemData;
        
        protected override void OnPicked(ThirdController controller)
        {
            PlayerInventoryManager.Instance.InventoryData.AddItem(ItemData, ItemData.ItemCount);
            PlayerInventoryManager.Instance.InventoryUI.RefreshUI();
            
            PlayPickupFeedback();
            Destroy(gameObject);
        }
    }
}