using UnityEngine;
using NaivePhysics;

namespace NaivePhysics
{
    [ExecuteInEditMode]
    public class DoubleSpringConstraint : NaiveEngine.Constraint
    {
        [Header("Connected Bodies")]
        public NaiveBody m_bodyA;
        public NaiveBody m_bodyB;

        [Header("Spring Properties")]
        public float m_fStiffness = 50.0f;
        public Color m_color = Color.yellow;
        private float m_fRestLength;

        private void OnEnable()
        {
            if (m_bodyA != null && m_bodyB != null)
                m_fRestLength = Vector2.Distance(m_bodyA.transform.position, m_bodyB.transform.position);
        }

        public override void TickConstraint()
        {
            if (m_bodyA == null || m_bodyB == null) return;

            Vector2 dir = (m_bodyB.transform.position - m_bodyA.transform.position);
            float currentLength = dir.magnitude;
            if (currentLength <= Mathf.Epsilon) return;

            float x = currentLength - m_fRestLength;
            Vector2 force = dir.normalized * m_fStiffness * x;

            m_bodyA.AddForce(force);
            m_bodyB.AddForce(-force);
        }

        public override void DrawConstraint()
        {
            if (m_bodyA == null || m_bodyB == null) return;

            Gizmos.color = m_color;
            Gizmos.DrawLine(m_bodyA.transform.position, m_bodyB.transform.position);
        }
    }
}
