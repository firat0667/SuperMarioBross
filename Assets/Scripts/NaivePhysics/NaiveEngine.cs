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
        [ExecuteInEditMode]
        public abstract class Shape : MonoBehaviour
        {
            protected Rect m_bounds;

            #region Properties

            public Vector2 Position => transform.position;

            public Rect Bounds => m_bounds;

            #endregion

            protected virtual void OnValidate()
            {
                OnMoved();
            }

            protected virtual void Update()
            {
                if (transform.hasChanged)
                {
                    OnMoved();
                    transform.hasChanged = false;
                }
            }

            protected abstract Rect CalculateBounds();

            public abstract void DrawShape();

            protected virtual void OnMoved()
            {
                m_bounds = CalculateBounds();
            }
        }

        private class ShapeEdge
        {
            public Shape m_shape;
            public bool m_bLeft;

            #region Properties

            public float X => m_bLeft ? m_shape.Bounds.xMin : m_shape.Bounds.xMax;

            #endregion
        }

        public abstract class Constraint : MonoBehaviour
        {
            #region Properties

            public Vector2 Position => transform.position;

            #endregion

            public abstract void DrawConstraint();

            public abstract void TickConstraint();
        }


        #region Properties

        public Shape[] Shapes => GetComponentsInChildren<Shape>();

        public NaiveBody[] Bodies => GetComponentsInChildren<NaiveBody>();

        public Constraint[] Constraints => GetComponentsInChildren<Constraint>();

        #endregion

        private void FixedUpdate()
        {
            // tick constraints
            foreach (Constraint constraint in Constraints)
            {
                constraint.TickConstraint();
            }

            // temp wind!
            /*
            {
                foreach (NaiveBody body in Bodies)
                {
                    body.AddForce(Vector2.right * 10.0f);
                }
            }*/

            // update all the bodies in the simulation
            foreach (NaiveBody body in Bodies)
            {
                body.TickBody();
            }

            // get all collisions
            List<Collision> collisions = GetCollisionPairs_SweepAndPrune(out _);

            // resolve collisions
            foreach (Collision collision in collisions)
            {
                NaiveBody A = collision.A.GetComponent<NaiveBody>();
                NaiveBody B = collision.B.GetComponent<NaiveBody>();
                if (A != null) A.ResolveCollision(collision, B != null ? B.m_fMass : 0.0f, B != null ? B.Velocity : Vector2.zero);
                if (B != null) B.ResolveCollision(collision, A != null ? A.m_fMass : 0.0f, A != null ? A.Velocity : Vector2.zero);
            }
        }

        private void OnDrawGizmos()
        {
            //List<Shape[]> collisions = GetCollisionPairs_VeryNaive(out _);
            List<Collision> collisions = GetCollisionPairs_SweepAndPrune(out _);

            // draw all the shapes
            foreach (Shape shape in Shapes)
            {
                // find a (if any) collision for the shape we are drawing
                List<Collision> shapeCollisions = collisions.FindAll(c => c.Contains(shape));

                // draw line between collision shapes
                foreach (Collision c in shapeCollisions)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(c.m_vPosition, 0.05f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(shape.transform.position, shape.transform.position + (Vector3)c.GetNormal(shape) * 0.5f);
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(c.m_vPosition - c.m_vNormalAB * c.m_fPenetration * 0.5f, c.m_vPosition + c.m_vNormalAB * c.m_fPenetration * 0.5f);
                }

                // draw shape bounds                
                //Rect bounds = shape.Bounds;
                //Gizmos.DrawWireCube(bounds.center, bounds.size);

                // draw actual shape
                Gizmos.color = shapeCollisions.Count > 0 ? Color.red : Color.green;
                shape.DrawShape();
            }

            // draw constraints
            foreach (Constraint constraint in Constraints)
            {
                constraint.DrawConstraint();
            }
        }

        public List<Collision> GetCollisionPairs_VeryNaive(out int iNumPerformedTests)
        {
            iNumPerformedTests = 0;
            Shape[] shapes = Shapes;
            List<Collision> result = new List<Collision>();
            for (int i = 0; i < shapes.Length; ++i)
            {
                for (int j = i + 1; j < shapes.Length; ++j)
                {
                    iNumPerformedTests++;
                    Collision collision = Overlaps(shapes[i], shapes[j]);
                    if (collision != null)
                    {
                        result.Add(collision);
                    }
                }
            }
            return result;
        }

        public List<Collision> GetCollisionPairs_SweepAndPrune(out int iNumPerformedTests)
        {
            iNumPerformedTests = 0;
            Shape[] shapes = Shapes;

            // create the edges
            List<ShapeEdge> edges = new List<ShapeEdge>(shapes.Length * 2);
            foreach (Shape shape in shapes)
            {
                edges.Add(new ShapeEdge { m_shape = shape, m_bLeft = true });
                edges.Add(new ShapeEdge { m_shape = shape, m_bLeft = false });
            }

            // sort the edges in X
            edges.Sort((ShapeEdge se1, ShapeEdge se2) => se1.X.CompareTo(se2.X));

            // start the sweep
            List<Collision> result = new List<Collision>();
            HashSet<Shape> touching = new HashSet<Shape>();
            foreach (ShapeEdge edge in edges)
            {
                if (edge.m_bLeft)
                {
                    foreach (Shape other in touching)
                    {
                        // perform expensive overlap test!
                        iNumPerformedTests++;
                        Collision collision = Overlaps(edge.m_shape, other);
                        if (collision != null)
                        {
                            result.Add(collision);
                        }
                    }

                    touching.Add(edge.m_shape);
                }
                else
                {
                    touching.Remove(edge.m_shape);
                }
            }

            return result;
        }

        public static Collision Overlaps(Shape A, Shape B)
        {
            // Draw out performed tests
            //Debug.DrawLine(A.transform.position, B.transform.position, Color.magenta);

            // first to the cheap AABB
            if (A.Bounds.xMax < B.Bounds.xMin ||        // X part is basically done by the Sweep n Prune algorithm
                A.Bounds.xMin > B.Bounds.xMax ||
                A.Bounds.yMax < B.Bounds.yMin ||
                A.Bounds.yMin > B.Bounds.yMax)
            {
                return null;
            }

            Sphere sphereA = A as Sphere;
            Sphere sphereB = B as Sphere;
            AlignedBox boxA = A as AlignedBox;
            AlignedBox boxB = B as AlignedBox;
            Triangle triangleA = A as Triangle;
            Triangle triangleB = B as Triangle;

            if (sphereA != null && sphereB != null) return Overlap_SphereSphere(sphereA, sphereB);
            if (boxA != null && boxB != null) return Overlap_BoxBox(boxA, boxB);
            if (sphereA != null && boxB != null) return Overlap_SphereBox(sphereA, boxB);
            if (boxA != null && sphereB != null) return Overlap_SphereBox(sphereB, boxA);
            
            //if (triangleA != null && triangleB != null) return Overlap_TriangleTriangle(triangleA, triangleB);
            //if (triangleA != null && sphereB != null)   return Overlap_TriangleSphere(triangleA, sphereB);
            //if (triangleB != null && sphereA != null)   return Overlap_TriangleSphere(triangleB, sphereA);
            //if (triangleA != null && boxB != null)      return Overlap_TriangleBox(triangleA, boxB);
            //if (triangleB != null && boxA != null)      return Overlap_TriangleBox(triangleB, boxA);
            

            //default: no overlap!
            return null;
        }

        private static Collision Overlap_SphereSphere(Sphere sphereA, Sphere sphereB)
        {
            Vector2 vA = sphereA.transform.position;
            Vector2 vB = sphereB.transform.position;

            float fDistance = Vector2.Distance(vA, vB);
            if (fDistance < sphereA.m_fRadius + sphereB.m_fRadius)
            {
                Vector2 vAtoB = (vB - vA).normalized;
                return new Collision
                {
                    A = sphereA,
                    B = sphereB,
                    m_vPosition = (vA + vAtoB * sphereA.m_fRadius + vB - vAtoB * sphereB.m_fRadius) * 0.5f,
                    m_vNormalAB = vAtoB,
                    m_fPenetration = (sphereA.m_fRadius + sphereB.m_fRadius) - fDistance
                };
            }

            // no collision :)
            return null;
        }

        private static Collision Overlap_BoxBox(AlignedBox boxA, AlignedBox boxB)
        {
            Vector2 vA = boxA.transform.position;
            Vector2 vB = boxB.transform.position;
            Vector2 vAtoB = vB - vA;

            Vector2 vExtentsA = boxA.m_vSize * 0.5f;
            Vector2 vExtentsB = boxB.m_vSize * 0.5f;

            float fOverlapX = vExtentsA.x + vExtentsB.x - Mathf.Abs(vAtoB.x);
            float fOverlapY = vExtentsA.y + vExtentsB.y - Mathf.Abs(vAtoB.y);

            Vector2 vCollisionNormal = fOverlapX < fOverlapY ? new Vector2(Mathf.Sign(vAtoB.x), 0.0f) :
                                                               new Vector2(0.0f, Mathf.Sign(vAtoB.y));

            return new Collision
            {
                A = boxA,
                B = boxB,
                m_vPosition = (vA + vB) * 0.5f,        // TODO: should calculate the center of the overlapped area!
                m_vNormalAB = vCollisionNormal,
                m_fPenetration = fOverlapX < fOverlapY ? fOverlapX : fOverlapY
            };
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

        private static Collision Overlap_SphereBox(Sphere sphereA, AlignedBox boxB)
        {
            Vector2 vA = sphereA.transform.position;
            Vector2 vB = boxB.transform.position;

            // check distance to the lines of the box
            float fBestDistance = float.MaxValue;
            Collision bestCollision = null;

            foreach ((Vector2 p1, Vector2 p2) line in boxB.Lines)
            {
                Vector2 vCP = NaiveMath.ClosestPointOnSegment(vA, line.p1, line.p2);
                float fDistanceToSegment = Vector2.Distance(vA, vCP);

                if (fDistanceToSegment < sphereA.m_fRadius &&
                    fDistanceToSegment < fBestDistance)
                {
                    // collision!
                    Vector2 vAtoB = (vCP - vA).normalized;
                    float fPenetration = (sphereA.m_fRadius - fDistanceToSegment);

                    bestCollision = new Collision
                    {
                        A = sphereA,
                        B = boxB,
                        m_vPosition = vA + vAtoB * (sphereA.m_fRadius - fPenetration * 0.5f),
                        m_vNormalAB = vAtoB,
                        m_fPenetration = fPenetration
                    };
                }
            }

            return bestCollision;

            /*
                Vector2 vPosA = sphereA.transform.position;
                Rect boundsB = boxB.Bounds;

                // is the sphere inside the box?        // TODO: Calculate collision for spere inside box case
                if (vPosA.x >= boundsB.xMin &&
                    vPosA.x <= boundsB.xMax &&
                    vPosA.y >= boundsB.yMin &&
                    vPosA.y <= boundsB.yMax)
                {
                    return true;
                }



                // nope... all good!
                return false;*/
        }

        /*
        private static bool Overlap_TriangleSphere(Triangle triangleA, Sphere sphereB)
        {
            // TODO: at home!
            return false;
        }

        private static bool Overlap_TriangleBox(Triangle triangleA, AlignedBox boxB)
        {
            // TODO: at home!
            return false;
        }*/
    }
}