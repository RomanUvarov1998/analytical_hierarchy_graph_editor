using Database.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleTest
{
  class Program
  {
    static void Main(string[] args) {
      Console.WriteLine("Starting...");

      using (var ctx = new Context()) {
        ctx.EnsureDatabaseCreated();
        var g = new Graph();

        var sc1 = new RangeScale();
        sc1.RangeScaleValues.Add(new RangeScaleValue()
        {
          Min = 0,
          Max = 1,
        });
        sc1.RangeScaleValues.Add(new RangeScaleValue()
        {
          Min = 2,
          Max = 3,
        });
        g.Scales.Add(sc1);

        var sc2 = new NameScale();
        sc2.NameScaleValues.Add(new NameScaleValue()
        {
          ValueName = "Red",
        });
        sc2.NameScaleValues.Add(new NameScaleValue()
        {
          ValueName = "Green",
        });
        g.Scales.Add(sc2);

        ctx.Add(g);
        ctx.SaveChanges();
      }

      using (var ctx = new Context()) {
        List<Graph> graphs = ctx.Graphs.Include(g => g.Scales).ThenInclude(sc => sc.ScaleValues).ToList();
      }

      Console.WriteLine("Completed!");
    }
  }
}
