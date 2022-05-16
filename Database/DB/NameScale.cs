using System.Collections.Generic;
using System.Linq;

namespace Database.DB
{
  public class NameScale : Scale
  {
    public List<NameScaleValue> NameScaleValues { get; set; } = new List<NameScaleValue>();

    internal override bool IsCompleted
    {
      get
      {
        if (!NameScaleValues.All(scv => scv.IsCompleted)) {
          return false;
        }

        List<string> unique_names = NameScaleValues.Select(scv => scv.ValueName).Distinct().ToList();

        return unique_names.Count == NameScaleValues.Count();
      }
    }
  }
}
