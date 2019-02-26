using System;
using System.Collections.Generic;
using System.Text;

namespace SimTest.Domain
{
    public class Grid
    {
        public Array[,] Space = new Array[100,100];
        public Grid()
        {
            Space.SetValue(new Point { Taken = true }, 1, 1);
        }

        public bool IsFree(int x, int y) { var point = Space.GetValue(x, y) as Point;  return point.Taken; }
    }

    public interface ICharacter
    {
        int Id { get; set; }
        bool EatAble { get; }
        bool Size { get; set; }
        int Growth { get; set; }
    }



    public class Point {
        public bool Taken { get; set; }
        public ICharacter character { get; set; }
        //public I
    }



}
