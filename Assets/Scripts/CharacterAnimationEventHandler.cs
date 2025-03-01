using System;
using System.Collections;
using System.Collections.Generic;
using Farm;
using UnityEngine;

namespace Farm
{
    /// <summary>
    /// This allows to lock and unlock controlling the Player Controller. This is used by Animation Event to stop moving
    /// during e.g. a tool animation. This is added on the same object that have the Animation Controller so it can
    /// receive animation event.
    /// </summary>
    public class CharacterAnimationEventHandler : MonoBehaviour
    {
        private PlayerController m_Controller;

        private void Awake()
        {
            //Get In Parent as this is on the GameObject with the animator and the controller is at the root of the
            //character.
            m_Controller = GetComponentInParent<PlayerController>();
        }

        void LockControl()
        {
            m_Controller.ToggleControl(false);
        }

        void UnlockControl()
        {
            m_Controller.ToggleControl(true);
        }
    }
}
