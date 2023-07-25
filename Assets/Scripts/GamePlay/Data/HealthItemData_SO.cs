using UnityEngine;

namespace GamePlay.Data
{
    [CreateAssetMenu(fileName = "Health Item Data", menuName = "Inventory/Health Item Data")]
    public class HealthItemData_SO : ScriptableObject
    {
        public int HealthAmount;
    }
}