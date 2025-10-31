using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaivePhysics
{
    public class AlignedBox : NaiveEngine.Shape
    {
        [SerializeField]
        public Vector2      m_vSize = new Vector2(0.5f, 0.75f);

        protected Vector2[] m_corners;

        #region Properties

        public IEnumerable<(Vector2, Vector2)> Lines
        {
            get
            {
                if (m_corners != null)
                {
                    for (int i = 0; i < m_corners.Length; i++)
                    {
                        yield return (m_corners[i], m_corners[(i + 1) % m_corners.Length]);
                    }
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
        }

        protected override void OnMoved()
        {
            base.OnMoved();

            m_corners = new Vector2[]
            {
                new Vector2(m_bounds.xMin, m_bounds.yMin),
                new Vector2(m_bounds.xMax, m_bounds.yMin),
                new Vector2(m_bounds.xMax, m_bounds.yMax),
                new Vector2(m_bounds.xMin, m_bounds.yMax),
            };
        }

        protected override Rect CalculateBounds()
        {
            return new Rect(transform.position.x - m_vSize.x * 0.5f,
                            transform.position.y - m_vSize.y * 0.5f,
                            m_vSize.x,
                            m_vSize.y);
        }
    }
}