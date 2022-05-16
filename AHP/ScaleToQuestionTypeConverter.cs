using Database.DB;
using System.Windows.Data;
using System;
using System.Globalization;

namespace AHP
{
  public class ScaleToQuestionTypeConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var scale = (Scale)value;
      return scale == null ? "Доп. вопрос" : $"Со шкалой '{scale.Title}'";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
