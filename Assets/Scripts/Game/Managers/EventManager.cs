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
            if (s_EventLookUps.ContainsKey(evt)) return;

            Action<GameEvent> newAction = e => evt((T)e);
            s_EventLookUps[evt] = newAction;

            if (s_Events.TryGetValue(typeof(T), out var internalAction))
                s_Events[typeof(T)] = internalAction + newAction;
            else
                s_Events[typeof(T)] = newAction;
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