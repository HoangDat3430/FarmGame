using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIPanelBase 
{
    void Init(IUIHandlerBase handler);
    void Show();
    void Hide();
}
