using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    private static readonly Dictionary<Type, Action<object>> _subscribers = new();

    public static void Subscribe<T>(Action<T> callback)
    {
        Type type = typeof(T);
        if (!_subscribers.ContainsKey(type)) _subscribers[type] = _ => { };
        _subscribers[type] += o => callback((T)o);
    }
    public static void Unsubscribe<T>(Action<T> callback)
    {
        Type type = typeof(T);
        if (_subscribers.ContainsKey(type))
        {
            _subscribers[type] -= o => callback((T)o);
        }
    }
    public static void Publish<T>(T message)
    {
        Type type = typeof(T);
        if (_subscribers.ContainsKey(type))
        {
            _subscribers[type].Invoke(message);
        }
    }
}
