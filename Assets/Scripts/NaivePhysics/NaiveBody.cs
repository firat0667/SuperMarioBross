using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaivePhysics
{
    [RequireComponent(typeof(NaiveEngine.Shape))]
    public class NaiveBody : MonoBehaviour
    {
        [SerializeField]
        public float                    m_fMass = 1.0f;

        private NaiveEngine.Shape       m_shape;

        protected Vector2               m_vForces;
        protected Vector2               m_vVelocity;

        public const float              GRAVITY = 9.82f;
        public const float              DRAG = 0.3f;
        public const float              BOUNCE = 0.99f;
        public const float              POSITION_CORRECTION = 0.3f;
        public const float              MAX_VELOCITY = 100.0f;

        #region Properties

        public NaiveEngine.Shape Shape => m_shape;

        public Vector2 Velocity => m_vVelocity;

        #endregion

        protected virtual void Start()
        {
            m_shape = GetComponent<NaiveEngine.Shape>();
        }

        public virtual void AddForce(Vector2 vForce)
        {
            m_vForces += vForce;
        }

        public virtual void TickBody()
        {
            // calculate acceleration
            Vector2 vGravity = Vector2.down * GRAVITY;
            Vector2 vAcceleration = vGravity + m_vForces;
            m_vForces = Vector2.zero;

            // update velocity
            m_vVelocity += vAcceleration * Time.fixedDeltaTime;

            // add some drag
            m_vVelocity -= m_vVelocity * Time.fixedDeltaTime * DRAG;

            // TODO: Add Friction here!

            // cap at a max velocity
            if (m_vVelocity.magnitude > MAX_VELOCITY)
            {
                m_vVelocity = m_vVelocity.normalized * MAX_VELOCITY;
            }

            // update position
            transform.position += (Vector3)m_vVelocity * Time.fixedDeltaTime;
        }

        public virtual void ResolveCollision(Collision collision, float fOtherMass, Vector2 vOtherVelocity)
        {
            // calculate relative velocities
            Vector2 vRelativeVelocity = vOtherVelocity - m_vVelocity;
            Vector2 vCollisionNormal = collision.GetNormal(m_shape);
            float fVelocityAlongNormal = Vector2.Dot(vRelativeVelocity, vCollisionNormal);

            // are the objects already separating?
            if (fVelocityAlongNormal <= 0.0f)
            {
                return;
            }

            // inverse the masses
            float fMassInv = 1.0f / m_fMass;
            float fOtherMassInv = fOtherMass <= 0.0001f ? 0.0f : (1.0f / fOtherMass);
            float fSeparationForce = -(1.0f + BOUNCE) * fVelocityAlongNormal;

            // how much should this collision affect me (me being this NaiveBody)
            fSeparationForce /= (fMassInv + fOtherMassInv);

            // this is our collision response!
            m_vVelocity -= fSeparationForce * vCollisionNormal * fMassInv;

            // positional correction
            Vector2 vCorrection = (collision.m_fPenetration / (fMassInv + fOtherMass)) * POSITION_CORRECTION * vCollisionNormal;
            transform.position += (Vector3)vCorrection * fMassInv;
        }
    }
}
