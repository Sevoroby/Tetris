using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Crossword
{
    public partial class Form1 : Form
    {
        public static bool isEnd { get; set; }
        public static bool[][] Field { get; set; }
        public static List<Figure> Figures { get; set; } = new List<Figure>();
        public static Color CrosswordColor { get; set; }
        public static int CellHeight { get; set; }
        public static int CellWidth { get; set; }
        public static int CellsCountWidth { get; set; } = 15;
        public static int CellsCountHeight { get; set; } = 10;
        public static int TaskDuration { get; set; } = 800;
        public static SolidBrush BorderTrub { get; set; } = new SolidBrush(Color.Black);
        public static SolidBrush BodyTrub { get; set; } = new SolidBrush(Color.LightSteelBlue);
        public static SolidBrush BackgroundTrub { get; set; } = new SolidBrush(Color.Silver);
        public static Dictionary<Color, string> ColorDict { get; set; }

        public Form1()
        {
            InitializeComponent();

            int width = pictureBox1.Size.Width;
            int height = pictureBox1.Size.Height;
            while (width % 10 != 0) width++;
            while (height % 10 != 0) height++;
            pictureBox1.Size = new Size(width, height);

            Cell.BorderWithBodySize = new Size(pictureBox1.Size.Width / CellsCountWidth, pictureBox1.Size.Height / CellsCountHeight);
            Cell.BorderWidth = 2;
            Cell.BodySize = new Size(Cell.BorderWithBodySize.Width - (Cell.BorderWidth * 2), Cell.BorderWithBodySize.Height - (Cell.BorderWidth * 2));

            

            ColorDict = new Dictionary<Color, string>();
            ColorDict.Add(Color.Yellow, "Yellow");
            ColorDict.Add(Color.White, "White");
            ColorDict.Add(Color.Red, "Red");
            ColorDict.Add(Color.Green, "Green");
            ColorDict.Add(Color.Blue, "Blue");

            var cbo = this.comboBox1;
            cbo.DataSource = new BindingSource(ColorDict, null);
            cbo.DisplayMember = "Value";
            cbo.ValueMember = "Key";
            cbo.DrawMode = DrawMode.OwnerDrawFixed;
        }

        Graphics g { get; set; }

        private void button1_Click(object sender, EventArgs e)
        {
            g = pictureBox1.CreateGraphics();
            g.Clear(BackgroundTrub.Color);
            button1.Visible = false;
            comboBox1.Visible = true;
            isEnd = false;
            Figures = new List<Figure>();
            Task.Run(() => this.Invoke(new Action(() =>
            {
                StartGame();
            })));
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            int index = e.Index >= 0 ? e.Index : 0;
            e.DrawBackground();
            Rectangle rectangle = new Rectangle(2, e.Bounds.Top + 2,
              e.Bounds.Height, e.Bounds.Height - 4);
            var item = (KeyValuePair<Color, string>)this.comboBox1.Items[e.Index];
            e.Graphics.FillRectangle(new SolidBrush(item.Key), rectangle);
            e.Graphics.DrawString(item.Value, new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold), System.Drawing.Brushes.Black, new Rectangle(e.Bounds.X + 25, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
            e.DrawFocusRectangle();
        }


        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            var item = (KeyValuePair<Color, string>)this.comboBox1.SelectedItem;
            BodyTrub.Color = item.Key;
            var figure = Figures.FirstOrDefault(l => l.isActive);
            if (figure != null)
                foreach (Cell c in figure.Cells)
                {
                    DrawCell(c);
                }
        }


        private async Task StartGame()
        {

            while (!isEnd)
            {
                await CreateFigures();
            }
            MessageBox.Show("Игра окончена!");
            button1.Visible = true;
        }

        private async Task CreateFigures()
        {
            TaskDuration = 800;
            comboBox1.SelectedItem = ColorDict.ElementAt(new Random().Next(0, ColorDict.Count()));
            BodyTrub.Color = ((KeyValuePair<Color, string>)comboBox1.SelectedItem).Key;
            var randCellCount = new Random().Next(1, 5);
            var randPosition = new Random().Next(1, pictureBox1.Width / Cell.BorderWithBodySize.Width - randCellCount+1) * Cell.BorderWithBodySize.Width;
            var figure = new Figure(randCellCount, randPosition);
            figure.LeftPosition = randPosition;
            figure.RightPosition = randPosition + (figure.Cells.Length * Cell.BorderWithBodySize.Width);
            figure.LowPosition += Cell.BorderWithBodySize.Height;
            figure.isActive = true;
            Figures.Add(figure);

            if (CheckFinal())
            {
                figure.isActive = false;
                return;
            }

            while (figure.LowPosition <= pictureBox1.Height)
            {
                foreach (Cell c in figure.Cells)
                {
                    DrawCell(c);
                }
                await Task.Delay(TaskDuration);
                var copy = CopyCells(figure);
                if (FindCrossVertical(figure)) break;
                if (figure.LowPosition < pictureBox1.Height) MoveFigure(figure);
                else break;
                if (figure.LowPosition <= pictureBox1.Height) ClearLine(copy);
            }
            figure.isActive = false;
        }

        private Figure CopyCells(Figure figure)
        {
            Cell[] copyarr = new Cell[figure.Cells.Length];
            foreach (var cell in figure.Cells.Select((value, i) => new { i, value }))
            {
                copyarr[cell.i] = new Cell(cell.value.BorderPosition, 'c');
            };
            return new Figure(copyarr);
        }
        private void MoveFigure(Figure figure)
        {
            foreach (Cell c in figure.Cells)
            {
                c.BorderPosition += new Size(0, Cell.BorderWithBodySize.Height);
                c.BodyPosition += new Size(0, Cell.BorderWithBodySize.Height);
            }
            figure.LowPosition += Cell.BorderWithBodySize.Height;
        }
        private void DrawCell(Cell c)
        {
            g.FillRectangle(BorderTrub, new Rectangle(c.BorderPosition, Cell.BorderWithBodySize));
            g.FillRectangle(BodyTrub, new Rectangle(c.BodyPosition, Cell.BodySize));
        }
        private void ClearLine(Figure copyfigure)
        {
            foreach (Cell c in copyfigure.Cells)
            {
                g.FillRectangle(BackgroundTrub, new Rectangle(c.BorderPosition, Cell.BorderWithBodySize));
            }
        }
        private bool CheckFinal()
        {
            if (Figures.FirstOrDefault(x => (x.LowPosition) == Cell.BorderWithBodySize.Height && !x.isActive) != null)
            {
                return isEnd = true;
            }
            return false;
        }
      

        private bool FindCrossVertical(Figure figure)
        {
            foreach (var l in Figures)
            {
                if (figure.Equals(l)) continue;
                foreach (var c in l.Cells)
                {
                    if (figure.Cells.FirstOrDefault(x => x.BorderPosition == (c.BorderPosition - new Size(0, Cell.BorderWithBodySize.Height))) != null) return true;
                }
            }
            return false;
        }
        private bool FindCrossHorizontal(Figure figure, FigureMoveDestination fmd)
        {
            foreach (var l in Figures)
            {
                if (figure.Equals(l)) continue;
                foreach (var c in l.Cells)
                {
                    if (figure.Cells.FirstOrDefault(x => (x.BorderPosition + new Size((fmd == FigureMoveDestination.Left) ? (-Cell.BorderWithBodySize.Width) : (Cell.BorderWithBodySize.Width), 0)) == c.BorderPosition) != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            var figure = Figures.FirstOrDefault(l => l.isActive);
            switch (e.KeyCode)
            {
                case (Keys.Left):
                    {
                        if (figure.LeftPosition > 0)
                        {
                            if (!FindCrossHorizontal(figure, FigureMoveDestination.Left))
                            {
                                ClearLine(figure);
                                foreach (Cell c in figure.Cells)
                                {
                                    c.BorderPosition -= new Size(Cell.BorderWithBodySize.Width, 0);
                                    c.BodyPosition -= new Size(Cell.BorderWithBodySize.Width, 0);
                                    DrawCell(c);
                                }
                                figure.LeftPosition -= Cell.BorderWithBodySize.Width;
                                figure.RightPosition -= Cell.BorderWithBodySize.Width;
                            }
                        }
                        break;
                    }
                case (Keys.Right):
                    {
                        if (figure.RightPosition < pictureBox1.Width)
                        {
                            if (!FindCrossHorizontal(figure, FigureMoveDestination.Right))
                            {
                                ClearLine(figure);
                                foreach (Cell c in figure.Cells)
                                {
                                    c.BorderPosition += new Size(Cell.BorderWithBodySize.Width, 0);
                                    c.BodyPosition += new Size(Cell.BorderWithBodySize.Width, 0);
                                    DrawCell(c);
                                }
                                figure.LeftPosition += Cell.BorderWithBodySize.Width;
                                figure.RightPosition += Cell.BorderWithBodySize.Width;
                            }
                        }
                        break;
                    }
                case (Keys.Down):
                    {
                        TaskDuration = 10;
                        break;
                    }
            }
        }
        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void comboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
        enum FigureMoveDestination { Left = 1, Right = 2 };
    }
}
