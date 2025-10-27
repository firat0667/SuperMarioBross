using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaivePhysics
{
    public class Triangle : NaiveEngine.Shape
    {
        [SerializeField]
        public Vector2[] m_localCorners = new Vector2[]
        {
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(0, -1),
        };

        private Vector2[] m_worldCorners;

        #region Properties

        public Vector2[] WorldCorners => m_worldCorners;

        public IEnumerable<(Vector2, Vector2)> Lines
        {
            get
            {
                if (m_worldCorners != null)
                {
                    for (int i = 0; i < m_worldCorners.Length; i++)
                    {
                        yield return (m_worldCorners[i], m_worldCorners[(i + 1) % m_worldCorners.Length]);
                    }
                }
            }
        }

        public IEnumerable<Plane> Planes
        {
            get
            {
                for (int i = 0; i < m_worldCorners.Length; ++i)
                {
                    int iNext = (i + 1) % m_worldCorners.Length;
                    int iPrev = (i + 2) % m_worldCorners.Length;

                    Vector3 vEdge = m_worldCorners[iNext] - m_worldCorners[i];
                    Vector3 vNormal = Vector3.Cross(Vector3.forward, vEdge).normalized;

                    Vector2 vPrev = m_worldCorners[iPrev];
                    float fDotToPrev = Vector3.Dot(vNormal, vPrev - m_worldCorners[i]);

                    vNormal *= fDotToPrev > 0.0f ? -1.0f : 1.0f;

                    yield return new Plane(vNormal, m_worldCorners[i]);
                }
            }
        }

        #endregion

        public override void DrawShape()
        {
            

            foreach ((Vector2 p1, Vector2 p2) line in Lines)
            {
                Gizmos.DrawLine(line.p1, line.p2);
            }

            #if false
            // draw out planes
            List<Plane> planes = new List<Plane>(Planes);
            for (int i = 0; i < m_worldCorners.Length; ++i)
            {
                int iNext = (i + 1) % m_worldCorners.Length;
                Vector2 A = m_worldCorners[i];
                Vector2 B = m_worldCorners[iNext];
                Vector2 vCenter = (A + B) * 0.5f;
                float fSize = Vector2.Distance(A, B);
                Gizmos.DrawLine(vCenter, vCenter + (Vector2)planes[i].normal * fSize * 0.25f);
            }
            #endif
        }

        protected override void OnMoved()
        {
            m_worldCorners = System.Array.ConvertAll(m_localCorners, v => (Vector2)transform.TransformPoint(v));

            base.OnMoved();
        }

        protected override Rect CalculateBounds()
        {
            Vector2 vMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 vMax = new Vector2(-float.MaxValue, -float.MaxValue);
            foreach (Vector2 c in m_worldCorners)
            {
                vMin.x = Mathf.Min(vMin.x, c.x);
                vMin.y = Mathf.Min(vMin.y, c.y);
                vMax.x = Mathf.Max(vMax.x, c.x);
                vMax.y = Mathf.Max(vMax.y, c.y);
            }

            return new Rect(vMin, vMax - vMin);
        }
    }
}