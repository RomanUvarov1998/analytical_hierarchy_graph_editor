namespace Database.DB
{
  public class RelativeRate
  {
    public RelativeRate() {}

    internal RelativeRate(Element a, Element b, Element root, double value) {
      A = a;
      B = b;
      Root = root;
      Value = value;
    }

    public int GraphId { get; set; }
    public int AId { get; set; }
    public int BId { get; set; }
    public int RootId { get; set; }
    public double Value { get; set; }

    public Element A { get; set; }
    public Element B { get; set; }
    public Element Root { get; set; }
  }
}
