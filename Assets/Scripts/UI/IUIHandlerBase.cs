using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IUIHandlerBase
{
    void OnShow();
    void OnHide();
    void RefreshData();
}
public interface IUIHandlerBase<TPanel> : IUIHandlerBase where TPanel : IUIPanelBase
{
    void AttachPanel(TPanel panel);
}
