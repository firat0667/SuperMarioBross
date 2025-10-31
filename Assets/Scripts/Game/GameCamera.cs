using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Camera))]
    public class GameCamera : MonoBehaviour
    {
        [SerializeField]
        private GameBody        m_target;

        [SerializeField]
        private Vector2         m_vCameraOffset;

        void Update()
        {
            // smooth follow target
            if (m_target != null)
            {
                Vector3 vCamTarget = m_target.transform.position + (m_target.IsGrounded ? (Vector3)m_vCameraOffset : Vector3.zero) - Vector3.forward * 15.0f;
                vCamTarget += (Vector3)m_target.Velocity * 0.25f;
                transform.position += (vCamTarget - transform.position) * Time.deltaTime * 4.0f;
            }
        }
    }
}