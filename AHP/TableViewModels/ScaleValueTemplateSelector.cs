using Database.DB;
using System.Windows;
using System.Windows.Controls;

namespace AHP.TableViewModels
{
  public class ScaleValueTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container) {
      FrameworkElement element = container as FrameworkElement;

      var scv = (ScaleValue)item;

      if (scv is RangeScaleValue) return element.FindResource("RangeScaleValueTemplate") as DataTemplate;
      if (scv is NameScaleValue) return element.FindResource("NameScaleValueTemplate") as DataTemplate;
      return null;
    }
  }
}
