using Database.DB;

namespace AHP.GraphViewModels
{
  public abstract class ScaleValueBaseGVM {
    internal abstract void RemoveSelfFromScale();
    internal abstract ScaleValue ScaleValue { get; }
    internal abstract Element AttachedElement { get; set; }
  }
}
