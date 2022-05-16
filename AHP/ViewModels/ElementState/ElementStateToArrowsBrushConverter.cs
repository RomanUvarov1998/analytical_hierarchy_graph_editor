using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AHP.ViewModels.ElementState
{
  public class ElementStateToArrowsBrushConverter : IValueConverter
  {
    public Brush NoConnectionsBrush { get; set; } = Brushes.LightGray;
    public Brush NotRatedBrush { get; set; } = Brushes.DeepSkyBlue;
    public Brush InconsistentBrush { get; set; } = Brushes.Orange;
    public Brush ConsistentBrush { get; set; } = Brushes.GreenYellow;
    public Brush NotEnoughSubCriteriumsBrush { get; set; } = Brushes.Red;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      Consistensy cons;

      if (value is ElementGoal g) {
        cons = g.Consistensy;
      }
      else if (value is ElementCriterium cr) {
        cons = cr.Consistensy;
      }
      else if (value is ElementAlternative) {
        return Brushes.White;
      }
      else {
        return null;
      }

      switch (cons) {
        case Consistensy.NotAllConnectionsRated: return NotRatedBrush;
        case Consistensy.Inconsistent: return InconsistentBrush;
        case Consistensy.Consistent: return ConsistentBrush;
        case Consistensy.NotEnoughSubCriteriums: return NotEnoughSubCriteriumsBrush;
        default:
          Debug.Fail("Not valid element state");
          return null;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
