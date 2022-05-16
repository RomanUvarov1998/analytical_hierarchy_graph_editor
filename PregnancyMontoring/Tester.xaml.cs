using Database.DB;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace PregnancyMontoring
{
  /// <summary>
  /// Interaction logic for Tester.xaml
  /// </summary>
  public partial class Tester : Window, INotifyPropertyChanged
  {
    public Tester(int graph_id) {
      InitializeComponent();

      ctx = new Context();
      graph = ctx.LoadGraphForTesting(graph_id);

      Questions = graph.Questions.Select(q => new QuestionTestVM(q, UpdateSaveBtnAndToolTip, graph)).ToList();

      DataContext = this;
    }


    //----------------------------- GUI -------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public List<QuestionTestVM> Questions { get; }

    public string Result
    {
      get => result;
      set
      {
        result = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Result)));
      }
    }

    public List<Element> WeightedAlternatives
    {
      get => weightedAlternatives;
      set
      {
        weightedAlternatives = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WeightedAlternatives)));
      }
    }


    //----------------------------- Events -------------------------------

    private void Button_SaveBtn_Click(object sender, RoutedEventArgs e) {
      if (Questions.All(q => q.IsCorrect) == false) {
        MessageBox.Show("Необходимо ответить на все обязательные вопросы (помеченные '*')");
        return;
      }

      var ts = new TestSession()
      {
        Graph = graph,
        Answers = Questions.Select(q => q.Answer).ToList(),
        Result = this.Result,
      };
      ctx.Add(ts);
      ctx.SaveChanges();
      ctx.Dispose();
      DialogResult = true;
    }


    //----------------------------- Private members -------------------------------

    private void UpdateSaveBtnAndToolTip() {
      Result = string.Empty;

      if (Questions.All(q => q.IsCorrect) == false) {
        return;
      }

      List<ScaleValue> scale_values = Questions
          .Select(q => q.Answer.ScaleValue)
          .Where(sv => sv != null)
          .ToList();

      WeightedAlternatives = new List<Element>();
      WeightedAlternatives = graph.GiveAnswer(scale_values);

      Element best = WeightedAlternatives[0];
      for (int i = 1; i < WeightedAlternatives.Count; i++) {
        if (best.GlobalPriority < WeightedAlternatives[i].GlobalPriority) {
          best = WeightedAlternatives[i];
        }
      }

      Result = best.Title;
    }

    private Graph graph;
    private Context ctx;
    private string result = string.Empty;
    private List<Element> weightedAlternatives = new List<Element>();
  }
}
