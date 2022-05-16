using Database.DB;
using Database.MathModel;
using AHP.TableViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AHP
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    public MainWindow() {
      InitializeComponent();
      DataContext = this;
    }


    //----------------------------- GUI -------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsUIEnabled
    {
      get => isUIEnabled;
      set
      {
        isUIEnabled = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUIEnabled)));
      }
    }

    public List<GraphTVM> Graphs
    {
      get => graphs;
      private set
      {
        graphs = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Graphs)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GraphTVM.TestSessions)));
      }
    }

    public bool BtnEditGraphIsEnabled => SelectedGraph != null;
    public bool BtnQuestionsIsEnabled => SelectedGraph != null && SelectedGraph.Graph.IsCompleted();

    public GraphTVM SelectedGraph
    {
      get => selectedGraph;
      set
      {
        if (freeze_selected_graph) return;
        selectedGraph = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedGraph)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BtnEditGraphIsEnabled)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BtnQuestionsIsEnabled)));
      }
    }

    //----------------------------- Events -------------------------------

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      LoadGraphs(true);
    }

    private void Button_AddGraph_Click(object sender, RoutedEventArgs e) {
      var dlg = new GraphEditor(0);
      dlg.ShowDialog();
      LoadGraphs(false);
    }

    private void Button_EditGraph_Click(object sender, RoutedEventArgs e) {
      if (SelectedGraph == null) return;
      var dlg = new GraphEditor(SelectedGraph.Graph.Id);
      dlg.ShowDialog();
      LoadGraphs(false);
    }

    private void Button_DeleteItem_Click(object sender, RoutedEventArgs e) {
      var btn = (Button)sender;
      using (var ctx = new Context()) {
        if (btn.DataContext is GraphTVM g) {
          ctx.Remove(g.Graph);
        }
        else if (btn.DataContext is TestSessionTVM ts) {
          ctx.Remove(ts.TestSession);
        }
        else {
          Debug.Fail("Selected item type is not handled");
        }
        ctx.SaveChanges();
      }
      LoadGraphs(false);
    }

    private void Button_SetupQuestions_Click(object sender, RoutedEventArgs e) {
      var dlg = new QuestionsEditor(SelectedGraph.Graph.Id);
      dlg.ShowDialog();
      LoadGraphs(false);
    }


    //----------------------------- Private members -------------------------------

    private List<GraphTVM> graphs = new List<GraphTVM>();
    private GraphTVM selectedGraph;
    private bool freeze_selected_graph = false;

    private void LoadGraphs(bool ensure_db_created) {
      IsUIEnabled = false;

      var worker = new BackgroundWorker();

      worker.DoWork += (s, e) =>
      {
        List<GraphTVM> graph_mvs;

        using (var ctx = new Context()) {
          if (ensure_db_created) {
            ctx.EnsureDatabaseCreated();
          }

          graph_mvs = ctx.LoadGraphs()
            .Select(g => new GraphTVM(g))
            .ToList();
        }

        Dispatcher.Invoke(() =>
        {
          freeze_selected_graph = true;

          Graphs = graph_mvs;

          if (SelectedGraph != null) {
            freeze_selected_graph = false;
            SelectedGraph = Graphs.FirstOrDefault(g => g.Graph.Id == SelectedGraph.Graph.Id);
          }
          else {
            freeze_selected_graph = false;
            SelectedGraph = null;
          }

          IsUIEnabled = true;
        });
      };

      worker.RunWorkerAsync();
    }


    //----------------------------- Private members -------------------------------

    private bool isUIEnabled = true;
  }
}
