/* 
	================================================================================
	Copyright (c) 2011, Jose Esteve. http://www.joesfer.com
	All rights reserved. 

	Redistribution and use in source and binary forms, with or without modification, 
	are permitted provided that the following conditions are met: 

	* Redistributions of source code must retain the above copyright notice, this 
	  list of conditions and the following disclaimer. 
	
	* Redistributions in binary form must reproduce the above copyright notice, 
	  this list of conditions and the following disclaimer in the documentation 
	  and/or other materials provided with the distribution. 
	
	* Neither the name of the organization nor the names of its contributors may 
	  be used to endorse or promote products derived from this software without 
	  specific prior written permission. 

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR 
	ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON 
	ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
	================================================================================
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BlueNoise
{
    public class Vertex
    {
        public Vertex() { x = 0.0f; y = 0.0f; }
        public Vertex(float _x, float _y) { x = _x; y = _y; }

        public float x;
        public float y;

        public static float Distance(Vertex a, Vertex b)
        {
            return (float)Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
        }
        public static float Dot(Vertex a, Vertex b)
        {
            return a.x * b.x + a.y * b.y;
        }
        public static Vertex operator +(Vertex a, Vertex b)
        {
            return new Vertex(a.x + b.x, a.y + b.y);
        }
        public static Vertex operator -(Vertex a, Vertex b)
        {
            return new Vertex(a.x - b.x, a.y - b.y);
        }
        public static Vertex operator *(Vertex a, float f)
        {
            return new Vertex(a.x * f, a.y * f);
        }
        public static Vertex Normalize(Vertex v)
        {
            float f = v.Length();
            return new Vertex(v.x / f, v.y / f);
        }
        public float Length()
        {
            return (float)Math.Sqrt(x * x + y * y);
        }
    }

    class GeomUtils
    {
        static float Signed2DTriangleArea(Vertex A, Vertex B, Vertex C)
        {
            return (A.x - C.x) * (B.y - C.y) - (A.y - C.y) * (B.x - C.x);
        }

        public static bool SegmentIntersect(Vertex A, Vertex B, Vertex C, Vertex D, out Vertex p )
        {
            // using triangle areas (Real Time Collision Detection book)

            float a1 = Signed2DTriangleArea(A, B, D); // compute winding of ABD [ + or - ]
            float a2 = Signed2DTriangleArea(A, B, C); // to intersect, must have sign opposite of a1

            // if c and d are on different sides of AB, areas have different signs
            if (Math.Abs(a1) > 1e-3f && Math.Abs(a2) > 1e-3f && a1 * a2 < 0.0f)
            {
                // Compute signs of a and b with respect to segment cd
                float a3 = Signed2DTriangleArea(C, D, A); // Compute winding of cda [ + or - ]
                // since area is ant a1 - a2 = a3 - a4, or a4 = a3 + a2 - a1
                //float a4 = Signed2DTriangleArea( C, D, B );
                float a4 = a3 + a2 - a1;
                // Points a and b on different sides of cd if areas have different signs
                if (a3 * a4 < 0.0f)
                {
                    // Segments intersect. Find intersection poitn along L(t) = a + t * ( b - a )
                    float t = a3 / ( a3 - a4 );
                    p = A + ( B - A ) * t;
                    return true;
                }
            }
            // segments not intersecting (or collinear)
            p = new Vertex(-1, -1);
            return false;
        }

        public static bool SegmentIntersect(Vertex A, Vertex B, Vertex C, Vertex D)
        {
            Vertex p;
            return SegmentIntersect(A, B, C, D, out p);
        }

        public static bool PointInPolygon(Vertex p, List<Vertex> polygon)
        {
            if ( polygon.Count < 4 ) return false; // idx 0 and n are the same, so we need 2 extra points minimum

            // polygon is considered a closed shape made of edges linking vi and vi+1
            Debug.Assert(polygon[0] == polygon[polygon.Count - 1]);

            // trace an "infinite" line from the source point and count the number 
            // of intersections with the polygon edges. Even number = the point 
            // is outside, odd number if it's inside.

            // We will use a horizontal line, but to avoid intersections with the 
            // VERTICES on any segment, we'll sort the Y coordinates of every vertex
            // and find a gap

            Vertex rectEndPoint = null;

            List<float> y = new List<float>();
            for (int i = 0; i < polygon.Count - 1; i++)
            {
                y.Add(polygon[i].y);
            }
            y.Sort();
            if (p.y <= y[0] || p.y >= y[y.Count - 1]) return false;

            int index = y.BinarySearch(p.y);
            if ( index < 0 ) index = ~index; // if not found, index will be <0 with the closest greater number in bitwise complement
            
            // we've already ruled out the cases where p.y < all values and p.y > all values, so index must be within 1..n-1
            float approxY = (y[index-1] + y[index]) * 0.5f;
            rectEndPoint = new Vertex( p.x + 10000.0f, //make it a big number, but not as much as float.MaxValue to avoid numerical issues
                                       approxY);
        
            // count intersections
            int intersections = 0;
            for (int i = 0; i < polygon.Count-1; i++)
            {
                if (SegmentIntersect(polygon[i], polygon[i + 1], p, rectEndPoint)) intersections++;
            }

            return intersections > 0 && intersections % 2 !=  0;
        }
    }
}
