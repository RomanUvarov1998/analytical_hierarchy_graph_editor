using System.Collections.Generic;

namespace Database.DB
{
  public class Question
  {
    public int Id { get; set; }
    public int GraphId { get; set; }
    public int? ScaleId { get; set; }
    public string Content { get; set; }
    public bool IsRequired { get; set; }
    public int Ind { get; set; }

    public Graph Graph { get; set; }
    public Scale Scale { get; set; }
    public List<Answer> Answers { get; set; } = new List<Answer>();
  }
}
