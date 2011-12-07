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
    /// <summary>
    /// The AIS class is an implementation of the method described in the paper
    /// "Adaptive Incremental Stippling using the Poisson-Disk Distribution"
    /// Ignacio Ascencio-Lopez and Oscar Meruvia-Pastor and Hugo Hidalgo-Silva
    /// 
    /// http://jgt.akpeters.com/papers/Ascencio-Lopez10/
    /// </summary>
    class AIS
    {
        /// <summary>
        /// Main method: builds a stippled representation from a source image
        /// </summary>
        /// <param name="source">Source image</param>
        /// <param name="K">Meassure of darkness on the result. The lower the value, the darker the image. Sensible range around 256 to 1024</param>
        /// <param name="result">Resulting stippled image</param>
        public static void Stipple(Image source, int K, Bitmap result )
        {
            Graphics g = Graphics.FromImage(result);
            g.Clear(Color.White);

            Random r = new Random();
            Queue<Disk> q = new Queue<Disk>();
            Bitmap diskBuffer = new Bitmap(source.Width, source.Height);
            Graphics dgb = Graphics.FromImage(diskBuffer);
            dgb.Clear(Color.Black);

            Bitmap sourceBmp = new Bitmap(source.Width, source.Height);
            {
                Graphics srcg = Graphics.FromImage(sourceBmp);
                srcg.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height));
            }

            int totalDensity = TotalDensity(sourceBmp);
            float r1 = (float)Math.Sqrt((float)(result.Width * result.Height) / (totalDensity / K));
            {
                Disk Q = new Disk(new PointF((float)r.Next(result.Width), (float)r.Next(result.Height)), r1);
                q.Enqueue(Q);
                AddStipple(Q, result);
                DrawDisk(Q, dgb);
            }
            int steps = 0;
            while (q.Count > 0)
            {
                steps++;
                if (steps == 100)
                {
                   // Graphics g2 = Graphics.FromImage(result);
                   // g2.DrawImage(diskBuffer, new Rectangle(0, 0, result.Width, result.Height));
                    if (OnAISDebugStep != null) OnAISDebugStep(result);
                    steps = 0;
                }
                Disk Q = q.Dequeue();
                int attempts = 0;
                while (Q.HasAvailableRange && attempts < 5) 
                {
                    attempts++; // avoid getting stuck 
                    float alpha = Q.AvailableAngle();
                    Disk P = new Disk(new PointF(Q.center.X + (Q.radius + r1) * (float)Math.Cos(alpha), Q.center.Y + (Q.radius + r1) * (float)Math.Sin(alpha)), r1);
                    Disk D;
                    bool insideImage = P.center.X >= 0 && P.center.X < sourceBmp.Width && P.center.Y >= 0 && P.center.Y < sourceBmp.Height;
                    if ( insideImage && 
                        ChangeRadiusBasedOnLocalDensity(Q, P, alpha, sourceBmp, K, out D) && 
                        !Overlaps(D, diskBuffer))
                    {
                        q.Enqueue(D);
                        AddStipple(D, result);
                        DrawDisk(D, dgb);
                        Q.SubstractFromAvailableRange(D);
                    }
                }
            }
        }

        /// <summary>
        /// Returns whether a disk overlaps any pixel on the provided disk buffer.
        /// The test is performed in image space (and therefore not very accurate)
        /// </summary>
        private static bool Overlaps(Disk P, Bitmap diskBuffer)
        {
            if ( float.IsInfinity(P.radius)) return true;

            for(int j = Math.Max(0,(int)(P.center.Y - P.radius)); j < Math.Min(diskBuffer.Height, (int)Math.Ceiling(P.center.Y + P.radius)); j++)
            {
                for (int i = Math.Max(0,(int)(P.center.X - P.radius)); i < Math.Min(diskBuffer.Width,(int)Math.Ceiling(P.center.X + P.radius)); i++)
                {
                    float r2 = (P.center.X - i) * (P.center.X - i) + (P.center.Y - j) * (P.center.Y - j);
                    if (r2 > P.radius) continue;
                    if (diskBuffer.GetPixel(i, j).R > 0) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Rasterizes a disk into the disk buffer. This buffer is used to check
        /// for disk overlapping. 
        /// </summary>
        private static void DrawDisk(Disk d, Graphics dgb)
        {
            dgb.FillEllipse(whiteBrush, d.center.X - d.radius, d.center.Y - d.radius, (float)(2.0f * d.radius), (float)(2.0f * d.radius));
        }

        private static void AddStipple(Disk d, Bitmap bmp)
        {
            if (d.center.X < 0 || d.center.Y < 0 || d.center.X >= bmp.Width || d.center.Y >= bmp.Height) return;
            bmp.SetPixel((int)d.center.X, (int)d.center.Y, Color.Black);
        }

        /// <summary>
        /// Given a disk P sampled around Q's periphery, representing a Poisson 
        /// sample over a density area on the source image, adjust its radius to
        /// produce a second disk D based on the local intensity of the image 
        /// around its center.
        /// Darker pixels on the source image will produce smaller disk radius,
        /// leading to higher density of samples in the area. On the other hand,
        /// lighter regions will space the samples further apart by increasing
        /// their radius.
        /// </summary>
        /// <param name="Q">Previous disk, defining the boundary along which P 
        /// was generated</param>
        /// <param name="P">Disk to adjust</param>
        /// <param name="alpha">Angle of P's center respect of Q's</param>
        /// <param name="bmp">Source image to sample density from</param>
        /// <param name="K">Meassure of darkness</param>
        /// <param name="D">Resulting disk</param>
        private static bool ChangeRadiusBasedOnLocalDensity(Disk Q, Disk P, float alpha, Bitmap bmp, float K, out Disk D)
        {
            float diskDensity = DiskDensity(P, bmp);
            if (diskDensity == 0)
            {
                D = P;
                return false;
            }
            // Iterativelly adjust P's radius until its total density converges to the
            // desired value K
            float ratioR = (float)Math.Sqrt(K / diskDensity);
            const float epsilon = 0.01f;
            int i = 0;
            while (Math.Abs(ratioR - 1.0f) > epsilon && i < 6)
            {
                float rn = P.radius * ratioR;
                P = new Disk(new PointF(Q.center.X + (Q.radius + rn) * (float)Math.Cos(alpha), Q.center.Y + (Q.radius + rn) * (float)Math.Sin(alpha)), rn);
                diskDensity = DiskDensity(P, bmp);
                ratioR = (float)Math.Sqrt(K / diskDensity);
                i++;
            }

            D = P;
            return true;
        }

        /// <summary>
        /// Sample density within a disk
        /// </summary>
        private static float DiskDensity(Disk d, Bitmap bmp)
        {
            float diskDensity = 0.0f;
            float radius = Math.Max(0.5f, d.radius);
            float squareArea;
            if (radius == 0.5f)
            {
                diskDensity = (1.0f - bmp.GetPixel((int)Math.Max(0, Math.Min(bmp.Width - 1, d.center.X)), Math.Max(0, Math.Min(bmp.Height - 1, (int)d.center.Y))).GetBrightness());
                squareArea = 1.0f;
            }
            else
            {
                for (int j = Math.Max(0, (int)(d.center.Y - radius)); j <= Math.Min(bmp.Height - 1, (int)(d.center.Y + radius)); j++)
                {
                    for (int i = Math.Max(0, (int)(d.center.X - radius)); i <= Math.Min(bmp.Width - 1, (int)(d.center.X + radius)); i++)
                    {
                        //float r2 = (d.center.X - i) * (d.center.X - i) + (d.center.Y - j) * (d.center.Y - j);
                        //if (r2 > d.radius * d.radius) continue;
                        diskDensity += 1.0f - bmp.GetPixel(i, j).GetBrightness();
                    }
                }
                squareArea = (Math.Min(bmp.Height, (int)(d.center.Y + radius)) - Math.Max(0, (int)(d.center.Y - radius)) + 1) *
                     (Math.Min(bmp.Width, (int)(d.center.X + radius)) - Math.Max(0, (int)(d.center.X - radius)) + 1);
            }

            float diskArea = d.radius * d.radius * (float)Math.PI;
#if USE_SQUARE_AREA
            // despite the square and disk areas should behave similarly for the heuristic, I find 
            // the later to converge more reliably, producing less artifacts (disks diverging to incorrect sizes)
            return diskDensity * 255;
#else
            // use disk area
            return diskDensity * diskArea / squareArea * 255;
#endif
        }

        /// <summary>
        /// Total density on the image
        /// </summary>
        private static int TotalDensity(Bitmap bmp)
        {
            int totalDensity = 0;
            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
                {
                    Color c = bmp.GetPixel(i, j);
                    totalDensity += (int)((1.0f - c.GetBrightness()) * 255);
                }
            }
            return totalDensity;
        }

        /// <summary>
        /// Auxiliary class representing disks, used to define Poisson samples and
        /// their boundaries.
        /// </summary>
        private class Disk
        {
            public Disk(PointF p, float r)
            {
                center = p; radius = r;
                availableRange = new List<Arc>();
                availableRange.Add(new Arc(0, 2.0f * (float)Math.PI));
            }
            public PointF center;
            public float radius;
            
            /// <summary>
            /// Auxiliary class used to define regions on the boundary from which
            /// to generate new disk centers.
            /// </summary>
            public class Arc
            {
                public Arc(float f, float t)
                {
                    Debug.Assert(f < t);
                    from = f; to = t;
                }
                public float from, to;
            }
           
            public bool HasAvailableRange { get { return availableRange.Count > 0; } }
            private List<Arc> availableRange;


            private Random random = new Random();
            internal float AvailableAngle()
            {
                Arc a = availableRange[random.Next(availableRange.Count)];
                return (float)random.NextDouble() * (a.to - a.from) + a.from;
            }

            internal void SubstractFromAvailableRange(Disk D)
            {
                float dist = (float)Math.Sqrt((D.center.X - center.X) * (D.center.X - center.X) + (D.center.Y - center.Y) * (D.center.Y - center.Y));

                float cosBeta = Math.Max(-1.0f, Math.Min(1.0f, (this.radius + D.radius) * 0.5f / (2.0f * D.radius)));
                float beta = (float)Math.Acos(cosBeta);
                if (beta < 1e-3f) return;
                float dx = D.center.X - center.X;
                float dY = D.center.Y - center.Y;
                float l = (float)Math.Sqrt(dx * dx + dY * dY);
                dx /= l;
                dY /= l;
                float alpha = (float)Math.Atan2(dY, dx);
                const float TwoPi = (float)(2.0f * Math.PI);
                if (alpha < 0) alpha += TwoPi;

                List<Arc> clippers = new List<Arc>();
                float from = alpha - beta;
                float to = alpha + beta;
                if (from >= 0 && to <= TwoPi)
                {
                    clippers.Add(new Arc(from, to));
                }
                else
                {
                    // clipping arc crosses 0 degrees. Split in two
                    if (from < 0)
                    {
                        Debug.Assert(to >= 0);
                        if ( to > 0 ) clippers.Add(new Arc(0, to));
                        clippers.Add(new Arc(from + TwoPi, TwoPi));
                    }
                    if (to > TwoPi)
                    {
                        Debug.Assert(from <= TwoPi);
                        if (from < TwoPi) clippers.Add(new Arc(from, TwoPi));
                        clippers.Add(new Arc(0, to - TwoPi));
                    }
                }
                for (int j = 0; j < clippers.Count; j++)
                {
                    Arc clipper = clippers[j];
                    List<Arc> remaining = new List<Arc>();
                    for (int i = 0; i < availableRange.Count; i++)
                    {
                        Arc arc = availableRange[i];
                        if (arc.from >= clipper.from && arc.to <= clipper.to)
                            // completely culled
                            continue;
                        else if (arc.to < clipper.from || arc.from > clipper.to)
                            // untouched
                            remaining.Add(arc);
                        else if (arc.from <= clipper.from && arc.to >= clipper.from && arc.to <= clipper.to)
                        {
                            float f = arc.from;
                            float t = clipper.from;
                            remaining.Add(new Arc(f, t));
                        }
                        else if (arc.from >= clipper.from && arc.from <= clipper.to && arc.to >= clipper.to)
                        {
                            float f = clipper.to;
                            float t = arc.to;
                            remaining.Add(new Arc(f, t));
                        }
                        else
                        {
                            // split in two
                            float f0 = arc.from;
                            float t0 = clipper.from;
                            remaining.Add(new Arc(f0, t0));
                            float f1 = clipper.to;
                            float t1 = arc.to;
                            remaining.Add(new Arc(f1, t1));
                        }
                    }
                    availableRange = remaining;
                }
            }
        }

        private static Brush whiteBrush = new SolidBrush(Color.White);

        #region Debug callback
        public delegate void AISDebugDelegate(Image img);
        public static AISDebugDelegate OnAISDebugStep = null;
        #endregion
    }
}
