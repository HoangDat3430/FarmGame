using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private Dictionary<Type, IUIPanelBase> m_LoadedPanels = new Dictionary<Type, IUIPanelBase>();
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        UIFactory.Registry();
        SubscribeEvent();
    }
    private void Start()
    {
        ShowUI<MainUI>();
    }
    public TPanel LoadUI<TPanel>()
        where TPanel : IUIPanelBase
    {
        Type type = typeof(TPanel);
        if (!m_LoadedPanels.TryGetValue(type, out var panel))
        {
            panel = UIFactory.Create<TPanel>(transform);
            m_LoadedPanels.Add(type, panel);
        }
        return (TPanel)panel;
    }
    public void ShowUI<TPanel>()
        where TPanel : IUIPanelBase
    {
        if (!m_LoadedPanels.TryGetValue(typeof(TPanel), out IUIPanelBase ui))
        {
            ui = LoadUI<TPanel>();
        }
        ui.Show();
    }
    public void HideUI<TPanel>()
        where TPanel : IUIPanelBase
    {
        if (m_LoadedPanels.TryGetValue(typeof(TPanel), out IUIPanelBase ui))
        {
            ui.Hide();
        }
        else
        {
            throw new Exception("Invalid UI System");
        }
    }
    public TPanel GetPanel<TPanel>() where TPanel : IUIPanelBase
    {
        Type panel = typeof(TPanel);
        if (m_LoadedPanels.TryGetValue(panel, out IUIPanelBase ui))
        {
            return (TPanel)ui;
        }
        else
        {
            throw new Exception($"UI {panel.Name} hasn't been loaded");
        }
    }
    private void SubscribeEvent()
    {
    }
}
