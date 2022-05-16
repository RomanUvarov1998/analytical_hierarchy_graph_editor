using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using AHP.ViewModels;
using Database.MathModel;
using Database.DB;

namespace AHP
{
  /// <summary>
  /// Interaction logic for ElementEditor.xaml
  /// </summary>
  public partial class ConsistensyMatrixEditor : Window, INotifyPropertyChanged
  {
    public ConsistensyMatrixEditor(ElementVM element, IReadOnlyList<ElementVM> connected_elements, Graph graph) {
      InitializeComponent();

      this.connected_elements = connected_elements;
      this.element = element;

      input_fields = new TextBox[connected_elements.Count, connected_elements.Count];

      Title = $"Редактированике элемента '{element.Title}'";

      cm = new ConsistensyMatrix(connected_elements.Count);

      for (int i = 0; i < connected_elements.Count; i++) {
        double? w = graph.GetLocalPriority(connected_elements[i].Element, element.Element);
        if (w.HasValue) 
          cm.Weights[i] = w.Value;
      }

      for (int i_a = 0; i_a < connected_elements.Count; i_a++) {
        for (int i_b = 0; i_b < connected_elements.Count; i_b++) {
          if (i_a == i_b) continue;
          double? rate = graph.GetRelativeRate(connected_elements[i_a].Element, connected_elements[i_b].Element, element.Element);
          if (rate.HasValue)
            cm.RatesMatrix[i_a, i_b] = rate.Value;
        }
      }

      Loaded += ConsistensyMatrixEditor_Loaded;
    }


    //----------------------------- API -------------------------

    internal ConsistensyMatrix ConsistensyMatrix => cm;

    //----------------------------- GUI -------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public string A
    {
      get => a;
      set
      {
        a = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Instruction)));
      }
    }

    public string B
    {
      get => b;
      set
      {
        b = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Instruction)));
      }
    }

    public string Instruction =>
@$"1: '{A}' и '{B}' одинаково важны по отношению к '{element.Title}';
3: '{A}' незначительно важнее, чем '{B}', по отношению к '{element.Title}';
5: '{A}' значительно важнее '{B}' по отношению к '{element.Title}';
7: '{A}' явно важнее '{B}' по отношению к '{element.Title}';
9: '{A}' по своей значительности абсолютно превосходит '{B}' по отношению к '{element.Title}';";

    public string Conclusion
    {
      get => conclusion;
      set
      {
        conclusion = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Conclusion)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConclusionBrush)));
      }
    }

    public Brush ConclusionBrush => cm.IsConsistent ? Brushes.Green : Brushes.OrangeRed;


    //----------------------------- Events -------------------------------

    private void ConsistensyMatrixEditor_Loaded(object sender, RoutedEventArgs e) {
      table_grid.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto,
      });
      foreach (var _ in connected_elements) {
        table_grid.ColumnDefinitions.Add(new ColumnDefinition());
      }
      table_grid.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto,
      });
      foreach (var _ in connected_elements) {
        table_grid.RowDefinitions.Add(new RowDefinition());
      }

      // Columns' headers
      for (int i = 0; i < connected_elements.Count; i++) {
        var header = new TextBlock()
        {
          Text = connected_elements[i].Title,
          HorizontalAlignment = HorizontalAlignment.Center,
          VerticalAlignment = VerticalAlignment.Bottom,
          TextWrapping = TextWrapping.Wrap,
        };
        table_grid.Children.Add(header);
        Grid.SetRow(header, 0);
        Grid.SetColumn(header, i + 1);
      }

      // Rows' headers
      table_grid.RowDefinitions.Add(new RowDefinition());
      for (int i = 0; i < connected_elements.Count; i++) {
        var header = new TextBlock()
        {
          Text = connected_elements[i].Title,
          HorizontalAlignment = HorizontalAlignment.Right,
          VerticalAlignment = VerticalAlignment.Center,
          TextWrapping = TextWrapping.Wrap,
        };
        table_grid.Children.Add(header);
        Grid.SetRow(header, i + 1);
        Grid.SetColumn(header, 0);
      }

      // Input fields
      for (int row_ind = 0; row_ind < connected_elements.Count; row_ind++) {
        for (int col_ind = 0; col_ind < connected_elements.Count; col_ind++) {
          var input_field = new TextBox();

          if (double.IsNaN(cm.RatesMatrix[row_ind, col_ind])) {
            input_field.Text = string.Empty;
          }
          else {
            input_field.Text = cm.RatesMatrix[row_ind, col_ind].ToString(DOUBLE_FORMAT);
          }

          if (row_ind >= col_ind) {
            input_field.IsEnabled = false;
          }
          else {
            int row = row_ind;
            int col = col_ind;

            input_field.TextChanged += (s, e) => OnEditValue(input_field, row, col);

            input_field.GotFocus += (s, e) =>
            {
              A = connected_elements[row].Title;
              B = connected_elements[col].Title;
            };
          }

          input_fields[row_ind, col_ind] = input_field;

          table_grid.Children.Add(input_field);
          Grid.SetRow(input_field, row_ind + 1);
          Grid.SetColumn(input_field, col_ind + 1);
        }
      }

      // Weights table
      w_grid.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto,
      });
      w_grid.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto,
      });

      foreach (var _ in connected_elements) {
        w_grid.ColumnDefinitions.Add(new ColumnDefinition());
      }

      w_fields = new TextBlock[connected_elements.Count];

      for (int i = 0; i < connected_elements.Count; i++) {
        // field name
        var header = new TextBlock()
        {
          Text = connected_elements[i].Title,
          HorizontalAlignment = HorizontalAlignment.Center,
          VerticalAlignment = VerticalAlignment.Center,
          TextWrapping = TextWrapping.Wrap,
        };

        var h_border = new Border()
        {
          BorderBrush = Brushes.Gray,
          BorderThickness = new Thickness(1),
        };
        h_border.Child = header;

        w_grid.Children.Add(h_border);
        Grid.SetRow(h_border, 0);
        Grid.SetColumn(h_border, i);

        // field value
        var w_tb = new TextBlock()
        {
          HorizontalAlignment = HorizontalAlignment.Center,
          VerticalAlignment = VerticalAlignment.Center,
        };

        w_fields[i] = w_tb;

        var w_border = new Border()
        {
          BorderBrush = Brushes.Gray,
          BorderThickness = new Thickness(1),
        };
        w_border.Child = w_tb;

        w_grid.Children.Add(w_border);
        Grid.SetRow(w_border, 1);
        Grid.SetColumn(w_border, i);
      }

      if (cm.AllRatesAreGiven) {
        if (!cm.AllWeightsAreCounted) {
          cm.CalculateWeights();
        }
        cm.CalculateCR();
      }

      UpdateUI();

      DataContext = null;
      DataContext = this;
    }

    private void Button_Save_Click(object sender, RoutedEventArgs e) {
      DialogResult = true;
    }


    //----------------------------- Private members -------------------------------

    private void OnEditValue(TextBox tb, int row_ind, int col_ind) {
      string text = input_fields[row_ind, col_ind].Text;
      if (double.TryParse(text, out double result)) {
        if (cm.RateIsValid(result)) {
          DrawCorrectRate(row_ind, col_ind, result);
        }
        else {
          DrawIncorrectRate(row_ind, col_ind);
        }
      }
      else {
        string[] parts = text.Split('/');

        if (double.TryParse(parts[0], out double num) && double.TryParse(parts[1], out double den)) {
          double rate = num / den;
          if (cm.RateIsValid(rate)) {
            DrawCorrectRate(row_ind, col_ind, num, den);
          }
          else {
            DrawIncorrectRate(row_ind, col_ind);
          }
        }
        else {
          DrawIncorrectRate(row_ind, col_ind);
        }
      }

      if (cm.AllRatesAreGiven) {
        cm.CalculateWeights();
        cm.CalculateCR();
      }

      UpdateUI();
    }

    private void UpdateUI() {
      if (cm.AllRatesAreGiven) {
        Conclusion = cm.GetConclusion();

        if (cm.IsConsistent) {
          for (int i = 0; i < cm.Weights.Length; i++) {
            w_fields[i].Text = cm.Weights[i].ToString(DOUBLE_FORMAT);
          }
        }
        else {
          for (int i = 0; i < cm.Weights.Length; i++) {
            w_fields[i].Text = EMPTY_STRING;
          }
        }
      }
      else {
        for (int i = 0; i < cm.Weights.Length; i++) {
          w_fields[i].Text = EMPTY_STRING;
        }

        Conclusion = EMPTY_STRING;
      }
    }

    private void DrawCorrectRate(int row_ind, int col_ind, double rate) {
      input_fields[row_ind, col_ind].Foreground = Brushes.Black;
      input_fields[col_ind, row_ind].Text = $"1 / {rate.ToString(DOUBLE_FORMAT)} ({( 1 / rate ).ToString(DOUBLE_FORMAT)})";

      cm.RatesMatrix[row_ind, col_ind] = rate;
      cm.RatesMatrix[col_ind, row_ind] = 1 / rate;
    }

    private void DrawCorrectRate(int row_ind, int col_ind, double num, double den) {
      input_fields[row_ind, col_ind].Foreground = Brushes.Black;
      input_fields[col_ind, row_ind].Text = $"{den.ToString(DOUBLE_FORMAT)} / {num.ToString(DOUBLE_FORMAT)} ({( den / num ).ToString(DOUBLE_FORMAT)})";

      cm.RatesMatrix[row_ind, col_ind] = num / den;
      cm.RatesMatrix[col_ind, row_ind] = den / num;
    }

    private void DrawIncorrectRate(int row_ind, int col_ind) {
      input_fields[row_ind, col_ind].Foreground = Brushes.Red;
      input_fields[col_ind, row_ind].Text = EMPTY_STRING;

      cm.RatesMatrix[row_ind, col_ind] = double.NaN;
      cm.RatesMatrix[col_ind, row_ind] = double.NaN;
    }

    private ElementVM element;
    private ConsistensyMatrix cm;
    private IReadOnlyList<ElementVM> connected_elements;
    private TextBox[,] input_fields;
    private TextBlock[] w_fields;
    private string a = EMPTY_STRING;
    private string b = EMPTY_STRING;
    private const string DOUBLE_FORMAT = "0.00";
    private const string EMPTY_STRING = "...";
    private string conclusion;
  }
}
