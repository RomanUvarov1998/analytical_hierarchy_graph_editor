using System.Windows;

namespace AHP.ViewModels
{
  interface IDragTarget
  {
    double DragTargetCenterX { get; }

    Point DragTargetLeft { get; }
    Point DragTargetCenter { get; }
    Point DragTargetRight { get; }

    double DragTargetHeight { get; }
  }
}
