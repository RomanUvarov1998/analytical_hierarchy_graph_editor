using Database.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PregnancyMontoring
{
  public class QuestionRangeScaleTestVM : QuestionBaseTestVM, INotifyPropertyChanged
  {
    internal QuestionRangeScaleTestVM(Question q, Answer ans, Action on_answered) : base(q, ans, on_answered) {
      ScaleValues = ( (RangeScale)q.Scale ).RangeScaleValues.Select(scv => new RangeScaleValueTestVM(scv)).ToList();
    }


    //----------------------------- API -------------------------------

    internal override bool IsCorrect => IsValid && ( q.IsRequired && !string.IsNullOrEmpty(AnswerContent?.Trim()) || !q.IsRequired );
    
    internal bool IsValid => ErrMsg == string.Empty;

    internal override string TemplateName => "QuestionRangeScaleTestVMTemplate";


    //----------------------------- GUI -------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public string ErrMsg
    {
      get => errMsg;
      set
      {
        errMsg = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrMsg)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrMsgVisibility)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AnswerContentBrush)));
      }
    }

    public Visibility ErrMsgVisibility => ErrMsg == string.Empty ? Visibility.Collapsed : Visibility.Visible;

    public Brush AnswerContentBrush => IsValid ? Brushes.Black : Brushes.Red;

    public string AnswerContent
    {
      get => Answer.Content;
      set
      {
        Answer.Content = value;

        foreach (RangeScaleValueTestVM scv in ScaleValues) {
          scv.IsSelected = false;
        }

        if (double.TryParse(value, out double res)) {
          RangeScaleValueTestVM according_value = ScaleValues.FirstOrDefault(scv => scv.Min <= res && res <= scv.Max);
          Answer.ScaleValue = according_value?.ScaleValue;
          if (according_value != null) {
            according_value.IsSelected = true;
          }
          ErrMsg = string.Empty;
        }
        else {
          Answer.ScaleValue = null;
          ErrMsg = "Значение должно быть дробным числом";
        }

        on_answered.Invoke();
      }
    }

    public List<RangeScaleValueTestVM> ScaleValues { get; }


    //----------------------------- Private members -------------------------------

    private string errMsg = string.Empty;
  }
}
