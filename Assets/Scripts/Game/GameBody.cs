using NaivePhysics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameBody : NaiveBody
    {
        private float m_fLastGroundedTime = -1.0f;

        #region Properties

        public bool IsGrounded => (Time.time - m_fLastGroundedTime) < 0.05f;

        #endregion

        protected override void Start()
        {
            base.Start();

            Shape.ShouldHandleCollisions = true;
        }

        public override void ResolveCollision(NaivePhysics.Collision collision, float fOtherMass, Vector2 vOtherVelocity)
        {
            // store the frame when we are grounded
            Vector2 vCollisionNormal = collision.GetNormal(Shape);
            if (vCollisionNormal.y > 0.5f)
            {
                m_fLastGroundedTime = Time.time;
            }

            // debug draw normal
            //Debug.DrawLine(transform.position, transform.position + (Vector3)vCollisionNormal * 2.0f, Color.magenta, 3.0f);

            base.ResolveCollision(collision, fOtherMass, vOtherVelocity);
        }
    }
}