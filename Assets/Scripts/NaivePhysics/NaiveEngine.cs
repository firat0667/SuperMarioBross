using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NaivePhysics
{
    [ExecuteInEditMode]
    public class NaiveEngine : MonoBehaviour
    { 
        // Base class for all shapes (box, sphere, triangle)
        [ExecuteInEditMode]
        public abstract class Shape : MonoBehaviour
        {
            // Stores the shape’s bounding box
            protected Rect m_bounds;

            #region Properties

            public Rect Bounds => m_bounds;

            #endregion
            // Automatically updates when object changes in Inspector for edit mode 
            protected virtual void OnValidate()
            {
                OnMoved();
            }

            // Checks every frame if the transform changed
            protected virtual void Update()
            {
                if (transform.hasChanged)
                {
                    OnMoved();
                    transform.hasChanged = false;
                }
            }

            // Must be implemented by child shapes to compute their bounds
            protected abstract Rect CalculateBounds();

            // Each shape must draw itself using Gizmos
            public abstract void DrawShape();

            // Recalculates the shape’s bounds when moved
            protected virtual void OnMoved()
            {
                m_bounds = CalculateBounds();
            }
        }

        #region Properties

        // Returns all Shape components under this engine
        public Shape[] Shapes => GetComponentsInChildren<Shape>();

        #endregion

        // Draws all shapes and highlights collisions
        private void OnDrawGizmos()
        {
            List<Shape[]> collisions = GetCollisionPairs_VeryNaive(out _);

            // draw all the shapes
            foreach (Shape shape in Shapes)
            {
                // find a (if any) collision for the shape we are drawing
                Shape[] collision = collisions.Find(c => c[0] == shape || c[1] == shape);

                // draw line between collision shapes
                Gizmos.color = Color.red;
                if (collision != null)
                {
                    Gizmos.DrawLine(collision[0].transform.position, collision[1].transform.position);
                }

                // draw shape bounds
                Rect bounds = shape.Bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);

                // draw actual shape
                Gizmos.color = collision != null ? Color.red : Color.green;
                shape.DrawShape();
            }
        }
        //  Checks every shape pair for collision (very slow but simple)
        public List<Shape[]> GetCollisionPairs_VeryNaive(out int iNumPerformedTests)
        {
            iNumPerformedTests = 0;
            Shape[] shapes = Shapes;
            List<Shape[]> result = new List<Shape[]>();
            for (int i = 0; i < shapes.Length; ++i)
            {
                for (int j = i + 1; j < shapes.Length; ++j)
                {
                    iNumPerformedTests++;
                    if (Overlaps(shapes[i], shapes[j]))
                    {
                        result.Add(new Shape[] { shapes[i], shapes[j] });
                    }
                }
            }
            return result;
        }
        // Main collision function — decides which check to use
        public static bool Overlaps(Shape A, Shape B)
        {
            if (A.Bounds.xMax < B.Bounds.xMin ||
                A.Bounds.xMin > B.Bounds.xMax ||
                A.Bounds.yMax < B.Bounds.yMin ||
                A.Bounds.yMin > B.Bounds.yMax)
            {
                return false;
            }

            Sphere sphereA = A as Sphere;
            Sphere sphereB = B as Sphere;
            AlignedBox boxA = A as AlignedBox;
            AlignedBox boxB = B as AlignedBox;
            Triangle triangleA = A as Triangle;
            Triangle triangleB = B as Triangle;

            if (sphereA != null && sphereB != null)     return Overlap_SphereSphere(sphereA, sphereB);
            if (boxA != null && boxB != null)           return Overlap_BoxBox(boxA, boxB);
            if (sphereA != null && boxB != null)        return Overlap_SphereBox(sphereA, boxB);
            if (boxA != null && sphereB != null)        return Overlap_SphereBox(sphereB, boxA);
            if (triangleA != null && triangleB != null) return Overlap_TriangleTriangle(triangleA, triangleB);
            if (triangleA != null && sphereB != null)   return Overlap_TriangleSphere(triangleA, sphereB);
            if (triangleB != null && sphereA != null)   return Overlap_TriangleSphere(triangleB, sphereA);
            if (triangleA != null && boxB != null)      return Overlap_TriangleBox(triangleA, boxB);
            if (triangleB != null && boxA != null)      return Overlap_TriangleBox(triangleB, boxA);

            // default: no overlap!
            return false;
        }

        private static bool Overlap_SphereSphere(Sphere sphereA, Sphere sphereB)
        {
            float fDistance = Vector2.Distance(sphereA.transform.position, sphereB.transform.position);
            if (fDistance < sphereA.m_fRadius + sphereB.m_fRadius)
            {
                return true;
            }

            return false;
        }

        private static bool Overlap_BoxBox(AlignedBox boxA, AlignedBox boxB)
        {
            return true;
        }

        private static bool Overlap_TriangleTriangle(Triangle triangleA, Triangle triangleB)
        {
            // Triangle A => Triangle B
            foreach (Plane plane in triangleA.Planes)
            {
                // are all the points of the other triangle on the positive side?
                if (Array.FindIndex(triangleB.WorldCorners, wc => !plane.GetSide(wc)) < 0)
                {
                    return false;
                }
            }

            // Triangle B => Triangle A
            foreach (Plane plane in triangleB.Planes)
            {
                // are all the points of the other triangle on the positive side?
                if (Array.FindIndex(triangleA.WorldCorners, wc => !plane.GetSide(wc)) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool Overlap_SphereBox(Sphere sphereA, AlignedBox boxB)
        {
            Vector2 vPosA = sphereA.transform.position;
            Rect boundsB = boxB.Bounds;

            // is the sphere inside the box?
            if (vPosA.x >= boundsB.xMin &&
                vPosA.x <= boundsB.xMax &&
                vPosA.y >= boundsB.yMin &&
                vPosA.y <= boundsB.yMax)
            {
                return true;
            }

            // check distance to the lines of the box
            foreach ((Vector2 p1, Vector2 p2) line in boxB.Lines)
            {
                float fDistanceToSegment = NaiveMath.DistanceToSegment(sphereA.transform.position, line.p1, line.p2);
                if (fDistanceToSegment < sphereA.m_fRadius)
                {
                    return true;
                }
            }

            // nope... all good!
            return false;
        }

        private static bool Overlap_TriangleSphere(Triangle triangleA, Sphere sphereB)
        {
            Vector2 spherePos = sphereB.transform.position;

    
            if (NaiveMath.PointInTriangle(
                spherePos,
                triangleA.WorldCorners[0],
                triangleA.WorldCorners[1],
                triangleA.WorldCorners[2]))
                return true;

            foreach (var line in triangleA.Lines)
            {
                float dist = NaiveMath.DistanceToSegment(spherePos, line.Item1, line.Item2);
                if (dist < sphereB.m_fRadius)
                    return true;
            }

            return false;
        }

        private static bool Overlap_TriangleBox(Triangle triangleA, AlignedBox boxB)
        {
           
            foreach (Vector2 corner in triangleA.WorldCorners)
            {
                if (corner.x >= boxB.Bounds.xMin && corner.x <= boxB.Bounds.xMax &&
                    corner.y >= boxB.Bounds.yMin && corner.y <= boxB.Bounds.yMax)
                    return true;
            }

            Vector2[] boxCorners = new Vector2[]
            {
        new Vector2(boxB.Bounds.xMin, boxB.Bounds.yMin),
        new Vector2(boxB.Bounds.xMax, boxB.Bounds.yMin),
        new Vector2(boxB.Bounds.xMax, boxB.Bounds.yMax),
        new Vector2(boxB.Bounds.xMin, boxB.Bounds.yMax)
            };
            foreach (Vector2 corner in boxCorners)
            {
                if (NaiveMath.PointInTriangle(
                    corner,
                    triangleA.WorldCorners[0],
                    triangleA.WorldCorners[1],
                    triangleA.WorldCorners[2]))
                    return true;
            }

            foreach (var tLine in triangleA.Lines)
            {
                foreach (var bLine in boxB.Lines)
                {
                    if (LinesIntersect(tLine.Item1, tLine.Item2, bLine.Item1, bLine.Item2))
                        return true;
                }
            }

            return false;
        }
        private static bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            float d = (a2.x - a1.x) * (b2.y - b1.y) - (a2.y - a1.y) * (b2.x - b1.x);
            if (Mathf.Approximately(d, 0f)) return false; // paralel

            float u = ((b1.x - a1.x) * (b2.y - b1.y) - (b1.y - a1.y) * (b2.x - b1.x)) / d;
            float v = ((b1.x - a1.x) * (a2.y - a1.y) - (b1.y - a1.y) * (a2.x - a1.x)) / d;

            return (u >= 0f && u <= 1f && v >= 0f && v <= 1f);
        }


    }
}