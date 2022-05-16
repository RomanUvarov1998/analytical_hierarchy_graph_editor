using System.Collections.Generic;
using System.ComponentModel;

namespace AHP.TableViewModels
{
  public abstract class ScaleValueBaseTVM : INotifyPropertyChanged
  {
    //----------------------------- API -------------------------------

    internal abstract string TemplateName { get; }

    internal abstract void UpdateStatus();


    //----------------------------- GUI -------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public abstract bool IsUsed { get; }


    //----------------------------- Protected members -------------------------------

    protected void OnPropertyChanged(string prop_name) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
    }
  }
}
