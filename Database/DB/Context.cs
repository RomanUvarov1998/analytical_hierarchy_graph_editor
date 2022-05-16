using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Database.DB
{
  public class Context : DbContext
  {
    public DbSet<Graph> Graphs { get; set; }
    public DbSet<Layer> Layers { get; set; }
    public DbSet<Element> Elements { get; set; }
    public DbSet<LocalPriority> LocalPriorities { get; set; }
    public DbSet<RelativeRate> RelativeRates { get; set; }

    public DbSet<Scale> Scales { get; set; }
    public DbSet<ScaleValue> ScaleValues { get; set; }

    public DbSet<RangeScale> RangeScales { get; set; }
    public DbSet<RangeScaleValue> RangeScaleValues { get; set; }

    public DbSet<NameScale> NameScales { get; set; }
    public DbSet<NameScaleValue> NameScaleValues { get; set; }

    public DbSet<Question> Questions { get; set; }
    public DbSet<TestSession> TestSessions { get; set; }
    public DbSet<Answer> Answers { get; set; }

    public string DbPath { get; }

    public const string DB_FOLDER_NAME = "AHP";
    public const string DB_NAME = "database.db";

    public Context() {
      string app_data_folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      string db_folder = Path.Combine(app_data_folder, DB_FOLDER_NAME);
      if (!Directory.Exists(db_folder)) {
        Directory.CreateDirectory(db_folder);
      }
      DbPath = Path.Combine(db_folder, DB_NAME);

      this.ChangeTracker.LazyLoadingEnabled = false;
    }

    public bool DatabaseExists() {
      return Database.GetService<IRelationalDatabaseCreator>().Exists();
    }

    public void EnsureDatabaseCreated() {
      Database.EnsureCreated();
    }

    public Graph LoadGraphForEdiding(int id) {
      Graph g = Graphs
        .Include(g => g.Scales).ThenInclude(sc => sc.ScaleValues).ThenInclude(scv => scv.Scale)
        .Include(g => g.Scales).ThenInclude(sc => sc.Graph)
        .Include(g => g.Layers).ThenInclude(lay => lay.Elements).ThenInclude(elt => elt.ScaleValue)
        .Include(g => g.RelativeRates).ThenInclude(rr => rr.A)
        .Include(g => g.RelativeRates).ThenInclude(rr => rr.B)
        .Include(g => g.RelativeRates).ThenInclude(rr => rr.Root)
        .Include(g => g.LocalPriorities).ThenInclude(lp => lp.A)
        .Include(g => g.LocalPriorities).ThenInclude(lp => lp.B)

        .FirstOrDefault(g => g.Id == id);

      if (g != null) {
        g.SortChildren();
      }

      return g;
    }

    public Graph LoadGraphForTesting(int id) {
      Graph g = Graphs
        .Include(g => g.Scales).ThenInclude(sc => sc.ScaleValues).ThenInclude(scv => scv.Scale)
        .Include(g => g.Scales).ThenInclude(sc => sc.Graph)
        .Include(g => g.Layers).ThenInclude(lay => lay.Elements).ThenInclude(elt => elt.ScaleValue)
        .Include(g => g.RelativeRates).ThenInclude(rr => rr.A)
        .Include(g => g.RelativeRates).ThenInclude(rr => rr.B)
        .Include(g => g.RelativeRates).ThenInclude(rr => rr.Root)
        .Include(g => g.LocalPriorities).ThenInclude(lp => lp.A)
        .Include(g => g.LocalPriorities).ThenInclude(lp => lp.B)

        .Include(g => g.Scales).ThenInclude(sc => sc.ScaleValues).ThenInclude(scv => scv.Scale)
        .Include(g => g.Questions).ThenInclude(q => q.Scale).ThenInclude(sc => sc.ScaleValues).ThenInclude(scv => scv.Scale)

        .FirstOrDefault(g => g.Id == id);

      if (g != null) {
        g.SortChildren();
      }

      return g;
    }

    public List<Graph> LoadGraphs() {
      List<Graph> graphs = Graphs
        .Include(g => g.Scales).ThenInclude(sc => sc.ScaleValues).ThenInclude(scv => scv.Scale)
        .Include(g => g.Scales).ThenInclude(sc => sc.Graph)
        .Include(g => g.Layers).ThenInclude(lay => lay.Elements).ThenInclude(elt => elt.ScaleValue)
        .Include(g => g.RelativeRates).ThenInclude(rr => rr.A)
        .Include(g => g.RelativeRates).ThenInclude(rr => rr.B)
        .Include(g => g.RelativeRates).ThenInclude(rr => rr.Root)
        .Include(g => g.LocalPriorities).ThenInclude(lp => lp.A)
        .Include(g => g.LocalPriorities).ThenInclude(lp => lp.B)

        .Include(g => g.Scales).ThenInclude(sc => sc.ScaleValues).ThenInclude(scv => scv.Scale)
        .Include(g => g.Questions).ThenInclude(q => q.Scale).ThenInclude(sc => sc.ScaleValues).ThenInclude(scv => scv.Scale)

        .Include(g => g.TestSessions).ThenInclude(ts => ts.Answers).ThenInclude(a => a.Question)
        .Include(g => g.TestSessions).ThenInclude(ts => ts.Answers).ThenInclude(a => a.ScaleValue).ThenInclude(scv => scv.Scale)

        .ToList();

      foreach (Graph g in graphs) {
        g.SortChildren();
      }

      return graphs;
    }

    public async Task<List<Graph>> LoadGraphsAsync() {
      List<Graph> graphs = await Graphs
        .Include(g => g.Scales).ThenInclude(sc => sc.ScaleValues).ThenInclude(scv => scv.Scale)
        .Include(g => g.Scales).ThenInclude(sc => sc.Graph)
        .Include(g => g.Layers).ThenInclude(lay => lay.Elements).ThenInclude(elt => elt.ScaleValue)
        .Include(g => g.RelativeRates).ThenInclude(rr => rr.A)
        .Include(g => g.RelativeRates).ThenInclude(rr => rr.B)
        .Include(g => g.RelativeRates).ThenInclude(rr => rr.Root)
        .Include(g => g.LocalPriorities).ThenInclude(lp => lp.A)
        .Include(g => g.LocalPriorities).ThenInclude(lp => lp.B)

        .Include(g => g.Scales).ThenInclude(sc => sc.ScaleValues).ThenInclude(scv => scv.Scale)
        .Include(g => g.Questions).ThenInclude(q => q.Scale).ThenInclude(sc => sc.ScaleValues).ThenInclude(scv => scv.Scale)

        .Include(g => g.TestSessions).ThenInclude(ts => ts.Answers).ThenInclude(a => a.Question)
        .Include(g => g.TestSessions).ThenInclude(ts => ts.Answers).ThenInclude(a => a.ScaleValue).ThenInclude(scv => scv.Scale)

        .ToListAsync();

      foreach (Graph g in graphs) {
        g.SortChildren();
      }

      return graphs;
    }

    public Graph LoadGraphWithQuestions(int id) {
      Graph g = Graphs

        .Include(g => g.Scales).ThenInclude(sc => sc.ScaleValues).ThenInclude(scv => scv.Scale)
        .Include(g => g.Questions).ThenInclude(q => q.Scale).ThenInclude(sc => sc.ScaleValues).ThenInclude(scv => scv.Scale)

        .FirstOrDefault(g => g.Id == id);

      if (g != null) {
        g.SortChildren();
      }

      return g;
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options) {
      options.UseSqlite($"Data Source={DbPath}");
      options.EnableSensitiveDataLogging(false);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      modelBuilder.Entity<Graph>()
          .HasMany(g => g.Layers)
          .WithOne(lay => lay.Graph);
      modelBuilder.Entity<Graph>()
          .HasMany(g => g.LocalPriorities)
          .WithOne();
      modelBuilder.Entity<Graph>()
          .HasMany(g => g.RelativeRates)
          .WithOne();
      modelBuilder.Entity<Graph>()
          .HasMany(g => g.Scales)
          .WithOne(sc => sc.Graph);
      modelBuilder.Entity<Graph>()
          .HasMany(g => g.TestSessions)
          .WithOne(sc => sc.Graph);
      modelBuilder.Entity<Graph>()
          .HasMany(g => g.Questions)
          .WithOne(q => q.Graph);

      modelBuilder.Entity<Layer>()
          .HasMany(lay => lay.Elements)
          .WithOne(elt => elt.Layer);

      modelBuilder.Entity<Element>()
          .HasOne(scv => scv.ScaleValue)
          .WithOne(elt => elt.Element)
          .HasForeignKey<ScaleValue>(e => e.ElementId);

      modelBuilder.Entity<LocalPriority>()
          .HasKey(p => new { p.GraphId, p.AId, p.BId });
      modelBuilder.Entity<LocalPriority>()
          .HasOne(p => p.A)
          .WithMany();
      modelBuilder.Entity<LocalPriority>()
          .HasOne(p => p.B)
          .WithMany();

      modelBuilder.Entity<RelativeRate>()
          .HasKey(p => new { p.GraphId, p.AId, p.BId, p.RootId });
      modelBuilder.Entity<RelativeRate>()
          .HasOne(p => p.A)
          .WithMany();
      modelBuilder.Entity<RelativeRate>()
          .HasOne(p => p.B)
          .WithMany();
      modelBuilder.Entity<RelativeRate>()
          .HasOne(p => p.Root)
          .WithMany();


      modelBuilder.Entity<RangeScale>()
          .HasMany(sc => sc.RangeScaleValues)
          .WithOne(scv => scv.RangeScale)
          .HasForeignKey(e => e.ScaleId);

      modelBuilder.Entity<NameScale>()
          .HasMany(sc => sc.NameScaleValues)
          .WithOne(scv => scv.NameScale)
          .HasForeignKey(e => e.ScaleId);


      modelBuilder.Entity<RangeScale>()
          .HasBaseType(typeof(Scale));

      modelBuilder.Entity<NameScale>()
          .HasBaseType(typeof(Scale));

      modelBuilder.Entity<RangeScaleValue>()
        .HasBaseType(typeof(ScaleValue));

      modelBuilder.Entity<NameScaleValue>()
        .HasBaseType(typeof(ScaleValue));


      modelBuilder.Entity<Scale>()
        .HasMany(sc => sc.ScaleValues)
        .WithOne(scv => scv.Scale)
        .HasForeignKey(scv => scv.ScaleId);


      modelBuilder.Entity<TestSession>()
          .HasMany(g => g.Answers)
          .WithOne();

      modelBuilder.Entity<Question>()
          .HasOne(q => q.Scale)
          .WithOne()
          .HasForeignKey<Question>(q => q.ScaleId);
      modelBuilder.Entity<Question>()
          .HasMany(q => q.Answers)
          .WithOne(a => a.Question)
          .HasForeignKey(a => a.QuestionId);
    }


    //----------------------------- Private members -------------------------------

  }
}
