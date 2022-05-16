namespace Database.DB
{
  public class Answer
  {
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public int TestSessionId { get; set; }
    public int? ScaleValueId { get; set; }
    public string Content { get; set; }

    public Question Question { get; set; }
    public ScaleValue ScaleValue { get; set; }
  }
}
