using UnityEngine;

namespace Assets.Scripts.Collisions
{   /// <summary>
    /// A collection of helper function for collision calculation
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Calculates the closest points of two line segments.
        /// </summary>
        /// <param name="p1"> Startpoint of segment 1 </param>
        /// <param name="d1"> Direction/Length of segment 1 </param>
        /// <param name="p2"> Startpoint of segment 2 </param>
        /// <param name="d2"> Direction/Length of segment 2 </param>
        /// <param name="clamp"> should s and t be clamped to 0, 1 </param>
        /// <param name="s"> Position of nearest point in line 1 coordinates </param>
        /// <param name="t"> Position of nearest point in line 2 coordinates </param>
        /// <param name="c1"> Point on segmaint 1 </param>
        /// <param name="c2"> Point on segmaint 2 </param>
        /// <returns> The squared distance between c1 and c2 </returns>
        public static float ClosestPointTwoLines(Vector3 p1, Vector3 d1, Vector3 p2, Vector3 d2, bool clamp, out float s, out float t, out Vector3 c1, out Vector3 c2)
        {
            s = 0; t = 0; c1 = Vector3.zero; c2 = Vector3.zero;
            Vector3 r = p1 - p2;
            float a = Vector3.Dot(d1, d1);  // Quadratische Länge von 1
            float e = Vector3.Dot(d2, d2);  // Quadratische Länge von 2
            float f = Vector3.Dot(d2, r);

            if (a <= Vector3.kEpsilon && e <= Vector3.kEpsilon)  //Segmente sind Punkte
            {
                s = t = 0f; c1 = p1; c2 = p2; return Vector3.Dot(c1 - c2, c1 - c2);
            }
            if (a <= Vector3.kEpsilon) // 1 ist Punkt
            {
                s = 0f; t = (clamp) ? Mathf.Clamp01(f / e) : (f / e);
            }
            else
            {
                float c = Vector3.Dot(d1, r);
                if (e <= Vector3.kEpsilon)   // 2 ist Punkt
                {
                    t = 0f;
                    s = (clamp) ? Mathf.Clamp01(-c / a) : -c / a;
                }
                else
                {
                    float b = Vector3.Dot(d1, d2);
                    float denom = a * e - b * b;

                    if (denom >= Vector3.kEpsilon)
                        s = (clamp) ? Mathf.Clamp01((b * f - c * e) / denom) : (b * f - c * e) / denom;
                    else
                        s = 0f;
                    float tnom = (b * s + f);
                    if (tnom < 0)
                    {
                        t = 0f; s = (clamp) ? Mathf.Clamp01(-c / a) : -c / a;
                    }
                    else if (tnom > e)
                    {
                        t = 1f; s = (clamp) ? Mathf.Clamp01((b - c) / a) : (b - c) / a;
                    }
                    else
                    {
                        t = tnom / e;
                    }
                }
            }

            c1 = p1 + d1 * s;
            c2 = p2 + d2 * t;
            return Vector3.Dot(c1 - c2, c1 - c2);
        }

        /// <summary>
        /// Calculates all clipping points of a rectangle and a quadriteral (polygon with 4 vertices)
        /// </summary>
        /// <param name="rect"> the rectangle in form [-/+x, -/+y] </param>
        /// <param name="p"> polygon vertices </param>
        /// <param name="rPoints"> resulting points </param>
        /// <returns>number of resulting points, max is 8 </returns>
        public static int IntersectRectQuad2D(float[] rect, Vector2[] p, out Vector2[] rPoints)
        {
            rPoints = new Vector2[8];
            Vector2 start = p[p.Length - 1];
            Vector2 end;
            int pPointer = 0;
            int nr = 0, n;
            Vector2[] csPoints;
            while (pPointer < p.Length)
            {
                if (ComputeOutCode(start, rect) == 0)
                {
                    rPoints[nr] = start;
                    nr++;
                }

                end = p[pPointer];
                n = CohenSutherland(rect, start, end, out csPoints);
                for (var i = 0; i < n; i++)
                {
                    rPoints[nr] = csPoints[i];
                    nr++;
                }
                start = end;
                pPointer++;
            }
            return nr;
        }

        /// <summary>
        /// Computes if a Point is inside or outside of a rectangle
        /// </summary>
        /// <param name="p"> The Point</param>
        /// <param name="r"> THe Rechtangle in form: [-/+x, -/+y] </param>
        /// <returns></returns>
        public static int ComputeOutCode(Vector2 p, float[] r)
        {
            int code = 0;
            if (p.x < -r[0])
                code |= 1;  //Left
            else if (p.x > r[0])
                code |= 2;   //right
            if (p.y < -r[1])
                code |= 4;  //Bottom
            else if (p.y > r[1])
                code |= 8;  //Top
            return code;
        }

        /// <summary>
        /// Computes if a Point is inside or outside of a rectangle
        /// </summary>
        /// <param name="p"> The Point</param>
        /// <param name="r"> THe Rechtangle in form: [-/+x, -/+y] </param>
        /// <returns></returns>
        public static int ComputeOutCode(float x, float y, float[] r)
        {
            int code = 0;
            if (x < -r[0])
                code |= 1;  //Left
            else if (x > r[0])
                code |= 2;   //right
            if (y < -r[1])
                code |= 4;  //Bottom
            else if (y > r[1])
                code |= 8;  //Top
            return code;
        }

        /// <summary>
        ///  Basic Cohen Sutherland clipping algorithmus (clipping aof a line against a rect)
        /// </summary>
        /// <param name="rect"> the rectangle in form [-/+x, -/+y] </param>
        /// <param name="start"> start point of line </param>
        /// <param name="end"> end point of line </param>
        /// <param name="resultPoints"> maximal 2 clipping points </param>
        /// <returns> number of clipping points </returns>
        public static int CohenSutherland(float[] rect, Vector2 start, Vector2 end, out Vector2[] resultPoints)
        {
            resultPoints = new Vector2[2];
            int nr = 0;
            float x0 = start.x, x1 = end.x, y0 = start.y, y1 = end.y;
            int outCode0 = ComputeOutCode(start, rect);
            int outCode1 = ComputeOutCode(end, rect);

            while (true)
            {
                if ((outCode0 | outCode1) == 0) break;
                else if ((outCode0 & outCode1) != 0) break;
                else
                {
                    float x = 0, y = 0;
                    int outCodeOut = outCode0 > 0 ? outCode0 : outCode1;
                    if ((outCodeOut & 8) != 0) { x = x0 + (x1 - x0) * (rect[1] - y0) / (y1 - y0); y = rect[1]; }
                    else if ((outCodeOut & 4) != 0) { x = x0 + (x1 - x0) * (-rect[1] - y0) / (y1 - y0); y = -rect[1]; }
                    else if ((outCodeOut & 2) != 0) { y = y0 + (y1 - y0) * (rect[0] - x0) / (x1 - x0); x = rect[0]; }
                    else if ((outCodeOut & 1) != 0) { y = y0 + (y1 - y0) * (-rect[0] - x0) / (x1 - x0); x = -rect[0]; }

                    if (outCodeOut == outCode0) { x0 = x; y0 = y; outCode0 = ComputeOutCode(x0, y0, rect); }
                    else { x1 = x; y1 = y; outCode1 = ComputeOutCode(x1, y1, rect); }
                }
            }
            if (x0 != start.x || y0 != start.y)
            {
                resultPoints[nr] = new Vector2(x0, y0);
                nr++;
            }
            if (x1 != end.x || y1 != end.y)
            {
                resultPoints[nr] = new Vector2(x1, y1);
                nr++;
            }
            return nr;
        }
    }
}