using Database.DB;
using System.Collections.Generic;
using System.Linq;

namespace PregnancyMontoring.TableViewModels
{
  public class TestSessionTVM
  {
    internal TestSessionTVM(TestSession ts) {
      TestSession = ts;
      Answers = ts.Answers.Select(a => new AnswerTVM(a)).ToList();
    }

    internal TestSession TestSession { get; }

    public string DateStr => TestSession.Date.ToString("dd.MM.yyyy hh:mm");
    public string Result => TestSession.Result;
    public List<AnswerTVM> Answers { get; }
  }
}
