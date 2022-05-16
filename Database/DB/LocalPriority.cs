namespace Database.DB
{
  public class LocalPriority
  {
    public LocalPriority() {}

    internal LocalPriority(Element a, Element b, double value) {
      A = a;
      B = b;
      Value = value;
    }

    public int GraphId { get; set; }
    public int AId { get; set; }
    public int BId { get; set; }
    public double Value { get; set; }

    public Element A { get; set; }
    public Element B { get; set; }
  }
}
