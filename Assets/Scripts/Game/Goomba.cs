using NaivePhysics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Goomba : GameBody
    {
        protected bool      m_bWalkRight = true;
        protected bool      m_bIsAlive = true;

        #region Properties

        protected virtual bool ShouldWalk => true;

        #endregion

        protected override void Start()
        {
            base.Start();

            StartCoroutine(MoveFeet());
        }

        public override void TickBody()
        {
            base.TickBody();

            // walking right or left?
            if (IsGrounded && ShouldWalk)
            {
                m_vVelocity.x = Mathf.MoveTowards(m_vVelocity.x, m_bWalkRight ? 1.0f : -1.0f, Time.fixedDeltaTime * 2.0f);
            }

            // self destruct?
            if (m_bIsAlive && transform.position.y < -50.0f)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void SwitchDirection()
        {
            m_bWalkRight = !m_bWalkRight;
        }

        protected virtual void OnTakeDamage()
        {
            m_bIsAlive = false;
        }
        
        public override void ResolveCollision(NaivePhysics.Collision collision, float fOtherMass, Vector2 vOtherVelocity)
        {
            if (!m_bIsAlive)
            {
                return;
            }

            // does goomba switch direction?
            Vector2 vNormal = collision.GetNormal(Shape);
            float fDot = Vector2.Dot(vNormal, Vector2.right);

            // is this a collision in X?
            if (Mathf.Abs(fDot) > 0.7f)
            {
                if ((fDot < 0.0f && m_bWalkRight) ||
                    (fDot > 0.0f && !m_bWalkRight))
                {
                    SwitchDirection();
                }
            }
            else if(Vector2.Dot(vNormal, Vector2.down) > 0.7f)
            {
                // death from above!
                OnTakeDamage();
            }

            base.ResolveCollision(collision, fOtherMass, vOtherVelocity);
        }

        IEnumerator MoveFeet()
        {
            Transform[] feet = new Transform[]
            {
                transform.Find("RightFoot"),
                transform.Find("LeftFoot")
            };

            // this is a short way of under ones
            Vector3[] localPositions = System.Array.ConvertAll(feet, f => f.localPosition);

            // they are equals 
            //Vector3[] localPositions= new Vector3[feet.Length];
            //for(int i=0; i<feet.Length; i++)
            //{
            //    localPositions[i]=feet[i].localPosition;
            //}


            float fTime = 0.0f;

            while (true)
            {
                // only animate if we are touching ground!
                if (IsGrounded)
                {
                    fTime += Time.deltaTime;
                }

                for (int i = 0; i < 2; ++i)
                {
                    float fFootTime = fTime * 8.0f + i * Mathf.PI * 0.5f;
                    float fX = Mathf.Sin(fFootTime) * 0.07f;
                    float fY = Mathf.Abs(Mathf.Cos(fFootTime) * 0.03f);
                    Vector3 vLocalPos = localPositions[i] + new Vector3(fX, fY, 0.0f);
                    feet[i].localPosition = vLocalPos;
                }

                yield return null;
            }
        }

    }
}