using UnityEngine;
using NaivePhysics;

namespace NaivePhysics
{
    [ExecuteInEditMode]
    public class SpringConstraint : NaiveEngine.Constraint
    {
        [Header("Target Body")]
        public NaiveBody m_body; // Kuvvet uygulanacak obje

        [Header("Spring Properties")]
        public float m_fStiffness = 50.0f; // Yay sabiti (k)
        public float m_fDamping = 1.5f;    // Sönümleme katsayýsý
        public float m_fBreakForce = 200.0f; // Kopma limiti
        public Color m_color = Color.red;  // SceneView çizimi

        private float m_fRestLength; // Dinlenme uzunluðu (L0)
        private bool m_bBroken = false; // Yay koptu mu?

        private void OnEnable()
        {
            m_bBroken = false;

            if (m_body != null)
                m_fRestLength = Vector2.Distance(Position, m_body.transform.position);
        }

        public override void TickConstraint()
        {
            if (m_body == null || m_bBroken) return;

            // (1) Yön ve uzunluk
            Vector2 dir = (Vector2)m_body.transform.position - Position;
            float currentLength = dir.magnitude;
            if (currentLength <= Mathf.Epsilon) return;

            // (2) Uzama miktarý
            float x = currentLength - m_fRestLength;

            // (3) Hooke’s Law kuvveti: F = -k * x
            Vector2 springForce = -dir.normalized * m_fStiffness * x;

            // (4) Kopma kontrolü
            if (springForce.magnitude > m_fBreakForce)
            {
                m_bBroken = true;
                Debug.LogWarning($"{name} spring broke! Force: {springForce.magnitude:F2}");
                return;
            }

            // (5) Sönümleme (damping)
            Vector2 dampingForce = -m_body.Velocity * m_fDamping;

            // (6) Toplam kuvvet
            Vector2 totalForce = springForce + dampingForce;

            // (7) Uygula
            m_body.AddForce(totalForce);
        }

        public override void DrawConstraint()
        {
            if (m_body == null) return;

            Gizmos.color = m_bBroken ? Color.gray : m_color;
            Gizmos.DrawLine(Position, m_body.transform.position);
        }
    }
}
