using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ThreadSample
{
    internal sealed class Block
    {
        private int blockSize;
        private int col;
        private int x;
        private int y;
        private Rectangle rectangle;
        private Brush brush;

        public Block(int blockSize, int x, int y, int col)
        {
            this.blockSize = blockSize;
            this.x = x;
            this.y = y;
            this.col = col;
            rectangle = new Rectangle(x, y * blockSize, blockSize, blockSize);
            SetBrush();
        }

        public List<Block> Neighbors { get; private set; } = new List<Block>();

        private void SetBrush()
        {
            var random = new Random().Next(1, 5);
            brush = Brushes.LightGray;
            switch (random)
            {
                case 1: brush = Brushes.DarkGray; break;
                case 2: brush = Brushes.Red; break;
                case 3: brush = Brushes.Blue; break;
                case 4: brush = Brushes.Green; break;
                case 5: brush = Brushes.AliceBlue; break;
                default:
                    break;
            }
        }

        internal void BrushBlock(Graphics graphics)
        {
            graphics.FillEllipse(brush, rectangle);
        }

        internal void BlockMoveLeft()
        {
            rectangle.X -= blockSize;
        }

        internal IEnumerable<Block> Shooting(List<Block> blocks, ref bool shooting)
        {
            rectangle.X += 20;
            var block = blocks.Single(b => b.rectangle.Y == rectangle.Y);
            if (rectangle.X > block.rectangle.X - blockSize)
            {
                rectangle.X = block.rectangle.X - blockSize;
                Neighbors.Add(block);
                shooting = false;
            }
            return SameColors(block);
        }

        internal void Move(int y)
        {
            rectangle.Y = y * blockSize;
        }

        internal bool NeighborY(Block block)
        {
            return block.rectangle.Y == rectangle.Y;
        }

        private IEnumerable<Block> SameColors(Block block)
        {
            if (!block.brush.Equals(brush)) return Enumerable.Empty<Block>();

            var collection = new List<Block>();
            collection.Add(block);
            foreach (var neighbor in block.Neighbors)
            {
                if (collection.Contains(neighbor)) continue;
                if (!block.brush.Equals(brush)) continue;
                var neighbors = SameColors(neighbor);
                if (neighbors.Any())
                {
                    neighbors.Where(n => !collection.Contains(n)).ToList().ForEach((n) => collection.Add(n));
                }
            }

            return collection;
        }
    }
}