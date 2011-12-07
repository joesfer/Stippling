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
    /// Generate Voronoi Diagrams from the dual Delaunay Triangulation
    /// </summary>
    class Voronoi
    {
        #region Public Methods
        public static Voronoi FromDelaunay(Adjacency adjacency)
        {
            Voronoi v = new Voronoi();

            int[] triangleRemapping = new int[adjacency.triangles.Count];

            for (int i = 0; i < adjacency.triangles.Count; i++)
            {
                Adjacency.Triangle t = adjacency.triangles[i];
                if (!t.valid)
                {
                    triangleRemapping[i] = -1;
                    continue;
                }
                Vertex c; float r;
                t.CircumCircle( adjacency.vertices, out c, out r);
                
                triangleRemapping[i] = v.vertices.Count;
                v.vertices.Add(c);
            }
            for (int i = 0; i < adjacency.triangles.Count; i++)
            {
                Adjacency.Triangle t = adjacency.triangles[i];
                if (!t.valid)
                {
                    continue;
                }
                for (int e = 0; e < 3; e++)
                {
                    int adjT = adjacency.AdjacentTriangle(i, e);
                    if (adjT < 0 || adjacency.triangles[adjT].valid == false) continue;
                    if (triangleRemapping[adjT] < 0) continue; // already processed
                    int voronoiV0 = triangleRemapping[i];
                    int voronoiV1 = triangleRemapping[adjT];
                    Debug.Assert(voronoiV0 >= 0 && voronoiV1 >= 0);

                    // the 2 vertices shared by the adjacent triangles will become
                    // the voronoi cells divided by the edge we're creating
                    int cellA = adjacency.edges[Math.Abs(t.edges[e]) - 1].vertices[0];
                    int cellB = adjacency.edges[Math.Abs(t.edges[e]) - 1].vertices[1];                    

                    v.edges.Add(new Edge(voronoiV0, voronoiV1, cellA, cellB));
                }
                triangleRemapping[i] = -1;
             
            }
            return v;
        }

        public void ToImage(Image img, Adjacency adjacency)
        {
            SolidBrush vertexColor = new SolidBrush(Color.Green);
            Pen boundaryColor = new Pen(Color.Blue);
            Pen cellColor = new Pen(Color.Gray);

            Graphics g = Graphics.FromImage(img);
            g.Clear(Color.White);
            for (int e = 0; e < edges.Count; e++)
            {
                Vertex v0 = vertices[ edges[e].v[0] ];
                Vertex v1 = vertices[ edges[e].v[1] ];
                g.DrawLine(boundaryColor, v0.x * img.Width, v0.y * img.Height, v1.x * img.Width, v1.y * img.Height);

                Vertex cellA = adjacency.vertices[edges[e].cell[0]];
                Vertex cellB = adjacency.vertices[edges[e].cell[1]];
                Vertex centerV = new Vertex((v0.x + v1.x) * 0.5f, (v0.y + v1.y) * 0.5f);
                g.DrawLine(cellColor, centerV.x * img.Width, centerV.y * img.Height, cellA.x * img.Width, cellA.y * img.Height);
                g.DrawLine(cellColor, centerV.x * img.Width, centerV.y * img.Height, cellB.x * img.Width, cellB.y * img.Height);
            }
            for (int v = 0; v < vertices.Count; v++)
            {
                const float r = 3;
                g.FillEllipse(vertexColor, new RectangleF(vertices[v].x * img.Width - r, vertices[v].y * img.Height - r, 2 * r, 2 * r));
            }
        }
        #endregion

        #region Public Data
        public class Edge
        {
            /*
             * delaunay:
             * 
             *       A ------C
             *      / \ v1  /
             *     /   \   / 
             *    / v0  \ /     v0,v1 centroids of ABD and ABC
             *   D------ B
             *  
             * voronoi edge:
             * 
             *    cellA  :
             *          v1......
             *         /     
             *  ....v0/ 
             *       :  cellB
             *       :
             * v0 and v1 are centroids of adjacent delaunay triangles
             * cellA and cellB are vertices linked by the common edge of those
             * adjacent delaunay triangles
             */
            public int[] v;
            public int[] cell;
            public Edge( int v0, int v1, int cellA, int cellB )
            {
                v = new int[2];
                v[0] = v0;
                v[1] = v1;
                cell = new int[2];
                cell[0] = cellA;
                cell[1] = cellB;
            }
        }
        public List<Vertex> vertices;
        public List<Edge> edges;
        #endregion
        
        #region Private Methods
        /// <summary>
        /// private constructor to avoid direct instantiation
        /// </summary>
        private Voronoi()
        {
            vertices = new List<Vertex>();
            edges = new List<Edge>();
        }
        #endregion

    }
}
