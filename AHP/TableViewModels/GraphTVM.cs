using Database.DB;
using Database.MathModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace AHP.TableViewModels
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

    public string GraphToolTip {
      get
      {
        var sb = new StringBuilder();
        bool has_err = false;
        if (!Graph.IsCompleted()) {
          sb.AppendLine("Граф некорректен");
          has_err = true;
        }
        if (!Graph.Questions.Any()) {
          sb.AppendLine("Не добавлены вопросы");
          has_err = true;
        }
        return has_err ? sb.ToString() : null;
      }
    }

    public List<TestSessionTVM> TestSessions { get; }

    public Graph Graph { get; }


    //----------------------------- API -------------------------------

    internal void UpdateTableCells() {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Goal)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreatedDateStr)));
    }
  }
}
