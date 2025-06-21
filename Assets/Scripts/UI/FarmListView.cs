using UnityEngine;
using UnityEngine.UI;
using Farm;
using TMPro;
using System;

public class FarmListView : UIPanelBase<FarmListView, FarmListHandler>
{
    public Transform ViewPort;
    public GameObject FarmTpl;
    public Button CloseBtn;

    public Action OnBuyFarm;

    protected override void RegisterInternalEvents()
    {
        CloseBtn.onClick.AddListener(Hide);
    }
    public void ShowLandList()
    {
        var farmList = GameManager.Instance.TerrainMgr.FieldGroups;
        var lockedFarms = GameManager.Instance.TerrainMgr.RemaningLock;
        bool enough = farmList.Count <= ViewPort.childCount;
        if (!enough)
        {
            int diff = farmList.Count - ViewPort.childCount;
            for (int i = 0; i < diff; i++)
            {
                Instantiate(FarmTpl, ViewPort);
            }
        }
        for (int i = 1; i <= farmList.Count; i++)
        {
            GameObject farm = ViewPort.GetChild(i - 1).gameObject;
            Image icon = farm.transform.Find("Icon").GetComponent<Image>();
            Button buyFarm = icon.GetComponent<Button>();
            TMP_Text leftTime = farm.transform.Find("Time/remaning").GetComponent<TMP_Text>();
            buyFarm.enabled = false;
            if (lockedFarms.ContainsKey(i))
            {
                icon.sprite = Resources.Load<Sprite>("Sprites/UI/Lock");
                leftTime.transform.parent.gameObject.SetActive(false);
                buyFarm.enabled = true;
                buyFarm.onClick.RemoveAllListeners();
                buyFarm.onClick.AddListener(()=> OnBuyFarm?.Invoke());
            }
            else
            {
                TerrainManager.CropData cropData = GameManager.Instance.TerrainMgr.GetCropDataByFieldID(i);
                if (cropData != null)
                {
                    icon.gameObject.SetActive(true);
                    icon.sprite = Resources.Load<Sprite>(cropData.GrowingCrop.Product.IconPath);
                    leftTime.transform.parent.gameObject.SetActive(true);
                    int time = (int)(cropData.GrowingCrop.GrowthTime - cropData.GrowthTimer);
                    TimeSpan timeSpan = TimeSpan.FromSeconds(time);
                    string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        timeSpan.Hours,
                        timeSpan.Minutes,
                        timeSpan.Seconds);
                    leftTime.text = formattedTime;
                }
                else
                {
                    icon.gameObject.SetActive(false);
                    leftTime.transform.parent.gameObject.SetActive(false);
                }
            }
        }
    }
}
