using Database.DB;
using System;
using System.Linq;

namespace AHP.TableViewModels
{
  public class AnswerTVM
  {
    internal AnswerTVM(Answer ans) {
      Answer = ans;
    }

    internal Answer Answer { get; }

    public string Question => Answer.Question.Content;

    public string Ans
    {
      get
      {
        if (Answer.ScaleValue == null) {
          return Answer.Content;
        }
        else if (Answer.ScaleValue is RangeScaleValue range_scv) {
          return $"\"{range_scv.Scale.Title}\" '{range_scv.Min}' - '{range_scv.Max}'";
        }
        else if (Answer.ScaleValue is NameScaleValue name_scv) {
          return $"\"{name_scv.Scale.Title}\" '{name_scv.ValueName}'";
        }
        else {
          throw new NotImplementedException();
        }
      }
    }
  }
}
