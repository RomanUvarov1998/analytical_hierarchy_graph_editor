using System;

namespace AHP.ViewModels.ElementState
{
  public abstract class ConnectedElementStateBase : ElementStateBase
  {
    internal ConnectedElementStateBase(Consistensy cons) {
      Consistensy = cons;
    }

    public Consistensy Consistensy { get; set; }

    public override string Tooltip
    {
      get
      {
        switch (Consistensy) {
          case Consistensy.NotEnoughSubCriteriums: return "Должно быть хотя бы 2 подкритерия/альтернативы";
          case Consistensy.NotAllConnectionsRated: return "Не все относительные приоритеты указаны";
          case Consistensy.Inconsistent: return "Матрица несогласована";
          case Consistensy.Consistent: return "Матрица согласована";
          default: throw new InvalidOperationException();
        }
      }
    }
  }
}
