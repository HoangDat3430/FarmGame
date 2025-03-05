using System.Collections;
using System.Collections.Generic;
using Farm;
using UnityEngine;

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
    private void Awake()
    {
        GameManager.Instance.WorkerMgr = this;
        m_WorkerPrefab = Resources.Load<GameObject>("Prefabs/Worker");
    }
    void Start()
    {
        
    }

    void Update()
    {
        for (int i = 0; i < m_Workers.Count; i++)
        {
            if (m_Workers[i].IsIdle)
            {
                int fieldId = GameManager.Instance.TerrainMgr.GetHarvestableField();
                if (fieldId != -1)
                {
                    Vector3Int fieldPosition = GameManager.Instance.TerrainMgr.AddWaitingToHarvestField(fieldId);
                    m_Workers[i].GoToHavest(fieldId, fieldPosition);
                    return;
                }
                        
            }
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
        for(int i = 0; i < data.WorkerCount; i++)
        {
            EmployWorker();
            m_Workers[i].Load(data.workerSaveDatas[i]);
        }
    }
}
[System.Serializable]
public struct WorkerMgrSaveData
{
    public List<WorkerSaveData> workerSaveDatas;
    public int WorkerCount;
}
