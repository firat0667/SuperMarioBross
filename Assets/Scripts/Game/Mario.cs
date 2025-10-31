using NaivePhysics;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Animator))]
    public class Mario : GameBody
    {
        [SerializeField, Range(1.0f, 12.0f)]
        private float       m_fMoveSpeed = 6.0f;

        [SerializeField, Range(1.0f, 12.0f)]
        private float       m_fJumpSpeed = 10.0f;

        private Animator    m_animator;
        private bool        m_bFaceRight;
        private bool        m_bWantToJump;

        #region Properties

        #endregion

        protected override void Start()
        {
            base.Start();

            m_animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                m_bWantToJump = true;
            }
        }

        public override void TickBody()
        {
            base.TickBody();

            // move Mario left or right
            float fTargetMoveSpeed = 0.0f;
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                fTargetMoveSpeed = -m_fMoveSpeed;
                m_bFaceRight = false;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                fTargetMoveSpeed = m_fMoveSpeed;
                m_bFaceRight = true;
            }

            // update velocity
            m_vVelocity.x = Mathf.MoveTowards(m_vVelocity.x, fTargetMoveSpeed, Time.fixedDeltaTime * 6.0f);

            // should mario jump?
            if (m_bWantToJump && IsGrounded)
            {
                m_bWantToJump = false;
                m_vVelocity.y = m_fJumpSpeed;
            }

            // update mario's facing / rotation
            Quaternion qTarget = Quaternion.LookRotation(Vector3.forward * (m_bFaceRight ? -1.0f : 1.0f));
            transform.rotation = Quaternion.Slerp(transform.rotation, qTarget, Time.deltaTime * 4.0f);

            // control mario animations
            m_animator.SetFloat("Speed", Mathf.Clamp01(Mathf.Abs(m_vVelocity.x)));
            m_animator.SetBool("Jump", !IsGrounded);
        }

        public override void ResolveCollision(NaivePhysics.Collision collision, float fOtherMass, Vector2 vOtherVelocity)
        {
            base.ResolveCollision(collision, fOtherMass, vOtherVelocity);
        }
    }
}