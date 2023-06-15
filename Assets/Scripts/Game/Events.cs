using Game.Managers;
using UnityEngine;

namespace Game
{
    public class Events : MonoBehaviour
    {
        public static EnemyKillEvent EnemyKillEvent = new();
        public static AllObjectivesCompletedEvent AllObjectivesCompletedEvent = new();
        public static PickUpEvent PickUpEvent = new();
        public static PlayerDeathEvent PlayerDeathEvent = new();
        public static DisplayMessageEvent DisplayMessageEvent = new();
    }

    public class AllObjectivesCompletedEvent : GameEvent
    {
    }

    public class PlayerDeathEvent : GameEvent
    {
    }

    public class EnemyKillEvent : GameEvent
    {
        public GameObject Enemy;
        public int RemainingEnemyCount;
    }

    public class PickUpEvent : GameEvent
    {
        public GameObject Pickup;
    }

    public class DisplayMessageEvent : GameEvent
    {
        public float DelayBeforeDisplay;
        public string message;
    }
}