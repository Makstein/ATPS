using System;
using GamePlay.Data;
using UI.Inventory;
using UnityEngine;

namespace GamePlay.Managers
{
    public class PlayerInventoryManager : MonoBehaviour
    {
        // Scriptable Object: Backpack
        public ItemList_SO InventoryData;

        public static PlayerInventoryManager Instance;
        
        [Header("Containers")] public ContainerUI InventoryUI;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InventoryUI.RefreshUI();
        }
    }
}