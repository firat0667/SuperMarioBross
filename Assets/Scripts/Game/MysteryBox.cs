using NaivePhysics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Animator))]
    public class MysteryBox : Platform
    {
        private bool    m_bHaveCoin = true;

        private void Start()
        {
            ShouldHandleCollisions = true;
        }

        protected override void OnNaiveCollisionEnter(NaiveEngine.Shape other, NaivePhysics.Collision collision)
        {
            base.OnNaiveCollisionEnter(other, collision);

            if (m_bHaveCoin)
            {
                Vector2 vCollisionNormal = collision.GetNormal(this);
                if (vCollisionNormal.y > 0.5f &&
                    other.GetComponent<Mario>() != null)
                {
                    GetComponent<Animator>().SetTrigger("Coin");
                    m_bHaveCoin = false;
                }
            }
        }
    }
}