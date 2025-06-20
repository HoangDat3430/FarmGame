using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIHandlerBase<TPanel> : IUIHandlerBase<TPanel> where TPanel : IUIPanelBase
{
    protected TPanel _panel;
    public virtual void AttachPanel(TPanel panel)
    {
        _panel = panel;
        RegisterEvents();
    }
    public virtual void OnShow()
    {
        
    }
    public virtual void OnHide()
    {
        
    }
    public virtual void RefreshData()
    {
    }
    protected virtual void RegisterEvents()
    {
    }
    protected virtual void UnregisterEvents()
    {
    }
}
