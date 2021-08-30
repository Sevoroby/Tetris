using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
    public class Figure
    {
        public Cell[] Cells { get; set; }
        public int LeftPosition { get; set; }
        public int RightPosition { get; set; }
        public int LowPosition { get; set; }

        public bool Orientation { get; set; }
        public bool isActive { get; set; } = false;

        public Figure(Cell[] cells)
        {
            this.Cells = cells;
        }
        public Figure(int cellsCount,int x)
        {
            Cells = MakeCells(cellsCount, x);
        }
        public static Cell[] MakeCells (int cellsCount, int x)
        {
            var cells = new Cell[cellsCount];
            for (int i = 0; i < cells.Length; i++)
            {
                int tempX = x;
                var p = new Point(tempX, 0);
                char l = 'c';
                cells[i] = new Cell(p, l);
                x += (Cell.BorderWithBodySize.Width);
            }
            return cells;
        }
    }
}
