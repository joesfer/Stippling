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
using System.Drawing;
using System.Diagnostics;

namespace BlueNoise
{

    class ImageTreeNode
    {
        public ImageTreeNode() { children = null; tile = -1; tileDensity = 1.0f; }
        public ImageTreeNode[,] children;
        public int tile;
        public float tileDensity;
    }
    /// <summary>
    /// Stipple using Wang Tiles
    /// </summary>
    class WTStipple
    {
        public WTStipple(Image source, WangTileSet tileSet, int tonalRange, Bitmap dest )
        {
            // Convert source image to grayscale

            Bitmap bmp = new Bitmap(source.Width, source.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height));
            float[,] grayscale = ToDensity(bmp);
            this.tileSet = tileSet;
            
            // Subdivide image recusively as long as we need more density on each leave's tile
            // to account for the underlying sampled region on the image
            ImageTreeNode root = new ImageTreeNode();
            Random r = new Random(tileSet.tiles.Count);
            root.tile = r.Next(tileSet.tiles.Count);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            int minSize = 8;

            Graphics g2 = Graphics.FromImage(dest);
            g2.Clear(Color.White);
            Refine_r(root, rect, grayscale, minSize, 0, 5, tonalRange, dest );
        }

        private static Random random = new Random();

        private static float AreaDensity(float[,] img, Rectangle rect)
        {
            float sum = 0;
            for( int j = rect.Y; j < rect.Y + rect.Height; j++ )
            {
                for( int i = rect.X; i < rect.X + rect.Width; i++ )
                {
                    sum += img[i, j];
                }
            }
            return sum;
        }
        private static float DiskDensity(float[,] img, Point center, int radius)
        {
            float sum = 0;
            for (int j = Math.Max(0, center.Y - radius) ; j < Math.Min( img.GetLength(1), center.Y + radius); j++)
            {
                for (int i = Math.Max(0, center.X - radius); i < Math.Min(img.GetLength(0), center.X + radius) ; i++)
                {
                    int d2 = (i - center.X) * (i - center.X) + (j - center.Y) * (j - center.Y);
                    if (d2 > radius * radius) continue;
                    sum += img[i, j];
                }
            }
            return sum;
        }
        private static float DiskDensity(Bitmap img, Point center, int radius)
        {
            float sum = 0;
            for (int j = Math.Max(0, center.Y - radius); j < Math.Min(img.Height, center.Y + radius); j++)
            {
                for (int i = Math.Max(0, center.X - radius); i < Math.Min(img.Width, center.X + radius); i++)
                {
                    int d2 = (i - center.X) * (i - center.X) + (j - center.Y) * (j - center.Y);
                    if (d2 > radius * radius) continue;
                    sum += 1.0f - img.GetPixel(i,j).GetBrightness();
                }
            }
            return sum;
        }
        private void Refine_r(ImageTreeNode node, Rectangle rect, float[,] density, int minSize, int depth, int maxDepth, int toneScale, Bitmap dest)
        {
            Debug.Assert(node.tile != -1);
            List<PoissonDist.PoissonSample> distribution = tileSet.tiles[node.tile].distribution;
            float tileMaxDensity = (float)distribution.Count;
            float requiredDensity = AreaDensity(density, rect);

            // Cover the area with the current tile

            float tileAvgDensity = Math.Min(1.0f, tileMaxDensity / (rect.Width * rect.Height));

            for (int i = 0; i < distribution.Count; i++)
            {
                int stippleX = rect.Left + (int)(rect.Width * distribution[i].x);
                int stippleY = rect.Top + (int)(rect.Height * distribution[i].y);
                float r = 1;//Math.Max(1, (distribution[i].radius * rect.Width));
                float area = (float)(r*r * Math.PI);
                float diskDensity = DiskDensity(density, new Point(stippleX, stippleY), (int)r);
                float diskAvgDensity = diskDensity / area;

                float factor = (float)(0.1f / Math.Pow(1, -2) * Math.Pow(4, 2.0f * depth) / toneScale);
                if (diskAvgDensity < (float)i * factor) continue;
                dest.SetPixel(stippleX, stippleY, Color.Black);                    
            }
            
            // Check whether we need to keep subdividing

            if (rect.Width <= minSize || rect.Height <= minSize || depth == maxDepth) return;

            if (Math.Pow(0.1, -2) / Math.Pow(4, 2 * depth) * toneScale - tileMaxDensity >  16 * tileMaxDensity )
            {
                // we need to subdivide
                int[,] subd;
                int splitsPerDimension;
                tileSet.GetSubdivisions(node.tile, out subd, out splitsPerDimension);
                node.children = new ImageTreeNode[splitsPerDimension, splitsPerDimension];
                Size childRectSize = new Size((int)Math.Floor((float)rect.Width / splitsPerDimension),
                                              (int)Math.Floor((float)rect.Height / splitsPerDimension));
                for (int j = 0; j < splitsPerDimension; j++)
                {
                    for (int i = 0; i < splitsPerDimension; i++)
                    {
                        node.children[i, j] = new ImageTreeNode();
                        node.children[i, j].tile = subd[i, j];
                        Rectangle childRect = new Rectangle(rect.X + i * childRectSize.Width, 
                                                            rect.Y + j * childRectSize.Height, 
                                                            childRectSize.Width, 
                                                            childRectSize.Height);
                        if (i == splitsPerDimension - 1) // adjust borders
                            childRect.Width = rect.Right - childRect.X;
                        if (j == splitsPerDimension - 1) // adjust borders
                            childRect.Height = rect.Bottom - childRect.Y;
                        Refine_r(node.children[i, j], childRect, density, minSize, depth + 1, maxDepth, toneScale, dest);
                    }
                }
            }
        }
        private float[,] ToDensity(Image img)
        {
            Bitmap bmp = new Bitmap(img);
            float[,] res = new float[img.Width, img.Height];
            for (int j = 0; j < img.Height; j++)
            {
                for (int i = 0; i < img.Width; i++)
                {
                    Color c = bmp.GetPixel(i, j);
                    res[i, j] = (1.0f - c.GetBrightness());
                }
            }
            return res;
        }
        
        private WangTileSet tileSet;

    }
}

