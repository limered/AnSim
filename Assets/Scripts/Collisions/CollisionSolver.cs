using UnityEngine;

namespace Assets.Scripts.Collisions
{
    /// <summary>
    /// Contains all static methods for checking if cubes are intersecting or not.
    /// </summary>
    internal class CollisionSolver
    {
        
        #region Faces-Intersection
        /// <summary>
        /// Checks if a given interval intersects on a given axis. If a intersection is detected it finds
        /// the depth of intersection along the normal axis (r_min) and saves the axis normal for this intersection (n_min).
        /// If the normal should be flipped, it's saved in invert_normal.
        /// </summary>
        /// <param name="r"> Length of projection vector </param>
        /// <param name="ra"> Length of As projection on vector</param>
        /// <param name="rb"> Length of Bs projection on vector</param>
        /// <param name="normal"> the normal of separating axis</param>
        /// <param name="code"> indicates wich test was performed (for later evaluation) </param>
        /// <param name="coll"> Collision Info containing all important data </param>
        /// <returns> found collision or not </returns>
        private static bool _IntersectAxis(float r, float ra, float rb, Vector3 normal, int code, int face, ref CollisionSystem.CollisionInfo coll)
        {
            float r_check = Mathf.Abs(r) - (ra + rb);
            if (r_check > 0) return false; //no intersection
            if (r_check > coll.r_min)
            {
                coll.r_min = r_check;
                coll.n_min = normal;
                coll.invert_normal = r < 0;
                coll.code = code;
                coll.tested_axis_0 = face;
            }
            return true;
        }

        /// <summary>
        /// Tests all the face normals of both cubes for intersection on axes
        /// </summary>
        /// <param name="coll"> Collision Info containing all important data </param>
        /// <returns> True, if no separating axes were found. -> further testing required </returns>
        public static bool FaceNormalsIntersect(ref CollisionSystem.CollisionInfo coll)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_IntersectAxis(
                    coll.relativePosRot[i],
                    coll.A.extents[i],
                    coll.B.extents[0] * coll.AbsR[i, 0] + coll.B.extents[1] * coll.AbsR[i, 1] + coll.B.extents[2] * coll.AbsR[i, 2],
                    coll.A.axis[i],
                    i + 1, i,
                    ref coll
                    ) == false)
                {
                    return false;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                if (_IntersectAxis(
                    coll.relativePosRot[0] * coll.R[0, i] + coll.relativePosRot[1] * coll.R[1, i] + coll.relativePosRot[2] * coll.R[2, i],
                    coll.A.extents[0] * coll.AbsR[0, i] + coll.A.extents[1] * coll.AbsR[1, i] + coll.A.extents[2] * coll.AbsR[2, i],
                    coll.B.extents[i],
                    coll.B.axis[i],
                    4 + i, i,
                    ref coll
                    ) == false)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion Faces-Intersection

        #region Edges-Intersection
        /// <summary>
        /// Checks if a given interval intersects on a given axis. If a intersection is detected it finds
        /// the depth of intersection along the normal axis (r_min) and saves the axis normal for this intersection (n_min).
        /// If the normal should be flipped, it's saved in invert_normal.
        /// </summary>
        /// <param name="r"> Length of projection vector </param>
        /// <param name="ra"> Length of As projection on vector</param>
        /// <param name="rb"> Length of Bs projection on vector</param>
        /// <param name="n0"> Tested normal x </param>
        /// <param name="n1"> Tested normal y </param>
        /// <param name="n2"> Tested normal z </param>
        /// <param name="code"> indicates wich test was performed (for later evaluation) </param>
        /// <param name="face0"> Tested direction of A </param>
        /// <param name="face1">Tested direction of B</param>
        /// <param name="coll"> Collision Info containing all important data </param>
        /// <returns> True, if no separating axis exists (found collision) </returns>
        private static bool _IntersectAxis(float r, float ra, float rb, float n0, float n1, float n2, int code, int face0, int face1, ref CollisionSystem.CollisionInfo coll)
        {
            float r_check = Mathf.Abs(r) - (ra + rb);
            if (r_check > 0) return false;
            float l = Mathf.Sqrt(n0 * n0 + n1 * n1 + n2 + n2);
            if (l > 0)
            {
                //r_check = r_check;
                if (r_check > coll.r_min)
                {
                    coll.r_min = r_check;
                    coll.n_min = Vector3.zero;
                    coll.normal_c.Set(n0, n1, n2);
                    Vector3.Normalize(coll.normal_c);
                    coll.invert_normal = (r < 0);
                    coll.code = code;
                    coll.tested_axis_0 = face0;
                    coll.tested_axis_1 = face1;
                }
            }
            return true;
        }

        /// <summary>
        /// Enumerates all combinations of edges, that could intersect between two OBBs
        /// </summary>
        /// <param name="coll"> Collision Info containing all important data </param>
        /// <returns> True, if OBBs intersect </returns>
        public static bool EdgesIntersect(ref CollisionSystem.CollisionInfo coll)
        {
            // A[0] x B[0]
            if (_IntersectAxis(
                coll.relativePosRot[2] * coll.R[1, 0] - coll.relativePosRot[1] * coll.R[2, 0],
                coll.A.extents[1] * coll.AbsR[2, 0] + coll.A.extents[2] * coll.AbsR[1, 0],
                coll.B.extents[1] * coll.AbsR[0, 2] + coll.B.extents[2] * coll.AbsR[0, 1],
                0, -coll.R[2, 0], coll.R[1, 0], 7, 0, 0,
                ref coll
                ) == false)
                return false;
            // A[0] x B[1]
            if (_IntersectAxis(
                coll.relativePosRot[2] * coll.R[1, 1] - coll.relativePosRot[1] * coll.R[2, 1],
                coll.A.extents[1] * coll.AbsR[2, 1] + coll.A.extents[2] * coll.AbsR[1, 1],
                coll.B.extents[0] * coll.AbsR[0, 2] + coll.B.extents[2] * coll.AbsR[0, 0],
                0, -coll.R[2, 1], coll.R[1, 1], 8, 0, 1,
                ref coll
                ) == false)
                return false;
            // A[0] x B[2]
            if (_IntersectAxis(
                coll.relativePosRot[2] * coll.R[1, 2] - coll.relativePosRot[1] * coll.R[2, 2],
                coll.A.extents[1] * coll.AbsR[2, 2] + coll.A.extents[2] * coll.AbsR[1, 2],
                coll.B.extents[0] * coll.AbsR[0, 1] + coll.B.extents[1] * coll.AbsR[0, 0],
                0, -coll.R[2, 2], coll.R[1, 2], 9, 0, 2,
                ref coll
                ) == false)
                return false;

            // A[1] x B[0]
            if (_IntersectAxis(
                coll.relativePosRot[0] * coll.R[2, 0] - coll.relativePosRot[2] * coll.R[0, 0],
                coll.A.extents[0] * coll.AbsR[2, 0] + coll.A.extents[2] * coll.AbsR[0, 0],
                coll.B.extents[1] * coll.AbsR[1, 2] + coll.B.extents[2] * coll.AbsR[1, 1],
                coll.R[2, 0], 0, -coll.R[0, 0], 10, 1, 0,
                ref coll
                ) == false)
                return false;
            // A[1] x B[1]
            if (_IntersectAxis(
                coll.relativePosRot[0] * coll.R[2, 1] - coll.relativePosRot[2] * coll.R[0, 1],
                coll.A.extents[0] * coll.AbsR[2, 1] + coll.A.extents[2] * coll.AbsR[0, 1],
                coll.B.extents[0] * coll.AbsR[1, 2] + coll.B.extents[2] * coll.AbsR[1, 0],
                coll.R[2, 1], 0, -coll.R[0, 1], 11, 1, 1,
                ref coll
                ) == false)
                return false;
            // A[1] x B[2]
            if (_IntersectAxis(
                coll.relativePosRot[0] * coll.R[2, 2] - coll.relativePosRot[2] * coll.R[0, 2],
                coll.A.extents[0] * coll.AbsR[2, 2] + coll.A.extents[2] * coll.AbsR[0, 2],
                coll.B.extents[0] * coll.AbsR[1, 1] + coll.B.extents[1] * coll.AbsR[1, 0],
                coll.R[2, 2], 0, -coll.R[0, 2], 12, 1, 2,
                ref coll
                ) == false)
                return false;

            // A[2] x B[0]
            if (_IntersectAxis(
                coll.relativePosRot[1] * coll.R[0, 0] - coll.relativePosRot[0] * coll.R[1, 0],
                coll.A.extents[0] * coll.AbsR[1, 0] + coll.A.extents[1] * coll.AbsR[0, 0],
                coll.B.extents[1] * coll.AbsR[2, 2] + coll.B.extents[2] * coll.AbsR[2, 1],
                -coll.R[1, 0], coll.R[0, 0], 0, 13, 2, 0,
                ref coll
                ) == false)
                return false;
            // A[2] x B[1]
            if (_IntersectAxis(
                coll.relativePosRot[1] * coll.R[0, 1] - coll.relativePosRot[0] * coll.R[1, 1],
                coll.A.extents[0] * coll.AbsR[1, 1] + coll.A.extents[1] * coll.AbsR[0, 1],
                coll.B.extents[0] * coll.AbsR[2, 2] + coll.B.extents[2] * coll.AbsR[2, 0],
                -coll.R[1, 1], coll.R[0, 1], 0, 14, 2, 1,
                ref coll
                ) == false)
                return false;
            // A[2] x B[2]
            if (_IntersectAxis(
                coll.relativePosRot[1] * coll.R[0, 2] - coll.relativePosRot[0] * coll.R[1, 2],
                coll.A.extents[0] * coll.AbsR[1, 2] + coll.A.extents[1] * coll.AbsR[0, 2],
                coll.B.extents[0] * coll.AbsR[2, 1] + coll.B.extents[1] * coll.AbsR[2, 0],
                -coll.R[1, 2], coll.R[0, 2], 0, 15, 2, 2,
                ref coll
                ) == false)
                return false;

            return true;
        }
        #endregion Edges-Intersection

        #region Time-Intersection


        /// <summary>
        /// Checks, if a intersection could happen sometime in the future.
        /// </summary>
        /// <param name="coll"> Collision Info containing all important data </param>
        /// <returns> True, if intersection could happen </returns>
        public static bool IntervalIntersectTime(ref CollisionSystem.CollisionInfo coll)
        {
            var WD = Vector3.Cross(coll.relativeVel, coll.relativePos);
            float r = 0f;
            float[] alpha = new float[3];
            float[] alphaAbs = new float[3];
            float[] beta = new float[3];
            float[] betaAbs = new float[3];

            for (int i = 0; i < 3; i++)
            {
                alpha[i] = Vector3.Dot(coll.relativeVel, coll.A.axis[i]);
                alphaAbs[i] = Mathf.Abs(alpha[i]);
                beta[i] = Vector3.Dot(coll.relativeVel, coll.B.axis[i]);
                betaAbs[i] = Mathf.Abs(beta[i]);
            }

            // W x A0
            coll.ra = coll.A.extents[1] * alphaAbs[2] + coll.A.extents[2] * alphaAbs[1];
            coll.rb = 0;
            for (int i = 0; i < 3; i++)
            {
                coll.rb += coll.B.extents[i] * Mathf.Abs(coll.R[1, i] * alpha[2] - coll.R[2, i] * alpha[1]);
            }
            r = Vector3.Dot(coll.A.axis[0], WD);
            if (Mathf.Abs(r) > coll.ra + coll.rb) return false;
            // W x A1
            coll.ra = coll.A.extents[0] * alphaAbs[2] + coll.A.extents[2] * alphaAbs[0];
            coll.rb = 0;
            for (int i = 0; i < 3; i++)
            {
                coll.rb += coll.B.extents[i] * Mathf.Abs(coll.R[0, i] * alpha[2] - coll.R[2, i] * alpha[0]);
            }
            r = Vector3.Dot(coll.A.axis[1], WD);
            if (Mathf.Abs(r) > coll.ra + coll.rb) return false;
            // W x A2
            coll.ra = coll.A.extents[0] * alphaAbs[1] + coll.A.extents[1] * alphaAbs[0];
            coll.rb = 0;
            for (int i = 0; i < 3; i++)
            {
                coll.rb += coll.B.extents[i] * Mathf.Abs(coll.R[0, i] * alpha[1] - coll.R[1, i] * alpha[0]);
            }
            r = Vector3.Dot(coll.A.axis[2], WD);
            if (Mathf.Abs(r) > coll.ra + coll.rb) return false;

            // W x B0
            coll.rb = coll.B.extents[1] * betaAbs[2] + coll.B.extents[2] * betaAbs[1];
            coll.ra = 0;
            for (int i = 0; i < 3; i++)
            {
                coll.ra += coll.A.extents[i] * Mathf.Abs(coll.R[1, i] * beta[2] - coll.R[2, i] * beta[1]);
            }
            r = Vector3.Dot(coll.B.axis[0], WD);
            if (Mathf.Abs(r) > coll.ra + coll.rb) return false;
            // W x B1
            coll.rb = coll.B.extents[0] * betaAbs[2] + coll.B.extents[2] * betaAbs[0];
            coll.ra = 0;
            for (int i = 0; i < 3; i++)
            {
                coll.ra += coll.A.extents[i] * Mathf.Abs(coll.R[0, i] * beta[2] - coll.R[2, i] * beta[0]);
            }
            r = Vector3.Dot(coll.B.axis[1], WD);
            if (Mathf.Abs(r) > coll.ra + coll.rb) return false;
            // W x B2
            coll.rb = coll.B.extents[0] * betaAbs[1] + coll.B.extents[1] * betaAbs[0];
            coll.ra = 0;
            for (int i = 0; i < 3; i++)
            {
                coll.ra += coll.A.extents[i] * Mathf.Abs(coll.R[0, i] * beta[1] - coll.R[1, i] * beta[0]);
            }
            r = Vector3.Dot(coll.B.axis[2], WD);
            if (Mathf.Abs(r) > coll.ra + coll.rb) return false;

            return true;
        }
        #endregion Time-Intersection
    }
}