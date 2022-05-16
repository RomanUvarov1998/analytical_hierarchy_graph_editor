using System.ComponentModel.DataAnnotations.Schema;

namespace Database.DB
{
  public abstract class ScaleValue
  {
    public int Id { get; set; }
    public int ScaleId { get; set; }
    public int? ElementId { get; set; }
    public int Ind { get; set; }

    public Scale Scale { get; set; }
    public Element Element { get; set; }

    internal abstract bool IsCompleted { get; }

    public abstract string GetElementTitle();
  }
}
