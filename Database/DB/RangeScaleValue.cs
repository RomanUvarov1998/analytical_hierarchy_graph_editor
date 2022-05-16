using System.ComponentModel.DataAnnotations.Schema;

namespace Database.DB
{
  public class RangeScaleValue : ScaleValue
  {
    public double Min { get; set; }
    public double Max { get; set; }

    public RangeScale RangeScale { get; set; }

    internal override bool IsCompleted => !double.IsNaN(Min) && !double.IsNaN(Max) && Min <= Max;

    public override string GetElementTitle() => $"\"{Scale.Title}\" '{Min.ToString("0.00")}' - '{Max.ToString("0.00")}'";
  }
}
