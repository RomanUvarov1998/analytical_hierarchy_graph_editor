using Database.MathModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Database.DB
{
  public class Graph
  {
    //----------------------------- DB -------------------------------

    public Graph() {
      CreatedDate = DateTime.Now;
      UpdatedDate = DateTime.Now;
    }

    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public List<Layer> Layers { get; set; } = new List<Layer>();
    public List<LocalPriority> LocalPriorities { get; set; } = new List<LocalPriority>();
    public List<RelativeRate> RelativeRates { get; set; } = new List<RelativeRate>();
    public List<Scale> Scales { get; set; } = new List<Scale>();
    public List<Question> Questions { get; set; } = new List<Question>();
    public List<TestSession> TestSessions { get; set; } = new List<TestSession>();


    //----------------------------- API -------------------------------

    public void SortChildren() {
      Layers.Sort((lay1, lay2) => lay1.Ind.CompareTo(lay2.Ind));

      foreach (Layer layer in Layers) {
        layer.Elements.Sort((elt1, elt2) => elt1.Ind.CompareTo(elt2.Ind));
      }

      Scales.Sort((sc1, sc2) => sc1.Ind.CompareTo(sc2.Ind));

      foreach (Scale scale in Scales) {
        scale.ScaleValues.Sort((scv1, scv2) => scv1.Ind.CompareTo(scv2.Ind));
      }

      Questions.Sort((q1, q2) => q1.Ind.CompareTo(q2.Ind));

      TestSessions.Sort((ts1, ts2) => ts1.Date.CompareTo(ts2.Date));
    }

    public void UpdateInds() {
      for (int lay_i = 0; lay_i < Layers.Count; lay_i++) {
        Layers[lay_i].Ind = lay_i;
        for (int elt_i = 0; elt_i < Layers[lay_i].Elements.Count; elt_i++) {
          Layers[lay_i].Elements[elt_i].Ind = elt_i;
        }
      }

      List<RangeScale> range_scales = Scales.Where(sc => sc is RangeScale).Cast<RangeScale>().ToList();
      for (int sc_i = 0; sc_i < range_scales.Count; sc_i++) {
        range_scales[sc_i].Ind = sc_i;
      }

      List<NameScale> name_scales = Scales.Where(sc => sc is NameScale).Cast<NameScale>().ToList();
      for (int sc_i = 0; sc_i < range_scales.Count; sc_i++) {
        range_scales[sc_i].Ind = sc_i;
      }

      foreach (Scale sc in Scales) {
        for (int v_i = 0; v_i < sc.ScaleValues.Count; v_i++) {
          sc.ScaleValues[v_i].Ind = v_i;
        }
      }

      for (int q_i = 0; q_i < Questions.Count; q_i++) {
        Questions[q_i].Ind = q_i;
      }
    }

    public void SetLocalPriority(Element a, Element b, double value) {
      LocalPriority c = LocalPriorities.FirstOrDefault(c => c.A == a && c.B == b);
      if (c == null) {
        c = new LocalPriority(a, b, value);
        LocalPriorities.Add(c);
      }
      c.Value = value;
    }

    public double? GetLocalPriority(Element a, Element b) {
      return LocalPriorities.FirstOrDefault(c => c.A == a && c.B == b)?.Value;
    }

    public void SetRelativeRate(Element a, Element b, Element root, double rate) {
      RelativeRate c = RelativeRates.FirstOrDefault(c => c.A == a && c.B == b && c.Root == root);
      if (c == null) {
        c = new RelativeRate(a, b, root, rate);
        RelativeRates.Add(c);
      }
      c.Value = rate;

      RelativeRate c_sym = RelativeRates.FirstOrDefault(c => c.A == b && c.B == a && c.Root == root);
      if (c_sym == null) {
        c_sym = new RelativeRate(b, a, root, rate);
        RelativeRates.Add(c_sym);
      }
      c_sym.Value = 1.0 / rate;
    }

    public double? GetRelativeRate(Element a, Element b, Element root) {
      return RelativeRates.FirstOrDefault(c => c.A == a && c.B == b && c.Root == root)?.Value;
    }

    public ConsistensyMatrix ConsistensyMatrixForElement(Element elt, Layer connected_layer) {
      int N = connected_layer.Elements.Count;
      var cm = new ConsistensyMatrix(N);

      if (cm == null) return null;

      for (int row = 0; row < N; row++) {
        for (int col = 0; col < N; col++) {
          if (row == col) continue;

          cm.RatesMatrix[row, col] = GetRelativeRate(
            connected_layer.Elements[row],
            connected_layer.Elements[col],
            elt)
            ?? double.NaN;
          cm.RatesMatrix[col, row] = 1.0 / cm.RatesMatrix[row, col];
        }
      }

      if (cm.AllRatesAreGiven) {
        cm.CalculateWeights();

        for (int i = 0; i < connected_layer.Elements.Count; i++) {
          SetLocalPriority(connected_layer.Elements[i], elt, cm.Weights[i]);
        }

        cm.CalculateCR();
      }

      return cm;
    }

    public string GetErrMsgs() {
      var sb = new StringBuilder();

      if (!HasThreeOrMoreLayers) {
        sb.AppendLine("Должно быть хотя бы 3 слоя (цель, критерии, альтернативы)");
      }

      if (!HasOneGoal) {
        sb.AppendLine("Должен быть ровно один элемент, представляющий цель");
      }

      if (!EachCriteriumLayerHasTwoOrMoreCriteriums()) {
        sb.AppendLine("В каждом слое критериев должно быть хотя бы 2 элемента");
      }

      if (!AllRatesAreGiven()) {
        sb.AppendLine("Даны не все попарные оценки критериев");
      }
      else if (!AllLocalPrioritiesAreCounted()) {
        sb.AppendLine("Не все локальные приоритеты расчитаны");
      }
      else if (!IsConsistent()) {
        sb.AppendLine("Имеются несовместные матрицы");
      }

      if (!GoalAndAlternativesDontHaveScaleValue()) {
        sb.AppendLine("Только элементы, являющиеся критериями, могут быть диапазонами значений шкал");
      }

      if (!AllScaleValuesAreCorrect()) {
        sb.AppendLine("Не все значения пределов шкал указаны корректно");
      }

      if (!AllScalesAreUsed()) {
        sb.AppendLine("Не все диапазоны шкал использованы в графе");
      }

      return sb.ToString();
    }

    public bool IsCompleted() => HasThreeOrMoreLayers &&
                                    HasOneGoal &&
                                    EachCriteriumLayerHasTwoOrMoreCriteriums() &&
                                    AllRatesAreGiven() &&
                                    AllLocalPrioritiesAreCounted() &&
                                    IsConsistent() &&
                                    GoalAndAlternativesDontHaveScaleValue() &&
                                    AllScaleValuesAreCorrect() &&
                                    AllScalesAreUsed();

    public bool QuestionsCompleted() {
      if (Questions.Count == 0) return false;

      foreach (Scale sc in Scales) {
        if (Questions.Count(q => q.Scale == sc) > 1) {
          return false;
        }
      }

      return true;
    }

    public List<Element> GiveAnswer(List<ScaleValue> scale_values) {
      var matrices = new List<double[,]>();

      Func<Element, bool> is_excluded = elt => elt.ScaleValue != null && scale_values.Any(scv => scv.Scale == elt.ScaleValue.Scale) && !scale_values.Contains(elt.ScaleValue);

      for (int lay_i = 0; lay_i < Layers.Count - 1; lay_i++) {
        Layer curr_lay = Layers[lay_i];
        Layer next_lay = Layers[lay_i + 1];

        var mat = new double[curr_lay.Elements.Count, next_lay.Elements.Count];

        for (int b_i = 0; b_i < curr_lay.Elements.Count; b_i++) {
          Element b = curr_lay.Elements[b_i];

          bool b_is_excluded = is_excluded(b);

          for (int a_i = 0; a_i < next_lay.Elements.Count; a_i++) {
            Element a = next_lay.Elements[a_i];

            bool a_is_excluded = is_excluded(a);

            mat[b_i, a_i] = ( b_is_excluded  || a_is_excluded ) ? 0.0 : GetLocalPriority(a, b).Value;
          }
        }

        matrices.Add(mat);
      }

      var res = matrices[0];

      for (int i = 1; i < matrices.Count; i++) {
        res = MatProd(res, matrices[i]);
      }

      Layer last_lay = Layers.Last();
      for (int i = 0; i < last_lay.Elements.Count; i++) {
        last_lay.Elements[i].GlobalPriority = res[0, i];
      }

      // normalize
      double sum = last_lay.Elements.Select(elt => elt.GlobalPriority).Sum();
      for (int i = 0; i < last_lay.Elements.Count; i++) {
        last_lay.Elements[i].GlobalPriority /= sum;
      }

      return last_lay.Elements;
    }

    //----------------------------- Private members -------------------------------

    private bool HasThreeOrMoreLayers => Layers.Count >= 3;

    private bool HasOneGoal => Layers.Count > 0 && Layers[0].Elements.Count == 1;

    private bool EachCriteriumLayerHasTwoOrMoreCriteriums() {
      for (int lay_i = 1; lay_i < Layers.Count - 1; lay_i++) {
        if (Layers[lay_i].Elements.Count < 2) return false;
      }
      return true;
    }

    private bool AllRatesAreGiven() {
      for (int lay_i = 0; lay_i < Layers.Count - 1; lay_i++) {
        Layer next_layer = Layers[lay_i + 1];

        for (int elt_i = 0; elt_i < Layers[lay_i].Elements.Count; elt_i++) {
          Element root = Layers[lay_i].Elements[elt_i];

          foreach (Element a in next_layer.Elements) {
            foreach (Element b in next_layer.Elements) {
              if (a == b) continue;
              double? rate = GetRelativeRate(a, b, root);
              if (!rate.HasValue) return false;
            }
          }
        }
      }
      return true;
    }

    private bool AllLocalPrioritiesAreCounted() {
      for (int lay_i = 0; lay_i < Layers.Count - 1; lay_i++) {
        Layer next_layer = Layers[lay_i + 1];

        for (int elt_i = 0; elt_i < Layers[lay_i].Elements.Count; elt_i++) {
          Element b = Layers[lay_i].Elements[elt_i];

          foreach (Element a in next_layer.Elements) {
            double? rate = GetLocalPriority(a, b);
            if (!rate.HasValue) return false;
          }
        }
      }
      return true;
    }

    private bool IsConsistent() {
      for (int lay_i = 0; lay_i < Layers.Count - 1; lay_i++) {
        for (int elt_i = 0; elt_i < Layers[lay_i].Elements.Count; elt_i++) {
          Element elt = Layers[lay_i].Elements[elt_i];

          if (Layers[lay_i + 1].Elements.Count < 2) {
            return false;
          }
          else {
            ConsistensyMatrix cm = ConsistensyMatrixForElement(elt, Layers[lay_i + 1]);
            if (!cm.AllRatesAreGiven) {
              return false;
            }
            else {
              if (!cm.IsConsistent) return false;
            }
          }
        }
      }
      return true;
    }

    private bool GoalAndAlternativesDontHaveScaleValue() {
      if (Layers.Count <= 2) return true;
      Layer goal = Layers[0];
      Layer alternatives = Layers.Last();
      return goal.Elements.All(elt => elt.ScaleValue == null) &&
        alternatives.Elements.All(elt => elt.ScaleValue == null);
    }

    private bool AllScaleValuesAreCorrect() => Scales.All(sc => sc.IsCompleted);

    private bool AllScalesAreUsed() {
      foreach (Scale sc in Scales) {
        foreach (ScaleValue scv in sc.ScaleValues) {
          bool is_used = false;
          for (int lay_i = 1; lay_i < Layers.Count - 1; lay_i++) {
            if (Layers[lay_i].Elements.Any(elt => elt.ScaleValue == scv)) {
              is_used = true;
              break;
            }
          }
          if (!is_used) return false;
        }
      }
      return true;
    }
  
    private double[,] MatProd(double[,] a, double[,] b) {
      var res = new double[a.GetLength(0), b.GetLength(1)];
      Debug.Assert(a.GetLength(1) == b.GetLength(0));
      int N = b.GetLength(0);

      for (int row = 0; row < a.GetLength(0); row++) {
        for (int col = 0; col < b.GetLength(1); col++) {
          res[row, col] = 0.0;
          for (int i = 0; i < N; i++) {
            res[row, col] += a[row, i] * b[i, col];
          }
        }
      }

      return res;
    }
  }
}
