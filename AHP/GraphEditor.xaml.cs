using AHP.GraphViewModels;
using AHP.TableViewModels;
using AHP.ViewModels;
using AHP.ViewModels.ElementState;
using Database.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AHP
{
  /// <summary>
  /// Interaction logic for GraphEditor.xaml
  /// </summary>
  public partial class GraphEditor : Window, INotifyPropertyChanged
  {
    public GraphEditor(int graph_id) {
      InitializeComponent();

      ctx = new Context();
      if (graph_id == 0) {
        graph = new Graph();
        ctx.Add(graph);

        var layer_1 = new Layer();
        var elt_1_1 = new Element("Ведение пациента");
        layer_1.Elements.Add(elt_1_1);

        var layer_2 = new Layer();
        var elt_2_1 = new Element("Кат 1");
        var elt_2_2 = new Element("Кат 2");
        var elt_2_3 = new Element("Кат 3");
        layer_2.Elements.AddRange(new Element[] { elt_2_1, elt_2_2, elt_2_3 });

        var layer_3 = new Layer();
        var elt_3_1 = new Element("Норма");
        var elt_3_2 = new Element("Требуется осмотр");
        var elt_3_3 = new Element("Требуется госпитализация");
        layer_3.Elements.AddRange(new Element[] { elt_3_1, elt_3_2, elt_3_3 });

        graph.Layers.Add(layer_1);
        graph.Layers.Add(layer_2);
        graph.Layers.Add(layer_3);

        graph.SetRelativeRate(elt_2_1, elt_2_2, elt_1_1, 3);
        graph.SetRelativeRate(elt_2_1, elt_2_3, elt_1_1, 5);
        graph.SetRelativeRate(elt_2_2, elt_2_3, elt_1_1, 4);

        graph.SetRelativeRate(elt_3_1, elt_3_2, elt_2_1, 3);
        graph.SetRelativeRate(elt_3_1, elt_3_3, elt_2_1, 5);
        graph.SetRelativeRate(elt_3_2, elt_3_3, elt_2_1, 4);

        graph.SetRelativeRate(elt_3_1, elt_3_2, elt_2_2, 3);
        graph.SetRelativeRate(elt_3_1, elt_3_3, elt_2_2, 5);
        graph.SetRelativeRate(elt_3_2, elt_3_3, elt_2_2, 4);

        graph.SetRelativeRate(elt_3_1, elt_3_2, elt_2_3, 3);
        graph.SetRelativeRate(elt_3_1, elt_3_3, elt_2_3, 5);
        graph.SetRelativeRate(elt_3_2, elt_3_3, elt_2_3, 4);
      }
      else {
        graph = ctx.LoadGraphForEdiding(graph_id);
      }

      Func<Scale, ScaleBaseGVM> selector = sc =>
      {
        if (sc is RangeScale range_sc) {
          return new RangeScaleGVM(range_sc, () => { UpdateElementsTitles(); UpdateErrsAndScales(); });
        }
        if (sc is NameScale name_sc) {
          return new NameScaleGVM(name_sc, () => { UpdateElementsTitles(); UpdateErrsAndScales(); });
        }
        else {
          throw new NotImplementedException();
        }
      };

      Scales = new ObservableCollection<ScaleBaseGVM>(graph.Scales.Select(selector));

      graph_vm = new GraphVM(canv, graph, UpdateErrsAndScales, OnElementAttached);

      DataContext = this;
    }


    //----------------------------- GUI -------------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public string ErrMessage
    {
      get => errMessage;
      set
      {
        errMessage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrMessage)));
      }
    }

    public ObservableCollection<ScaleBaseGVM> Scales { get; }

    //----------------------------- API -------------------------------

    public Graph Graph => graph;


    //----------------------------- Events -------------------------------

    private void canv_Loaded(object sender, RoutedEventArgs e) {
      graph_vm.Offset = new Point(canv.ActualWidth / 2, 0);
      graph_vm.FadeInChildren();
    }

    private void Button_Save_Click(object sender, RoutedEventArgs e) {
      graph.UpdatedDate = DateTime.Now;
      graph.UpdateInds();
      ctx.SaveChanges();
      DialogResult = true;
    }

    private void Button_AddRangeScale_Click(object sender, RoutedEventArgs e) {
      var sc = new RangeScale()
      {
        Title = $"Шкала {Scales.Count}",
      };
      Scales.Add(new RangeScaleGVM(sc, () => { UpdateElementsTitles(); UpdateErrsAndScales(); }));
      graph.Scales.Add(sc);
      UpdateErrsAndScales();
    }

    private void Button_AddNameScale_Click(object sender, RoutedEventArgs e) {
      var sc = new NameScale()
      {
        Title = $"Шкала {Scales.Count}",
      };
      Scales.Add(new NameScaleGVM(sc, () => { UpdateElementsTitles(); UpdateErrsAndScales(); }));
      graph.Scales.Add(sc);
      UpdateErrsAndScales();
    }

    private void Button_DeleteScale_Click(object sender, RoutedEventArgs e) {
      var btn = (Button)sender;
      var scale = (ScaleBaseGVM)btn.DataContext;
      graph.Scales.Remove(scale.Scale);
      Scales.Remove(scale);
      foreach (ScaleValueBaseGVM scv in scale.ScaleValues) {
        RemoveScaleValueFromItsElement(scv.ScaleValue);
      }
      UpdateErrsAndScales();
    }

    private void Button_AddScaleValue_Click(object sender, RoutedEventArgs e) {
      var btn = (Button)sender;
      var scale = (ScaleBaseGVM)btn.DataContext;
      scale.AddScaleValue();
      UpdateErrsAndScales();
    }

    private void Button_DeleteScaleValue_Click(object sender, RoutedEventArgs e) {
      var btn = (Button)sender;
      var scale_value = (ScaleValueBaseGVM)btn.DataContext;

      scale_value.RemoveSelfFromScale();

      RemoveScaleValueFromItsElement(scale_value.ScaleValue);

      UpdateErrsAndScales();
    }

    private void UpdateErrsAndScales() {
      ErrMessage = graph.GetErrMsgs();

      foreach (ScaleBaseGVM sc in Scales) {
        sc.UpdateValidation();
      }
    }

    private void UpdateElementsTitles() {
      foreach (LayerVM layer in graph_vm.Layers) {
        foreach (ElementVM elt in layer.Elements) {
          elt.UpdateTitle();
        }
      }
    }

    private void OnElementAttached(Element elt, ScaleValue scv) {
      foreach (ScaleBaseGVM sc_gvm in Scales) {
        foreach (ScaleValueBaseGVM scv_gvm in sc_gvm.ScaleValues) {
          if (scv_gvm.ScaleValue != scv && scv_gvm.AttachedElement == elt) {
            scv_gvm.AttachedElement = null;
          }
        }
      }
    }

    private void Button_AssignElementForScaleValue_Click(object sender, RoutedEventArgs e) {
      var btn = (Button)sender;
      var scale_value_gvm = (ScaleValueBaseGVM)btn.DataContext;
      graph_vm.ScaleValueTVM_ToSetElement = scale_value_gvm;
    }

    //----------------------------- Private members -------------------------------

    private void RemoveScaleValueFromItsElement(ScaleValue scv) {
      foreach (LayerVM layer in graph_vm.Layers) {
        ElementVM containing_elt = layer.Elements.FirstOrDefault(elt => elt.Element.ScaleValue == scv);
        if (containing_elt != null) {
          containing_elt.Element.ScaleValue = null;
          containing_elt.UpdateTitle();
          break;
        }
      }
    }

    private GraphVM graph_vm;
    private Graph graph;
    private string errMessage;
    private Context ctx;
  }
}
