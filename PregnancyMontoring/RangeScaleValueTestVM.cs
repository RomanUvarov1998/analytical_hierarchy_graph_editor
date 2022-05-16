using Database.DB;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace PregnancyMontoring
{
  public class RangeScaleValueTestVM : INotifyPropertyChanged
  {
    internal RangeScaleValueTestVM(RangeScaleValue scv) {
      this.scv = scv;
    }


    //----------------------------- API -------------------------------

    internal RangeScaleValue ScaleValue => scv;

    internal bool IsSelected
    {
      get => isSelected;
      set
      {
        isSelected = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundBrush)));
      }
    }

    //----------------------------- GUI -------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public double Max => scv.Max;
    public double Min => scv.Min;

    public Brush BackgroundBrush => IsSelected ? Brushes.DeepSkyBlue : Brushes.White;


    //----------------------------- Private members -------------------------------

    private RangeScaleValue scv;
    private bool isSelected;
  }
}
