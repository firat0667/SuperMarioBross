using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaivePhysics
{
    public class Sphere : NaiveEngine.Shape
    {
        [SerializeField, Range(0.1f, 25.0f)]
        public float m_fRadius = 0.5f;

        public override void DrawShape()
        {
            Gizmos.DrawWireSphere(transform.position, m_fRadius);
        }

        protected override Rect CalculateBounds()
        {
            return new Rect(transform.position.x - m_fRadius,
                            transform.position.y - m_fRadius,
                            m_fRadius * 2.0f,
                            m_fRadius * 2.0f);
        }
    }
}