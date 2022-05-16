using System.ComponentModel.DataAnnotations.Schema;

namespace Database.DB
{
  public class Element
  {
    public Element() {}

    public Element(string title) {
      Title = title;
    }

    public int Id { get; set; }
    public int LayerId { get; set; }
    public string Title { get; set; }
    public int Ind { get; set; }

    public Layer Layer { get; set; }
    public ScaleValue ScaleValue { get; set; }

    public override string ToString() => Title;

    [NotMapped]
    public double GlobalPriority { get; set; }
  }
}
