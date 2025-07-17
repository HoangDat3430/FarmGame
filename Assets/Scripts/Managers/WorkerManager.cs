using System;
using System.Collections.Generic;
using Farm;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class WorkerManager : MonoBehaviour
{
    private List<Worker> m_Workers = new List<Worker>();
    private GameObject m_WorkerPrefab;
    public List<Worker> Workers
    {
        get
        {
            return m_Workers;
        }
    }
    public int IdleWorkersCount = 0;
    private void Awake()
    {
        GameManager.Instance.WorkerMgr = this;
        m_WorkerPrefab = Resources.Load<GameObject>("Prefabs/Worker");
    }
    private void OnEnable()
    {
        EventBus.Subscribe<WorkerStateChangedEvent>(LookingForHarvestableField);
        EventBus.Subscribe<CropNeedToHarvestEvent>(LookingForIdleWorker);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<WorkerStateChangedEvent>(LookingForHarvestableField);
        EventBus.Unsubscribe<CropNeedToHarvestEvent>(LookingForIdleWorker);
    }
    private void LookingForHarvestableField(WorkerStateChangedEvent e)
    {
        if (e.isIdle)
        {
            Worker worker = m_Workers.Find(x => x.IsIdle);
            if (worker != null)
            {
                int fieldId = GameManager.Instance.TerrainMgr.GetHarvestableField();
                if (fieldId != -1)
                {
                    Vector3Int fieldPosition = GameManager.Instance.TerrainMgr.AddWaitingToHarvestField(fieldId);
                    worker.GoToHavest(fieldId, fieldPosition);
                    return;
                }
            }
        }
    }
    private void LookingForIdleWorker(CropNeedToHarvestEvent e)
    {
        Worker worker = m_Workers.Find(x => x.IsIdle);
        if (worker != null)
        {
            Vector3Int fieldPosition = GameManager.Instance.TerrainMgr.AddWaitingToHarvestField(e.fieldID);
            worker.GoToHavest(e.fieldID, fieldPosition);
            return;
        }
    }
    public void EmployWorker()
    {
        GameObject worker = Instantiate(m_WorkerPrefab);
        Worker newWorker = worker.GetComponent<Worker>();
        m_Workers.Add(newWorker);
    }
    public int GetIdleWorkersCount()
    {
        int count = 0;
        for (int i = 0; i < m_Workers.Count; i++)
        {
            if (m_Workers[i].IsIdle)
            {
                count++;
            }
        }
        return count;
    }
    public void Save(ref WorkerMgrSaveData data)
    {
        data.workerSaveDatas = new List<WorkerSaveData>();
        for (int i = 0; i < m_Workers.Count; i++)
        {
            data.workerSaveDatas.Add(m_Workers[i].Save());
        }
        data.WorkerCount = m_Workers.Count;
    }
    public void Load(WorkerMgrSaveData data)
    {
        Workers.Clear();
        for (int i = 0; i < data.WorkerCount; i++)
        {
            EmployWorker();
            m_Workers[i].Load(data.workerSaveDatas[i]);
        }
    }
}
[Serializable]
public struct WorkerMgrSaveData
{
    public List<WorkerSaveData> workerSaveDatas;
    public int WorkerCount;
}
