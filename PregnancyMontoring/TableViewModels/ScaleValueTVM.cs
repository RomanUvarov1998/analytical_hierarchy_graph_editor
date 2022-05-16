using Database.DB;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace PregnancyMontoring.TableViewModels
{
  enum ScaleValueTVMState
  {
    Ok,
    IsOverlapped,
    MinGreaterThanMax,
  }

  public class ScaleValueTVM : INotifyPropertyChanged
  {
    internal ScaleValueTVM(double min, double max, ScaleTVM scale_tvm, Graph graph, Action on_scale_updated) {
      ScaleValue = new RangeScaleValue()
      {
        Min = min,
        Max = max,
      };
      this.min = min.ToString("0.00");
      this.max = max.ToString("0.00");
      minIsCorrect = true;
      maxIsCorrect = true;
      ScaleTVM = scale_tvm;
      this.graph = graph;
      this.on_scale_updated = on_scale_updated;
      UpdateStatus();
    }
    internal ScaleValueTVM(RangeScaleValue scale_value, ScaleTVM scale_tvm, Graph graph, Action on_scale_updated) {
      ScaleValue = scale_value;
      this.min = scale_value.Min.ToString("0.00");
      this.max = scale_value.Max.ToString("0.00");
      minIsCorrect = true;
      maxIsCorrect = true;
      ScaleTVM = scale_tvm;
      this.graph = graph;
      this.on_scale_updated = on_scale_updated;
      UpdateStatus();
    }


    //----------------------------- API -------------------------------

    internal RangeScaleValue ScaleValue { get; }
    internal ScaleTVM ScaleTVM { get; }

    internal bool IsLast
    {
      get => isLast;
      set
      {
        isLast = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MiddleTitle)));
      }
    }

    internal bool MinIsCorrect
    {
      get => minIsCorrect;
      set
      {
        minIsCorrect = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MinForegroundBrush)));
      }
    }

    internal bool MaxIsCorrect
    {
      get => maxIsCorrect;
      set
      {
        maxIsCorrect = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxForegroundBrush)));
      }
    }

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

    internal bool IsCorrect => !double.IsNaN(ScaleValue.Min) && !double.IsNaN(ScaleValue.Max);

    internal void UpdateStatus() {
      HasAssignedElement = graph.Layers.Any(layer => layer.Elements.Any(elt => elt.ScaleValue == ScaleValue));
    }

    internal bool HasAssignedElement
    {
      get => hasAssignedElement;
      set
      {
        hasAssignedElement = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScaleValueBrush)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScaleValueToolTip)));
      }
    }


    //----------------------------- GUI -------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

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
    public Brush MinForegroundBrush => MinIsCorrect ? Brushes.Black : Brushes.Red;
    public Brush MaxForegroundBrush => MaxIsCorrect ? Brushes.Black : Brushes.Red;

    public string Min
    {
      get => min;
      set
      {
        min = value;
        if (double.TryParse(value, out double res)) {
          ScaleValue.Min = res;
          MinIsCorrect = true;
          ScaleTVM.OnValuesUpdated();
          on_scale_updated.Invoke();
        }
        else {
          MinIsCorrect = false;
        }
      }
    }

    public string Max
    {
      get => max;
      set
      {
        max = value;
        if (double.TryParse(value, out double res)) {
          ScaleValue.Max = res;
          MaxIsCorrect = true;
          ScaleTVM.OnValuesUpdated();
          on_scale_updated.Invoke();
        }
        else {
          MaxIsCorrect = false;
        }
      }
    }

    public Brush ScaleValueBrush => HasAssignedElement ? Brushes.White : Brushes.Yellow;
    public string ScaleValueToolTip => HasAssignedElement ? null : "Диапазон значений не соотнесен к элементу";


    //----------------------------- Private members -------------------------------

    private Graph graph;
    private bool minIsCorrect;
    private bool maxIsCorrect;
    private string min;
    private string max;
    private bool isLast;
    private ScaleValueTVMState min_state;
    private ScaleValueTVMState maxState;
    private bool hasAssignedElement;
    private Action on_scale_updated;
  }
}
