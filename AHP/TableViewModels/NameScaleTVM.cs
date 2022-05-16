using Database.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AHP.TableViewModels
{
  public class NameScaleTVM : ScaleBaseTVM
  {
    internal NameScaleTVM(NameScale scale, Action on_scale_updated) : base(scale, on_scale_updated) {
      NameScaleValues = new ObservableCollection<NameScaleValueTVM>(
        scale.NameScaleValues.Select(scv => new NameScaleValueTVM(scv.ValueName)));
      OnValuesUpdated();
    }


    //----------------------------- API -------------------------------
    
    internal override void OnValuesUpdated() {
      throw new NotImplementedException();
    }


    //----------------------------- GUI -------------------------------

    public ObservableCollection<NameScaleValueTVM> NameScaleValues { get; }

    internal override IEnumerable<ScaleValueBaseTVM> ScaleValues => NameScaleValues.Select(scv => scv);
  }
}
