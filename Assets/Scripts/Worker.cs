using System;
using System.Collections.Generic;
using UnityEngine;
using static Farm.TerrainManager;

namespace Farm
{
    public class OnWorkerReadyEvent{}
    public class Worker : MonoBehaviour
    {
        private Vector3 m_CurrentTarget;
        private int m_Speed = 3;
        private bool m_IsIdle;
        private float m_HarvestTime = 2;
        private float m_HarvestTimer;
        private int m_FieldToHavest;
        public bool IsIdle
        {
            get
            {
                return m_IsIdle;
            }
            set
            {
                if (m_IsIdle != value)
                {
                    m_IsIdle = value;
                    if (m_IsIdle)
                    {
                        EventBus.Publish(new OnWorkerReadyEvent());
                    }
                }
            }
        }
        // Start is called before the first frame update
        void Awake()
        {
            m_FieldToHavest = -1;
            IsIdle = true;
            m_HarvestTimer = 0;
            m_CurrentTarget = GameManager.Instance.WorkerMgr.transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (transform.position != m_CurrentTarget)
            {
                transform.position = Vector3.MoveTowards(transform.position, m_CurrentTarget, m_Speed * Time.deltaTime);
                if (!m_IsIdle)
                {
                    CropData cropData = GameManager.Instance.TerrainMgr.GetCropDataByFieldID(m_FieldToHavest);
                    if (cropData == null || !Mathf.Approximately(cropData.GrowthRatio, 1.0f))
                    {
                        GoToWareHouse();
                    }
                }
            }
            else
            {
                if (m_CurrentTarget != GameManager.Instance.WorkerMgr.transform.position)
                {
                    if (m_HarvestTimer < m_HarvestTime)
                    {
                        m_HarvestTimer += Time.deltaTime;
                    }
                    else
                    {
                        Harvest();
                        m_HarvestTimer = 0;
                        GameManager.Instance.TerrainMgr.OnWorkerHavestDone(m_FieldToHavest);
                        GoToWareHouse();
                    }
                }
            }
        }
        private void Harvest()
        {
            if (m_FieldToHavest == -1)
            {
                return;
            }
            List<Vector3Int> fieldToHarvest = GameManager.Instance.TerrainMgr.FieldGroups[m_FieldToHavest];
            for (int i = 0; i < fieldToHarvest.Count; i++)
            {
                var product = GameManager.Instance.TerrainMgr.HarvestAt(fieldToHarvest[i]);

                if (product != null)
                {
                    float toolBoost = (float)(GameManager.Instance.Player.ToolLevel - 1) / 10;
                    float productivityBoost = product.ProductPerHarvest + toolBoost;
                    GameManager.Instance.Player.AddItem(product.Product, productivityBoost);
                }
            }
        }
        public void GoToHavest(int fieldId, Vector3Int des)
        {
            m_FieldToHavest = fieldId;
            m_CurrentTarget = des;
            IsIdle = false;
            GameManager.Instance.UpdateIdleWorkers();
        }
        public void GoToWareHouse()
        {
            m_FieldToHavest = -1;
            m_CurrentTarget = GameManager.Instance.WorkerMgr.transform.position;
            IsIdle = true;
            GameManager.Instance.UpdateIdleWorkers();
        }
        public WorkerSaveData Save()
        {
            WorkerSaveData data = new WorkerSaveData();
            data.Position = transform.position;
            data.CurrentTarget = m_CurrentTarget;
            data.IsIdle = m_IsIdle;
            data.HarvestTimer = m_HarvestTimer;
            data.FieldToHavest = m_FieldToHavest;
            return data;
        }
        public void Load(WorkerSaveData data)
        {
            transform.position = data.Position;
            m_CurrentTarget = data.CurrentTarget;
            IsIdle = data.IsIdle;
            m_HarvestTimer = data.HarvestTimer;
            m_FieldToHavest = data.FieldToHavest;
        }
    }
    [System.Serializable]
    public struct WorkerSaveData
    {
        public Vector3 Position;
        public Vector3 CurrentTarget;
        public bool IsIdle;
        public float HarvestTimer;
        public int FieldToHavest;
    }
}


