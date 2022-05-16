using Database.DB;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AHP
{
  /// <summary>
  /// Interaction logic for QuestionsEditor.xaml
  /// </summary>
  public partial class QuestionsEditor : Window, INotifyPropertyChanged
  {
    public QuestionsEditor(int graph_id) {
      InitializeComponent();

      Debug.Assert(graph_id != 0);

      ctx = new Context();
      graph = ctx.LoadGraphWithQuestions(graph_id);

      Questions = new ObservableCollection<QuestionTVM>(graph.Questions
        .Select(q => new QuestionTVM(q, graph, UpdateErrMsg)));

      UpdateErrMsg();

      DataContext = this;
    }


    //----------------------------- GUI -------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<QuestionTVM> Questions { get; }

    public string ErrMsg
    {
      get => errMsg;
      set
      {
        errMsg = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrMsg)));
      }
    }

    public bool BtnSaveIsEnabled => ErrMsg == string.Empty;


    //----------------------------- Events -------------------------------

    private void Button_AddQuestion_Click(object sender, RoutedEventArgs e) {
      var q = new Question();
      Questions.Add(new QuestionTVM(q, graph, UpdateErrMsg));
      graph.Questions.Add(q);
      UpdateErrMsg();
    }

    private void Button_DeleteQuestion_Click(object sender, RoutedEventArgs e) {
      var btn = (Button)sender;
      var q = (QuestionTVM)btn.DataContext;
      Questions.Remove(q);
      graph.Questions.Remove(q.Question);
      UpdateErrMsg();
    }

    private void Button_Save_Click(object sender, RoutedEventArgs e) {
      graph.UpdatedDate = DateTime.Now;
      graph.UpdateInds();
      ctx.SaveChanges();
      DialogResult = true;
    }


    //----------------------------- Private members -------------------------------

    private void UpdateErrMsg() {
      var sb = new StringBuilder(string.Empty);

      foreach (Scale sc in graph.Scales) {
        if (Questions.Count(q => q.Question.Scale == sc) > 1) {
          sb.AppendLine($"Шкала \"{sc.Title}\" использована более, чем 1 раз");
        }
      }

      ErrMsg = sb.ToString();

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BtnSaveIsEnabled)));
    }

    private Graph graph;
    private Context ctx;
    private string errMsg;
  }
}
