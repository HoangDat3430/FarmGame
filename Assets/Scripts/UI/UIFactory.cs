using Farm;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface IUIEntry
{
    IUIHandlerBase GetHandler();
    string PrefabPath{get;}
}
class UIEntry<TPanel> : IUIEntry where TPanel : IUIPanelBase
{
    private readonly string _prefabPath;
    private readonly Func<IUIHandlerBase<TPanel>> _handlerFactory;
    public string PrefabPath => _prefabPath;
    public UIEntry(string prefabPath, Func<IUIHandlerBase<TPanel>> handlerFactory)
    {
        _prefabPath = prefabPath;
        _handlerFactory = handlerFactory;
    }
    public IUIHandlerBase GetHandler()
    {
        return _handlerFactory.Invoke();
    }
}

public static class UIFactory
{
    private static readonly Dictionary<Type, IUIEntry> _uiEntries = new();
    public static void Registry()
    {
        RegistryPanel("MainUI", () => new MainUIHandler());
        RegistryPanel("FarmList", () => new FarmListHandler());
        RegistryPanel("InventoryPanel", () => new InventoryHandler());
    }
    public static void RegistryPanel<TPanel>(string prefabName, Func<IUIHandlerBase<TPanel>> handlerFactory)
        where TPanel : IUIPanelBase
    {
        string path = Path.Combine("Prefabs", "UI", prefabName);
        _uiEntries[typeof(TPanel)] = new UIEntry<TPanel>(path, handlerFactory);
    }
    public static TPanel Create<TPanel>(Transform parent)
        where TPanel : IUIPanelBase
    {
        Type type = typeof(TPanel);
        if (!_uiEntries.TryGetValue(type, out var entry))
        {
            throw new Exception($"UI panel not registered: {type.Name}");
        }

        GameObject prefab = Resources.Load<GameObject>(entry.PrefabPath);
        if (prefab == null)
        {
            throw new Exception($"Prefab not found at path: {entry}");
        }

        GameObject instance = GameObject.Instantiate(prefab, parent);
        TPanel panel = instance.GetComponent<TPanel>();
        panel.Init(entry.GetHandler());
        return panel;
    }
}

