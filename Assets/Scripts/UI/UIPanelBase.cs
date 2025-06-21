using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public abstract class UIPanelBase<TPanel, THandler> : MonoBehaviour, IUIPanelBase
    where TPanel : UIPanelBase<TPanel, THandler>
    where THandler : IUIHandlerBase<TPanel>
{
    protected THandler _handler;
    public THandler Handler => _handler;
    public virtual void Init(IUIHandlerBase handler)
    {
        _handler = (THandler)handler;
        _handler.AttachPanel((TPanel)this);
        RegisterInternalEvents();
    }
    protected virtual void RegisterInternalEvents()
    {
    }
    public virtual void Show()
    {
        gameObject.SetActive(true);
        _handler.OnShow();
    }
    public virtual void Hide()
    {
        gameObject.SetActive(false);
        _handler.OnHide();
    }
}
