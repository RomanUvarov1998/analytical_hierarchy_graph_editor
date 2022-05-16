using System.Collections.Generic;

namespace Database.DB
{
  public abstract class Scale
  {
    public int Id { get; set; }
    public int GraphId { get; set; }
    public string Title { get; set; }
    public int Ind { get; set; }

    public Graph Graph { get; set; }
    public List<ScaleValue> ScaleValues { get; set; } = new List<ScaleValue>();

    internal abstract bool IsCompleted { get; }
  }
}
