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
using System.Drawing;

namespace BlueNoise
{	
    /// <summary>
    /// Delaunay Triangulation. 
    /// Implements Lawson algorithm: 
    /// http://www.henrikzimmer.com/VoronoiDelaunay.pdf			
    /// </summary>
    class Delaunay2D
    {
		
    	public static bool Delaunay2DTriangulate( List<Vertex> vertices, bool removeSurroundingTriangle, out List<int> outTriangles, ref Adjacency adjacencyInfo ) 
        {
			/* Implements Lawson algorithm: 
			http://www.henrikzimmer.com/VoronoiDelaunay.pdf
			
			Unlike the Watson algorithm the Lawson algorithm is not based finding circumcircles and deleting
			triangles to obtain an insertion polygon. Lawson's incremental insertion algorithm utilizes edge
			flippings to achieve the same result and thus avoids a possibly faulty, degenerate mesh which can
			occur using Watsons method.
			In the same way as before, if we are not given a start triangulation we use a super triangle
			enclosing all vertices in V . In every insertion step a vertex p belonging to V is inserted. 
			A simple retriangulation is made where the edges are inserted between p and the corner vertices of 
			the triangle containing p (fig.11). For all new triangles formed the circumcircles are checked, if they contain
			any neighbouring vertex the edge between them is flipped. This process is executed recursively
			until there are no more faulty triangles and no more flips are required (fig.12). The algorithm then
			moves on, inserting another vertex from V until they have all been triangulated into a mesh, then,
			like before the super triangle and its edge are removed.
			*/

			if ( vertices.Count < 3 ) 
            {
                outTriangles = new List<int>();
                return false;
			}

			Adjacency adjacency = adjacencyInfo;
			if ( adjacencyInfo == null ) {
				adjacency = new Adjacency();
			};
			
			int stIdx1, stIdx2, stIdx3;
			Delaunay2DCreateSuperTriangle(vertices, adjacency, out stIdx1, out stIdx2, out stIdx3 );

            if (!Delaunay2DInsertPoints(vertices, adjacency))
            {
                outTriangles = new List<int>();
                return false;
            }

            if (removeSurroundingTriangle)
            {
			    Delaunay2DRemoveSuperTriangle(adjacency, stIdx1, stIdx2, stIdx3, out outTriangles, vertices);
            }
            else
            {
                // move the 3 vertices from the supertriangle right to the end of
                // the array instead of the beginning, so that the N first vertices
                // match with the point insertion order
                foreach (Adjacency.Edge e in adjacency.edges)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        e.vertices[i] -= 3;
                        if (e.vertices[i] < 0) e.vertices[i] += adjacency.vertices.Count;
                    }
                }

                foreach (Adjacency.Triangle t in adjacency.triangles)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        t.vertices[i] -= 3;
                        if (t.vertices[i] < 0) t.vertices[i] += adjacency.vertices.Count;
                    }
                }

                adjacency.vertices.Add(adjacency.vertices[0]);
                adjacency.vertices.Add(adjacency.vertices[1]);
                adjacency.vertices.Add(adjacency.vertices[2]);
                adjacency.vertices.RemoveAt(0);
                adjacency.vertices.RemoveAt(0);
                adjacency.vertices.RemoveAt(0);

                // 
                outTriangles = new List<int>();
                for (int i = 0; i < adjacency.triangles.Count; i++)
                {
                    if (adjacency.triangles[i].valid)
                    {
                        outTriangles.Add(adjacency.triangles[i].vertices[0]);
                        outTriangles.Add(adjacency.triangles[i].vertices[1]);
                        outTriangles.Add(adjacency.triangles[i].vertices[2]);
                    }
                }

            }

            return true;
		}

        #region Private Methods     
        private static void Delaunay2DRemoveSuperTriangle(Adjacency adjacency, int stIdx1, int stIdx2, int stIdx3, out List<int> outTriangles, List<Vertex> vertices)
        {
            outTriangles = new List<int>();
            // remove triangles containing vertices from the supertriangle
            for (int i = 0; i < adjacency.triangles.Count; i++)
            {
                Adjacency.Triangle triangle = adjacency.triangles[i];
                if (!triangle.valid)
                {
                    continue;
                }
                if (triangle.Contains(stIdx1) || triangle.Contains(stIdx2) || triangle.Contains(stIdx3))
                {
                    adjacency.RemoveTriangle(i);
                }
                else
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Debug.Assert(triangle.vertices[j] >= 0 && triangle.vertices[j] < adjacency.vertices.Count);
                        outTriangles.Add(triangle.vertices[j]);
                    }
                }
            }
            adjacency.invalidTriangles.Clear();

            // remove first 3 vertices (belonging to the supertriangle) and remap all remaining tris

            for (int i = 0; i < outTriangles.Count; i++)
            {
                outTriangles[i] -= 3;
            }

            int numTris = adjacency.triangles.Count;
            for (int i = 0; i < numTris; i++)
            {
                if (!adjacency.triangles[i].valid)
                {
                    int remappedTriangle = numTris - 1;
                    if (remappedTriangle > i)
                    {
                        adjacency.triangles[i] = adjacency.triangles[remappedTriangle];
                        numTris--;

                        // remap edges pointing to this triangle
                        for (int e = 0; e < 3; e++)
                        {
                            int edgeIndex = Math.Abs(adjacency.triangles[i].edges[e]) - 1;
                            if (adjacency.edges[edgeIndex].triangles[0] == remappedTriangle)
                                adjacency.edges[edgeIndex].triangles[0] = i;
                            if (adjacency.edges[edgeIndex].triangles[1] == remappedTriangle)
                                adjacency.edges[edgeIndex].triangles[1] = i;
                        }
#if  false
                        for (int e = 0; e < adjacency.edges.Count; e++)
                        {
                            Adjacency.Edge edge = adjacency.edges[e];
                            for (int et = 0; et < 2; et++)
                            {
                                Debug.Assert(edge.triangles[et] != i);
                                if (edge.triangles[et] == remappedTriangle)
                                {
                                    edge.triangles[et] = i;
                                }
                            }
                        }
#endif

                        i--;
                    }
                }
                else
                {
                    Adjacency.Triangle t = adjacency.triangles[i];
                    t.vertices[0] -= 3;
                    t.vertices[1] -= 3;
                    t.vertices[2] -= 3;
                }
            }
            adjacency.triangles.RemoveRange(numTris, adjacency.triangles.Count - numTris);
            adjacency.vertices = vertices;
            for (int i = 0; i < adjacency.edges.Count; i++)
            {
                adjacency.edges[i].vertices[0] -= 3;
                adjacency.edges[i].vertices[1] -= 3;
            }
        }

        private static void Delaunay2DCreateSuperTriangle(List<Vertex> vertices, Adjacency adjacency,
                                            out int stIdx1, out int stIdx2, out int stIdx3)
        {
            // Calculate super triangle

            Bounds2D bounds = new Bounds2D();
            for (int i = 0; i < vertices.Count; i++)
            {
                bounds.AddPoint(vertices[i]);
            }

            Vertex center;
            float radius;
            bounds.BoundingCircunference(out center, out radius);

            Vertex st1 = new Vertex();
            Vertex st2 = new Vertex();
            Vertex st3 = new Vertex();

            const float DEG2RAD = (float)Math.PI / 180.0f;

            st1.x = center.x + 2.0f * radius * (float)Math.Cos(0.0f * DEG2RAD);
            st1.y = center.y + 2.0f * radius * (float)Math.Sin(0.0f * DEG2RAD);
            st2.x = center.x + 2.0f * radius * (float)Math.Cos(120.0f * DEG2RAD);
            st2.y = center.y + 2.0f * radius * (float)Math.Sin(120.0f * DEG2RAD);
            st3.x = center.x + 2.0f * radius * (float)Math.Cos(240.0f * DEG2RAD);
            st3.y = center.y + 2.0f * radius * (float)Math.Sin(240.0f * DEG2RAD);

            adjacency.vertices.Add(st1);
            adjacency.vertices.Add(st2);
            adjacency.vertices.Add(st3);

            stIdx1 = adjacency.vertices.Count - 3;
            stIdx2 = adjacency.vertices.Count - 2;
            stIdx3 = adjacency.vertices.Count - 1;

            adjacency.CreateTriangle(stIdx1, stIdx2, stIdx3);
        }

        private static bool Delaunay2DInsertPoints(List<Vertex> vertices, Adjacency adjacency)
        {
            // Insert points

            HashSet<int> toCheck = new HashSet<int>();

            for (int i = 0; i < vertices.Count; i++)
            {

                // Insert Vi
                Vertex Vi = vertices[i];

                bool skip = false;
                for (int j = 0; j < adjacency.vertices.Count; j++)
                {
                    if ((Vi - adjacency.vertices[j]).Length() <= COINCIDENT_POINTS_DISTANCE_EPSILON)
                    {
                        // the point has already been inserted. Skip it
                        skip = true;
                        break;
                    }
                }
                if (skip)
                {
                    continue;
                }

                if (OnDelaunayInsertPoint != null)
                    OnDelaunayInsertPoint(adjacency, Vi);

                int tri = adjacency.PointInTriangle(Vi);

                if (tri < 0)
                {
                    Debug.Assert(false);
                    return false;
                }

                // check whether the point lies exactly on one edge of the triangle
                int edgeIdx = adjacency.PointInTriangleEdge(Vi, tri);
                if (edgeIdx >= 0)
                {
                    // split the edge by Vi
                    int[] result;
                    adjacency.SplitEdge(edgeIdx, Vi, out result);
                    for (int j = 0; j < 4; j++)
                    {
                        if (result[j] >= 0)
                        {
                            toCheck.Add(result[j]);
                        }
                    }
                }
                else
                {
                    // split the triangle in 3
                    int[] result;
                    adjacency.SplitTriangle(tri, Vi, out result);
                    for (int j = 0; j < 3; j++)
                    {
                        toCheck.Add(result[j]);
                    }
                }

                while (toCheck.Count > 0)
                {
                    int t = toCheck.Last<int>();
                    toCheck.Remove(t);

                    Adjacency.Triangle triangle = adjacency.triangles[t];
                    if (!triangle.valid)
                    {
                        continue;
                    }

                    if (OnDelaunayTriangleCheck != null)
                        OnDelaunayTriangleCheck(adjacency, t);

                    // check Delaunay condition
                    for (int e = 0; e < 3; e++)
                    {
                        if (!adjacency.triangles[t].valid) continue;

                        int adjacentIdx = adjacency.AdjacentTriangle(t, e);
                        if (adjacentIdx < 0)
                        {
                            continue;
                        }
                        int globalEdgeIndex = Math.Abs(triangle.edges[e]) - 1;
                        Adjacency.Triangle adjacent = adjacency.triangles[adjacentIdx];
                        if (!adjacent.valid)
                        {
                            continue;
                        }
                        Debug.Assert(adjacent.valid);
                        int edgeFromAdjacent = adjacent.LocalEdgeIndex(globalEdgeIndex);
                        Debug.Assert(edgeFromAdjacent >= 0);
                        int v = adjacency.VertexOutOfTriEdge(adjacentIdx, edgeFromAdjacent);
                        Debug.Assert(v >= 0);
                        Debug.Assert(!triangle.Contains(v));

                        if (triangle.InsideCircumcircle(adjacency.vertices[v], adjacency.vertices) > INSIDE_CIRCUMCIRCLE_EPSILON)
                        {
                            int[] result;
                            if (adjacency.FlipTriangles(t, adjacentIdx, out result))
                            {
                                toCheck.Add(result[0]);
                                toCheck.Add(result[1]);
                                //break;
                            }
                        }
                    }
                }

                if (OnDelaunayStep != null)
                    OnDelaunayStep(adjacency);
            }
            return true;
        }		

        #endregion

        #region Public Data
        public delegate void DelaunayInsertPointDelegate(Adjacency currentState, Vertex point);
        public delegate void DelaunayTriangleCheckDelegate(Adjacency currentState, int triangle);
        public delegate void DelaunayStepDelegate(Adjacency currentState);

        public static DelaunayStepDelegate OnDelaunayStep = null;
        public static DelaunayInsertPointDelegate OnDelaunayInsertPoint = null;
        public static DelaunayTriangleCheckDelegate OnDelaunayTriangleCheck = null;
        #endregion

        #region Private data
        private static float INSIDE_CIRCUMCIRCLE_EPSILON = 1e-2f;
        private static float COINCIDENT_POINTS_DISTANCE_EPSILON = 1e-1f;
        #endregion
    }

    public class Bounds2D
    {
        public Bounds2D()
        {
            min = new Vertex(float.MaxValue, float.MaxValue);
            max = new Vertex(float.MinValue, float.MinValue);
        }
        public void AddPoint(Vertex p)
        {
            min.x = Math.Min(min.x, p.x);
            max.x = Math.Max(max.x, p.x);
            min.y = Math.Min(min.y, p.y);
            max.y = Math.Max(max.y, p.y);
        }
        public void BoundingCircunference(out Vertex center, out float radius)
        {
            center = (max + min) * 0.5f;
            radius = Vertex.Distance(center, min);
        }
        public Vertex min, max;
    }

    /// <summary>
    /// Geometric predicates used in the Delaunay Triangulation
    /// </summary>
    public class Predicates
    {
        public static float Orient(Vertex a,
                    Vertex b,
                    Vertex c)
        {
            Matrix3 m = new Matrix3(a.x, a.y, 1,
                                    b.x, b.y, 1,
                                    c.x, c.y, 1);
            return m.Determinant();
        }

        // returns > 0 if p is inside the circle described by A,B,C < 0 outside, = 0 on the circle			
        public static float InCircle(Vertex a,
                                Vertex b,
                                Vertex c,
                                Vertex p)
        {

            Matrix4 m = new Matrix4(a.x, a.y, a.x * a.x + a.y * a.y, 1,
                                    b.x, b.y, b.x * b.x + b.y * b.y, 1,
                                    c.x, c.y, c.x * c.x + c.y * c.y, 1,
                                    p.x, p.y, p.x * p.x + p.y * p.y, 1);
            float det = m.Determinant();
            if (Orient(a, b, c) > 0.0f)
            {
                return det;
            }
            else
            {
                return -det;
            }
        }

    }

    class Adjacency
    {

        #region classes

        public class Edge
        {
            public int[] vertices;
            public int[] triangles;

            public Edge()
            {
                vertices = new int[2];
                triangles = new int[2];
                vertices[0] = vertices[1] = -1;
                triangles[0] = triangles[1] = -1;
            }
            public bool Links(int v1, int v2)
            {
                Debug.Assert(v1 != v2);
                return (vertices[0] == v1 && vertices[1] == v2) || (vertices[0] == v2 && vertices[1] == v1);
            }
        }
        public class Triangle
        {
            public int[] vertices;
            public int[] edges; // signed, 1-based
            public bool valid;

            public Triangle()
            {
                vertices = new int[3];
                edges = new int[3];
                vertices[0] = vertices[1] = vertices[2] = -1;
                edges[0] = edges[1] = edges[2] = Int32.MaxValue;
                valid = false;
            }

            public bool Contains(int v)
            {
                return (vertices[0] == v || vertices[1] == v || vertices[2] == v);
            }

            public bool Contains(int a, int b, int c)
            {
                return Contains(a) && Contains(b) && Contains(c);
            }

            public int LocalEdgeIndex(int globalEdgeIndex)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (Math.Abs(edges[i]) - 1 == globalEdgeIndex)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public float InsideCircumcircle(Vertex p, List<Vertex> verts)
            {
                Vertex A = verts[vertices[0]];
                Vertex B = verts[vertices[1]];
                Vertex C = verts[vertices[2]];
                float det = Predicates.InCircle(A, B, C, p);
                return det;
            }

            public bool CircumCircle(List<Vertex> verts, out Vertex center, out float radius)
            {
                // Calculate the circle that passes through 3 coplanar points
                // http://mathworld.wolfram.com/Circle.html

                Vertex p0 = verts[vertices[0]];
                Vertex p1 = verts[vertices[1]];
                Vertex p2 = verts[vertices[2]];


                Matrix3 ma = new Matrix3(p0.x, p0.y, 1,
                                            p1.x, p1.y, 1,
                                            p2.x, p2.y, 1);
                float a = ma.Determinant();
                if (Math.Abs(a) < 1e-5f)
                {
                    center = new Vertex(0, 0);
                    radius = 0;
                    return false;
                }

                Matrix3 md = new Matrix3(p0.x * p0.x + p0.y * p0.y, p0.y, 1,
                                          p1.x * p1.x + p1.y * p1.y, p1.y, 1,
                                          p2.x * p2.x + p2.y * p2.y, p2.y, 1);
                float d = -md.Determinant();

                Matrix3 me = new Matrix3(p0.x * p0.x + p0.y * p0.y, p0.x, 1,
                                          p1.x * p1.x + p1.y * p1.y, p1.x, 1,
                                          p2.x * p2.x + p2.y * p2.y, p2.x, 1);
                float e = me.Determinant();

                Matrix3 mf = new Matrix3(p0.x * p0.x + p0.y * p0.y, p0.x, p0.y,
                                          p1.x * p1.x + p1.y * p1.y, p1.x, p1.y,
                                          p2.x * p2.x + p2.y * p2.y, p2.x, p2.y);
                float f = -mf.Determinant();

                center = new Vertex(-d / (2 * a), -e / (2 * a));
                radius = (float)Math.Sqrt((d * d + e * e) / (4 * a * a) - f / a);

                return true;
            }
        }
        #endregion

        public List<Edge> edges;
        public List<Triangle> triangles;
        public List<Vertex> vertices;

        List<List<int>> vertexTriangleAdjacency;
        public List<int> invalidTriangles;

        private static float POINT_ON_SEGMENT_DISTANCE_EPSILON = 1e-4f;
        private static float POINT_ON_SEGMENT_PARAMETRIC_EPSILON = 1e-5f;

        public Adjacency()
        {
            vertexTriangleAdjacency = new List<List<int>>();
            edges = new List<Edge>();
            triangles = new List<Triangle>();
            vertices = new List<Vertex>();
            invalidTriangles = new List<int>();
        }

        private bool CheckConsistency()
        {
            #region triangle consistency
            for (int t = 0; t < triangles.Count; t++)
            {
                Triangle triangle = triangles[t];
                if (!triangle.valid) continue;

                for (int i = 0; i < 3; i++)
                {
                    int edgeIndex = triangle.edges[i];
                    Edge edge = edges[Math.Abs(edgeIndex) - 1];
                    if (edgeIndex < 0)
                    {
                        if (edge.triangles[1] != t) return false;
                    }
                    else
                    {
                        if (edge.triangles[0] != t) return false;
                    }
                }

            }
            #endregion

            #region edge consistency
            for (int e = 0; e < edges.Count; e++)
            {
                Edge edge = edges[e];
                for (int t = 0; t < 2; t++)
                {
                    if (edge.triangles[t] >= 0 &&
                        !triangles[edge.triangles[t]].valid) return false;
                }
            }
            #endregion

            #region adjacents consistency
            for (int v = 0; v < vertexTriangleAdjacency.Count; v++)
            {
                List<int> tris = vertexTriangleAdjacency[v];
                for (int t = 0; t < tris.Count; t++)
                {
                    if (!triangles[tris[t]].valid ||
                        !triangles[tris[t]].Contains(v)) return false;
                }
            }
            #endregion

            return true;
        }

        public int CreateTriangle(int a, int b, int c)
        {
            Debug.Assert(a != b && a != c && b != c);

            int triangleIndex = triangles.Count;

            Triangle triangle = new Triangle();
            triangle.vertices[0] = a;
            triangle.vertices[1] = b;
            triangle.vertices[2] = c;
            triangles.Add(triangle);

            if (vertexTriangleAdjacency.Count != 0)
            {
                vertexTriangleAdjacency[a].Add(triangleIndex);
                vertexTriangleAdjacency[b].Add(triangleIndex);
                vertexTriangleAdjacency[c].Add(triangleIndex);
            }

            for (int i = 0; i < 3; i++)
            {
                int e = FindEdge(triangle.vertices[i], triangle.vertices[(i + 1) % 3]);
                if (e >= 0)
                {
                    Edge edge = edges[e];
                    int edgeTriIdx = edge.vertices[0] == triangle.vertices[i] ? 0 : 1;
                    Debug.Assert(edge.triangles[edgeTriIdx] < 0);
                    edge.triangles[edgeTriIdx] = triangleIndex;
                    triangle.edges[i] = edgeTriIdx == 0 ? e + 1 : -(e + 1);
                }
                else
                {
                    e = CreateEdge(triangle.vertices[i], triangle.vertices[(i + 1) % 3]);
                    Edge edge = edges[e];
                    edge.triangles[0] = triangleIndex;
                    triangle.edges[i] = e + 1;
                }
            }

            triangle.valid = true;
#if DEBUG
            Debug.Assert(CheckConsistency());
#endif
            return triangleIndex;
        }

        public void RemoveTriangle(int t)
        {
            Triangle triangle = triangles[t];
            Debug.Assert(triangle.valid);

            for (int i = 0; i < 3; i++)
            {
                int edgeIndex = triangle.edges[i];
                Edge edge = edges[Math.Abs(edgeIndex) - 1];
                if (edgeIndex < 0)
                {
                    Debug.Assert(edge.triangles[1] == t);
                    edge.triangles[1] = -1;
                }
                else
                {
                    Debug.Assert(edge.triangles[0] == t);
                    edge.triangles[0] = -1;
                }
            }

            for (int i = 0; i < vertexTriangleAdjacency.Count; i++)
            {
                List<int> adjacents = vertexTriangleAdjacency[i];
                adjacents.Remove(t);
                Debug.Assert(!adjacents.Contains(t));
            }

            triangle.valid = false;
            invalidTriangles.Add(t);
#if DEBUG
            Debug.Assert(CheckConsistency());
#endif
        }

        // returns the triangle index of the triangle containing p
        public int PointInTriangle(Vertex p)
        {
            for (int i = 0; i < triangles.Count; i++)
            {

                Triangle t = triangles[i];
                if (!t.valid)
                {
                    continue;
                }

                bool hit = false;

                // use barycentric coordinates to determine whether the point is inside the triangle

                // http://steve.hollasch.net/cgindex/math/barycentric.html

                Vertex v0 = vertices[t.vertices[0]];
                Vertex v1 = vertices[t.vertices[1]];
                Vertex v2 = vertices[t.vertices[2]];

                float b0 = (v1.x - v0.x) * (v2.y - v0.y) - (v2.x - v0.x) * (v1.y - v0.y);
                if (b0 != 0)
                {
                    float b1 = ((v1.x - p.x) * (v2.y - p.y) - (v2.x - p.x) * (v1.y - p.y)) / b0;
                    float b2 = ((v2.x - p.x) * (v0.y - p.y) - (v0.x - p.x) * (v2.y - p.y)) / b0;
                    float b3 = ((v0.x - p.x) * (v1.y - p.y) - (v1.x - p.x) * (v0.y - p.y)) / b0;

                    hit = b1 >= 0 && b2 >= 0 && b3 >= 0;
                }

                if (hit)
                {
                    return i;
                }
            }
            return -1;
        }

        bool PointOnSegment(Vertex A, Vertex B,
                             Vertex P,
                             float distanceEpsilon,
                             float parametricEpsilon,
                             out float pointSegmentDistance)
        {
            // Determines whether P lies within the segment A-B

            float segmentLength = Vertex.Distance(A, B);
            float tangentABdist = Vertex.Dot(Vertex.Normalize(B - A), Vertex.Normalize(P - A)) * (P - A).Length();
            float u = tangentABdist / segmentLength;
            if (u < parametricEpsilon || u > 1.0 - parametricEpsilon)
            {
                pointSegmentDistance = float.MaxValue;
                return false;
            }

            Vertex isect = A + (B - A) * u;
            float dist = (P - isect).Length();

            pointSegmentDistance = dist;

            if (dist > distanceEpsilon * segmentLength)
            {
                return false;
            }
            return true;
        }

        public int PointInTriangleEdge(Vertex p, int t)
        {

            Triangle triangle = triangles[t];
            Debug.Assert(triangle.valid);

            int closestEdge = -1;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                Debug.Assert(triangle.edges[i] != Int32.MaxValue);
                int edgeIdx = Math.Abs(triangle.edges[i]) - 1;
                Edge edge = edges[edgeIdx];
                float distance;
                if (PointOnSegment(vertices[edge.vertices[0]],
                                     vertices[edge.vertices[1]],
                                     p,
                                     POINT_ON_SEGMENT_DISTANCE_EPSILON,
                                     POINT_ON_SEGMENT_PARAMETRIC_EPSILON,
                                     out distance))
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEdge = edgeIdx;
                    }
                }
            }

            return closestEdge;
        }

        public int VertexOutOfTriEdge(int triIndex, int edgeIndex)
        {
            Triangle triangle = triangles[triIndex];
            Debug.Assert(triangle.valid);
            Debug.Assert(edgeIndex >= 0 && edgeIndex < 3);

            int globalEdgeIdx = Math.Abs(triangle.edges[edgeIndex]) - 1;
            Edge edge = edges[globalEdgeIdx];
            if (edge.Links(triangle.vertices[0], triangle.vertices[1]))
            {
                return triangle.vertices[2];
            }
            else if (edge.Links(triangle.vertices[0], triangle.vertices[2]))
            {
                return triangle.vertices[1];
            }
            else
            {
                Debug.Assert(edge.Links(triangle.vertices[1], triangle.vertices[2]));
                return triangle.vertices[0];
            }
        }

        bool IsWorthFlipping(int A, int B, int C, int D)
        {
            /*
             *   A----B
             *   |   /|
             *   |  / |
             *   | /  |
             *   |/___|
             *   C    D
             */
            float closestAngleBefore = Math.Min(ClosestAngleOnTri(A, B, C), ClosestAngleOnTri(B, D, C));
            float closestAngleAfter = Math.Min(ClosestAngleOnTri(A, B, D), ClosestAngleOnTri(A, D, C));


            return closestAngleBefore < closestAngleAfter;
        }

        float ClosestAngleOnTri(int vA, int vB, int vC)
        {
            int[] indices = new int[3];
            indices[0] = vA;
            indices[1] = vB;
            indices[2] = vC;

            double closestAngle = Math.PI;
            for (int i = 0; i < 3; i++)
            {
                Vertex nAB = Vertex.Normalize(vertices[indices[(i + 1) % 3]] - vertices[indices[i]]);
                Vertex nAC = Vertex.Normalize(vertices[indices[(i + 2) % 3]] - vertices[indices[i]]);
                float dot = Vertex.Dot(nAB, nAC);
                Debug.Assert(dot >= -1.01f && dot <= 1.01f);
                double angle = Math.Acos(dot);
                if (angle < closestAngle)
                {
                    closestAngle = angle;
                }
            }
            return (float)closestAngle;
        }


        public bool FlipTriangles(int tri1, int tri2, out int[] result)
        {

            Triangle triangle1 = triangles[tri1];
            Debug.Assert(triangle1.valid);
            Triangle triangle2 = triangles[tri2];
            Debug.Assert(triangle2.valid);

            int commonEdgeIdx = CommonEdge(triangle1, triangle2);
            Debug.Assert(commonEdgeIdx >= 0);
            Edge commonEdge = edges[commonEdgeIdx];
            Debug.Assert(commonEdge.triangles[0] == tri1 || commonEdge.triangles[0] == tri2);
            Debug.Assert(commonEdge.triangles[1] == tri1 || commonEdge.triangles[1] == tri2);

            // Tri1 = A,B,C
            // Tri2 = B,D,C
            // commonEdge = B,C

            int localEdgeT1 = triangle1.LocalEdgeIndex(commonEdgeIdx);
            Debug.Assert(localEdgeT1 >= 0);
            int localEdgeT2 = triangle2.LocalEdgeIndex(commonEdgeIdx);
            Debug.Assert(localEdgeT2 >= 0);

            int A = VertexOutOfTriEdge(tri1, localEdgeT1);
            int D = VertexOutOfTriEdge(tri2, localEdgeT2);
            Debug.Assert(A >= 0);
            Debug.Assert(D >= 0);

            int B = (commonEdge.triangles[0] == tri1 ? commonEdge.vertices[0] : commonEdge.vertices[1]);
            int C = (B == commonEdge.vertices[0] ? commonEdge.vertices[1] : commonEdge.vertices[0]);

            Debug.Assert(A != B && A != C && A != D && B != C && B != D && C != D);

            result = new int[2];

            if (!GeomUtils.SegmentIntersect(vertices[A], vertices[D], vertices[B], vertices[C]))
            {
                // can't flip
                result[0] = result[1] = -1;
                return false;
            }

            float closestAngleBefore = Math.Min(ClosestAngleOnTri(A, B, C), ClosestAngleOnTri(B, D, C));

            if (!IsWorthFlipping(A, B, C, D))
            {
                // The resulting triangles would not improve the triangulation, don't bother
                return false;
            }

            RemoveTriangle(tri1);
            RemoveTriangle(tri2);

            result[0] = CreateTriangle(A, B, D);
            result[1] = CreateTriangle(A, D, C);

            float closestAngleAfter = Math.Min(ClosestAngleOnTri(A, B, D), ClosestAngleOnTri(A, D, C));
            //Debug.Assert(closestAngleBefore < closestAngleAfter);			
#if DEBUG
            Debug.Assert(CheckConsistency());
#endif
            return true;
        }


        public void SplitEdge(int edgeIndex, Vertex p, out int[] result)
        {

            Edge edge = edges[edgeIndex];
            float distance;
            Debug.Assert(PointOnSegment(vertices[edge.vertices[0]],
                                          vertices[edge.vertices[1]],
                                          p,
                                          POINT_ON_SEGMENT_DISTANCE_EPSILON,
                                          POINT_ON_SEGMENT_PARAMETRIC_EPSILON,
                                          out distance));

            /*
                       B = edge.v[0]	
                      /|\
                     / | \         T1 = A,B,C   T2 = D,C,B
                    /  |  \
                   /   |   \       T1' = A,B,P  T2' = D,C,P  T3' = A,P,C  T4' = B,D,P
                  /    |    \
                A ---- P ---- D
                  \    |    /
                   \   |   /
                    \  |  /
                     \ | /
                      \|/
                       C = edge.v[1]




            */

            result = new int[4];
            for (int idx = 0; idx < 4; idx++)
            {
                result[idx] = -1;
            }

            int triIdx1 = edge.triangles[0];
            int triIdx2 = edge.triangles[1];

            int tri1edge = triIdx1 >= 0 ? triangles[triIdx1].LocalEdgeIndex(edgeIndex) : -1;
            Debug.Assert(triIdx1 < 0 || tri1edge >= 0);
            int tri2edge = triIdx2 >= 0 ? triangles[triIdx2].LocalEdgeIndex(edgeIndex) : -1;
            Debug.Assert(triIdx2 < 0 || tri2edge >= 0);
            int A = triIdx1 >= 0 ? VertexOutOfTriEdge(triIdx1, tri1edge) : -1;
            Debug.Assert(triIdx1 < 0 || A >= 0);
            int D = triIdx2 >= 0 ? VertexOutOfTriEdge(triIdx2, tri2edge) : -1;
            Debug.Assert(triIdx2 < 0 || D >= 0);

            if (A < 0 || D < 0) return;

            int B = edge.vertices[0];
            int C = edge.vertices[1];

            Debug.Assert(B >= 0 && C >= 0);
            if (B < 0 || C < 0) return;

            Debug.Assert(triangles[triIdx1].Contains(A, B, C));
            Debug.Assert(triangles[triIdx2].Contains(C, B, D));
            if (triIdx1 >= 0) RemoveTriangle(triIdx1);
            if (triIdx2 >= 0) RemoveTriangle(triIdx2);
            Debug.Assert(edge.triangles[0] < 0 && edge.triangles[1] < 0);

            vertices.Add(p);
            int newVertex = vertices.Count - 1;

            int index = 0;
            if (A >= 0 && B >= 0)
            {
                result[index++] = CreateTriangle(A, B, newVertex);
            }
            if (D >= 0 && C >= 0)
            {
                result[index++] = CreateTriangle(D, C, newVertex);
            }
            if (A >= 0 && C >= 0)
            {
                result[index++] = CreateTriangle(A, newVertex, C);
            }
            if (B >= 0 && D >= 0)
            {
                result[index++] = CreateTriangle(B, D, newVertex);
            }
#if DEBUG
            Debug.Assert(CheckConsistency());
#endif
        }

        public void SplitTriangle(int triIdx, Vertex p, out int[] result)
        {
            int newVertex = vertices.Count;
            vertices.Add(p);

            Triangle oldTri = triangles[triIdx];
            Debug.Assert(oldTri.valid);

            int A = oldTri.vertices[0];
            int B = oldTri.vertices[1];
            int C = oldTri.vertices[2];
            Debug.Assert(A != B && A != C && A != newVertex && B != C && B != newVertex && C != newVertex);

            RemoveTriangle(triIdx);

            result = new int[3];
            result[0] = CreateTriangle(A, B, newVertex);
            result[1] = CreateTriangle(B, C, newVertex);
            result[2] = CreateTriangle(C, A, newVertex);
#if DEBUG
            Debug.Assert(CheckConsistency());
#endif
        }

        public int AdjacentTriangle(int triIndex, int edgeIndex)
        {
            Triangle triangle = triangles[triIndex];
            Debug.Assert(triangle.valid);
            Debug.Assert(edgeIndex >= 0 && edgeIndex < 3);

            int edgeGlobalIdx = Math.Abs(triangle.edges[edgeIndex]) - 1;
            Edge edge = edges[edgeGlobalIdx];
            if (triangle.edges[edgeIndex] < 0)
            {
                Debug.Assert(edge.triangles[1] == triIndex);
                return edge.triangles[0];
            }
            else
            {
                Debug.Assert(edge.triangles[0] == triIndex);
                return edge.triangles[1];
            }
        }

        #region utils
        #region edges
        int FindEdge(int v1, int v2)
        {
            Debug.Assert(v1 != v2);
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].Links(v1, v2))
                {
                    return i;
                }
            }
            return -1;
        }
        int CreateEdge(int v1, int v2)
        {
            Edge e = new Edge();
            e.vertices[0] = v1;
            e.vertices[1] = v2;
            e.triangles[0] = -1;
            e.triangles[1] = -1;
            edges.Add(e);
            return edges.Count - 1;
        }

        int CommonEdge(Triangle tri1, Triangle tri2)
        {
            for (int i = 0; i < 3; i++)
            {
                int edgeT1 = Math.Abs(tri1.edges[i]) - 1;
                for (int j = 0; j < 3; j++)
                {
                    int edgeT2 = Math.Abs(tri2.edges[j]) - 1;
                    if (edgeT1 == edgeT2)
                    {
                        return edgeT1;
                    }
                }
            }
            return -1;
        }


        #endregion

        public void ToImage(Image img)
        {
            Graphics g = Graphics.FromImage(img);
            SolidBrush white = new SolidBrush(Color.White);
            Pen black = new Pen(Color.Black);
            SolidBrush red = new SolidBrush(Color.Red);
            g.FillRectangle(white, new Rectangle(0, 0, img.Width, img.Height));

            for (int j = 0; j < triangles.Count; j++)
            {
                if (!triangles[j].valid) continue;

                int v0 = triangles[j].vertices[0];
                int v1 = triangles[j].vertices[1];
                int v2 = triangles[j].vertices[2];
                g.DrawLine(black,
                           vertices[v0].x * img.Width, vertices[v0].y * img.Height,
                           vertices[v1].x * img.Width, vertices[v1].y * img.Height);
                g.DrawLine(black,
                           vertices[v2].x * img.Width, vertices[v2].y * img.Height,
                           vertices[v1].x * img.Width, vertices[v1].y * img.Height);
                g.DrawLine(black,
                           vertices[v0].x * img.Width, vertices[v0].y * img.Height,
                           vertices[v2].x * img.Width, vertices[v2].y * img.Height);
            }

            for (int j = 0; j < vertices.Count; j++)
            {
                const float r = 3;
                g.FillEllipse(red, new RectangleF(vertices[j].x * img.Width - r, vertices[j].y * img.Height - r, 2 * r, 2 * r));
            }

        }
        #endregion


    }
}