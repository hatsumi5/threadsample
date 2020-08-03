using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ThreadSample
{
    public partial class Form : System.Windows.Forms.Form
    {
        private bool run;
        private bool shooting;
        private int mov;
        private int col;
        private int blockSize = 50;
        private int blockQtdCol;
        private Thread threadMain;
        private Thread threadBlocks;
        private List<Block> blocks = new List<Block>();
        private List<Block> blocksNears = new List<Block>();
        private bool makeBlocks;
        private Block currentBlock;

        public Form()
        {
            InitializeComponent();
        }

        private void btnPlayStop_Click(object sender, EventArgs e)
        {
            run = !run;
            if (run) Start();
            else Stop();
        }

        private void UpdatePaint(int width, int height)
        {
            while (run)
            {
                var bitmap = new Bitmap(width, height);
                var refresh = (Action)(() =>
                {
                    if (InvokeRequired)
                    {
                        try
                        {
                            Invoke((Action)(() =>
                            {
                                pictureBox.Image = bitmap;
                            }));
                        }
                        catch { }
                    }
                });
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    if (currentBlock == null)
                    {
                        currentBlock = new Block(blockSize, 0, mov, 0);
                    }
                    else
                    {
                        currentBlock.Move(mov);
                    }
                    currentBlock.BrushBlock(graphics);
                    refresh();

                    if (shooting)
                    {
                        var neighbors = currentBlock.Shooting(blocksNears, ref shooting);
                        currentBlock.BrushBlock(graphics);
                        if (!shooting)
                        {
                            blocks.RemoveAll(b => neighbors.Contains(b));
                            blocksNears.RemoveAll(b => neighbors.Contains(b));
                            blocks.ForEach(b => b.BlockMoveLeft());
                            makeBlocks = true;
                            currentBlock = null;
                        }
                        refresh();

                    }

                    if (makeBlocks) continue;
                    foreach (var block in blocks)
                    {
                        block.BrushBlock(graphics);
                    }
                    refresh();
                }
            }
        }

        private void MakeBlocks(int width, int height)
        {
            while (run)
            {
                if (!makeBlocks || shooting) continue;
                col++;
                var blocksNears = new List<Block>();
                for (int i = 0; i < blockQtdCol; i++)
                {
                    var block = new Block(blockSize, width - blockSize, i, col);
                    if (i > 0)
                    {
                        var prior = blocks.Last();
                        block.Neighbors.Add(prior);
                        prior.Neighbors.Add(block);
                    }
                    if (this.blocksNears.Any())
                    {
                        var prior = this.blocksNears.Single(b => b.NeighborY(block));
                        block.Neighbors.Add(prior);
                        prior.Neighbors.Add(block);
                    }
                    blocks.Add(block);
                    blocksNears.Add(block);
                }
                this.blocksNears = blocksNears;
                makeBlocks = false;
            }
        }

        private void Start()
        {
            ActiveControl = null;


            var width = pictureBox.ClientSize.Width;
            var height = pictureBox.ClientSize.Height;
            blockQtdCol = height / blockSize;
            makeBlocks = true;
            shooting = false;
            currentBlock = null;

            btnPlayStop.Text = "Stop";
            threadMain = threadMain ?? new Thread(() => UpdatePaint(width, height));
            threadMain.Priority = ThreadPriority.BelowNormal;
            threadMain.IsBackground = true;
            threadMain.Start();

            threadBlocks = threadBlocks ?? new Thread(() => MakeBlocks(width, height));
            threadBlocks.Priority = ThreadPriority.BelowNormal;
            threadBlocks.IsBackground = true;
            threadBlocks.Start();
        }

        private void Stop()
        {
            btnPlayStop.Text = "Play";
            threadMain = null;
            threadBlocks = null;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!run) return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                //case Keys.Return:
                //    break;
                //case Keys.Enter:
                //    break;
                case Keys.Space:
                    shooting = true;
                    break;
                //case Keys.Left:
                //    break;
                case Keys.Up:
                    if (mov >= 1) mov--;
                    break;
                //case Keys.Right:
                //    break;
                case Keys.Down:
                    if (mov < blockQtdCol - 1) mov++;
                    break;
                default:
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
