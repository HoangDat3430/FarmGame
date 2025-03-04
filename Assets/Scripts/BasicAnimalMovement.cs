using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;
using Random = UnityEngine.Random;


namespace Farm
{
    /// <summary>
    /// Simply make a GameObject "roam" inside a given collider 2D bound
    /// </summary>
    public class BasicAnimalMovement : MonoBehaviour
    {

        [Min(0)] public float MinIdleTime;
        [Min(0)] public float MaxIdleTime;

        [Min(0)] public float Speed = 2.0f;

        private float m_IdleTimer;
        private float m_CurrentIdleTarget;
        
        private Vector3Int[] Area = new Vector3Int[10];

        private Vector3 m_CurrentTarget;

        private bool m_IsIdle;

        private Animator m_Animator;
        private int SpeedHash = Animator.StringToHash("Speed");

        private void Start()
        {
            if (MaxIdleTime <= MinIdleTime)
                MaxIdleTime = MinIdleTime + 0.1f;

            m_Animator = GetComponentInChildren<Animator>();

            m_IsIdle = true;
        }

        private void Update()
        {
            if (m_IsIdle)
            {
                m_IdleTimer += Time.deltaTime;

                if (m_IdleTimer >= m_CurrentIdleTarget && Area.Length > 0)
                {
                    PickNewTarget();
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, m_CurrentTarget, Speed * Time.deltaTime);
                if (transform.position == m_CurrentTarget)
                {
                    PickNewIdleTime();
                }
            }
        }

        void PickNewIdleTime()
        {
            if (m_Animator != null)
                m_Animator.SetFloat(SpeedHash, 0.0f);

            m_IsIdle = true;
            m_CurrentIdleTarget = Random.Range(MinIdleTime, MaxIdleTime);
            m_IdleTimer = 0.0f;
        }

        void PickNewTarget()
        {
            m_IsIdle = false;

            int randomIndex = Random.Range(0, Area.Length-1);
            var pts = new Vector2(Area[randomIndex].x, Area[randomIndex].y);
            
            m_CurrentTarget = pts;
            var toTarget = m_CurrentTarget - transform.position;

            bool flipped = toTarget.x < 0;
            transform.localScale = new Vector3(flipped ? -1 : 1, 1, 1);

            if (m_Animator != null)
                m_Animator.SetFloat(SpeedHash, Speed);
        }
        public void SetField(List<Vector3Int> area)
        {
            Area = area.ToArray();
            PickNewTarget();
        }
    }
}