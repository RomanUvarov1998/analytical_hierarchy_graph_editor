using System;
using System.Collections.Generic;

namespace Database.DB
{
  public class TestSession
  {
    public TestSession() {
      Date = DateTime.Now;
    }

    public int Id { get; set; }
    public int GraphId { get; set; }
    public DateTime Date { get; set; }
    public string Result { get; set; }

    public Graph Graph { get; set; }
    public List<Answer> Answers { get; set; } = new List<Answer>();
  }
}
