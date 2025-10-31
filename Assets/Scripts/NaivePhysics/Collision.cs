using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

namespace NaivePhysics
{
    public class Collision 
    {
        public NaiveEngine.Shape    A;
        public NaiveEngine.Shape    B;
        public Vector2              m_vPosition;
        public Vector2              m_vNormalAB;
        public float                m_fPenetration;

        #region Properties

        #endregion

        public bool Contains(NaiveEngine.Shape shape)
        {
            return A == shape || B == shape;
        }

        public Vector2 GetNormal(NaiveEngine.Shape shape)
        {
            return shape == A ? -m_vNormalAB : m_vNormalAB;
        }
    }
}