using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
    public class Cell
    {
        public static Size BorderWithBodySize { get; set; }
        public Point BorderPosition { get; set; }
        public static Size BodySize { get; set; }
        public Point BodyPosition { get; set; }
        public static Color BorderColor { get; set; }
        public static Color BodyColor { get; set; }
        public static int BorderWidth { get; set; }
        public char Letter {get;set;}
        public Cell(Point Position, char letter)
        {
            this.Letter = letter;
            this.BorderPosition = Position;
            this.BodyPosition = new Point(Position.X + BorderWidth, Position.Y + BorderWidth);
        }
    }
}
