using UnityEngine;

namespace UI.Inventory
{
    public class ContainerUI : MonoBehaviour
    {
        public SlotHolder[] SlotHolders;

        public void RefreshUI()
        {
            for (var i = 0; i < SlotHolders.Length; ++i)
            {
                SlotHolders[i].ItemUI.Index = i;
                SlotHolders[i].UpdateItem();
            }
        }
    }
}