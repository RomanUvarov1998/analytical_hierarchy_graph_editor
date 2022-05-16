using System.Windows;

namespace AHP.ViewModels
{
  interface IPlaceable
  {
    public double X { get; set; }
    public double Y { get; set; }
    public Point? DestPos { get; set; }
    public Point Pos {
      get => new Point(X, Y);
      set {
        X = value.X;
        Y = value.Y;
      }
    }
    public bool IsJustAdded { get; set;  }
  }
}
