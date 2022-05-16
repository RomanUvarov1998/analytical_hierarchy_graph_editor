using Database.DB;
using System;
using System.Collections.ObjectModel;

namespace AHP.GraphViewModels
{
  public abstract class ScaleBaseGVM
  {
    internal static ScaleBaseGVM Create(Scale scale, Action on_value_changed) {
      if (scale is RangeScale rsc) {
        return new RangeScaleGVM(rsc, on_value_changed);
      }
      else if (scale is NameScale nsc) {
        return new NameScaleGVM(nsc, on_value_changed);
      }
      else {
        throw new NotImplementedException();
      }
    }

    public abstract ObservableCollection<ScaleValueBaseGVM> ScaleValues { get; }

    internal abstract Scale Scale { get; }

    internal abstract void UpdateValidation();
    internal abstract void AddScaleValue();
    internal abstract void RemoveScaleValue(ScaleValueBaseGVM scv);
    internal abstract string TemplateName { get; }
  }
}
