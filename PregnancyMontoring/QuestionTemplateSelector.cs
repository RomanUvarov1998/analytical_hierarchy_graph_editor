using System.Windows;
using System.Windows.Controls;

namespace PregnancyMontoring
{
  public class QuestionTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container) {
      FrameworkElement element = container as FrameworkElement;

      var tvm = (QuestionTestVM)item;

      var dt = element.FindResource(tvm.QuestionScaleTestVM.TemplateName) as DataTemplate;
      return dt;
    }
  }
}
