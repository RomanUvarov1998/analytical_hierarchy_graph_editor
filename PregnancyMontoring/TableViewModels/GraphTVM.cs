using Database.DB;
using Database.MathModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace PregnancyMontoring.TableViewModels
{
  public class GraphTVM : INotifyPropertyChanged
  {
    internal GraphTVM(Graph graph) {
      Graph = graph;
      TestSessions = graph.TestSessions.Select(ts => new TestSessionTVM(ts)).ToList();
    }


    //----------------------------- GUI -------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public string Goal
    {
      get
      {
        if (Graph.Layers.Count == 0) return null;
        if (Graph.Layers[0].Elements.Count == 0) return null;
        return Graph.Layers[0].Elements[0].Title;
      }
    }

    public string CreatedDateStr => Graph.CreatedDate.ToString("dd.MM.yyyy hh:mm");

    public string UpdatedDateStr => Graph.UpdatedDate.ToString("dd.MM.yyyy hh:mm");

    public Brush StateBrush => Graph.IsCompleted() && Graph.Questions.Any() ? Brushes.GreenYellow : Brushes.Yellow;

    public string GraphToolTip => !Graph.IsCompleted() || !Graph.Questions.Any() ? "Тектирование не готово" : null;

    public List<TestSessionTVM> TestSessions { get; }


    //----------------------------- API -------------------------------

    internal Graph Graph { get; }

    internal void UpdateTableCells() {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Goal)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreatedDateStr)));
    }
  }
}
