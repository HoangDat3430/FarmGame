using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIHandlerBase<TPanel> where TPanel : IUIPanelBase
{
    void AttachPanel(TPanel panel);
    void OnShow();
    void OnHide();
    void RefreshData();
}
