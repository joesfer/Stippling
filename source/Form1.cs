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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PoissonDist;
using System.Drawing.Imaging;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace BlueNoise
{
    public partial class StipplingForm : Form
    {
        #region Interesting methods

        private void GenerateWangTiles()
        {
            if (generatingTiles)
            {
                requestedAbort = true;
                return;
            }

            splitContainer2.Panel1.Enabled = false;

            generatingTiles = true;
            requestedAbort = false;
            wtGenerate.Text = "Cancel";

            if (wtDestFilePath.Text.Length == 0)
            {
                MessageBox.Show("Please select a valid destination file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            wtProgress.Value = 0;
            wtConsole.Items.Clear();
            debugSteps = new Queue<Image>();

            int samplesPerTile = Int32.Parse(wtSamplesPerTile.Text);
            int numColors = (int)wtEdgeColors.Value;

            if (WangTileSet.OnProgressReport == null) WangTileSet.OnProgressReport += new BlueNoise.WangTileSet.ProgressReportDelegate(OnWTProgress);
            if (WangTileSet.OnConsoleMessage == null) WangTileSet.OnConsoleMessage += new BlueNoise.WangTileSet.ConsoleDelegate(OnWTConsoleMessage);

            int numThreads = 8; // TODO: proper task-based system instead of limiting the worker thread count
            if (wtDebugSteps.Checked)
            {
                // restrict to single-threaded
                numThreads = 1;

                wtDebugPlayStop.Enabled = true;

                // Setup debug callbacks
                if (WangTile.OnDebugStep == null) WangTile.OnDebugStep += new BlueNoise.WangTile.WangTileDebugStepDelegate(OnWangTileDebugStep);
                if (WangTileSet.OnDebugStep == null) WangTileSet.OnDebugStep += new WangTileSet.WangTileDebugStepDelegate(OnWangTileDebugStep);
            }
            else
            {
                WangTile.OnDebugStep -= OnWangTileDebugStep;
                WangTileSet.OnDebugStep -= OnWangTileDebugStep;
            }

            WangTileSet wangTileSet = new WangTileSet();
            Thread t = new Thread(GenerateWangTilesThreadStart);
            progress = new AutoResetEvent(false);
            WaitHandle[] waitHandles = { progress };
            generateWangTilesThreadParams_t threadParams = new generateWangTilesThreadParams_t(wangTileSet, numColors, samplesPerTile, numThreads);
            t.Start(threadParams);
            bool aborted = false;
            do
            {
                aborted = this.Visible == false || requestedAbort;
                if (aborted)
                {
                    t.Abort();
                    break;
                }
                bool receivedProgress = WaitHandle.WaitAll(waitHandles, 100);
                if (receivedProgress)
                {
                    // progress report
                    ProcessConsoleMessages();
                    Image img = GetDebugImage();
                    if (img != null) wtDebugImg.Image = img;
                    wtProgress.Value = wtProgress.Minimum + (int)((wtProgress.Maximum - wtProgress.Minimum) * lastProgressReport);
                    if (stopDebug) Monitor.Enter(debugStepLocker);
                }
                Application.DoEvents(); // prevent the process from becoming unresponsive for too long and freeze
            } while (t.ThreadState != System.Threading.ThreadState.Stopped);

            if (aborted)
            {
                wtProgress.Value = 0;
                wtConsole.Items.Clear();
                debugSteps = new Queue<Image>();
                if (stopDebug) Monitor.Exit(debugStepLocker);
            }
            else
            {
                // flush pending console messages and debug images
                ProcessConsoleMessages();
                Image img;
                do
                {
                    img = GetDebugImage();
                    if (img != null) wtDebugImg.Image = img;
                } while (img != null);

                // Save results
                OnWTConsoleMessage("Saving results to " + wtDestFilePath.Text); ProcessConsoleMessages();
                wangTileSet.Serialize(wtDestFilePath.Text);
                OnWTConsoleMessage("Finished!"); ProcessConsoleMessages();
            }

            // restore UI status
            wtGenerate.Text = "Generate Tiles";
            generatingTiles = false;
            requestedAbort = false;
            splitContainer2.Panel1.Enabled = true;
            wtDebugPlayStop.Enabled = false;
            wtDebugStepOnce.Enabled = false;
            if (stopDebug)
            {
                stopDebug = false;
            }
        }
        private void GenerateWangTilesThreadStart(object obj)
        {
            generateWangTilesThreadParams_t p = (generateWangTilesThreadParams_t)obj;
            p.wangTileSet.Generate(p.numColors, p.samplesPerTile, p.numThreads);
        }
        private struct generateStipplingParams_t
        {
            public Image source;
            public string wangTilesPath;
            public float toneScaleBias;
            public Bitmap result;
        }
        generateStipplingParams_t stippleParams;
        private void GenerateStippling( object obj )
        {
            generateStipplingParams_t param = (generateStipplingParams_t)obj;
            if (param.wangTilesPath.Length > 0)
            {
                // Stipple using Wang Tiles

                WangTileSet wts = WangTileSet.FromFile(param.wangTilesPath);
                int tonalRange = (int)Math.Pow(10, (int)(param.toneScaleBias * 6)) + 10000; // this is merely a heuristic trying to produce sensible values for the tonalRange variable from a [0,1] source range
                WTStipple stipple = new WTStipple(param.source, wts, tonalRange, param.result);
            }
            else
            {
                // Stipple using Adaptive Incremental algorithm
                if (AIS.OnAISDebugStep == null) AIS.OnAISDebugStep += OnAISDebugStep;
                int K = (int)((1.0f - param.toneScaleBias) * 2048 + 64); // this is merely a heuristic trying to produce sensible values for the K variable from a [0,1] source range
                AIS.Stipple(param.source, K, param.result);
            }
            finishedStippling.Set();            
        }
        #endregion

        #region UI handling etc

        public StipplingForm()
        {
            InitializeComponent();
            
            // try setting a default wang tiles file
            if (File.Exists("wangTileSet_1024spt_3cols.xml"))
            {
                textBoxWangTilesPath.Text = "wangTileSet_1024spt_3cols.xml";
            }
            debugStepLocker = new object();
            consoleMessages = new Queue<string>();
        }

        /// <summary>
        /// Auxiliary struct used to pass in parameters to the thread performing
        /// the heavy calculation of generating the Wang Tiles
        /// </summary>
        private struct generateWangTilesThreadParams_t
        {
            public WangTileSet wangTileSet;
            public int numColors, samplesPerTile, numThreads;
            public generateWangTilesThreadParams_t(WangTileSet wangTileSet, int numColors, int samplesPerTile, int numThreads)
            {
                this.wangTileSet = wangTileSet;
                this.numColors = numColors;
                this.samplesPerTile = samplesPerTile;
                this.numThreads = numThreads;
            }
        }

        #region debug steps callbacks
        private Queue<Image> debugSteps;
        private Queue<string> consoleMessages;
        private float lastProgressReport;
        /// <summary>
        /// wait signal used to report when a debug callback has queued a new
        /// image on the debugSteps queue. The main thread will be monitoring
        /// this object and update the UI when appropriate. This way we ensure
        /// only one thread accesses the UI controls
        /// </summary>
        private AutoResetEvent progress;
        /// <summary>
        /// used to ensure single-thread access to the Debug Updates queue
        /// </summary>
        private object debugStepLocker;
        internal void QueueDebugImage(Image img)
        {
            Monitor.Enter(debugStepLocker);
            debugSteps.Enqueue((Image)img.Clone()); // we clone the image so that the thread can scratch it right after leaving the function
            progress.Set(); // signal progress
            Monitor.Exit(debugStepLocker);
        }
        internal Image GetDebugImage()
        {
            if (!Monitor.TryEnter(debugStepLocker)) return null;
            Image res = null;
            if (debugSteps.Count > 0)
            {
                res = debugSteps.Dequeue();
            }
            Monitor.Exit(debugStepLocker);
            return res;
        }
        private void OnWTConsoleMessage(string msg)
        {
            Monitor.Enter(debugStepLocker);
            consoleMessages.Enqueue(msg);
            progress.Set();
            Monitor.Exit(debugStepLocker);
        }
        private void ProcessConsoleMessages()
        {
            if (!Monitor.TryEnter(debugStepLocker)) return;
            while (consoleMessages.Count > 0)
            {
                wtConsole.Items.Add(consoleMessages.Dequeue());
                wtConsole.SelectedIndex = wtConsole.Items.Count - 1;
            }
            Monitor.Exit(debugStepLocker);
        }
        private void OnWTProgress(float value)
        {
            Monitor.Enter(debugStepLocker);            
            lastProgressReport = value;
            progress.Set();
            Monitor.Exit(debugStepLocker);
        }
        internal void OnDelaunayTriangleCheck(Adjacency adjacency, int triangle)
        {
            Bitmap img = new Bitmap(800,800);
            Random random = new Random();
            float scale = 1000.0f;
            for (int j = 0; j < adjacency.vertices.Count; j++)
            {
                adjacency.vertices[j].x /= scale;
                adjacency.vertices[j].y /= scale;
            }
            adjacency.ToImage(img);

            Graphics g = Graphics.FromImage(img);
            PointF[] points = new PointF[3];
            for (int i = 0; i < 3; i++)
            {
                Vertex v = adjacency.vertices[adjacency.triangles[triangle].vertices[i]];
                points[i] = new PointF(v.x * img.Width, v.y * img.Height);
            }

            g.FillPolygon(new SolidBrush(Color.FromArgb(random.Next(255), random.Next(255), random.Next(255))), points);


            for (int j = 0; j < adjacency.vertices.Count; j++)
            {
                adjacency.vertices[j].x *= scale;
                adjacency.vertices[j].y *= scale;
            }

            QueueDebugImage(img);
        }
        internal void OnDelaunayStep(Adjacency adjacency)
        {
            Bitmap bmp = new Bitmap(800, 800);
            float scale = 1000.0f;
            for (int j = 0; j < adjacency.vertices.Count; j++)
            {
                adjacency.vertices[j].x /= scale;
                adjacency.vertices[j].y /= scale;
            }
            adjacency.ToImage(bmp);
            for (int j = 0; j < adjacency.vertices.Count; j++)
            {
                adjacency.vertices[j].x *= scale;
                adjacency.vertices[j].y *= scale;
            }
            QueueDebugImage(bmp);
        }
        internal void OnWangTileDebugStep(Image img, string description)
        {
            QueueDebugImage(img);
            OnWTConsoleMessage(description);
        }
        private void wtDebugPlayStop_Click(object sender, EventArgs e)
        {
            stopDebug = !stopDebug;
            if (stopDebug)
            {
                Monitor.Enter(debugStepLocker);
            }
            else
            {
                Monitor.Exit(debugStepLocker);
            }
            wtDebugStepOnce.Enabled = stopDebug;
        }

        private void wtDebugStepOnce_Click(object sender, EventArgs e)
        {
            // note we're not changing the stopDebug flag
            Monitor.Exit(debugStepLocker);
        }

        internal void OnAISDebugStep(Image img)
        {
            if (stippleParams.result == null) return;
            pictureBoxResult.Image = (Bitmap)stippleParams.result.Clone();
        }
 
        #endregion

        AutoResetEvent finishedStippling = null;
        private void buttonGO_Click(object sender, EventArgs e)
        {
            if (stipplingThread != null)
            {
                stipplingThread.Abort();
                stipplingThread = null;
                buttonGO.Text = "GO!";
                finishedStippling.Set();
            }
            else
            {
                if (pictureBoxSource.Image == null)
                {
                    MessageBox.Show("Please browse for a valid source image", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (radioButtonWangTiles.Checked && textBoxWangTilesPath.Text.Length == 0)
                {
                    MessageBox.Show("Please select a valid Wang Tiles source file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                stippleParams.source = pictureBoxSource.Image;
                stippleParams.toneScaleBias = (float)(toneScale.Value - toneScale.Minimum) / (toneScale.Maximum - toneScale.Minimum);
                stippleParams.wangTilesPath = radioButtonWangTiles.Checked ? textBoxWangTilesPath.Text : "";
                stippleParams.result = new Bitmap(stippleParams.source.Width, stippleParams.source.Height);
                buttonGO.Text = "Cancel";
                finishedStippling = new AutoResetEvent(false);

                
                stipplingThread = new Thread(GenerateStippling);
                stipplingThread.Start(stippleParams);
                WaitHandle[] handles = {finishedStippling};
                while (!WaitHandle.WaitAll(handles, 500))
                {
                    Application.DoEvents(); // avoid freezing for too long
                }
                pictureBoxResult.Image = (Bitmap)stippleParams.result.Clone();
                buttonGO.Text = "GO!";
                stipplingThread = null;
            }
        }
       
        private void browseSourceButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images (*.jpg)|*.jpg";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                sourceImagePath.Text = ofd.FileName;
                this.pictureBoxSource.Image = new Bitmap(ofd.FileName);
            }
        }

        private void saveImgButton_Click(object sender, EventArgs e)
        {
            if (pictureBoxResult.Image == null) return;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Images (*.png)|*.png";
            sfd.AddExtension = true;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // stippled images look better when upsampled, so dots look rounder
                Image srcImage = pictureBoxResult.Image;
                Bitmap result = new Bitmap(srcImage.Width * 4, srcImage.Height * 4);
                Graphics g = Graphics.FromImage(result);
                g.DrawImage(srcImage, new Rectangle(0, 0, result.Width, result.Height));
                result.Save(sfd.FileName);
            }
        }

        private void radioButtonWangTiles_CheckedChanged(object sender, EventArgs e)
        {
            labelTilesFile.Enabled = radioButtonWangTiles.Checked;
            textBoxWangTilesPath.Enabled = radioButtonWangTiles.Checked;
            buttonBrowseTiles.Enabled = radioButtonWangTiles.Checked;
        }

        private void buttonBrowseTiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML Files (*.xml)|*.xml";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBoxWangTilesPath.Text = ofd.FileName;
            }
        }

        private void wtBrowseDestFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML Files (*.xml)|*.xml";
            sfd.AddExtension = true;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                wtDestFilePath.Text = sfd.FileName;
            }
        }

        private void wtGenerate_Click(object sender, EventArgs e)
        {
            GenerateWangTiles();
        }

        private bool stopDebug = false;
        private bool generatingTiles = false;
        private bool requestedAbort = false;
        private Thread stipplingThread = null;

        #endregion
    }
}
