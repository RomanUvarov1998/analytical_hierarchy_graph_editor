using System.Windows.Data;
using System;
using System.Globalization;

namespace AHP
{
  public class BoolToYesNoConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      bool val = (bool)value;
      return val ? "Да" : "Нет";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
