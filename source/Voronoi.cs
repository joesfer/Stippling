/* 
	================================================================================
	Copyright (c) 2012, Jose Esteve. http://www.joesfer.com
	This software is released under the LGPL-3.0 license: http://www.opensource.org/licenses/lgpl-3.0.html	
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
