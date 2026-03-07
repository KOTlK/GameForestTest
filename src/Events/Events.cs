using System;
using System.Collections.Generic;

public delegate void EventListener<T>(T evnt);

public static class Events {
    public static EventQueue                     GeneralQueue;
    public static Dictionary<string, EventQueue> PrivateQueues;

    public static void Init() {
        GeneralQueue = new();
        GeneralQueue.Clear();

        PrivateQueues = new();
        foreach(var (type, queue) in PrivateQueues) {
            queue.Clear();
        }
    }

    public static void RaiseGeneral<T>(T evnt) {
        GeneralQueue.RaiseEvent(evnt);
    }

    public static void SubGeneral<T>(EventListener<T> listener) {
        GeneralQueue.Subscribe<T>(listener);
    }

    public static void UnsubGeneral<T>(EventListener<T> listener) {
        GeneralQueue.Unsubscribe<T>(listener);
    }

    public static void RaisePrivate<T>(string name, T evnt) {
        if(PrivateQueues.ContainsKey(name) == false) {
            PrivateQueues.Add(name, new EventQueue());
        }

        PrivateQueues[name].RaiseEvent(evnt);
    }

    public static void SubPrivate<T>(string name, EventListener<T> listener) {
        if(PrivateQueues.ContainsKey(name) == false) {
            PrivateQueues.Add(name, new EventQueue());
        }

        PrivateQueues[name].Subscribe<T>(listener);
    }

    public static void UnsubPrivate<T>(string name, EventListener<T> listener) {
        PrivateQueues[name].Unsubscribe<T>(listener);
    }
}