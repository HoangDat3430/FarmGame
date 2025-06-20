using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FarmListView : UIPanelBase<FarmListView, FarmListHandler>
{
    public Transform ViewPort;
    public GameObject FarmTpl;
    public Button CloseBtn;

    protected override void RegisterInternalEvents()
    {
        CloseBtn.onClick.AddListener(Hide);
    }
}
