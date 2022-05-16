using Database.DB;
using System;
using System.ComponentModel;
using System.Windows.Media;

namespace AHP.GraphViewModels
{
  public class RangeScaleValueGVM : ScaleValueBaseGVM, INotifyPropertyChanged
  {
    internal RangeScaleValueGVM(RangeScaleValue scv, RangeScaleGVM scale_gvm, Action on_value_changed) {
      ScaleValue = scv;
      this.scale_gvm = scale_gvm;
      min = scv.Min.ToString("0.00");
      max = scv.Max.ToString("0.00");
      this.on_value_changed = on_value_changed;
    }


    //----------------------------------- API ----------------------------------

    internal bool IsCorrect => ErrMsg == string.Empty;

    internal bool IsLast
    {
      get => isLast;
      set
      {
        isLast = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MiddleTitle)));
      }
    }

    internal override ScaleValue ScaleValue { get; }

    internal RangeScaleValue RangeScaleValue => (RangeScaleValue)ScaleValue;

    internal ScaleValueTVMState MinState
    {
      get => min_state;
      set
      {
        min_state = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MinBackgroundBrush)));
      }
    }
    internal ScaleValueTVMState MaxState
    {
      get => maxState;
      set
      {
        maxState = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxBackgroundBrush)));
      }
    }

    internal override void RemoveSelfFromScale() {
      scale_gvm.RemoveScaleValue(this);
    }

    internal override Element AttachedElement
    {
      get => ScaleValue.Element;
      set
      {
        ScaleValue.Element = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScaleValueToolTip)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUsedBrush)));
      }
    }

    internal void UpdateIsUsedStatus() {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUsedBrush)));
    }


    //----------------------------------- GUI ----------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public string ScaleValueToolTip => AttachedElement != null ? null : "Диапазон значений не соотнесен к элементу";
    public string MiddleTitle => IsLast ? "≤ значение ≤" : "≤ значение <";

    public Brush MinBackgroundBrush
    {
      get
      {
        switch (MinState) {
          case ScaleValueTVMState.Ok: return Brushes.White;
          case ScaleValueTVMState.IsOverlapped: return Brushes.Yellow;
          case ScaleValueTVMState.MinGreaterThanMax: return Brushes.Orange;
          default: throw new InvalidOperationException();
        }
      }
    }
    public Brush MaxBackgroundBrush
    {
      get
      {
        switch (MaxState) {
          case ScaleValueTVMState.Ok: return Brushes.White;
          case ScaleValueTVMState.IsOverlapped: return Brushes.Yellow;
          case ScaleValueTVMState.MinGreaterThanMax: return Brushes.Orange;
          default: throw new InvalidOperationException();
        }
      }
    }

    public string Min
    {
      get => min;
      set
      {
        min = value;
        TrySetValues(min, max);
      }
    }

    public string Max
    {
      get => max;
      set
      {
        max = value;
        TrySetValues(min, max);
      }
    }

    public string ErrMsg { get; private set; }

    public Brush MinForegroundBrush { get; private set; } = Brushes.Black;

    public Brush MaxForegroundBrush { get; private set; } = Brushes.Black;

    public Brush IsUsedBrush => AttachedElement == null ? Brushes.Yellow : Brushes.GreenYellow;


    //-------------------------------- Private members -------------------

    private void TrySetValues(string input_min, string input_max) {
      ErrMsg = string.Empty;

      if (double.TryParse(input_min, out double res_min)) {
        RangeScaleValue.Min = res_min;
        MinForegroundBrush = Brushes.Black;
      }
      else {
        MinForegroundBrush = Brushes.Red;
        ErrMsg += "Значение должно быть числом";
      }

      if (double.TryParse(input_max, out double res_max)) {
        RangeScaleValue.Max = res_max;
        MaxForegroundBrush = Brushes.Black;
      }
      else {
        MaxForegroundBrush = Brushes.Red;
        ErrMsg += "Значение должно быть числом";
      }

      on_value_changed.Invoke();

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrMsg)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MinForegroundBrush)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxForegroundBrush)));
    }

    private RangeScaleGVM scale_gvm;
    private string min;
    private string max;
    private Action on_value_changed;
    private bool isLast;
    private ScaleValueTVMState min_state;
    private ScaleValueTVMState maxState;
  }
}
