using Database.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PregnancyMontoring
{
  public class QuestionNameScaleTestVM : QuestionBaseTestVM, INotifyPropertyChanged
  {
    internal QuestionNameScaleTestVM(Question q, Answer ans, Action on_answered) : base(q, ans, on_answered) {
      ScaleValues = ( (NameScale)q.Scale ).NameScaleValues.Select(scv => new NameScaleValueTestVM(scv, OnValueSelected)).ToList();
    }


    //----------------------------- API -------------------------------

    internal override bool IsCorrect => !q.IsRequired || q.IsRequired && string.IsNullOrEmpty(AnswerContent?.Trim()) == false;

    internal override string TemplateName => "QuestionNameScaleTestVMTemplate";


    //----------------------------- GUI -------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public bool AnotherAnswerIsGiven
    {
      get => anotherAnswerIsGiven;
      set
      {
        OnValueSelected(null);

        anotherAnswerIsGiven = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AnotherAnswerIsGiven)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScaleValuesAreEnabled)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AnotherAnswerTextBoxIsEnabled)));

        if (value) {
          foreach (NameScaleValueTestVM scv in ScaleValues) {
            scv.IsSelected = false;
          }
        }
      }
    }

    public bool ScaleValuesAreEnabled => AnotherAnswerIsGiven == false;
    public bool AnotherAnswerTextBoxIsEnabled => AnotherAnswerIsGiven == true;

    public string AnswerContent {
      get => Answer.Content;
      set
      {
        Answer.Content = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AnswerContent)));
      }
    }

    public List<NameScaleValueTestVM> ScaleValues { get; }

    private void OnValueSelected(NameScaleValue value) {
      Answer.ScaleValue = value;
      AnswerContent = value?.ValueName;
      on_answered.Invoke();
    }


    //----------------------------- Private members -------------------------------

    private bool anotherAnswerIsGiven = false;
  }
}
