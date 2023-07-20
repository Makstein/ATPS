using System;
using GamePlay.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Inventory
{
    [RequireComponent(typeof(ItemUI))]
    public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private ItemUI currentItemUI;
        private SlotHolder currentSlotHolder;
        private SlotHolder targetSlotHolder;

        private void Awake()
        {
            currentItemUI = GetComponent<ItemUI>();
            currentSlotHolder = GetComponentInParent<SlotHolder>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            PlayerInventoryManager.Instance.currentDrag = new DragData()
            {
                OriginSlotHolder = GetComponentInParent<SlotHolder>(),
                originalParent = (RectTransform)transform.parent
            };

            // 防止物品被背包空格挡住
            transform.SetParent(PlayerInventoryManager.Instance.DragCanvas.transform, true);
        }

        // 拖动过程中事件
        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (PlayerInventoryManager.Instance.CheckInInventoryUI(eventData.position))
                {
                    targetSlotHolder = eventData.pointerEnter.gameObject.GetComponent<SlotHolder>()
                        ? eventData.pointerEnter.gameObject.GetComponent<SlotHolder>()
                        : eventData.pointerEnter.gameObject.GetComponentInParent<SlotHolder>();

                    switch (targetSlotHolder.SlotType)
                    {
                        case SlotType.BAG:
                            PlayerInventoryManager.Instance.SwapItem(currentSlotHolder.ItemUI.Index,
                                targetSlotHolder.ItemUI.Index);
                            break;
                        case SlotType.ARMOR:
                            break;
                        case SlotType.ACTION:
                            break;
                        case SlotType.WEAPON:
                            break;
                    }
                    
                    currentSlotHolder.UpdateItem();
                    targetSlotHolder.UpdateItem();
                }
            }

            var t = transform;
            t.SetParent(PlayerInventoryManager.Instance.currentDrag.originalParent);
            
            var rt = t as RectTransform;
            rt!.offsetMax = -Vector2.one * 5;
            rt.offsetMin = Vector2.one * 5;
        }
    }
}