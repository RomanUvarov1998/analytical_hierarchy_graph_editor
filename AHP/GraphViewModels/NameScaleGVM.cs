using Database.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AHP.GraphViewModels
{
  public class NameScaleGVM : ScaleBaseGVM
  {
    internal NameScaleGVM(NameScale sc, Action on_changed) {
      Scale = sc;
      this.on_changed = on_changed;
      ScaleValues = new ObservableCollection<ScaleValueBaseGVM>(
        sc.NameScaleValues
        .Select(scv => new NameScaleValueGVM(scv, this, on_changed))
        .ToList());
    }

    internal override void UpdateValidation() {
      foreach (NameScaleValueGVM scv in ScaleValues) {
        scv.IsDuplicate = ScaleValues.Count(_scv => ((NameScaleValueGVM)_scv).ValueName == scv.ValueName) > 1;
        scv.UpdateIsUsedStatus();
      }
    }


    //-------------------------------- API ---------------------------

    internal override string TemplateName => "NameScaleValueTemplate";

    internal override void AddScaleValue() {
      var scv = new NameScaleValue()
      {
        ValueName = $"Значение {ScaleValues.Count}",
        Scale = this.Scale,
      };
      Scale.ScaleValues.Add(scv);
      var scv_gvm = new NameScaleValueGVM(scv, this, on_changed);
      ScaleValues.Add(scv_gvm);
      on_changed.Invoke();
    }

    internal override void RemoveScaleValue(ScaleValueBaseGVM scv) {
      Scale.ScaleValues.Remove(scv.ScaleValue);
      ScaleValueBaseGVM scv_gvm = ScaleValues.Single(_scv => _scv.ScaleValue == scv.ScaleValue);
      ScaleValues.Remove(scv_gvm);
      on_changed.Invoke();
    }

    internal override Scale Scale { get; }


    //-------------------------------- GUI ---------------------------

    public override ObservableCollection<ScaleValueBaseGVM> ScaleValues { get; }

    public string Title
    {
      get => Scale.Title;
      set
      {
        Scale.Title = value;
        on_changed?.Invoke();
      }
    }


    //-------------------------------- Private members ---------------------------

    private Action on_changed;
  }
}
