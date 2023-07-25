using UnityEngine;

namespace GamePlay.Data
{
    public enum ItemType
    {
        Usable,
        Weapon,
        Armor,
    }
    
    /// <summary>
    /// 物品属性SO
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data", order = 0)]
    public class ItemData_SO : ScriptableObject
    {
        public ItemType ItemType;
        public string ItemName;
        public Sprite ItemIcon;
        public int ItemCount;
        public bool Stackable; // 是否可堆叠

        [TextArea]
        public string description;

        [Header("Prefab")] public GameObject Prefab;

        [Header("Usable Item")] public HealthItemData_SO HealthItemData;
    }
}