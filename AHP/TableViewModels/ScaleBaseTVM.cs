using Database.DB;
using System;
using System.Collections.Generic;

namespace AHP.TableViewModels
{
  public abstract class ScaleBaseTVM
  {
    internal ScaleBaseTVM(Scale scale, Action on_scale_updated) {
      Scale = scale;
      this.on_scale_updated = on_scale_updated;
    }


    //----------------------------- GUI -------------------------------
    
    public string Title
    {
      get => Scale.Title;
      set
      {
        Scale.Title = value;
        on_scale_updated.Invoke();
      }
    }

    //----------------------------- API -------------------------------

    internal Scale Scale { get; }

    internal abstract void OnValuesUpdated();

    internal abstract IEnumerable<ScaleValueBaseTVM> ScaleValues { get; }


    //----------------------------- Ptotected members -------------------------------

    protected Action on_scale_updated;
  }
}
