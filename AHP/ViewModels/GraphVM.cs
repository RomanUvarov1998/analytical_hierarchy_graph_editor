using Database.DB;
using Database.MathModel;
using AHP.TableViewModels;
using AHP.ViewModels.ElementState;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AHP.GraphViewModels;

namespace AHP.ViewModels
{
  public class GraphVM : IDraggable
  {
    internal GraphVM(Canvas canvas, Graph graph, Action on_changed, Action<Element, ScaleValue> on_element_attached) {
      Layers = new List<LayerVM>();

      this.on_changed = on_changed;
      this.on_element_attached = on_element_attached;

      this.canvas = canvas;
      this.canvas.Cursor = Cursors.Arrow;
      this.graph = graph;

      this.canvas.MouseEnter += Canvas_MouseEnter;

      var canvas_center = new Point(canvas.ActualWidth / 2, canvas.ActualHeight / 2);

      for (int i = 0; i < graph.Layers.Count(); i++) {
        var layer = new LayerVM(canvas, this, Layers.Count);

        for (int j = 0; j < graph.Layers[i].Elements.Count; j++) {
          var elt = new ElementVM(canvas, this, graph.Layers[i].Elements[j]);
          elt.IsJustAdded = false;
          layer.Elements.Add(elt);
        }

        Layers.Add(layer);
      }

      pos_cursor = new PosCursor();
      canvas.Children.Add(pos_cursor);

      storyboard = new Storyboard()
      {
        FillBehavior = FillBehavior.Stop,
      };
      storyboard.Completed += OnAnimationCompleted;
    }


    //------------------------ IDraggable --------------------------

    public void Drag(Vector delta) {
      Offset += delta;
      foreach (LayerVM layer in Layers) {
        layer.Drag(delta);
      }
    }


    //----------------------------- API -------------------------

    internal void FadeInChildren() {
      foreach (LayerVM layer in Layers) {
        layer.PlaceAllChildrenAt(canvas.ActualWidth / 2, canvas.ActualHeight / 2);
      }
      
      RecountPositions(INITIAL_APPEARANCE_ANIMATION_DURATION_MS);
    }

    internal bool StartDragElement(ElementVM elt, MouseEventArgs e) {
      if (dragged_control != null) return false;

      // can assign scale value only to criterium
      if (ScaleValueTVM_ToSetElement != null) {
        if (elt.State is ElementCriterium) {
          // clear assigned value from previos element, if it exists
          foreach (LayerVM layer in Layers) {
            ElementVM old = layer.Elements.FirstOrDefault(elt => elt.Element.ScaleValue == ScaleValueTVM_ToSetElement.ScaleValue);
            if (old != null) {
              old.Element.ScaleValue = null;
              break;
            }
          }

          // set assigned value to new elt
          elt.Element.ScaleValue = ScaleValueTVM_ToSetElement.ScaleValue;
          ScaleValueTVM_ToSetElement.AttachedElement = elt.Element;
          on_element_attached.Invoke(elt.Element, ScaleValueTVM_ToSetElement.ScaleValue);
          ScaleValueTVM_ToSetElement = null;
          on_changed.Invoke();

          // update all elements' titles
          foreach (LayerVM layer in Layers) {
            foreach (ElementVM elt_vm in layer.Elements) {
              elt_vm.UpdateTitle();
            }
          }
        }

        return false;
      }

      elt.OutlineBrush = Brushes.Blue;

      // bringing to front
      canvas.Children.Remove(elt);
      canvas.Children.Add(elt);

      elt.PosBeforeDrag = elt.Pos;

      dragged_control = elt;
      drag_old_pos = e.GetPosition(canvas);

      return true;
    }

    internal void StopDragElement() {
      ElementVM dragged_elt;

      if (dragged_control is ElementVM dragged_control_as_element) {
        dragged_elt = dragged_control_as_element;
      }
      else {
        return;
      }

      dragged_elt.OutlineBrush = Brushes.Transparent;

      LayerVM old_layer = Layers.First(layer => layer.Elements.Contains(dragged_elt));

      (double min_dist, LayerVM closest_layer, IDragTarget closest_elt) = FindClosest(dragged_elt);

      if (min_dist <= MAX_DRAG_TARGET_DIST && closest_elt != dragged_elt) {
        if (closest_elt is ElementVM elt) {
          if (closest_layer == old_layer) {
            int closest_ind = closest_layer.Elements.IndexOf(elt);
            int dragged_ind = closest_layer.Elements.IndexOf(dragged_elt);

            old_layer.Elements.Remove(dragged_elt);

            if (dragged_ind < closest_ind) {
              closest_ind--;
            }

            if (dragged_elt.DraggedCenterX <= closest_elt.DragTargetCenterX) {
              closest_layer.Elements.Insert(closest_ind, dragged_elt);
            }
            else {
              closest_layer.Elements.Insert(closest_ind + 1, dragged_elt);
            }
          }
          else {
            old_layer.Elements.Remove(dragged_elt);

            int closest_ind = closest_layer.Elements.IndexOf(elt);

            if (dragged_elt.DraggedCenterX <= closest_elt.DragTargetCenterX) {
              closest_layer.Elements.Insert(closest_ind, dragged_elt);
            }
            else {
              closest_layer.Elements.Insert(closest_ind + 1, dragged_elt);
            }
          }
        }
        else {
          old_layer.Elements.Remove(dragged_elt);
          closest_layer.Elements.Add(dragged_elt);
        }
      }

      drag_old_pos = null;
      dragged_control = null;

      dragged_elt.PosBeforeDrag = null;

      pos_cursor.SetVisible(false);

      RecountPositions(ALIGNMENT_ANIMATION_DURATION_MS);
    }

    internal void ElementSizeChanged(ElementVM elt, SizeChangedEventArgs e) {
      RecountPositions(ZERO_ANIMATION_DURATION_MS);
    }

    internal void ButtonSizeChanged(MyButton btn, SizeChangedEventArgs e) {
      RecountPositions(ZERO_ANIMATION_DURATION_MS);
    }

    internal void DeleteElement(ElementVM elt) {
      LayerVM layer = Layers.First(lay => lay.Elements.Contains(elt));
      layer.Elements.Remove(elt);
      List<RelativeRate> rr_to_remove = graph.RelativeRates.Where(rl => rl.A == elt.Element || rl.B == elt.Element || rl.Root == elt.Element).ToList();
      foreach (RelativeRate rr in rr_to_remove) {
        graph.RelativeRates.Remove(rr);
      }
      List<LocalPriority> lp_to_remove = graph.LocalPriorities.Where(lp => lp.A == elt.Element || lp.B == elt.Element).ToList();
      foreach (LocalPriority lp in lp_to_remove) {
        graph.LocalPriorities.Remove(lp);
      }

      CreateAndBeginDisappearAnimation(elt);

      RecountPositions(ALIGNMENT_ANIMATION_DURATION_MS);
    }

    internal void EditElement(ElementVM elt) {
      LayerVM containing_layer = Layers.First(lay => lay.Elements.Contains(elt));
      int ind = Layers.IndexOf(containing_layer);

      List<ElementVM> connected_elements = Layers[ind + 1].Elements;

      if (elt.State is ConnectedElementStateBase ce && ce.Consistensy == Consistensy.NotEnoughSubCriteriums) {
        MessageBox.Show("Количество подкритериев должно быть более одного", "Слишком мало подкритериев", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        return;
      }

      ElementVM root_element = elt;

      var dlg = new ConsistensyMatrixEditor(elt, connected_elements, graph);

      if (dlg.ShowDialog().GetValueOrDefault() == false) return;

      for (int i = 0; i < connected_elements.Count; i++) {
        graph.SetLocalPriority(
          connected_elements[i].Element,
          root_element.Element,
          dlg.ConsistensyMatrix.Weights[i]);
      }

      for (int i_a = 0; i_a < connected_elements.Count; i_a++) {
        for (int i_b = 0; i_b < connected_elements.Count; i_b++) {
          if (i_a == i_b) continue;
          graph.SetRelativeRate(
            connected_elements[i_a].Element,
            connected_elements[i_b].Element,
            root_element.Element,
            dlg.ConsistensyMatrix.RatesMatrix[i_a, i_b]);
        }
      }

      UpdateElementsStates();
    }

    internal void Button_AddElement_Click(MyButton btn) {
      string new_elt_name = Layers.Select(l => l.Elements.Count).Sum().ToString();
      ElementVM elt = new ElementVM(canvas, this, new Element(new_elt_name));

      LayerVM layer = Layers.First(layer => layer.AddElementButton == btn);
      layer.Elements.Add(elt);

      elt.Loaded += (s, e) => RecountPositions(ALIGNMENT_ANIMATION_DURATION_MS);
    }

    internal void Button_AddLayer_Click(MyButton btn) {
      string new_elt_name = Layers.Select(l => l.Elements.Count).Sum().ToString();
      var layer = new LayerVM(canvas, this,
        0, 
        () => RecountPositions(ALIGNMENT_ANIMATION_DURATION_MS),
        new_elt_name);

      LayerVM upper_layer = Layers.First(lay => lay.AddLayerUnderButton == btn);
      int ind = Layers.IndexOf(upper_layer);
      Layers.Insert(ind + 1, layer);

      UpdateLayersInds();
    }

    internal void Button_DeleteLayer_Click(MyButton btn) {
      LayerVM layer = Layers.First(lay => lay.DeleteLayerButton == btn);
      while (layer.Elements.Count > 0) {
        DeleteElement(layer.Elements.Last());
      }
      Layers.Remove(layer);

      UpdateLayersInds();

      CreateAndBeginDisappearAnimation(layer.AddElementButton);
      CreateAndBeginDisappearAnimation(layer.DeleteLayerButton);
      CreateAndBeginDisappearAnimation(layer.AddLayerUnderButton);
      CreateAndBeginDisappearAnimation(layer.MoveLayerUpButton);
      CreateAndBeginDisappearAnimation(layer.MoveLayerDownButton);

      RecountPositions(ALIGNMENT_ANIMATION_DURATION_MS);
    }

    internal void Button_MoveLayerUp_Click(MyButton btn) {
      LayerVM layer = Layers.First(lay => lay.MoveLayerUpButton == btn);
      int ind = Layers.IndexOf(layer);

      if (ind == 0) return;

      LayerVM tmp = Layers[ind];
      Layers[ind] = Layers[ind - 1];
      Layers[ind - 1] = tmp;

      UpdateLayersInds();

      RecountPositions(ALIGNMENT_ANIMATION_DURATION_MS);
    }

    internal void Button_MoveLayerDown_Click(MyButton btn) {
      LayerVM layer = Layers.First(lay => lay.MoveLayerDownButton == btn);
      int ind = Layers.IndexOf(layer);

      if (ind >= Layers.Count - 1) return;

      LayerVM tmp = Layers[ind];
      Layers[ind] = Layers[ind + 1];
      Layers[ind + 1] = tmp;

      UpdateLayersInds();

      RecountPositions(ALIGNMENT_ANIMATION_DURATION_MS);
    }

    internal List<LayerVM> Layers { get; }

    internal ScaleValueBaseGVM ScaleValueTVM_ToSetElement
    {
      get => scaleValueTVM_ToSetElement;
      set
      {
        scaleValueTVM_ToSetElement = value;
        canvas.Cursor = value != null ? Cursors.Pen : Cursors.Arrow;
      }
    }

    internal Point Offset { get; set; }

    //----------------------------- Events -------------------------------

    private void Canvas_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
      e.Handled = true;
      canvas.MouseLeave += canvas_MouseLeave;
      canvas.MouseMove += canvas_MouseMove;
      canvas.MouseDown += canvas_MouseDown;
    }

    private void canvas_MouseLeave(object sender, MouseEventArgs e) {
      e.Handled = true;

      canvas.MouseDown -= canvas_MouseDown;
      canvas.MouseLeave -= canvas_MouseLeave;
      canvas.MouseMove -= canvas_MouseMove;
      canvas.MouseUp -= canvas_MouseUp;

      StopDragElement();

      dragged_control = null;
      drag_old_pos = null;
    }

    private void canvas_MouseDown(object sender, MouseButtonEventArgs e) {
      if (dragged_control != null) return;

      if (e.ChangedButton == MouseButton.Right) {
        e.Handled = true;

        dragged_control = this;
        drag_old_pos = e.GetPosition(canvas);

        canvas.MouseUp += canvas_MouseUp;
      }
    }

    private void canvas_MouseUp(object sender, MouseButtonEventArgs e) {
      if (e.ChangedButton == MouseButton.Right) {
        e.Handled = true;

        dragged_control = null;
        drag_old_pos = null;

        canvas.MouseUp -= canvas_MouseUp;
      }
    }

    private void canvas_MouseMove(object sender, MouseEventArgs e) {
      if (dragged_control == null) return;

      Vector drag_delta = e.GetPosition(canvas) - drag_old_pos.Value;
      dragged_control.Drag(drag_delta);

      drag_old_pos = e.GetPosition(canvas);

      if (dragged_control is ElementVM dragged_elt) {
        (double min_dist, LayerVM closest_layer, IDragTarget closest_elt) = FindClosest(dragged_elt);

        if (min_dist > MAX_DRAG_TARGET_DIST) {
          pos_cursor.SetVisible(false);
        }
        else {
          if (dragged_elt == closest_elt) {
            ( pos_cursor as IPlaceable ).Pos = closest_elt.DragTargetCenter;
          }
          else if (dragged_elt.DraggedCenterX <= closest_elt.DragTargetCenterX) {
            ( pos_cursor as IPlaceable ).Pos = closest_elt.DragTargetLeft;
          }
          else {
            ( pos_cursor as IPlaceable ).Pos = closest_elt.DragTargetRight;
          }
          pos_cursor.CursorHeight = closest_elt.DragTargetHeight;
          pos_cursor.SetVisible(true);
        }
      }
    }


    //----------------------------- Private members -------------------------------

    private void RecountPositions(double duration_ms) {
      storyboard.Children.Clear();

      foreach (LayerVM layer in Layers) {
        double layer_w = layer.Elements.Select(elt => elt.ActualWidth).Sum();

        Point prev_elt_top_right = new Point(Offset.X - layer_w / 2.0, Offset.Y + layer.LayerInd * LAYER_HEIGHT);

        if (layer.Elements.Any()) {
          PlaceUIElement(
            layer.Elements[0],
            prev_elt_top_right,
            duration_ms);

          prev_elt_top_right.X += layer.Elements[0].ActualWidth;

          for (int i = 1; i < layer.Elements.Count; i++) {
            PlaceUIElement(
              layer.Elements[i],
              prev_elt_top_right,
              duration_ms);

            prev_elt_top_right.X += layer.Elements[i].ActualWidth;
          }
        }

        PlaceUIElement(
          layer.MoveLayerUpButton,
          new Point(prev_elt_top_right.X, prev_elt_top_right.Y - 15),
          duration_ms);

        PlaceUIElement(
          layer.MoveLayerDownButton,
          new Point(prev_elt_top_right.X, prev_elt_top_right.Y + 15),
          duration_ms);

        prev_elt_top_right.X += layer.MoveLayerDownButton.ActualWidth;

        PlaceUIElement(
          layer.AddElementButton,
          prev_elt_top_right,
          duration_ms);

        prev_elt_top_right.X += layer.AddElementButton.ActualWidth;

        PlaceUIElement(
          layer.DeleteLayerButton,
          prev_elt_top_right,
          duration_ms);

        PlaceUIElement(
          layer.AddLayerUnderButton,
          new Point(Offset.X - layer.AddLayerUnderButton.ActualWidth / 2.0, prev_elt_top_right.Y + LAYER_HEIGHT * 0.6),
          duration_ms);
      }

      storyboard.Begin();
    }

    private void UpdateLayersInds() {
      for (int lay_i = 0; lay_i < Layers.Count; lay_i++) {
        Layers[lay_i].LayerInd = lay_i;
      }
    }

    private (double, LayerVM, IDragTarget) FindClosest(ElementVM dragged_elt) {
      double min_dist2 = double.MaxValue;
      LayerVM closest_layer = null;
      IDragTarget closest_elt = null;

      foreach (LayerVM layer in Layers) {
        double dist2 = ( layer.MoveLayerDownButton.Pos - dragged_elt.Pos ).LengthSquared;
        if (dist2 < min_dist2) {
          closest_layer = layer;
          closest_elt = layer.MoveLayerDownButton;
          min_dist2 = dist2;
        }

        foreach (ElementVM el in layer.Elements) {

          if (el == dragged_elt) {
            dist2 = ( dragged_elt.PosBeforeDrag.Value - dragged_elt.Pos ).LengthSquared;
          }
          else {
            dist2 = ( el.Pos - dragged_elt.Pos ).LengthSquared;
          }

          if (dist2 < min_dist2) {
            closest_layer = layer;
            closest_elt = el;
            min_dist2 = dist2;
          }
        }
      }

      double min_dist = Math.Sqrt(min_dist2);

      return (min_dist, closest_layer, closest_elt);
    }

    private void UpdateElementsStates() {
      ReassembleGraph();

      for (int lay_i = 0; lay_i < Layers.Count; lay_i++) {
        for (int elt_i = 0; elt_i < Layers[lay_i].Elements.Count; elt_i++) {
          ElementVM elt = Layers[lay_i].Elements[elt_i];

          if (lay_i == Layers.Count - 1) {
            elt.State = new ElementAlternative();
            continue;
          }

          Consistensy cons;

          if (Layers[lay_i + 1].Elements.Count < 2) {
            cons = Consistensy.NotEnoughSubCriteriums;
          }
          else {
            ConsistensyMatrix cm = graph.ConsistensyMatrixForElement(elt.Element, graph.Layers[lay_i + 1]);
            if (!cm.AllRatesAreGiven) {
              cons = Consistensy.NotAllConnectionsRated;
            }
            else {
              cons = cm.IsConsistent ? Consistensy.Consistent : Consistensy.Inconsistent;
            }
          }

          if (lay_i == 0) {
            elt.State = new ElementGoal(cons);
          }
          else {
            elt.State = new ElementCriterium(cons);
          }
        }
      }

      on_changed?.Invoke();
    }

    private void OnAnimationCompleted(object sender, EventArgs e) {
      for (int lay_i = 0; lay_i < Layers.Count; lay_i++) {
        Layers[lay_i].OnAnimationEnded();
      }

      UpdateElementsStates();
    }

    private void PlaceUIElement<E>(E ui_elt, Point to, double duration_ms)
      where E : UIElement, IPlaceable {
      PlaceUIElement(ui_elt, to.X, to.Y, duration_ms);
    }

    private void PlaceUIElement<E>(E ui_elt, double x_to, double y_to, double duration_ms)
      where E : UIElement, IPlaceable {
      if (ui_elt.Opacity < 1) {
        CreateAppearAnimation(ui_elt, duration_ms);
      }

      if (Math.Abs(duration_ms) < double.Epsilon || ui_elt.IsJustAdded) {
        ui_elt.X = x_to;
        ui_elt.Y = y_to;
        ui_elt.IsJustAdded = false;
      }
      else {
        ui_elt.DestPos = new Point(x_to, y_to);
        CreateAlignAnimation(ui_elt, new Point(x_to, y_to), duration_ms);
      }
    }

    private void CreateAlignAnimation<E>(E elt, Point point_to, double duration_ms)
      where E : UIElement, IPlaceable {
      var align_animation_x = new DoubleAnimation()
      {
        From = elt.X,
        To = point_to.X,
        FillBehavior = FillBehavior.HoldEnd,
        Duration = TimeSpan.FromMilliseconds(duration_ms),
      };

      var align_animation_y = new DoubleAnimation()
      {
        From = elt.Y,
        To = point_to.Y,
        FillBehavior = FillBehavior.HoldEnd,
        Duration = TimeSpan.FromMilliseconds(duration_ms),
      };

      Storyboard.SetTarget(align_animation_x, elt);
      Storyboard.SetTargetProperty(align_animation_x, new PropertyPath(Canvas.LeftProperty));

      Storyboard.SetTarget(align_animation_y, elt);
      Storyboard.SetTargetProperty(align_animation_y, new PropertyPath(Canvas.TopProperty));

      storyboard.Children.Add(align_animation_x);
      storyboard.Children.Add(align_animation_y);
    }

    private void CreateAppearAnimation<E>(E elt, double duration_ms)
      where E : UIElement, IPlaceable {
      var opac_animation = new DoubleAnimation()
      {
        From = 0,
        To = 1,
        FillBehavior = FillBehavior.HoldEnd,
        Duration = TimeSpan.FromMilliseconds(duration_ms),
        EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn, },
      };

      Storyboard.SetTarget(opac_animation, elt);
      Storyboard.SetTargetProperty(opac_animation, new PropertyPath(Canvas.OpacityProperty));

      opac_animation.Completed += (s, e) => elt.Opacity = 1;

      storyboard.Children.Add(opac_animation);
    }

    private void CreateAndBeginDisappearAnimation<E>(E elt)
      where E : UIElement, IPlaceable {
      var opac_animation = new DoubleAnimation()
      {
        From = 1,
        To = 0,
        FillBehavior = FillBehavior.Stop,
        Duration = TimeSpan.FromMilliseconds(DELETING_ANIMATION_DURATION_MS),
        EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut, },
      };

      Storyboard.SetTarget(opac_animation, elt);

      opac_animation.Completed += (s, e) =>
      {
        elt.Opacity = 0;
        canvas.Children.Remove(elt);
      };

      elt.BeginAnimation(Canvas.OpacityProperty, opac_animation);
    }

    private void ReassembleGraph() {
      for (int lay_i = 0; lay_i < this.Layers.Count; lay_i++) {
        if (lay_i >= graph.Layers.Count) {
          graph.Layers.Add(new Layer());
        }
        else {
          graph.Layers[lay_i].Elements.Clear();
        }

        for (int elt_i = 0; elt_i < this.Layers[lay_i].Elements.Count; elt_i++) {
          Element elt_from_vm = this.Layers[lay_i].Elements[elt_i].Element;
          graph.Layers[lay_i].Elements.Add(elt_from_vm);
        }
      }

      if (graph.Layers.Count > this.Layers.Count) {
        int ind_from = this.Layers.Count;
        int count_to_remove = graph.Layers.Count - this.Layers.Count;
        graph.Layers.RemoveRange(ind_from, count_to_remove);
      }

      // check
      Debug.Assert(this.Layers.Count == graph.Layers.Count);

      for (int lay_i = 0; lay_i < this.Layers.Count; lay_i++) {
        Debug.Assert(this.Layers[lay_i].Elements.Count == graph.Layers[lay_i].Elements.Count);

        for (int elt_i = 0; elt_i < this.Layers[lay_i].Elements.Count; elt_i++) {
          Debug.Assert(this.Layers[lay_i].Elements[elt_i].Element == graph.Layers[lay_i].Elements[elt_i]);
        }
      }
    }

    private Action on_changed;
    private Action<Element, ScaleValue> on_element_attached;
    private Graph graph;
    private Point? drag_old_pos = new Point();
    private IDraggable dragged_control = null;
    private Canvas canvas;
    private PosCursor pos_cursor;
    private const double MAX_DRAG_TARGET_DIST = 300.0;
    private const double LAYER_HEIGHT = 120.0;
    private const double ZERO_ANIMATION_DURATION_MS = 0.0;
    private const double ALIGNMENT_ANIMATION_DURATION_MS = 150.0;
    private const double DELETING_ANIMATION_DURATION_MS = 150.0;
    private const double INITIAL_APPEARANCE_ANIMATION_DURATION_MS = 500.0;
    private Storyboard storyboard;
    private ScaleValueBaseGVM scaleValueTVM_ToSetElement;
  }
}
