using Database.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace PregnancyMontoring.TableViewModels
{
  public class ScaleTVM
  {
    internal ScaleTVM(RangeScale scale, Graph graph, Action on_scale_updated) {
      Scale = scale;
      ScaleValues = new ObservableCollection<ScaleValueTVM>(
        Scale.RangeScaleValues.Select(sv => new ScaleValueTVM(sv, this, graph, on_scale_updated)));
      ScaleValues.CollectionChanged += ScaleValues_CollectionChanged;
      this.on_scale_updated = on_scale_updated;
      OnValuesUpdated();
    }


    //----------------------------- API -------------------------------

    internal void OnValuesUpdated() {
      if (ScaleValues.Count == 0) {
        return;
      }

      if (ScaleValues.Count == 1) {
        ScaleValues[0].IsLast = true;
        ScaleValues[0].MinState = ScaleValueTVMState.Ok;
        ScaleValues[0].MaxState = ScaleValueTVMState.Ok;
        return;
      }

      for (int i = 0; i < ScaleValues.Count; i++) {
        ScaleValues[i].IsLast = i == ScaleValues.Count - 1;

        ScaleValues[i].MinState = ScaleValueTVMState.Ok;
        ScaleValues[i].MaxState = ScaleValueTVMState.Ok;

        // check if min is overlapped
        if (0 < i && ScaleValues[i].ScaleValue.Min < ScaleValues[i - 1].ScaleValue.Max) {
          ScaleValues[i].MinState = ScaleValueTVMState.IsOverlapped;
        }
        // check if max is overlapped
        if (i < ScaleValues.Count - 1 && ScaleValues[i].ScaleValue.Max > ScaleValues[i + 1].ScaleValue.Min) {
          ScaleValues[i].MaxState = ScaleValueTVMState.IsOverlapped;
        }
        // check if min > max
        if (ScaleValues[i].ScaleValue.Min > ScaleValues[i].ScaleValue.Max) {
          ScaleValues[i].MinState = ScaleValueTVMState.MinGreaterThanMax;
          ScaleValues[i].MaxState = ScaleValueTVMState.MinGreaterThanMax;
        }
      }
    }

    private void ScaleValues_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
      switch (e.Action) {
        case NotifyCollectionChangedAction.Add:
          ScaleValueTVM added_item = null;
          foreach (ScaleValueTVM scv in e.NewItems) {
            if (e.OldItems == null || !e.OldItems.Contains(scv)) {
              added_item = scv;
              break;
            }
          }
          Debug.Assert(added_item != null);
          Scale.ScaleValues.Add(added_item.ScaleValue);
          break;

        case NotifyCollectionChangedAction.Remove:
          ScaleValueTVM removed_item = null;
          foreach (ScaleValueTVM scv in e.OldItems) {
            if (e.NewItems == null || !e.NewItems.Contains(scv)) {
              removed_item = scv;
              break;
            }
          }
          Debug.Assert(removed_item != null);
          Scale.ScaleValues.Remove(removed_item.ScaleValue);
          break;

        case NotifyCollectionChangedAction.Replace:
          throw new NotImplementedException();

        case NotifyCollectionChangedAction.Move:
          throw new NotImplementedException();

        case NotifyCollectionChangedAction.Reset:
          throw new NotImplementedException();
      }

      OnValuesUpdated();
    }

    internal RangeScale Scale { get; }


    //----------------------------- GUI -------------------------------

    public ObservableCollection<ScaleValueTVM> ScaleValues { get; }
    public string Title
    {
      get => Scale.Title;
      set
      {
        Scale.Title = value;
        on_scale_updated.Invoke();
      }
    }

    public string Question { get; set; }

    //----------------------------- Private members -------------------------------

    private Action on_scale_updated;
  }
}
