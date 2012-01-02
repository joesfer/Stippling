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
using PoissonDist;
using System.Diagnostics;
using System.Drawing;

namespace BlueNoise
{
    /// <summary>
    /// The Graph class turns a Voronoi Diagram into a graph in order to extract
    /// the minimum-cost seam path along the cell boundaries.
    /// </summary>
    class Graph
    {
        public class Edge
        {
            public int[] v;
            public float cost;
            public Edge(int v0, int v1, float c)
            {
                v = new int[2];
                v[0] = v0;
                v[1] = v1;
                cost = c;
            }
        }

        public List<Vertex> vertices;
        public List<WangTile.MergeCandidate> candidates;
        public List<Edge> edges;

        private static float INFINITY_COST = float.MaxValue / 2;

        public Graph( Voronoi voronoi, Adjacency adjacency, List<WangTile.MergeCandidate> candidates )
        {
            vertices = voronoi.vertices;
            this.candidates = candidates;
            edges = new List<Edge>();
            float maxCellDist = float.MinValue;
            for (int i = 0; i < voronoi.edges.Count; i++)
            {
                Voronoi.Edge edge = voronoi.edges[i];
                bool found = false;
                foreach( Edge e in edges )
                {
                    if ( e.v[0] == edge.v[0] && e.v[1] == edge.v[1] ||
                         e.v[0] == edge.v[1] && e.v[1] == edge.v[0])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Vertex cellA = edge.cell[0] < adjacency.vertices.Count ? adjacency.vertices[edge.cell[0]] : null;
                    Vertex cellB = edge.cell[1] < adjacency.vertices.Count ? adjacency.vertices[edge.cell[1]] : null;
                    float cellDist = (cellA != null & cellB != null) ? (cellA - cellB).Length() : float.MaxValue;
                    if ( cellDist < float.MaxValue ) maxCellDist = Math.Max(maxCellDist, cellDist);
                    Graph.Edge gEdge = new Graph.Edge(edge.v[0], edge.v[1], cellDist);

                    edges.Add(gEdge);
                }
            }
            for (int i = 0; i < edges.Count; i++)
            {
                float dist = edges[i].cost;
                edges[i].cost = dist < float.MaxValue ? (float)Math.Pow(1.0f - dist / maxCellDist, 100.0f) : INFINITY_COST; // the exponent value is suggested in the paper
            }
        }

        /// <summary>
        /// The voronoi diagram has edges that go beyond the original image boundaries.
        /// This method deals with those edges, trimming them so that our graph is bounded
        /// (this ensures the shortest path never goes out of the image).
        /// </summary>
        /// <param name="neighbour"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        public void Clip(WangTile.neighbour_e neighbour, out int v0, out int v1)
        {
            float epsilon = 1e-3f;
            List<int> deletedVertices = new List<int>();
            switch (neighbour)
            {
                case WangTile.neighbour_e.NORTH:
                    v0 = AddVertex(new Vertex(0, 1));
                    v1 = AddVertex(new Vertex(1, 1));
                    AddEdge(v0, v1, 1000.0f);
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        if (vertices[i].y > 1.0f + epsilon) deletedVertices.Add(i);
                    }
                    break;
                case WangTile.neighbour_e.SOUTH:
                    v0 = AddVertex(new Vertex(0, 0));
                    v1 = AddVertex(new Vertex(1, 0));
                    AddEdge(v0, v1, 1000.0f);
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        if (vertices[i].y < -epsilon) deletedVertices.Add(i);
                    }
                    break;
                case WangTile.neighbour_e.EAST:
                    v0 = AddVertex(new Vertex(1, 0));
                    v1 = AddVertex(new Vertex(1, 1));
                    AddEdge(v0, v1, 1000.0f);
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        if (vertices[i].x > 1.0f + epsilon) deletedVertices.Add(i);
                    }
                    break;
                case WangTile.neighbour_e.WEST:
                    v0 = AddVertex(new Vertex(0, 0));
                    v1 = AddVertex(new Vertex(0, 1));
                    AddEdge(v0, v1, 1000.0f);
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        if (vertices[i].x < -epsilon) deletedVertices.Add(i);
                    }
                    break;
                default:
                    v0 = v1 = 0;
                    break;
            }
            for (int e = 0; e < edges.Count; e++)
            {
                if (deletedVertices.Contains(edges[e].v[0]) || deletedVertices.Contains(edges[e].v[1]))
                {
                    edges.RemoveAt(e); // do not remove the vertices themselves to avoid having to reindex all other edges
                    e--;
                }
            }
        }

        public void ToImage(Image img)
        {
            int margin = 0;
            Graphics g = Graphics.FromImage(img);
            g.Clear(Color.White);
            SolidBrush vBrush = new SolidBrush(Color.Black);
            HashSet<int> distributions = new HashSet<int>();
            for( int i = 0; i < candidates.Count; i++ )
            {
                distributions.Add( candidates[i].sourceDist );
            }
            SolidBrush[] distributionColor = new SolidBrush[distributions.Count];
            Random rand = new Random();
            for( int i = 0; i < distributions.Count; i++ )
            {
                distributionColor[i] = new SolidBrush(Color.FromArgb(rand.Next(255),rand.Next(255),rand.Next(255)));
            }
            for (int i = 0; i < candidates.Count; i++)
            {
                WangTile.MergeCandidate c = candidates[i];
                const float r = 3;
                g.FillEllipse(distributionColor[c.sourceDist], new RectangleF(margin + c.sample.x * (img.Width- 2*margin) - r, margin + c.sample.y * (img.Height- 2*margin) - r, 2 * r, 2 * r));
            }
            for (int i = 0; i < edges.Count; i++)
            {
                Edge e = edges[i];
                Vertex v0 = vertices[e.v[0]];
                Vertex v1 = vertices[e.v[1]];
                Pen col = e.cost <= 1.0f ? new Pen(Color.FromArgb((int)(e.cost * 255), 0, 0), 2) :
                    (i % 2 == 0 ? new Pen(Color.Blue, 2) : new Pen(Color.Yellow, 2));
                g.DrawLine(col, margin + v0.x * (img.Width - 2 * margin), margin + v0.y * (img.Height - 2 * margin), margin + v1.x * (img.Width - 2 * margin), margin + v1.y * (img.Height - 2 * margin));
            }
            for (int i = 0; i < vertices.Count; i++)
            {
                g.DrawEllipse(new Pen(Color.LightGreen), new RectangleF(margin + vertices[i].x * (img.Width - 2 * margin), margin + vertices[i].y * (img.Height - 2 * margin), 2, 2));
            }
        }

        /// <summary>
        /// Implement Dijkstra's algorithm to find the shortest path on a weighteg graph
        /// </summary>
        public bool ShortestPath(int v0, int v1, out List<int> path)
        {
            path = new List<int>();

            float[] cost = new float[vertices.Count];
            int[] previous = new int[vertices.Count];
            bool[] visited = new bool[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                cost[i] = INFINITY_COST; // don't make it MaxValue or we won't be able to add up anything more
                previous[i] = -1;
                visited[i] = false;
            }

            List<Pair<int, float>>[] neighbors = new List<Pair<int, float>>[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                neighbors[i] = new List<Pair<int, float>>();
            }
            for (int e = 0; e < edges.Count; e++)
            {
                Edge edge = edges[e];
                for (int j = 0; j < 2; j++)
                {
                    neighbors[edge.v[j]].Add(new Pair<int, float>(edge.v[(j + 1) % 2], edge.cost));
                }
            }

            int current = v0;
            cost[current] = 0;
            int unvisited = vertices.Count;
            while (unvisited > 0)
            {
                foreach (Pair<int, float> neighbor in neighbors[current])
                {
                    if (visited[neighbor.First]) continue; // for each unvisited neighbor

                    float tentative = cost[current] + neighbor.Second; // tentative = cost reaching current + cost from current to neighbor
                    if (tentative < cost[neighbor.First])
                    {
                        cost[neighbor.First] = tentative;
                        previous[neighbor.First] = current;
                    }
                }
                visited[current] = true;
                unvisited--;

                if (current == v1) break;

                float minCost = float.MaxValue;
                int next = current;
                for (int i = 0; i < vertices.Count; i++)
                {
                    if (visited[i]) continue;
                    float c = cost[i];
                    if (c < minCost)
                    {
                        minCost = c;
                        next = i;
                    }
                }
                current = next;
            }

            if (current != v1) return false;

            while (current != v0)
            {
                path.Add(current);
                current = previous[current];
                Debug.Assert(current >= 0);
            }
            path.Add(v0);
            return true;
        }

        /// <summary>
        /// Turn a list of vertex indices into a list of vertices, closing the path
        /// by repeating the first item at the end of the list.
        /// </summary>
        public void GenerateShape(List<int> path, out List<Vertex> shape)
        {
            shape = new List<Vertex>();

            if (path.Count == 0) return;

            // generate a vertex list linking v0, v1, v2 ... vn, v0 in a closed shape

            for (int i = 0; i < path.Count; i++) shape.Add(vertices[path[i]]);
            shape.Add(vertices[path[0]]);
        }

        private int AddVertex(Vertex v)
        {
            const float tolerance = 1e-3f;
            for (int i = 0; i < vertices.Count; i++)
            {
                if ( Math.Abs( vertices[ i ].x - v.x) <= tolerance &&
                    Math.Abs(vertices[i].y - v.y) <= tolerance)
                {
                    return i;
                }
            }
            vertices.Add(v);
            return vertices.Count - 1;
        }
        private void AddEdge(int v0, int v1, float cost )
        {            
            // remove all edges crossing our clipping edge (TODO: could we split them instead?)
            Edge clippingEdge = new Edge(v0, v1, cost);
            Stack<Edge> candidates = new Stack<Edge>();
            candidates.Push(clippingEdge);
            while(candidates.Count > 0)
            {
                Edge cEdge = candidates.Pop();

                bool isect = false;
                if ((vertices[cEdge.v[0]] - vertices[cEdge.v[1]]).Length() > 1e-3f)
                {
                    foreach (Edge e in edges)
                    {
                        Vertex p;
                        if (e.v[0] != cEdge.v[0] && e.v[0] != cEdge.v[1] &&
                             e.v[1] != cEdge.v[0] && e.v[1] != cEdge.v[1] &&
                             GeomUtils.SegmentIntersect(vertices[e.v[0]], vertices[e.v[1]], vertices[cEdge.v[0]], vertices[cEdge.v[1]], out p))
                        {
                            vertices.Add(p);
                            int isectIdx = vertices.Count - 1;

                            edges.Remove(e);
                            edges.Add(new Edge(e.v[0], isectIdx, e.cost));
                            edges.Add(new Edge(isectIdx, e.v[1], e.cost));

                            candidates.Push(new Edge(cEdge.v[0], isectIdx, cost));
                            candidates.Push(new Edge(isectIdx, cEdge.v[1], cost));
                            isect = true;
                            break;
                        }
                    }
                    if (!isect) edges.Add(cEdge);
                }
            }
        }

        private class Pair<T, U>
        {
            public Pair()
            {
            }

            public Pair(T first, U second)
            {
                this.First = first;
                this.Second = second;
            }

            public T First { get; set; }
            public U Second { get; set; }
        };
    }
}