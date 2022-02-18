using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace Canvas
{
    public partial class Canvas : Form
    {
        private readonly IGrid _grid;
        private readonly RouteFinder _routeFinder;
        private bool _dirty;

        public Canvas(IGrid grid, RouteFinder routeFinder)
        {
            InitializeComponent();
            _grid = grid;
            _routeFinder = routeFinder;
            _dirty = true;
        }

        private void CreateGrid()
        {
            _dirty = true;
            Invalidate();

            var nodeCounts = _routeFinder.SelectNodeCount();
            _grid.SetNodeCounts(NodeCountHelper.MapNodeCounts(nodeCounts));
        }

        private void SaveGrid()
        {
            var canvas = this;
            var bitmap = new Bitmap(canvas.Width, canvas.Height, PixelFormat.Format32bppArgb);
            var gx = Graphics.FromImage(bitmap);

            gx.Clear(Color.White);
            PaintIt(gx);

            bitmap.Save($@"C:\Users\AndrewSmith\Documents\Fluviatile.png", ImageFormat.Png);
        }

        private void PaintIt(Graphics gx)
        {
            var scale = 80.0f;
            var translate = (x: 6.0f, y: 2.0f);
            var xfactor = 0.5f;
            var yfactor = 0.5f * (float)Math.Sqrt(3);

            var textBrush = new SolidBrush(Color.White);
            var font = new Font(DefaultFont, FontStyle.Bold);

            (float x, float y) Transform((float x, float y) point) =>
                (scale * (translate.x + point.x - xfactor * point.y), scale * (translate.y + point.y * yfactor));

            //var (xt, yt) = Transform((-3.3f, -0.6f));
            //gx.DrawString(_grid.DisplayText(), DefaultFont, textBrush, xt, yt);

            var pen = new Pen(Color.DarkSlateGray, 2);  // draw the line 
            var marginPen = new Pen(Color.White, 2);
            var brush = new SolidBrush(Color.Gray);

            foreach (var item in _grid.GetMargins())
            {
                gx.FillPolygon(brush, item
                    .Select(p => Transform(p))
                    .Select(p => new PointF(p.x, p.y))
                    .ToArray());
            }

            foreach (var (from, to) in _grid.MarginLines())
            {
                var (x1, y1) = Transform(from);
                var (x2, y2) = Transform(to);
                gx.DrawLine(marginPen, x1, y1, x2, y2);
            }

            foreach (var (from, to) in _grid.GridLines())
            {
                var (x1, y1) = Transform(from);
                var (x2, y2) = Transform(to);
                gx.DrawLine(pen, x1, y1, x2, y2);
            }

            foreach (var (x, y, count) in _grid.NodeCounts())
            {
                var text = count.ToString();
                var textSize = TextRenderer.MeasureText(text, font);

                var (xp1, yp1) = Transform((x, y));
                gx.DrawString(count.ToString(), font, textBrush, xp1 - 0.5f * textSize.Width, yp1 - 0.5f * textSize.Height);
            }

            _dirty = false;
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            if (_dirty)
            {
                PaintIt(CreateGraphics());
            }
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            CreateGrid();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveGrid();
        }
    }
}
