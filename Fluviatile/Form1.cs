using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Canvas : Form
    {
        private Random _random;
        private Grid _grid;
        private Bitmap _memoryImage;

        public Canvas(int? seed)
        {
            InitializeComponent();
            CreateGrid(seed);
        }

        private void CreateGrid(int? seed = null)
        {
            Invalidate();
            _grid = new Grid(8, 8, 45, new Pseudorandom(seed ?? Environment.TickCount));
        }

        private void SaveGrid()
        {
            var frm = this;
            var bitmap = new Bitmap(frm.Width, frm.Height, PixelFormat.Format32bppArgb);
            var gx = Graphics.FromImage(bitmap);

            gx.Clear(Color.White);
            PaintIt(gx);

            bitmap.Save(@"C:\Users\andrews\Documents\Grid.png", ImageFormat.Png);
        }

        private void PaintIt(Graphics gx)
        {
            var scale = 80.0f;
            var translate = (x: 6.0f, y: 1.0f);
            var xfactor = 0.5f;
            var yfactor = 0.5f * (float)Math.Sqrt(3);
            var textBrush = new SolidBrush(Color.DarkRed);

            (float x, float y) Transform((float x, float y) point) =>
                (scale * (translate.x + point.x - xfactor * point.y), scale * (translate.y + point.y * yfactor));

            var (xt, yt) = Transform((-2.5f, 0.4f));
            gx.DrawString(_grid.Seed.ToString(), DefaultFont, textBrush, xt, yt);

            var pen = new Pen(Color.DarkSlateGray, 2);  // draw the line 

            foreach (var (from, to) in _grid.GridLines())
            {
                var fromT = Transform(from);
                var toT = Transform(to);
                gx.DrawLine(pen, fromT.x, fromT.y, toT.x, toT.y);
            }

            foreach (var (y, count1, count2) in _grid.YCounts())
            {
                var (xp1, yp1) = Transform((-0.4f, y + 0.3f));
                gx.DrawString(count1.ToString(), DefaultFont, textBrush, xp1, yp1);

                var (xp2, yp2) = Transform((_grid.Width + 0.1f, y + 0.5f));
                gx.DrawString(count2.ToString(), DefaultFont, textBrush, xp2, yp2);
            }

            foreach (var (x, count1, count2) in _grid.XCounts())
            {
                var (xp1, yp1) = Transform((x + 0.3f, -0.4f));
                gx.DrawString(count1.ToString(), DefaultFont, textBrush, xp1, yp1);

                var (xp2, yp2) = Transform((x + 0.5f, _grid.Height + 0.1f));
                gx.DrawString(count2.ToString(), DefaultFont, textBrush, xp2, yp2);
            }
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            PaintIt(CreateGraphics());
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            CreateGrid();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveGrid();
        }
    }
}
