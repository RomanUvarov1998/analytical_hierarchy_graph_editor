using Database.DB;
using System;
using System.ComponentModel;

namespace PregnancyMontoring
{
  public class NameScaleValueTestVM : INotifyPropertyChanged
  {
    internal NameScaleValueTestVM(NameScaleValue scv, Action<NameScaleValue> on_selected) {
      this.scv = scv;
      this.on_selected = on_selected;
    }


    //----------------------------- API -------------------------------

    internal NameScaleValue ScaleValue => scv;

    //----------------------------- GUI -------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public string ValueName => scv.ValueName;

    public bool IsSelected
    {
      get => isSelected;
      set
      {
        isSelected = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        if (isSelected) on_selected.Invoke(scv);
      }
    }

    public string ScaleName => scv.Scale.Title;

    //----------------------------- Private members -------------------------------

    private NameScaleValue scv;
    private bool isSelected;
    private Action<NameScaleValue> on_selected;
  }
}
