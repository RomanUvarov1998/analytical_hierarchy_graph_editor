using Database.DB;
using System;

namespace PregnancyMontoring
{
  public abstract class QuestionBaseTestVM
  {
    internal QuestionBaseTestVM(Question q, Answer ans, Action on_answered) {
      this.on_answered = on_answered;
      this.Answer = ans;
      this.q = q;
    }

    internal abstract string TemplateName { get; }
    internal abstract bool IsCorrect { get; }
    internal Answer Answer { get; }
    protected Action on_answered;
    protected Question q;

  }
}
