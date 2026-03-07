using System.Collections.Generic;

public class EventQueue {
    public Dictionary<Type, Delegate> Listeners = new();

    public void Clear() {
        Listeners.Clear();
    }

    public void RaiseEvent<T> (T evnt) {
        var type = typeof(T);

        if(Listeners.TryGetValue(type, out var list)) {
            (list as EventListener<T>)?.Invoke(evnt);
        }
    }

    public void Subscribe<T>(EventListener<T> listener) {
        var type = typeof(T);

        if(Listeners.ContainsKey(type) == false) {
            Listeners[type] = listener;
        } else {
            Listeners[type] = (EventListener<T>)Listeners[type] + listener;
        }
    }

    public void Unsubscribe<T>(EventListener<T> listener) {
        var type = typeof(T);

        if(Listeners.ContainsKey(type)) {
            Listeners[type] = (EventListener<T>)Listeners[type] - listener;
        }
    }
}