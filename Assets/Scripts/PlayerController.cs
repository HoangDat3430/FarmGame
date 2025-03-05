using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Farm
{
    public class PlayerController : MonoBehaviour
    {
        public InputActionAsset InputAction;
        public float Speed = 4.0f;

        public SpriteRenderer Target;
        public Transform ItemAttachBone;

        public int Coins
        {
            get => m_Coins;
            set
            {
                m_Coins = value;
            }
        }

        public InventorySystem Inventory => m_Inventory;
        public Animator Animator => m_Animator;

        //This is private as we don't want to be able to set coins without going through the accessor above that ensure
        //the UI is updated, but is tagged as SerializedField so it appear in the editor so designer can set the starting
        //amount of coins
        [SerializeField]
        private int m_Coins = 10;

        [SerializeField]
        private InventorySystem m_Inventory;
        
        private Rigidbody2D m_Rigidbody;

        private InputAction m_MoveAction;
        private InputAction m_NextItemAction;
        private InputAction m_PrevItemAction;
        private InputAction m_UseItemAction;

        private Vector3 m_CurrentWorldMousePos;
        private Vector2 m_CurrentLookDirection;
        private Vector3Int m_CurrentTarget;

        private TargetMarker m_TargetMarker;

        private bool m_HasTarget = false;

        private Animator m_Animator;

        private InteractiveObject m_CurrentInteractiveTarget = null;
        private Collider2D[] m_CollidersCache = new Collider2D[8];

        private Dictionary<Item, ItemInstance> m_ItemVisualInstance = new();

        private int m_DirXHash = Animator.StringToHash("DirX");
        private int m_DirYHash = Animator.StringToHash("DirY");
        private int m_SpeedHash = Animator.StringToHash("Speed");

        void Awake()
        {
            if (GameManager.Instance.Player != null)
            {
                Destroy(gameObject);
                return;
            }
            
            m_Rigidbody = GetComponent<Rigidbody2D>();
            m_Animator = GetComponentInChildren<Animator>();
            m_TargetMarker = Target.GetComponent<TargetMarker>();
            m_TargetMarker.Hide();
            
            //we can only set DontDestroyOnLoad root object, so ensure its root (Level Designer sometime place the character
            //prefab already in the scene and can sometime tuck it under other object in the hierarchy)
            gameObject.transform.SetParent(null);
            
            GameManager.Instance.Player = this;
            DontDestroyOnLoad(gameObject);
            m_Inventory.Init();
        }

        void Start()
        {
            //Retrieve the action from the InputAction asset, enable them and add the callbacks.
            
            //Move action doesn't have any callback as it will be polled in the movement code directly.
            m_MoveAction = InputAction.FindAction("Gameplay/Move");
            m_MoveAction.Enable();

            m_NextItemAction = InputAction.FindAction("Gameplay/EquipNext");
            m_PrevItemAction = InputAction.FindAction("Gameplay/EquipPrev");

            m_NextItemAction.Enable();
            m_NextItemAction.performed += context =>
            {
                ToggleToolVisual(false);
                m_Inventory.EquipNext();
                ToggleToolVisual(true);
            };
            
            m_PrevItemAction.Enable();
            m_PrevItemAction.performed += context =>
            {
                ToggleToolVisual(false);
                m_Inventory.EquipPrev();
                ToggleToolVisual(true);
            };

            m_UseItemAction = InputAction.FindAction("Gameplay/Use");
            m_UseItemAction.Enable();

            m_UseItemAction.performed += context => UseObject();
            
            m_CurrentLookDirection = Vector2.right;
            
            foreach (var entry in m_Inventory.Entries)
            {
                if (entry.Item != null)
                    CreateItemVisual(entry.Item);
            }
            ToggleToolVisual(true);
        }

        private void Update()
        {
            m_CurrentInteractiveTarget = null;
            m_HasTarget = false;
            
            m_CurrentWorldMousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            //check if we are above an interactive object
            var overlapCount = Physics2D.OverlapPointNonAlloc(m_CurrentWorldMousePos, m_CollidersCache, 1 << 31);
            for (int i = 0; i < overlapCount; ++i)
            {
                var obj = m_CollidersCache[i].GetComponent<InteractiveObject>();
                if (obj != null)
                {
                    m_CurrentInteractiveTarget = obj;
                    m_HasTarget = false;
                    return;
                }
            }
            
            //If we reached here, we are not above UI or an interactive object, so set the cursor to the normal one

            var grid = GameManager.Instance.TerrainMgr?.Grid;

            //some scene may not have a terrain (interior scene)
            if (grid != null)
            {
                var currentCell = grid.WorldToCell(transform.position);
                var pointedCell = grid.WorldToCell(m_CurrentWorldMousePos);

                currentCell.z = 0;
                pointedCell.z = 0;

                var toTarget = pointedCell - currentCell;

                if (Mathf.Abs(toTarget.x) > 1)
                {
                    toTarget.x = (int)Mathf.Sign(toTarget.x);
                }

                if (Mathf.Abs(toTarget.y) > 1)
                {
                    toTarget.y = (int)Mathf.Sign(toTarget.y);
                }


                m_CurrentTarget = currentCell + toTarget;
                Target.transform.position = GameManager.Instance.TerrainMgr.Grid.GetCellCenterWorld(m_CurrentTarget);

                if (m_Inventory.EquippedItem != null
                    && m_Inventory.EquippedItem.CanUse(m_CurrentTarget))
                {
                    m_HasTarget = true;
                    m_TargetMarker.Activate();
                }
                else
                {
                    m_TargetMarker.Hide();
                }
            }
        }

        void UseObject()
        {   
            if (m_CurrentInteractiveTarget != null)
            {
                m_CurrentInteractiveTarget.InteractedWith();
                return;
            }
            
            if (m_Inventory.EquippedItem != null && m_Inventory.EquippedItem.NeedTarget() && !m_HasTarget) return;
            
            List<Vector3Int> tiles = GameManager.Instance.TerrainMgr.GetFieldByTile(m_CurrentTarget);
            if(tiles != null)
            {
                for (int i = 0; i < tiles.Count; i++)
                {
                    UseItem(tiles[i]);
                }
            }
        }

        void FixedUpdate()
        {
            var move = m_MoveAction.ReadValue<Vector2>();

            //note: == and != for vector2 is overriden to take in account floating point imprecision.
            if (move != Vector2.zero)
            {
                SetLookDirectionFrom(move);
            }
            else
            {
                //we aren't moving, look direction is based on the currently aimed toward point
                Vector3 posToMouse = m_CurrentWorldMousePos - transform.position;
                SetLookDirectionFrom(posToMouse);
            }

            var movement = move * Speed;
            var speed = movement.sqrMagnitude;
            
            m_Animator.SetFloat(m_DirXHash, m_CurrentLookDirection.x);
            m_Animator.SetFloat(m_DirYHash, m_CurrentLookDirection.y);
            m_Animator.SetFloat(m_SpeedHash, speed);

            m_Rigidbody.MovePosition(m_Rigidbody.position + movement * Time.deltaTime);
        }

        void SetLookDirectionFrom(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                m_CurrentLookDirection = direction.x > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                m_CurrentLookDirection = direction.y > 0 ? Vector2.up : Vector2.down;
            }
        }

        public bool CanFitInInventory(Item item, int count)
        {
            return m_Inventory.CanFitItem(item, count);
        }
        
        public bool AddItem(Item newItem, float amount = 1)
        {
            CreateItemVisual(newItem);
            return m_Inventory.AddItem(newItem, amount);
        }
        public void ToggleControl(bool canControl)
        {
            if (canControl)
            {
                m_MoveAction.Enable();
                m_NextItemAction.Enable();
                m_PrevItemAction.Enable();
                m_UseItemAction.Enable();
            }
            else
            {
                m_MoveAction.Disable();
                m_NextItemAction.Disable();
                m_PrevItemAction.Disable();
                m_UseItemAction.Disable();
            }
        }

        public void UseItem(Vector3Int target)
        {
            if(m_Inventory.EquippedItem == null)
                return;
            
            var previousEquipped = m_Inventory.EquippedItem;
            
            m_Inventory.UseEquippedObject(target);

            if (m_ItemVisualInstance.ContainsKey(previousEquipped))
            {
                var visual = m_ItemVisualInstance[previousEquipped];
                m_Animator.SetTrigger(visual.AnimatorHash);

                if (visual.Animator != null)
                {
                    if (!visual.Instance.activeInHierarchy)
                    {
                        //enable all parent as if it's disabled, value cannot be set
                        var current = visual.Instance.transform;
                        while (current != null)
                        {
                            current.gameObject.SetActive(true);
                            current = current.parent;
                        }
                    }
                    visual.Animator.SetFloat(m_DirXHash, m_CurrentLookDirection.x);
                    visual.Animator.SetFloat(m_DirYHash, m_CurrentLookDirection.y);
                    visual.Animator.SetTrigger("Use");
                }
            }
            
            if (m_Inventory.EquippedItem == null)
            {
                //this mean we finished using an item, the entry is now empty, so we need to disable the visual if any
                if (previousEquipped != null)
                {
                    //This is a bit of a quick fix, this will let any animation to finish playing before we disable the visual.
                    StartCoroutine(DelayedObjectDisable(previousEquipped));
                }
            }
        }

        IEnumerator DelayedObjectDisable(Item item)
        {
            yield return new WaitForSeconds(1.0f);
            ToggleVisualExplicit(false, item);
        }

        void ToggleToolVisual(bool enable)
        {
            if (m_Inventory.EquippedItem != null && m_ItemVisualInstance.TryGetValue(m_Inventory.EquippedItem, out var itemVisual))
            {
                itemVisual.Instance.SetActive(enable);
            }
        }

        void ToggleVisualExplicit(bool enable, Item item)
        {
            if (item != null && m_ItemVisualInstance.TryGetValue(item, out var itemVisual))
            {
                itemVisual.Instance.SetActive(enable);
            }
        }

        void CreateItemVisual(Item item)
        {
            GameObject visualPrefab = Resources.Load(item.PrefabPath) as GameObject;
            if (visualPrefab != null && !m_ItemVisualInstance.ContainsKey(item))
            {
                var newVisual = Instantiate(visualPrefab, ItemAttachBone, false);
                newVisual.SetActive(false);
                m_ItemVisualInstance[item] = new ItemInstance()
                {
                    Instance = newVisual,
                    Animator = newVisual.GetComponentInChildren<Animator>(),
                    AnimatorHash = Animator.StringToHash(item.PlayerAnimatorTriggerUse)
                };
            }
        }
    }

    class ItemInstance
    {
        public GameObject Instance;
        public Animator Animator;
        public int AnimatorHash;
    }
}