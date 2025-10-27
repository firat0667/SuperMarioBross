using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaivePhysics
{
    public static class NaiveMath
    {
        public static Vector2 ClosestPointOnSegment(Vector2 P, Vector2 A, Vector2 B)
        {
            Vector2 v = B - A;
            Vector2 w = P - A;

            float c1 = Vector2.Dot(w, v);
            if (c1 <= 0)
            {
                return A;
            }

            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1)
            {
                return B;
            }

            float b = c1 / c2;
            return A + b * v;
        }

        public static float DistanceToSegment(Vector2 P, Vector2 A, Vector2 B)
        {
            return Vector2.Distance(P, ClosestPointOnSegment(P, A, B));
        }

        public static bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            const float fMargin = 0.001f;

            float s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
            float t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

            if ((s < 0) != (t < 0))
            {
                return false;
            }

            float A = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;
            if (A < 0.0)
            {
                s = -s;
                t = -t;
                A = -A;
            }

            return s > -fMargin && t > -fMargin && (s + t) < (A + fMargin);
        }
    }
}