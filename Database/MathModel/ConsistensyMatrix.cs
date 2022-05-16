using System;
using System.Diagnostics;
using System.Linq;

namespace Database.MathModel
{
  public class ConsistensyMatrix
  {
    public ConsistensyMatrix(int n) {
      Debug.Assert(n >= 2);

      N = n;

      Weights = new double[N];
      RatesMatrix = new double[N, N];
      MatrixCR = double.NaN;

      if (N == 1) {
        Weights[0] = 1;
      } else {
        for (int i = 0; i < N; i++) {
          Weights[i] = double.NaN;
        }
      }

      for (int row = 0; row < N; row++) {
        for (int col = 0; col < N; col++) {
          RatesMatrix[row, col] = ( row == col ) ? 1.0 : double.NaN;
        }
      }
    }

    public bool AllRatesAreGiven
    {
      get
      {
        for (int i = 0; i < RatesMatrix.GetLength(0); i++) {
          for (int j = 0; j < RatesMatrix.GetLength(1); j++) {
            if (double.IsNaN(RatesMatrix[i, j])) {
              return false;
            }
          }
        }
        return true;
      }
    }

    public bool AllWeightsAreCounted => Weights.All(w => !double.IsNaN(w));

    public bool RateIsValid(double rate) {
      return MIN_RATE <= rate && rate <= MAX_RATE;
    }

    public void CalculateWeights() {
      // find w
      Weights = new double[N];

      // collect in w products of rows
      for (int row = 0; row < N; row++) {
        Weights[row] = 1;
        for (int col = 0; col < N; col++) {
          Weights[row] *= RatesMatrix[row, col];
        }
      }

      // calc w values
      double w_sum = Weights.Select(w => Math.Pow(w, 2)).Sum();
      double pow = 1.0 / (double)N;
      for (int i = 0; i < N; i++) {
        Weights[i] = Math.Pow(Weights[i], pow);
        Weights[i] /= w_sum;
      }
    }

    public void CalculateCR() {
      // find y
      double[] y = new double[N];

      // multiply count_matrix * w
      for (int row = 0; row < N; row++) {
        y[row] = 0;
        for (int col = 0; col < N; col++) {
          y[row] += RatesMatrix[row, col] * Weights[col];
        }
      }

      // find lambda_max
      double lambda_max = 0;
      for (int i = 0; i < N; i++) {
        lambda_max += y[i] / Weights[i];
      }
      lambda_max /= (double)N;

      double CI = ( lambda_max - N ) / ( N - 1 );
      MatrixCR = CI / RI_VALUES[N];
    }

    public double[] Weights { get; private set; }
    public double[,] RatesMatrix { get; private set; }
    internal double? MatrixCR { get; private set; }

    public bool IsConsistent => MatrixCR.HasValue && MatrixCR.Value < CR_MAX;

    public string GetConclusion() {
      if (!MatrixCR.HasValue) {
        return string.Empty;
      }

      if (IsConsistent) {
        return $"Индекс согласованности равен '{MatrixCR.Value.ToString("N15")}', " +
          $"что меньше или равно, чем '{CR_MAX.ToString(DOUBLE_FORMAT)}', матрица согласована.";
      }
      else {
        return $"Индекс согласованности равен '{MatrixCR.Value.ToString("N15")}', " +
          $"что больше, чем '{CR_MAX.ToString(DOUBLE_FORMAT)}', матрица несогласована.";
      }
    }

    public const double CR_MAX = 0.1;


    //----------------------------- Private members -------------------------------

    private const double MAX_RATE = 9.0;
    private const double MIN_RATE = 1.0 / MAX_RATE;
    private double[] RI_VALUES = new double[15] { 0.00, 0.00, 0.58, 0.90, 1.12, 1.24, 1.32, 1.41, 1.45, 1.49, 1.51, 1.48, 1.56, 1.57, 1.59 };
    private const string DOUBLE_FORMAT = "0.00";
    private int N;
  }
}
