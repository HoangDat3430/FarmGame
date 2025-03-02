using UnityEngine;


namespace Farm
{
    public abstract class InteractiveObject : MonoBehaviour
    {
        public abstract void InteractedWith();

        protected void Awake()
        {
            //The Player control raycast in layer 31 to check for interactive object
            gameObject.layer = 31;
        }
    }
}