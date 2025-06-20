using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class UIFactory
{
    private static readonly Dictionary<Type, Func<object>> _panelToHandlerMap = new();
    private static Dictionary<Type, string> _panelPathMap = new();
    public static void Registry()
    {
        RegistryPanel<MainUI, MainUIHandler>("MainUI");
        RegistryPanel<FarmListView, FarmListHandler>("FarmList");
    }
    public static void RegistryPanel<TPanel, THandler>(string prefabName) 
        where TPanel : IUIPanelBase
        where THandler : IUIHandlerBase<TPanel>, new()
    {
        string path = Path.Combine("Prefabs", "UI", prefabName);
        _panelPathMap.Add(typeof(TPanel), path);
        _panelToHandlerMap.Add(typeof(TPanel), () => new THandler());
    }
    
    public static TPanel Create<TPanel>(Transform parent)
        where TPanel : IUIPanelBase
    {
        Type type = typeof(TPanel);
        if (!_panelPathMap.TryGetValue(type, out var prefabPath))
        {
            throw new Exception($"UI panel not registered: {type.Name}");
        }

        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            throw new Exception($"Prefab not found at path: {prefabPath}");
        }

        GameObject instance = GameObject.Instantiate(prefab, parent);
        if(!_panelToHandlerMap.TryGetValue(type, out var handlerType))
        {
            throw new Exception($"UI handler not registered for panel: {type.Name}");
        }
        TPanel panel = instance.GetComponent<TPanel>();
        panel.Init();
        return panel;
    }
}

