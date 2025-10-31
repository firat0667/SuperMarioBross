using NaivePhysics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Turtle : Goomba
    {
        private bool m_bIsInShell = false;
        private bool m_bIsBouncing = false;

        #region Properties

        protected override bool ShouldWalk => !m_bIsInShell;

        #endregion

        protected override void SwitchDirection()
        {
            base.SwitchDirection();

            // show the right head!
            transform.Find("LeftHead").gameObject.SetActive(!m_bIsInShell && !m_bWalkRight);
            transform.Find("RightHead").gameObject.SetActive(!m_bIsInShell && m_bWalkRight);
        }

        protected override void OnTakeDamage()
        {
            if (!m_bIsInShell)
            {
                m_bIsInShell = true;
                transform.Find("LeftHead").gameObject.SetActive(false);
                transform.Find("RightHead").gameObject.SetActive(false);
                transform.Find("RightFoot").gameObject.SetActive(false);
                transform.Find("LeftFoot").gameObject.SetActive(false);
            }
            else
            {
                m_bIsBouncing = true;
                // TODO: make the turtle fly!
            }
        }
    }
}