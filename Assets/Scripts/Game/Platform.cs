using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaivePhysics;

namespace Game
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Platform : AlignedBox
    {
        private Mesh    m_mesh;

        private void OnEnable()
        {
            UpdateMesh();
        }

        protected override void OnMoved()
        {
            base.OnMoved();

            if (Application.isPlaying)
            {
                UpdateMesh();
            }
        }

        public void UpdateMesh()
        {
            if (m_mesh == null)
            {
                m_mesh = new Mesh();
                m_mesh.hideFlags = HideFlags.DontSave;
                m_mesh.name = "PlatformMesh";
            }

            Vector3[] corners = System.Array.ConvertAll(m_corners, c => transform.InverseTransformPoint(c));
            m_mesh.Clear();
            m_mesh.vertices = new Vector3[]{
                new Vector3(corners[0].x, corners[0].y, -0.5f), new Vector3(corners[0].x, corners[0].y, 0.5f), new Vector3(corners[1].x, corners[1].y, -0.5f), new Vector3(corners[1].x, corners[1].y, 0.5f),
                new Vector3(corners[2].x, corners[2].y, -0.5f), new Vector3(corners[2].x, corners[2].y, 0.5f), new Vector3(corners[3].x, corners[3].y, -0.5f), new Vector3(corners[3].x, corners[3].y, 0.5f),
                new Vector3(corners[0].x, corners[0].y, -0.5f), new Vector3(corners[0].x, corners[0].y, 0.5f), new Vector3(corners[3].x, corners[3].y, -0.5f), new Vector3(corners[3].x, corners[3].y, 0.5f),
                new Vector3(corners[1].x, corners[1].y, -0.5f), new Vector3(corners[1].x, corners[1].y, 0.5f), new Vector3(corners[2].x, corners[2].y, -0.5f), new Vector3(corners[2].x, corners[2].y, 0.5f),
                new Vector3(corners[0].x, corners[0].y, -0.5f), new Vector3(corners[1].x, corners[1].y, -0.5f), new Vector3(corners[2].x, corners[2].y, -0.5f), new Vector3(corners[3].x, corners[3].y, -0.5f),
            };

            Vector2Int vUV = new Vector2Int(Mathf.Max(Mathf.RoundToInt(m_vSize.x), 1),
                                            Mathf.Max(Mathf.RoundToInt(m_vSize.y), 1));

            m_mesh.uv = new Vector2[] {
                new Vector2(vUV.x, 0.0f), new Vector2(vUV.x, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f),
                new Vector2(vUV.x, 0.0f), new Vector2(vUV.x, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f),
                new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, vUV.y), new Vector2(1.0f, vUV.y),
                new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, vUV.y), new Vector2(1.0f, vUV.y),
                new Vector2(0.0f, 0.0f), new Vector2(vUV.x, 0.0f), new Vector2(vUV.x, vUV.y), new Vector2(0.0f, vUV.y),
            };

            m_mesh.triangles = new int[] { 
                0, 3, 1, 0, 2, 3,
                4, 7, 5, 4, 6, 7,
                8, 9, 11, 8, 11, 10,
                12, 15, 13, 12, 14, 15,
                16, 19, 18, 16, 18, 17
            };

            m_mesh.RecalculateBounds();
            m_mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = m_mesh;
        }
    }
}