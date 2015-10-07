using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    /// Gets Collision points of two OBB in wirld-coordinates
    /// </summary>
    internal class ContactGenerator
    {
        /// <summary>
        /// Computes relevant collision information for a later collision response
        /// </summary>
        /// <param name="coll"> Collision Info containing all important data </param>
        public static void ComputeCollisionInfo(ref CollisionInfo coll)
        {
            if (coll.n_min.sqrMagnitude > Vector3.kEpsilon)
            {
                coll.n = coll.n_min;
            }
            else
            {
                coll.n = coll.A.orientation * coll.normal_c;
            }
            if (coll.invert_normal)
            {
                coll.n *= -1;
            }
            //Debug.Log(coll.code);

            if (coll.code > 6)
            {
                _FindEdgePoints(ref coll);
            }
            else if (coll.code <= 6)
            {
                _FindFacePoints(8, ref coll);
            }
        }

        /// <summary>
        /// Finds the two points needed to calculate Edge-Edge interaction.
        /// </summary>
        /// <param name="depth">Penetration depth</param>
        /// <param name="coll"> Collision Info containing all important data </param>
        private static void _FindEdgePoints(ref CollisionInfo coll)
        {
            // Mittelpunkte der cubes
            Vector3 pa = coll.A.center;
            Vector3 pb = coll.B.center;
            float sign;

            //Berechne die globalen / lokale positionen von eckpunkten der Kanten
            for (var j = 0; j < 3; j++)
            {
                sign = (Vector3.Dot(coll.n, coll.A.axis[j]) > 0) ? 1f : -1f;
                pa += sign * coll.A.extents[j] * coll.A.axis[j];
            }
            for (var j = 0; j < 3; j++)
            {
                sign = (Vector3.Dot(coll.n, coll.B.axis[j]) > 0) ? -1f : 1f;
                pb += sign * coll.B.extents[j] * coll.B.axis[j];
            }

            // Berechne die Richtungen der edges (bekannt aus den seiten der cubes)
            Vector3 da = coll.A.axis[coll.tested_axis_0];
            Vector3 db = coll.B.axis[coll.tested_axis_1];

            // Berechne den jeweils anderen Punkt
            sign = (Vector3.Dot(pa - coll.A.center, da) > 0) ? -1f : 1f;
            da = sign * coll.A.extents[coll.tested_axis_0] * 2 * da;

            sign = (Vector3.Dot(pb - coll.B.center, db) > 0) ? -1f : 1f;
            db = sign * coll.B.extents[coll.tested_axis_1] * 2 * db;

            // Berechne den schnittpunkt der beiden geraden
            float alpha, beta;
            Vector3 point1, point2;
            Utils.ClosestPointTwoLines(pa, da, pb, db, true, out alpha, out beta, out point1, out point2);

            // Correct normal and depth to go from A to B
            Vector3 normal = (Vector3.Dot(coll.A.center - coll.B.center, coll.n) < 0) ? -coll.n : coll.n;
            float depth = (coll.r_min < 0) ? -coll.r_min : coll.r_min;

            Contact c1 = new Contact(coll.object_A, coll.object_B, Vector3.Lerp(point1, point2, 0.5f), normal, depth, coll.code);

            coll.AddContact(c1);
        }

        /// <summary>
        /// Finds all points necessery to generate a response for: Face-Face, Face-Vertex and face-Edge interaction.
        /// </summary>
        /// <param name="maxContacts"> Maximum number of points needed (not used atm) </param>
        /// <param name="coll"> Collision Info containing all important data </param>
        private static void _FindFacePoints(int maxContacts, ref CollisionInfo coll)
        {
            Vector3[] Aa, Ab;
            Vector3 Ca, Cb;
            float[] Ea, Eb;

            if (coll.code <= 3)
            {
                Aa = coll.A.axis;
                Ab = coll.B.axis;
                Ca = coll.A.center;
                Cb = coll.B.center;
                Ea = coll.A.extents;
                Eb = coll.B.extents;
            }
            else
            {
                Aa = coll.B.axis;
                Ab = coll.A.axis;
                Ca = coll.B.center;
                Cb = coll.A.center;
                Ea = coll.B.extents;
                Eb = coll.A.extents;
            }

            // nr = normal vector of reference face dotted with axes of incident box.
            // anr = absolute values of nr.
            Vector3 normal2, nr, anr = Vector3.zero;
            if (coll.code <= 3) normal2 = coll.n; else normal2 = -coll.n;

            //nr = Rb * normal2;//
            nr = new Vector3(Vector3.Dot(normal2, Ab[0]), Vector3.Dot(normal2, Ab[1]), Vector3.Dot(normal2, Ab[2]));
            anr[0] = Mathf.Abs(nr[0]);
            anr[1] = Mathf.Abs(nr[1]);
            anr[2] = Mathf.Abs(nr[2]);

            // find the largest compontent of anr: this corresponds to the normal
            // for the indident face. the other axis numbers of the indicent face
            // are stored in a1,a2.
            int lanr, a1, a2;
            if (anr[1] > anr[0])
            {
                if (anr[1] > anr[2]) { a1 = 0; lanr = 1; a2 = 2; }
                else { a1 = 0; a2 = 1; lanr = 2; }
            }
            else
            {
                if (anr[0] > anr[2]) { lanr = 0; a1 = 1; a2 = 2; }
                else { a1 = 0; a2 = 1; lanr = 2; }
            }

            // compute center point of incident face, in reference-face coordinates
            Vector3 center = Vector3.zero;
            if (nr[lanr] < 0)
                center = Cb - Ca + Eb[lanr] * Ab[lanr];
            else
                center = Cb - Ca - Eb[lanr] * Ab[lanr];

            // find the normal and non-normal axis numbers of the reference box
            int codeN, code1, code2;
            if (coll.code <= 3) codeN = coll.code - 1; else codeN = coll.code - 4;
            if (codeN == 0) { code1 = 1; code2 = 2; }
            else if (codeN == 1) { code1 = 0; code2 = 2; }
            else { code1 = 0; code2 = 1; }

            // find the four corners of the collision face, in reference-face coordinates
            Vector2[] quad = new Vector2[4];    // 2D coordinates of the face
            float c1, c2, m11, m12, m21, m22;
            c1 = Vector3.Dot(center, Aa[code1]);
            c2 = Vector3.Dot(center, Aa[code2]);

            m11 = Vector3.Dot(Aa[code1], Ab[a1]);
            m12 = Vector3.Dot(Aa[code1], Ab[a2]);
            m21 = Vector3.Dot(Aa[code2], Ab[a1]);
            m22 = Vector3.Dot(Aa[code2], Ab[a2]);
            {
                float k1 = m11 * Eb[a1];
                float k2 = m21 * Eb[a1];
                float k3 = m12 * Eb[a2];
                float k4 = m22 * Eb[a2];
                quad[0] = new Vector2(c1 - k1 - k3, c2 - k2 - k4);
                quad[1] = new Vector2(c1 - k1 + k3, c2 - k2 + k4);
                quad[2] = new Vector2(c1 + k1 + k3, c2 + k2 + k4);
                quad[3] = new Vector2(c1 + k1 - k3, c2 + k2 - k4);
            }

            // calculate the size of face
            float[] rect = new float[2];
            rect[0] = Ea[code1];
            rect[1] = Ea[code2];

            // intersect both faces
            Vector2[] ret;
            int n = Utils.IntersectRectQuad2D(rect, quad, out ret);
            if (n < 1) return;   //should not happen because of tested collision

            // Convert ret-points to face coords and compute contact position and depth
            Vector3[] points = new Vector3[8];
            float[] depths = new float[8];
            float det1 = 1f / (m11 * m22 - m12 * m21);
            m11 *= det1;
            m12 *= det1;
            m21 *= det1;
            m22 *= det1;
            int cnum = 0;
            for (int j = 0; j < n; j++)
            {
                float k1 = m22 * (ret[j].x - c1) - m12 * (ret[j].y - c2);
                float k2 = -m21 * (ret[j].x - c1) + m11 * (ret[j].y - c2);

                points[cnum] = center + k1 * Ab[a1] + k2 * Ab[a2];
                depths[cnum] = Ea[codeN] - Vector3.Dot(normal2, points[cnum]);
                if (depths[cnum] >= 0)
                {
                    ret[cnum] = ret[j];
                    cnum++;
                }
            }

            if (cnum < 1) return;

            // Correct normal to go from A to B
            Vector3 normal = (Vector3.Dot(coll.A.center - coll.B.center, coll.n) < 0) ? -coll.n : coll.n;

            for (int i = 0; i < cnum; i++)
            {
                depths[i] = (depths[i] < 0) ? -depths[i] : depths[i];
                coll.AddContact(new Contact(coll.object_A, coll.object_B, points[i] + Ca, normal, depths[i], coll.code));
            }
        }

        //public static void FindDynamicPoint(ref CollisionInfo coll)
        //{
        //    if (coll.code == 0) return;
        //    // Last intersection As face
        //    if (coll.code <= 3)
        //    {
        //        _GenerateDynamicFacePointA(ref coll);
        //    }
        //    else if (coll.code <= 6)
        //    {
        //        _GenerateDynamicFacePointB(ref coll);
        //    }
        //}

        //private static float _MaxVectorNumber(Vector3 v)
        //{
        //    return Math.Max(Math.Max(v.x, v.y), v.z);
        //}

        //private static float _MinVectorNumber(Vector3 v)
        //{
        //    return Math.Min(Math.Min(v.x, v.y), v.z);
        //}
    }
}