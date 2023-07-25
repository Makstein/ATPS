using System;
using System.Collections.Generic;

namespace Game.Managers
{
    public class GameEvent
    {
    }

    public static class EventManager
    {
        private static readonly Dictionary<Type, Action<GameEvent>> s_Events = new();
        private static readonly Dictionary<Delegate, Action<GameEvent>> s_EventLookUps = new();

        public static void AddListener<T>(Action<T> evt) where T : GameEvent
        {
            // 防止为事件添加重复响应函数 Avoid to add same callback to an event
            if (s_EventLookUps.ContainsKey(evt)) return;

            void NewAction(GameEvent e) => evt((T)e); // 等同于 Action<GameEvent> newAction = e => evt((T)e)，本地函数拥有更少的额外开销
            s_EventLookUps[evt] = NewAction;

            if (s_Events.TryGetValue(typeof(T), out var internalAction))
                s_Events[typeof(T)] = internalAction + NewAction;
            else
                s_Events[typeof(T)] = NewAction;
        }

        public static void RemoveListener<T>(Action<T> evt) where T : GameEvent
        {
            if (!s_EventLookUps.TryGetValue(evt, out var action)) return;

            if (!s_Events.TryGetValue(typeof(T), out var tempAction)) return;

            tempAction -= action;
            if (tempAction == null)
                s_Events.Remove(typeof(T));
            else
                s_Events[typeof(T)] = tempAction;

            s_EventLookUps.Remove(evt);
        }

        public static void Broadcast(GameEvent evt)
        {
            if (s_Events.TryGetValue(evt.GetType(), out var action)) action.Invoke(evt);
        }

        public static void Clear()
        {
            s_Events.Clear();
            s_EventLookUps.Clear();
        }
    }
}