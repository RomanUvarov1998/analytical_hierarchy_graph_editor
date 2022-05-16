using Database.DB;
using PregnancyMontoring.TableViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PregnancyMontoring
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

    public bool BtnPassSessionIsEnabled => SelectedGraph != null && SelectedGraph.Graph.IsCompleted() && SelectedGraph.Graph.QuestionsCompleted();

    public GraphTVM SelectedGraph
    {
      get => selectedGraph;
      set
      {
        if (freeze_selected_graph) return;
        selectedGraph = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedGraph)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BtnPassSessionIsEnabled)));
      }
    }

    public TestSessionTVM SelectedTestSession
    {
      get => selectedTestSession;
      set
      {
        if (freeze_selected_test_session) return;
        selectedTestSession = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTestSession)));
      }
    }


    //----------------------------- Events -------------------------------

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      LoadGraphs(true);
    }

    private void Button_StartTest_Click(object sender, RoutedEventArgs e) {
      var dlg = new Tester(SelectedGraph.Graph.Id);
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

    private void Button_Reload_Click(object sender, RoutedEventArgs e) {
      LoadGraphs(false);
    }

    private void LoadGraphs(bool ensure_database_exists) {
      IsUIEnabled = false;

      var worker = new BackgroundWorker();

      worker.DoWork += (s, e) =>
      {
        List<GraphTVM> graph_vms;
        using (var ctx = new Context()) {
          if (ensure_database_exists) {
            if (!ctx.DatabaseExists()) {
              Dispatcher.Invoke(() => {
                MessageBox.Show("База данных не обнаружена. Запустите програму для врача, чтобы создать базу данных.");
                Application.Current.Shutdown();
              });
              return;
            }
          }

          List<Graph> models = ctx.LoadGraphs();
          graph_vms = models
            .Select(g => new GraphTVM(g))
            .ToList();
        }

        Dispatcher.Invoke(() =>
        {
          freeze_selected_graph = true;
          freeze_selected_test_session = true;

          Graphs = graph_vms;

          if (SelectedGraph != null) {
            freeze_selected_graph = false;
            SelectedGraph = Graphs.FirstOrDefault(g => g.Graph.Id == SelectedGraph.Graph.Id);

            if (SelectedTestSession != null) {
              freeze_selected_test_session = false;
              SelectedTestSession = SelectedGraph.TestSessions.FirstOrDefault(ts => ts.TestSession.Id == SelectedTestSession.TestSession.Id);
            }
            else {
              freeze_selected_test_session = false;
              SelectedTestSession = null;
            }
          }
          else {
            freeze_selected_graph = false;
            freeze_selected_test_session = false;
            SelectedGraph = null;
            SelectedTestSession = null;
          }

          IsUIEnabled = true;
        });
      };

      worker.RunWorkerAsync();
    }


    //----------------------------- Private members -------------------------------

    private List<GraphTVM> graphs = new List<GraphTVM>();
    private GraphTVM selectedGraph;
    private TestSessionTVM selectedTestSession;
    private bool freeze_selected_graph = false;
    private bool freeze_selected_test_session = false;
    private bool isUIEnabled = true;
  }
}
