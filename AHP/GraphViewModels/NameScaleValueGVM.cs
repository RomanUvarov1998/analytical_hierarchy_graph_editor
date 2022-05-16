using Database.DB;
using System;
using System.ComponentModel;
using System.Windows.Media;

namespace AHP.GraphViewModels
{
  public class NameScaleValueGVM : ScaleValueBaseGVM, INotifyPropertyChanged
  {
    internal NameScaleValueGVM(NameScaleValue scv, NameScaleGVM scale_gvm, Action on_value_changed) {
      ScaleValue = scv;
      this.scale_gvm = scale_gvm;
      this.on_value_changed = on_value_changed;
      isDuplicate = false;
    }


    //----------------------------------- API ----------------------------------

    internal bool IsDuplicate
    {
      get => isDuplicate;
      set
      {
        isDuplicate = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueNameBackground)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScaleValueToolTip)));
      }
    }

    internal override void RemoveSelfFromScale() {
      scale_gvm.RemoveScaleValue(this);
    }

    internal override ScaleValue ScaleValue { get; }

    internal override Element AttachedElement
    {
      get => ScaleValue.Element;
      set
      {
        ScaleValue.Element = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScaleValueToolTip)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUsedBrush)));
        on_value_changed.Invoke();
      }
    }

    internal void UpdateIsUsedStatus() {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUsedBrush)));
    }

    internal string ErrMsg => IsDuplicate ? "Значение повторяется" : null;


    //----------------------------------- GUI ----------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public string ValueName
    {
      get => ( (NameScaleValue)ScaleValue ).ValueName;
      set
      {
        ( (NameScaleValue)ScaleValue ).ValueName = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueName)));
        on_value_changed.Invoke();
      }
    }

    public string ScaleValueToolTip => AttachedElement != null ? null : "Диапазон значений не соотнесен к элементу";
    public Brush ValueNameBackground => IsDuplicate ? Brushes.Yellow : Brushes.White;

    public Brush IsUsedBrush => AttachedElement == null ? Brushes.Yellow : Brushes.GreenYellow;


    //----------------------------------- Private members ----------------------------------
    private NameScaleGVM scale_gvm;
    private Action on_value_changed;
    private bool isDuplicate;
  }
}
