using Database.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AHP.GraphViewModels
{
  enum ScaleValueTVMState
  {
    Ok,
    IsOverlapped,
    MinGreaterThanMax,
  }

  public class RangeScaleGVM : ScaleBaseGVM
  {
    internal RangeScaleGVM(RangeScale sc, Action on_changed) {
      Scale = sc;
      this.on_changed = on_changed;
      ScaleValues = new ObservableCollection<ScaleValueBaseGVM>(
        sc.RangeScaleValues
        .Select(scv => new RangeScaleValueGVM(scv, this, on_changed)));
      UpdateMiddleTitles();
    }


    //-------------------------------- API ---------------------------

    internal override string TemplateName => "RangeScaleValueTemplate";

    internal override void AddScaleValue() {
      RangeScaleValueGVM last_scv_gvm = ScaleValues.Where(sc => sc is RangeScaleValueGVM).Cast<RangeScaleValueGVM>().LastOrDefault();
      RangeScaleValue last_scv = last_scv_gvm?.ScaleValue as RangeScaleValue;
      double last_max = last_scv?.Max ?? 0.0;
      var scv = new RangeScaleValue()
      {
        Min = last_max,
        Max = last_max + 10.0,
        Scale = this.Scale,
      };
      Scale.ScaleValues.Add(scv);
      var scv_gvm = new RangeScaleValueGVM(scv, this, on_changed);
      ScaleValues.Add(scv_gvm);
      UpdateMiddleTitles();
      on_changed.Invoke();
    }

    internal override void RemoveScaleValue(ScaleValueBaseGVM scv) {
      Scale.ScaleValues.Remove(scv.ScaleValue);
      ScaleValueBaseGVM scv_gvm = ScaleValues.Single(_scv => _scv.ScaleValue == scv.ScaleValue);
      ScaleValues.Remove(scv_gvm);
      UpdateMiddleTitles();
      on_changed.Invoke();
    }

    internal override void UpdateValidation() {
      List<RangeScaleValueGVM> scvs = ScaleValues.Cast<RangeScaleValueGVM>().ToList();
      if (ScaleValues.Count == 0) {
        return;
      }

      if (ScaleValues.Count == 1) {
        scvs[0].MinState = ScaleValueTVMState.Ok;
        scvs[0].MaxState = ScaleValueTVMState.Ok;
        return;
      }

      for (int i = 0; i < ScaleValues.Count; i++) {
        scvs[i].MinState = ScaleValueTVMState.Ok;
        scvs[i].MaxState = ScaleValueTVMState.Ok;

        // check if min is overlapped
        if (0 < i && scvs[i].RangeScaleValue.Min < scvs[i - 1].RangeScaleValue.Max) {
          scvs[i].MinState = ScaleValueTVMState.IsOverlapped;
        }
        // check if max is overlapped
        if (i < scvs.Count - 1 && scvs[i].RangeScaleValue.Max > scvs[i + 1].RangeScaleValue.Min) {
          scvs[i].MaxState = ScaleValueTVMState.IsOverlapped;
        }
        // check if min > max
        if (scvs[i].RangeScaleValue.Min > scvs[i].RangeScaleValue.Max) {
          scvs[i].MinState = ScaleValueTVMState.MinGreaterThanMax;
          scvs[i].MaxState = ScaleValueTVMState.MinGreaterThanMax;
        }
      }

      foreach (RangeScaleValueGVM scv in ScaleValues) {
        scv.UpdateIsUsedStatus();
      }
    }

    internal override Scale Scale { get; }


    //-------------------------------- GUI ---------------------------

    public string Title
    {
      get => Scale.Title;
      set
      {
        Scale.Title = value;
        on_changed?.Invoke();
      }
    }

    public override ObservableCollection<ScaleValueBaseGVM> ScaleValues { get; }


    //----------------------------- Private members --------------------

    private void OnValueChanged() {
      
    }

    private void UpdateMiddleTitles() {
      for (int i = 0; i < ScaleValues.Count; i++) {
        ((RangeScaleValueGVM)ScaleValues[i]).IsLast = i == ScaleValues.Count - 1;
      }
    }

    private Action on_changed;
  }
}
