using System;
using GamePlay.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class ItemToolTip : MonoBehaviour
    {
        public TMP_Text ItemName;
        public TMP_Text ItemInfo;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            UpdatePosition();
        }

        private void Update()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            var mousePos = Input.mousePosition;
            _rectTransform.position = mousePos;

            var corners = new Vector3[4];
            _rectTransform.GetWorldCorners(corners);

            var width = corners[3].x - corners[0].x;
            var height = corners[1].y - corners[0].y;

            if (mousePos.y < height)
            {
                _rectTransform.position = mousePos + Vector3.up * (0.6f * height);
            }
        }

        public void SetupToolTip(ItemData_SO item)
        {
            ItemName.text = item.ItemName;
            ItemInfo.text = item.description;
        }
    }
}