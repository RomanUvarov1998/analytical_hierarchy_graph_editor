using System.Collections.Generic;
using System.Linq;

namespace Database.DB
{
  public class RangeScale : Scale
  {
    public List<RangeScaleValue> RangeScaleValues { get; set; } = new List<RangeScaleValue>();

    internal override bool IsCompleted
    {
      get
      {
        for (int i = 0; i < RangeScaleValues.Count; i++) {
          if (!RangeScaleValues[i].IsCompleted) return false;
          if (i < RangeScaleValues.Count - 1 && RangeScaleValues[i].Max > RangeScaleValues[i + 1].Min) return false;
        }

        return true;
      }
    }
  }
}
