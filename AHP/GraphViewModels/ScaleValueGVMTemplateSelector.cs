using System.Windows;
using System.Windows.Controls;

namespace AHP.GraphViewModels
{
  public class ScaleValueGVMTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container) {
      FrameworkElement element = container as FrameworkElement;

      var tvm = (ScaleBaseGVM)item;

      var dt = element.FindResource(tvm.TemplateName) as DataTemplate;
      return dt;
    }
  }
}
