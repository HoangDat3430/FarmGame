using System.Collections;
using System.Collections.Generic;
using Farm;
using Unity.VisualScripting;
using UnityEngine;
using static Farm.TerrainManager;

namespace Farm
{
    public class Worker : MonoBehaviour
    {
        private Vector3 m_CurrentTarget;
        private int m_Speed = 3;
        private bool m_IsIdle;
        private float m_HarvestTime;
        private float m_HarvestTimer;
        private int m_fieldToHavest;
        public bool IsIdle
        {
            get
            {
                return m_IsIdle;
            }
        }
        // Start is called before the first frame update
        void Awake()
        {
            m_fieldToHavest = -1;
            m_IsIdle = true;
            m_HarvestTime = 2;
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
                    CropData cropData = GameManager.Instance.TerrainMgr.GetCropDataByFieldID(m_fieldToHavest);
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
                    if(m_HarvestTimer < m_HarvestTime)
                    {
                        m_HarvestTimer += Time.deltaTime;
                    }
                    else
                    {
                        Harvest();
                        m_HarvestTimer = 0;
                        GameManager.Instance.TerrainMgr.OnWorkerHavestDone(m_fieldToHavest);
                        GoToWareHouse();
                    }
                }
            }
        }
        private void Harvest()
        {
            if(m_fieldToHavest == -1)
            {
                return;
            }
            List<Vector3Int> fieldToHarvest = GameManager.Instance.TerrainMgr.FieldGroups[m_fieldToHavest];
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
            m_fieldToHavest = fieldId;
            m_CurrentTarget = des;
            m_IsIdle = false;
            GameManager.Instance.UpdateIdleWorkers();
        }
        public void GoToWareHouse()
        {
            m_fieldToHavest = -1;
            m_CurrentTarget = GameManager.Instance.WorkerMgr.transform.position;
            m_IsIdle = true;
            GameManager.Instance.UpdateIdleWorkers();
        }
    }
}

