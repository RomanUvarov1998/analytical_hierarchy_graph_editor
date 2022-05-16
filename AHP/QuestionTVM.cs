using Database.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AHP
{
  public class QuestionTVM : INotifyPropertyChanged
  {
    internal QuestionTVM(Question q, Graph graph, Action on_scale_selected) {
      Question = q;
      this.graph = graph;
      this.on_scale_selected = on_scale_selected;
      usesScale = Question.Scale != null;
      scaleComboboxIsEnabled = usesScale;
      CanUseScale = graph.Scales.Any();
    }


    //----------------------------- API -------------------------------

    internal Question Question { get; }


    //----------------------------- GUI -------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public string Content
    {
      get => Question.Content;
      set => Question.Content = value;
    }

    public bool IsRequired
    {
      get => Question.IsRequired;
      set => Question.IsRequired = value;
    }

    public bool CanUseScale { get; }

    public bool UsesScale
    {
      get => usesScale;
      set
      {
        usesScale = value;

        if (!usesScale) {
          SelectedScale = null;
        }
        else {
          SelectedScale = Scales.First();
        }

        ScaleComboboxIsEnabled = value;
      }
    }

    public bool ScaleComboboxIsEnabled
    {
      get => scaleComboboxIsEnabled;
      set
      {
        scaleComboboxIsEnabled = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScaleComboboxIsEnabled)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scales)));
      }
    }

    public Scale SelectedScale
    {
      get => Question.Scale;
      set
      {
        Question.Scale = value;
        on_scale_selected.Invoke();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedScale)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scales)));
      }
    }

    public List<Scale> Scales => UsesScale ? graph.Scales : null;


    //----------------------------- Private members -------------------------------

    private Graph graph;
    private bool scaleComboboxIsEnabled;
    private bool usesScale;
    private Action on_scale_selected;
  }
}
