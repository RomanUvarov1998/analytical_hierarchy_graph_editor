using Database.DB;
using System;
using System.ComponentModel;

namespace PregnancyMontoring
{
  public class QuestionNoScaleTestVM : QuestionBaseTestVM
  {
    internal QuestionNoScaleTestVM(Question q, Answer ans, Action on_answered) : base(q, ans, on_answered) {

    }


    //----------------------------- GUI -------------------------------

    public string AnswerContent
    {
      get => Answer.Content;
      set
      {
        Answer.Content = value;
        on_answered.Invoke();
      }
    }


    //----------------------------- API -------------------------------

    internal override bool IsCorrect => !q.IsRequired || string.IsNullOrEmpty(AnswerContent?.Trim()) == false;

    internal override string TemplateName => "QuestionNoScaleTestVMTemplate";


    //----------------------------- Private members -------------------------------
  }
}
