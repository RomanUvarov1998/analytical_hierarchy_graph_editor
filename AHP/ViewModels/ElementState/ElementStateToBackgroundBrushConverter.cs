using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AHP.ViewModels.ElementState
{
  public class ElementStateToBackgroundBrushConverter : IValueConverter
  {
    public Brush GoalBrush { get; set; } = Brushes.GreenYellow;
    public Brush CriteriumBrush { get; set; } = Brushes.Yellow;
    public Brush AlternativeBrush { get; set; } = Brushes.Orange;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is ElementGoal) {
        return GoalBrush;
      }
      else if (value is ElementCriterium) {
        return CriteriumBrush;
      }
      else if (value is ElementAlternative) {
        return AlternativeBrush;
      }
      else {
        return null;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
