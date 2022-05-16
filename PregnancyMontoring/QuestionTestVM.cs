using Database.DB;
using System;
using System.Windows;

namespace PregnancyMontoring
{
  public class QuestionTestVM
  {
    internal QuestionTestVM(Question q, Action on_answered, Graph graph) {
      Question = q;
      Answer = new Answer()
      {
        Question = q,
      };

      if (q.Scale == null) {
        QuestionScaleTestVM = new QuestionNoScaleTestVM(Question, Answer, on_answered);
      }
      else if (q.Scale is RangeScale range_scale) {
        QuestionScaleTestVM = new QuestionRangeScaleTestVM(Question, Answer, on_answered);
      }
      else if (q.Scale is NameScale name_scale) {
        QuestionScaleTestVM = new QuestionNameScaleTestVM(Question, Answer, on_answered);
      }
      else {
        throw new NotImplementedException();
      }
    }


    //----------------------------- API -------------------------------

    internal Question Question { get; }
    internal Answer Answer { get; }
    internal bool IsCorrect => QuestionScaleTestVM.IsCorrect;


    //----------------------------- GUI -------------------------------

    public Scale QuestionScale => Question.Scale;

    public QuestionBaseTestVM QuestionScaleTestVM { get; private set; }

    public Visibility ScaleValuesVisibility => Question.Scale == null ? Visibility.Collapsed : Visibility.Visible;

    public string Asterisk => Question.IsRequired ? "*" : string.Empty;
    
    public string Content => Question.Content;


    //----------------------------- Private members -------------------------------
  }
}
