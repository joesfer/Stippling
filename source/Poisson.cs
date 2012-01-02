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
using System.Drawing;

namespace PoissonDist
{
    [Serializable]
    public class PoissonSample : IComparable
    {
        public PoissonSample() { }
        public PoissonSample(float _x, float _y, float rd, float rk) { x = _x; y = _y; radius = rd; ranking = rk;  }
        public float DistSquared(PoissonSample s) { return (x - s.x) * (x - s.x) + (y - s.y) * (y - s.y); }
        public float x, y;
        public float radius;
        public float ranking; // simply the sequence order
#if DEBUG
        public Color color;
#endif

        #region IComparable Members

        public int CompareTo(object obj) // implement IComparable to enable sort by ranking
        {
            int result = 1;
            if (obj != null && obj is PoissonSample)
            {
                PoissonSample p = obj as PoissonSample;
                result = this.ranking.CompareTo(p.ranking);
            }
            return result;
        }

        #endregion
    }

    [Serializable]
    public class PoissonDistribution
    {
        public PoissonDistribution() 
        { 
            UpdateFrequency = 0; 
            sampleAdded = null;
        }

        public void Generate(int desiredSamples, float initialRadius, int attemptsPerRadius, float radiusDecreaseFactor) 
        {
            ToroidalDartThrowing(desiredSamples, initialRadius, attemptsPerRadius, radiusDecreaseFactor);
        }
        
        public void Generate(int desiredSamples ) 
        { 
            ToroidalDartThrowing(desiredSamples, 0.15f, 1000, 0.99f); 
        }

        static public void ToImage(List<PoissonSample> samples, Color color, Image img, float coverage)
        {
            Graphics g = Graphics.FromImage(img);
            Brush bkgColor = new SolidBrush(Color.LightGray);
            Brush sampleColor = new SolidBrush(color);
            int r = Math.Max(1, (int)img.Width / 100);
            g.FillRectangle(bkgColor, new Rectangle(0, 0, img.Width, img.Height));
            for (int i = 0; i < (int)(samples.Count * coverage); i++)
            {
                g.FillEllipse(sampleColor, new Rectangle((int)( samples[i].x * img.Width )- r, (int)(samples[i].y * img.Height) - r, 2 * r, 2 * r));
            }
        }

        /// <summary>
        /// Toroidal Dart Throwing is when we apply the minimum-distance criterion also
        /// wrapping around on the edges of the plane, so that we can put the resulting
        /// distributions next to each other without clustering on the seams.
        /// </summary>
        private void ToroidalDartThrowing(int desiredSamples,
                           float initialRadius, // e.g. 0.15f
                           int attemptsPerRadius, // e.g. 1000
                           float radiusDecreaseFactor // e.g. 0.99f 
                          )
        {
            Random random = new Random();
            samples = new List<PoissonSample>();
            float radius = initialRadius;
            while (samples.Count < desiredSamples && radius > 0.0f)
            {
                int attemtps = 0;
                for (; attemtps < attemptsPerRadius; attemtps++)
                {
                    PoissonSample candidate = new PoissonSample((float)random.NextDouble(),
                                                  (float)random.NextDouble(),
                                                  radius,
                                                  (float)samples.Count / desiredSamples);
                    // As opposed to regular dart throwing, we plan to compose
                    // tiles made out of these Poisson distributions, meaning
                    // that a given distribution may fit with itself on the 
                    // opposite edges. The 'toroidal' bit is in charge of fixing
                    // this, by extending the distance checking wrapping around edges                    
                    
                    if (IsCandidateValid(candidate, radius))
                    {
                        samples.Add(candidate);
                        if (UpdateFrequency > 0 && samples.Count % UpdateFrequency == 0 && sampleAdded != null) sampleAdded(this, EventArgs.Empty);
                        break;
                    }
                }
                if (attemtps == attemptsPerRadius) radius *= radiusDecreaseFactor;
            }
        }

        private bool IsCandidateValid(PoissonSample c, float minDist )
        {
            float minDist2 = minDist * minDist;
            float cx = c.x;
            float cy = c.y;
            bool res = true;
            for (int i = 0; res && i < samples.Count; i++)
            {
                // regular dart throwing condition
                if (samples[i].DistSquared(c) < minDist2) res = false;

                // wrapped around edges
                c.x = cx + 1.0f;
                if (res && samples[i].DistSquared(c) < minDist2) res = false;
                c.x = cx - 1.0f;
                if (res && samples[i].DistSquared(c) < minDist2) res = false;
                c.x = cx;
                c.y = cy + 1.0f;
                if (res && samples[i].DistSquared(c) < minDist2) res = false;
                c.y = cy - 1.0f;
                if (res && samples[i].DistSquared(c) < minDist2) res = false;
                c.y = cy;
            }
            return res;
        }

        public delegate void SampleAddedHandler(object sender, EventArgs e);
        public SampleAddedHandler sampleAdded;

        public List<PoissonSample> Samples { get { return samples; } }
        public int UpdateFrequency;

        public List<PoissonSample> samples;
        
    }
}
