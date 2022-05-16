using System.Collections.Generic;

namespace Database.DB
{
  public class Layer
  {
    public int Id { get; set; }
    public int GraphId { get; set; }
    public int Ind { get; set; }

    public Graph Graph { get; set; }
    public List<Element> Elements { get; set; } = new List<Element>();
  }
}
