using System.ComponentModel.DataAnnotations.Schema;

namespace Database.DB
{
  public class NameScaleValue : ScaleValue
  {
    public string ValueName { get; set; }

    public NameScale NameScale { get; set; }

    internal override bool IsCompleted => !string.IsNullOrEmpty(ValueName);
    [NotMapped] public bool Excluded { get; set; }

    public override string GetElementTitle() => $"\"{Scale.Title}\" '{ValueName}'";
  }
}
