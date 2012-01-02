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
using System.Drawing;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Threading;

namespace BlueNoise
{
    /// <summary>
    /// A source tile is basically a wrapper for a Poisson-Disk distribution to be used
    /// as the seed for each Wang Tile
    /// </summary>
    public class SourceTile
    {
        public SourceTile(List<PoissonSample> d, int c)
        {
            distribution = d;
            color = c;
        }
        public List<PoissonSample> distribution;
        public int color;
    }    

    /// <summary>
    /// Wang Tiles, built from a source distribution and 4 more edge distributions merged in
    /// as described in http://johanneskopf.de/publications/blue_noise/
    /// </summary>
    [Serializable]
    public class WangTile
    {
        #region public methods
        public WangTile() { }

        public WangTile(List<PoissonSample> d, List<SourceTile> neighbors)
        {
            distribution = d;
            edgeColors = new int[4];

            // merge distribution with neighbor tiles
            Debug.Assert(neighbors.Count <= 4);
            for (int i = 0; i < neighbors.Count; i++) 
            {
                edgeColors[i] = neighbors[i].color;
                List<PoissonSample> res;
                MergeTiles(distribution, neighbors[i].distribution, (neighbour_e)i, out res);
                distribution = res;
            }

            MakeProgressive(ref distribution);
        }

        public struct MergeCandidate
        {
            public MergeCandidate(PoissonSample s, int src)
            {
                sample = s;
                sourceDist = src;
            }
            public PoissonSample sample;
            public int sourceDist;
        };
        
        static string NeighbourName(neighbour_e n)
        {
            switch (n)
            {
                case neighbour_e.NORTH: return "North";
                case neighbour_e.SOUTH: return "South";
                case neighbour_e.EAST: return "East";
                case neighbour_e.WEST: return "West";
                default: return "";
            }
        }
        #endregion

        #region Public Data
        public enum neighbour_e
        {
            NORTH = 0,
            SOUTH = 1,
            EAST = 2,
            WEST = 3,
            NUM = 4
        };

        public int[] edgeColors;
        public List<PoissonSample> distribution;

        public delegate void WangTileDebugStepDelegate(Image img, string description);
        public static WangTileDebugStepDelegate OnDebugStep = null;
        #endregion

        #region Private methods
        private static void MakeProgressive(ref List<PoissonSample> distribution)
        {
            if (distribution.Count < 2) return;

            distribution.Sort(); // PossionSample implements IComparable

            #region fix equal-ranking samples by random interleaving

            Random random = new Random();
            for (int i = 0; i < distribution.Count; i++)
            {
                int rankingFixStart = i;
                float ranking = distribution[i].ranking;
                while (i + 1 < distribution.Count && distribution[i + 1].ranking == ranking)
                {
                    i++;
                }
                if (rankingFixStart < i)
                {
                    int fixLength = i - rankingFixStart + 1;
                    int[] shuffle = new int[fixLength];
                    for (int j = 0; j < fixLength; j++) shuffle[j] = rankingFixStart + j;
                    for (int j = 0; j < fixLength; j++)
                    {
                        int a = random.Next(fixLength);
                        int b = random.Next(fixLength);
                        if (a == b) continue;
                        int aux = shuffle[a];
                        shuffle[a] = shuffle[b];
                        shuffle[b] = aux;
                    }
                    PoissonSample[] interleavedSamples = new PoissonSample[fixLength];
                    for (int j = 0; j < fixLength; j++)
                    {
                        interleavedSamples[j] = distribution[shuffle[j]];
                    }
                    for (int j = 0; j < fixLength; j++)
                    {
                        distribution[rankingFixStart + j] = interleavedSamples[j];
                    }
                }
            }
            for (int i = 0; i < distribution.Count; i++) distribution[i].ranking = (float)i / distribution.Count;

            #endregion
            #region fix seam-neighbors
            const float alpha = 0.5f;
            for (int i = 2; i < distribution.Count - 1; i++)
            {
                int j = i;
                float threshold = alpha * (float)Math.Sqrt(i);
                for (; j < distribution.Count - 1; j++)
                {
                    float d = float.MaxValue;
                    for (int k = 0; k < i; k++)
                    {
                        float dist = distribution[j].DistSquared(distribution[k]);
                        if (dist < d)
                        {
                            d = dist;
                        }
                    }
                    if ((float)Math.Sqrt(d) > threshold) break;
                }
                if (j != i)
                {
                    PoissonSample aux = distribution[i];
                    distribution[i] = distribution[j];
                    distribution[j] = aux;
                }
            }
            #endregion
        }

        private static void MergeTiles(List<PoissonSample> source, List<PoissonSample> toMerge,
                                         neighbour_e side,
                                         out List<PoissonSample> result)
        {
            List<MergeCandidate> candidates = new List<MergeCandidate>();
            foreach (PoissonSample s in source) candidates.Add(new MergeCandidate(s, 0));
            foreach (PoissonSample s in toMerge) candidates.Add(new MergeCandidate(s, 1));

            const float scale = 1000.0f; // apply a temporary scaling to the Poisson points (in 0-1 range)
            // before Delaunay triangulation (we'll undo this later) to avoid
            // requiring very small epsilons for point snapping etc which 
            // may lead to numeric issues.

            List<Vertex> vertices = new List<Vertex>();
            for (int i = 0; i < candidates.Count; i++)
            {
                vertices.Add(new Vertex(candidates[i].sample.x * scale, candidates[i].sample.y * scale));
            }

            List<int> outTriangles;
            Adjacency adjacency = new Adjacency();
            Delaunay2D.Delaunay2DTriangulate(vertices, false, out outTriangles, ref adjacency);

            for (int j = 0; j < adjacency.vertices.Count; j++)
            {
                adjacency.vertices[j].x /= scale;
                adjacency.vertices[j].y /= scale;
            }

            Image debugImage = null;
            if (OnDebugStep != null) debugImage = new Bitmap(800, 800);


            Voronoi v = Voronoi.FromDelaunay(adjacency);

#if DEBUG
            if (debugImage != null)
            {
                v.ToImage(debugImage, adjacency);
                OnDebugStep(debugImage, "Voronoi diagram generated from Delaunay Triangulation");
            }
#endif
            Graph g = new Graph(v, adjacency, candidates);
            if (debugImage != null)
            {
                g.ToImage(debugImage);
                OnDebugStep(debugImage, "Merging " + NeighbourName(side) + " tile: Convert voronoi diagram into a graph");
            }

            int v0, v1;
            for (int i = 0; i < 4; i++)
                if (i != (int)side) g.Clip((WangTile.neighbour_e)i, out v0, out v1);
            g.Clip(side, out v0, out v1);
#if DEBUG
            if (debugImage != null)
            {
                g.ToImage(debugImage);
                OnDebugStep(debugImage, "Graph generated from Voronoi Diagram");
            }
#endif
            List<int> path;
            g.ShortestPath(v0, v1, out path);
            List<Vertex> shape;
            g.GenerateShape(path, out shape);

            for (int i = 0; i < shape.Count - 1; i++) // skip last one as it is the same as the first element (they're references)
            {
                shape[i].x *= scale;
                shape[i].y *= scale;
            }

            if (debugImage != null)
            {
                Graphics grph = Graphics.FromImage(debugImage);
                Pen sp = new Pen(Color.Orange, 2);
                for (int i = 0; i < shape.Count - 1; i++)
                {
                    grph.DrawLine(sp,
                                  shape[i].x / scale * debugImage.Width,
                                  shape[i].y / scale * debugImage.Height,
                                  shape[i + 1].x / scale * debugImage.Width,
                                  shape[i + 1].y / scale * debugImage.Height);
                }
                OnDebugStep(debugImage, "Merging " + NeighbourName(side) + " tile: Orange line displays the lowest cost seam");
            }

            result = new List<PoissonSample>();

            for (int i = 0; i < candidates.Count; i++)
            {
                bool insideSeam = GeomUtils.PointInPolygon(new Vertex(candidates[i].sample.x * scale, candidates[i].sample.y * scale), shape);
                if ((insideSeam && candidates[i].sourceDist == 1) ||
                     (!insideSeam && candidates[i].sourceDist == 0))
                    result.Add(candidates[i].sample);
            }

            if (debugImage != null)
            {
                Graphics grph = Graphics.FromImage(debugImage);
                grph.Clear(Color.White);
                SolidBrush brushInside = new SolidBrush(Color.Red);
                SolidBrush brushOutside = new SolidBrush(Color.Gray);
                for (int i = 0; i < result.Count; i++)
                {
                    bool insideSeam = GeomUtils.PointInPolygon(new Vertex(result[i].x * scale, result[i].y * scale), shape);
                    float r = (float)debugImage.Width * 0.01f;
                    RectangleF rect = new RectangleF(result[i].x * debugImage.Width - r, result[i].y * debugImage.Height - r, 2.0f * r, 2.0f * r);
                    if (insideSeam)
                        grph.FillEllipse(brushInside, rect);
                    else
                        grph.FillEllipse(brushOutside, rect);
                }
                OnDebugStep(debugImage, "Merging " + NeighbourName(side) + " tile: gray dots = base tile, red dots = merged points");
            }
        }
        
        #endregion

    }

    /// <summary>
    /// A Wang tile set is a collection of Wang Tiles generated from a number of input colors and
    /// source distributions. We combine the edge colors in every possible combination to have
    /// the so-called Complete Set, which enables any permutation while covering the plane.
    /// Since this process takes a long time, the class is serializable and can be initialized from a file.
    /// </summary>
    [Serializable]
    public class WangTileSet
    {
        #region Public methods

        public WangTileSet()
        {
            numColors = 0;
            tiles = null;
            tilesSortedByWest = null;
            tilesSortedByWestNorth = null;
            random = new Random();
        }
        public void Generate(int numColors, int samplesPerTile, int numThreads )
        {
            random = new Random();
            this.numColors = numColors;
            tiles = new List<WangTile>();

            const int numEdges = 4;
            int numTiles = (int)Math.Pow(numColors, numEdges);

            #region generate Poisson distributions
            List<PoissonSample>[] distributions = new List<PoissonSample>[numTiles];
            generatePoissonDistParam_t[] threadParams = new generatePoissonDistParam_t[numThreads];
            PoissonDistribution[] generators = new PoissonDistribution[numThreads];
            AutoResetEvent[] waitHandles = new AutoResetEvent[numThreads];

            for (int i = 0; i < numThreads; i++)
            {
                threadParams[i].dist = new PoissonDistribution();
                threadParams[i].numSamples = samplesPerTile;
                waitHandles[i] = new AutoResetEvent(false);
                threadParams[i].autoResetEvent = waitHandles[i];
            }
            int t = 0;
            Queue<int> availableThreads = new Queue<int>();
            for (int i = 0; i < numThreads; i++) availableThreads.Enqueue(i);

            if (OnConsoleMessage != null) OnConsoleMessage("Generating " + numTiles + " source Poisson Distributions with " + samplesPerTile + " samples each...");

            Image debugImage = null;
            if (OnDebugStep != null) debugImage = new Bitmap(800, 800);

            while (t < numTiles || availableThreads.Count < numThreads)
            {
                while (availableThreads.Count > 0 && t < numTiles)
                {
                    int i = availableThreads.Dequeue();
                    if (OnProgressReport != null) OnProgressReport((float)(t + 1) / numTiles);
                    distributions[t] = new List<PoissonSample>(samplesPerTile);
                    threadParams[i].result = distributions[t];
                    ThreadPool.QueueUserWorkItem(new WaitCallback(GeneratePoissonDistThreaded), threadParams[i]);
                    t++;
                }
                int index = WaitHandle.WaitAny(waitHandles);
                if (debugImage != null)
                {
                    PoissonDistribution.ToImage( threadParams[index].dist.Samples, Color.Black, debugImage, 1);
                    OnDebugStep(debugImage, "Poisson Distribution " + t);
                }
                availableThreads.Enqueue(index);
            }

            #endregion

            #region generate seam tiles
            List<SourceTile> seamTiles = new List<SourceTile>();
#if DEBUG
            Color[] colors = new Color[numColors];
            for( int i = 0; i < numColors; i++ ) colors[i] = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
#endif
            for (int i = 0; i < numColors; i++)
            {
                PoissonDistribution distribution = new PoissonDistribution();
                distribution.Generate(samplesPerTile);
#if DEBUG
                foreach (PoissonSample p in distribution.Samples) p.color = colors[i];
#endif
                seamTiles.Add(new SourceTile(distribution.Samples, i));
            }
            #endregion

            #region generate all edge permutations
            /*
             * 0000 0001 0002 ... 000n
             * 0010 0011 0012 ... 001n
             * 0020 0021 0022 ... 002n
             * ...
             * 00n0 00n1 00n2 ... 00nn
             * 
             * 0100 0101 0102 ... 010n
             * 0110 0111 0112 ... 011n
             * ...
             * 01n0 01n1 01n2 ... 01nn
             * 
             * ...
             * 
             * nnn0 nnn1 nnn2 ... nnnn
             */
            int[,] edgeCol = new int[numTiles,numEdges];
            for (int i = 0; i < numEdges; i++) edgeCol[0, i] = 0;
            for (int i = 1; i < numTiles; i++)
            {
                for (int j = 0; j < numEdges; j++)
                {
                    edgeCol[i,j] = (edgeCol[i-1,j] + (i % (int)Math.Pow(numColors, j) == 0 ? 1 : 0)) % numColors;
                }
            }
            #endregion

            #region generate wang tiles
            WangTile[] tileArray = new WangTile[numTiles];
            t = 0;
            availableThreads.Clear();
            createWangTileParam_t[] createWangTileParams = new createWangTileParam_t[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                availableThreads.Enqueue(i);
                createWangTileParams[i] = new createWangTileParam_t();
                createWangTileParams[i].autoResetEvent = waitHandles[i];
                createWangTileParams[i].autoResetEvent.Reset();
                createWangTileParams[i].tileArray = tileArray;
            }

            if (OnConsoleMessage != null) OnConsoleMessage("Merging distributions to generate Wang Tiles..." );
            
            while (t < numTiles || availableThreads.Count < numThreads)
            {
                while (availableThreads.Count > 0 && t < numTiles)
                {
                    int i = availableThreads.Dequeue();
                    if (OnProgressReport != null) OnProgressReport((float)( t + 1 ) / numTiles);

                    createWangTileParams[i].tileBaseDistribution = distributions[t];
                    createWangTileParams[i].neighbours = new List<SourceTile>();
                    createWangTileParams[i].tileIndex = t;
                    if (numColors > 1)
                    {
                        for (int j = 0; j < numEdges; j++)
                            createWangTileParams[i].neighbours.Add(seamTiles[edgeCol[t, j]]);
                    }
                    ThreadPool.QueueUserWorkItem(new WaitCallback(CreateWangTileThreaded), createWangTileParams[i]);
                    t++;
                }
                int index = WaitHandle.WaitAny(waitHandles);
                availableThreads.Enqueue(index);
            }
            tiles = tileArray.ToList();
         
            SortTiles();
            MakeRecursive();

            if (OnDebugStep != null)
            {
                ToColorTiles(debugImage, new Size(8, 8));
                OnDebugStep(debugImage, "Sample coverage of the generated Wang Tiles" );
            }

            #endregion
        }
        public static WangTileSet FromFile(string sourceFile)
        {
            WangTileSet wts = new WangTileSet();
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(wts.GetType());
            System.IO.StreamReader file = new System.IO.StreamReader(sourceFile);
            wts = (WangTileSet)serializer.Deserialize(file);
            wts.SortTiles();
            wts.MakeRecursive();
            file.Close();
            return wts;
        }


        /// <summary>
        /// Debug: cover the image plane with a random distribution of color-filled tiles to check the aperiodicity
        /// </summary>
        public void ToColorTiles(Image img, Size tileSize)
        {
            Color[] colors = new Color[numColors];
            Random r = new Random();
            for (int i = 0; i < numColors; i++) colors[i] = Color.FromArgb(r.Next(256), r.Next(256), r.Next(256));

            List<Bitmap> tileImgs = new List<Bitmap>();
            foreach (WangTile wt in tiles)
            {
                Bitmap bmp = new Bitmap(tileSize.Width, tileSize.Height);
                Graphics gBmp = Graphics.FromImage(bmp);
                Point[] points = new Point[3];
                for (int i = 0; i < 4; i++)
                {
                    switch (i)
                    {
                        case (int)WangTile.neighbour_e.NORTH:
                            points[0] = new Point(0, 0);
                            points[1] = new Point(bmp.Width / 2, bmp.Height / 2);
                            points[2] = new Point(bmp.Width - 1, 0);
                            break;
                        case (int)WangTile.neighbour_e.SOUTH:
                            points[0] = new Point(0, bmp.Height - 1);
                            points[1] = new Point(bmp.Width / 2, bmp.Height / 2);
                            points[2] = new Point(bmp.Width - 1, bmp.Height - 1);
                            break;
                        case (int)WangTile.neighbour_e.EAST:
                            points[0] = new Point(bmp.Width - 1, 0);
                            points[1] = new Point(bmp.Width / 2, bmp.Height / 2);
                            points[2] = new Point(bmp.Width - 1, bmp.Height - 1);
                            break;
                        case (int)WangTile.neighbour_e.WEST:
                            points[0] = new Point(0, 0);
                            points[1] = new Point(bmp.Width / 2, bmp.Height / 2);
                            points[2] = new Point(0, bmp.Height - 1);
                            break;

                    }
                    gBmp.FillPolygon(new SolidBrush(colors[wt.edgeColors[i]]), points);
                    gBmp.DrawLine(new Pen(colors[wt.edgeColors[i]]), points[0], points[2]);

                }
                tileImgs.Add(bmp);
            }
            ToImage(img, tileSize, tileImgs);
        }

        /// <summary>
        /// Debug: cover the pla
        /// </summary>
        /// <param name="img"></param>
        /// <param name="tileSize"></param>
        /// <param name="coverage"></param>
        public void ToNoiseTiles(Image img, Size tileSize, float coverage)
        {
            List<Bitmap> tileImgs = new List<Bitmap>();
            foreach (WangTile wt in tiles)
            {
                Bitmap bmp = new Bitmap(tileSize.Width, tileSize.Height);
                PoissonDistribution.ToImage(wt.distribution, Color.Black, bmp, coverage);
                tileImgs.Add(bmp);
            }
            ToImage(img, tileSize, tileImgs);
        }

        public void GetSubdivisions(int tile, out int[,] subd, out int splitsPerDimension)
        {
            Debug.Assert(tile >= 0 && tile < tiles.Count);
            subd = subdivisions[tile];
            splitsPerDimension = this.splitsPerDimension;
        }

        public int SplitsPerDimension { get { return splitsPerDimension; } }

        #endregion

        #region Public Data

        public int numColors;
        public List<WangTile> tiles;

        public delegate void ConsoleDelegate(string msg);
        public delegate void ProgressReportDelegate(float progress);

        public static ConsoleDelegate OnConsoleMessage = null;
        public static ProgressReportDelegate OnProgressReport = null;

        public delegate void WangTileDebugStepDelegate(Image img, string description);
        public static WangTileDebugStepDelegate OnDebugStep = null;
        
        #endregion

        #region Private methods

        private struct generatePoissonDistParam_t
        {
            public int numSamples;
            public PoissonDistribution dist;
            public AutoResetEvent autoResetEvent;
            public List<PoissonSample> result;
        }
        private void GeneratePoissonDistThreaded(object obj)
        {
            generatePoissonDistParam_t param = (generatePoissonDistParam_t)obj;
            param.dist.Generate(param.numSamples);
            for (int i = 0; i < param.dist.Samples.Count; i++)
            {
                param.result.Add(param.dist.Samples[i]);
            }
            param.autoResetEvent.Set();            
        }
        private struct createWangTileParam_t
        {
            public List<PoissonSample> tileBaseDistribution;
            public List<SourceTile> neighbours;
            public int tileIndex;
            public AutoResetEvent autoResetEvent;
            public WangTile[] tileArray;
        }
        private void CreateWangTileThreaded(object obj)
        {
            createWangTileParam_t param = (createWangTileParam_t)obj;
            param.tileArray[ param.tileIndex ] = new WangTile(param.tileBaseDistribution, param.neighbours);
            param.autoResetEvent.Set();
        }

        private void MakeRecursive()
        {
            const int numSubdivisions = 4;
            #region create subdivision rules
            // for each color, generate a UNIQUE sequence of numSubdivisions colors
            // There is a total of pow(numColors, numSubdivisions) combinations,
            // we'll choose evenly space samples
            int combinatory = (int)Math.Pow(numColors, numSubdivisions);
            List<int>[] rules = new List<int>[numColors];
            for (int i = 0; i < numColors; i++)
            {
                List<int> rule = new List<int>();
                int ruleIndex = ( i + 1 ) * combinatory / ( numColors + 1 );
                for (int j = 0; j < numSubdivisions; j++)
                {
                    int power = (int)Math.Pow(numColors, numSubdivisions - j - 1 );
                    int element = ruleIndex / power;
                    ruleIndex -= element * power;
                    rule.Add(element);
                }
                rules[i] = rule;
            }
            #endregion
            #region subdivide each tile
            subdivisions = new List<int[,]>();
            this.splitsPerDimension = numSubdivisions;
            for (int i = 0; i < tiles.Count; i++)
            {
                int[,] subd;
                Subdivide(tiles[i], numSubdivisions, rules, out subd);
                subdivisions.Add(subd);
            }
            #endregion
        }

        private void Subdivide(WangTile tile, int subdivisions, List<int>[] rules, out int[,] result)
        {
            result = new int[subdivisions, subdivisions];
            
            // the rules determine, for each edge color on the source tile, a 
            // list of colors it is substituted with in the subdivided tile set

            /*
             *                              
             *                              _a__b__c__d_         _______________
             *   -------cN--------        m|\ /\ /\ /\ /|i      |\ /|\ /|\ /|\ /|
             *  |\               /|        |/          \|       |/_\|/_\|/_\|/_\|
             *  |  \           /  |       n|\          /|j      |\ /|\ /|\ /|\ /|
             *  |    \       /    |        |/          \|       |/_\|/_\|/_\|/_\|
             *  |      \   /      |       o|\          /|k      |\ /|\ /|\ /|\ /|
             *  cW       X       cE  -->   |/          \|  -->  |/_\|/_\|/_\|/_\|
             *  |      /   \      |       p|\          /|l      |\ /|\ /|\ /|\ /|
             *  |    /       \    |        |/_\/_\/_\/_\|       |/_\|/_\|/_\|/_\|
             *  |  /           \  |          e  f  g  h
             *  | /              \|
             *  |/------cS--------
             *  
             *  cN --> a,b,c,d
             *  cS --> e,f,g,h
             *  cE --> i,j,k,l
             *  cW --> m,n,o,p
             *  
             * Only corners have 2 restrictions (m-a, d-i, p-e, l-h), remaining 
             * edges have 1 restriction then we use the scanline algorithm to 
             * fill-in the gaps
             */
            int[] restrictions = new int[(int)WangTile.neighbour_e.NUM];
            int previous = -1;
            int[] upperRow = new int[subdivisions];

            for (int i = 0; i < subdivisions; i++)
            {
                for (int j = 0; j < subdivisions; j++)
                {
                    restrictions[(int)WangTile.neighbour_e.NORTH] = -1;
                    restrictions[(int)WangTile.neighbour_e.SOUTH] = -1;
                    restrictions[(int)WangTile.neighbour_e.EAST] = -1;
                    restrictions[(int)WangTile.neighbour_e.WEST] = -1;
                    
                    if (i == 0) restrictions[(int)WangTile.neighbour_e.NORTH] = rules[tile.edgeColors[(int)WangTile.neighbour_e.NORTH]][j];
                    if (i == subdivisions - 1 ) restrictions[(int)WangTile.neighbour_e.SOUTH] = rules[tile.edgeColors[(int)WangTile.neighbour_e.SOUTH]][j];
                    if (j == 0) restrictions[(int)WangTile.neighbour_e.WEST] = rules[tile.edgeColors[(int)WangTile.neighbour_e.WEST]][i];
                    if (j == subdivisions - 1 ) restrictions[(int)WangTile.neighbour_e.EAST] = rules[tile.edgeColors[(int)WangTile.neighbour_e.EAST]][i];

                    if (i > 0)
                        restrictions[(int)WangTile.neighbour_e.NORTH] = tiles[upperRow[j]].edgeColors[(int)WangTile.neighbour_e.SOUTH];
                    if (j > 0)
                        restrictions[(int)WangTile.neighbour_e.WEST] = tiles[upperRow[j]].edgeColors[(int)WangTile.neighbour_e.EAST];

                    result[i, j] = FindTile(restrictions);
                    previous = result[i, j];
                }
                for (int j = 0; j < subdivisions; j++) upperRow[j] = result[i, j];
            }
        }

        private int FindTile(int[] colors)
        {
            List<int> candidates = new List<int>();
            for (int i = 0; i < this.tiles.Count; i++)
            {
                WangTile t = tiles[i];
                bool match = true;
                for (int j = 0; j < (int)WangTile.neighbour_e.NUM; j++)
                {
                    if (colors[j] < 0) continue;
                    if (t.edgeColors[j] != colors[j])
                    {
                        match = false;
                        break;
                    }
                }
                if ( match ) candidates.Add(i);                
            }

            if (candidates.Count == 0) return -1;
            return candidates[random.Next(candidates.Count)];            
        }

        public void Serialize(string destFile)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(this.GetType());
            System.IO.StreamWriter file = new System.IO.StreamWriter(destFile);
            serializer.Serialize(file, this);
            file.Close();
        }

        private void SortTiles()
        {
            #region sort by west tile
            tilesSortedByWest = new List<int>[numColors];
            for (int i = 0; i < numColors; i++)
            {
                tilesSortedByWest[i] = new List<int>();
                for (int j = 0; j < tiles.Count; j++)
                {
                    if (tiles[j].edgeColors[(int)WangTile.neighbour_e.WEST] == i)
                        tilesSortedByWest[i].Add(j);
                }
            }
            #endregion
            #region sort by west and then north
            tilesSortedByWestNorth = new List<int>[numColors, numColors];
            for (int i = 0; i < numColors; i++)
            {
                for (int j = 0; j < numColors; j++)
                {
                    tilesSortedByWestNorth[i, j] = new List<int>();
                    foreach (int wt in tilesSortedByWest[i])
                    {
                        if (tiles[wt].edgeColors[(int)WangTile.neighbour_e.NORTH] == j)
                            tilesSortedByWestNorth[i, j].Add(wt);
                    }
                }
            }
            #endregion
        }

        private void ToImage(Image img, Size tileSize, List<Bitmap> tileImgs)
        {
            Graphics g = Graphics.FromImage(img);
            int tilesW = img.Width / tileSize.Width;
            int tilesH = img.Height / tileSize.Height;

            WangTile lastTile = null;
            int last = -1;
            int[] lastRow = new int[tilesW];
            int[] currentRow = new int[tilesW];
            for (int i = 0; i < tilesW; i++)
            {
                lastRow[i] = -1;
                currentRow[i] = -1;
            }
            for (int j = 0; j < tilesH; j++)
            {
                for (int i = 0; i < tilesW; i++)
                {
                    int current;
                    List<int> availableTiles = null;
                    if (last == -1)
                    {
                        // no restrictions
                        current = random.Next(tiles.Count);
                    }
                    else
                    {
                        if (j == 0)
                        {
                            // only restriction is west edge, which has to match
                            // the east edge of the last tile placed
                            availableTiles = tilesSortedByWest[lastTile.edgeColors[(int)WangTile.neighbour_e.EAST]];
                        }
                        else
                        {
                            int east = ( i == 0 ? random.Next(numColors) : lastTile.edgeColors[(int)WangTile.neighbour_e.EAST] );
                            int north = tiles[lastRow[i]].edgeColors[(int)WangTile.neighbour_e.SOUTH];
                            // restrict by west and north edges
                            availableTiles = tilesSortedByWestNorth[east,north];
                        }
                        current = availableTiles[random.Next(availableTiles.Count)]; ;
                    }

                    currentRow[i] = current;

                    g.DrawImage(tileImgs[current], new Point(i * tileSize.Width, j * tileSize.Height));
                    last = current;
                    lastTile = tiles[last];
                }
                for (int k = 0; k < tilesW; k++)
                {
                    lastRow[k] = currentRow[k];
                }
            }
        }

        #endregion

        #region Private Data

        private List<int[,]> subdivisions;
        private int splitsPerDimension;
        private List<int>[] tilesSortedByWest;
        private List<int>[,] tilesSortedByWestNorth;
        private Random random;

        #endregion
    }
}
