using Database.DB;
using System.ComponentModel;

namespace AHP.TableViewModels
{
  public class NameScaleValueTVM : ScaleValueBaseTVM, INotifyPropertyChanged
  {
    internal NameScaleValueTVM(string value_name)
      : this(new NameScaleValue() { ValueName = value_name }) {
    }

    internal NameScaleValueTVM(NameScaleValue scv) {
      ScaleValue = scv;
    }


    //----------------------------- API -------------------------------

    internal override string TemplateName => "NameScaleValueTemplate";

    internal NameScaleValue ScaleValue { get; }

    internal override void UpdateStatus() {
      throw new System.NotImplementedException();
    }


    //----------------------------- GUI -------------------------------

    public override bool IsUsed => throw new System.NotImplementedException();

    public string ValueName
    {
      get => ScaleValue.ValueName;
      set => ScaleValue.ValueName = value;
    }
  }
}
