using Database.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AHP.ViewModels
{
  internal class LayerVM
  {
    // when created on the start of editing
    internal LayerVM(Canvas canvas, GraphVM graph_vm, int layer_ind) {
      AddElementButton = new MyButton(canvas, ButtonType.AddElement, graph_vm.Button_AddElement_Click);
      DeleteLayerButton = new MyButton(canvas, ButtonType.DeleteLayer, graph_vm.Button_DeleteLayer_Click);
      AddLayerUnderButton = new MyButton(canvas, ButtonType.AddLayer, graph_vm.Button_AddLayer_Click);
      MoveLayerUpButton = new MyButton(canvas, ButtonType.MoveLayerUp, graph_vm.Button_MoveLayerUp_Click);
      MoveLayerDownButton = new MyButton(canvas, ButtonType.MoveLayerDown, graph_vm.Button_MoveLayerDown_Click);

      AddElementButton.IsJustAdded = false;
      DeleteLayerButton.IsJustAdded = false;
      AddLayerUnderButton.IsJustAdded = false;
      MoveLayerUpButton.IsJustAdded = false;
      MoveLayerDownButton.IsJustAdded = false;

      Elements = new List<ElementVM>();

      LayerInd = layer_ind;
    }

    // when created in the middle of editing
    internal LayerVM(Canvas canvas, GraphVM graph_vm, int layer_ind, Action on_loaded, string first_elt_name) : this(canvas, graph_vm, layer_ind) {
      var first_elt_vm = new ElementVM(canvas, graph_vm, new Element(first_elt_name));
      Elements.Add(first_elt_vm);

      first_elt_vm.Loaded += (s, e) => OnChildLoaded();
      AddElementButton.Loaded += (s, e) => OnChildLoaded();
      DeleteLayerButton.Loaded += (s, e) => OnChildLoaded();
      AddLayerUnderButton.Loaded += (s, e) => OnChildLoaded();
      MoveLayerUpButton.Loaded += (s, e) => OnChildLoaded();
      MoveLayerDownButton.Loaded += (s, e) => OnChildLoaded();
      this.on_loaded = on_loaded;

      first_elt_vm.IsJustAdded = true;
      AddElementButton.IsJustAdded = true;
      DeleteLayerButton.IsJustAdded = true;
      AddLayerUnderButton.IsJustAdded = true;
      MoveLayerUpButton.IsJustAdded = true;
      MoveLayerDownButton.IsJustAdded = true;

      total_children = 6;
    }

    //----------------------------- API -------------------------

    internal void PlaceAllChildrenAt(double x, double y) {
      foreach (ElementVM elt in Elements) {
        elt.X = x;
        elt.Y = y;
      }

      AddElementButton.X = x;
      AddElementButton.Y = y;

      DeleteLayerButton.X = x;
      DeleteLayerButton.Y = y;

      AddLayerUnderButton.X = x;
      AddLayerUnderButton.Y = y;

      MoveLayerUpButton.X = x;
      MoveLayerUpButton.Y = y;

      MoveLayerDownButton.X = x;
      MoveLayerDownButton.Y = y;
    }

    internal double Height
    {
      get
      {
        double max_height = 0;
        if (Elements.Any()) {
          max_height = Elements.Select(elt => elt.ActualHeight).Max();
        }

        return Math.Max(max_height, AddElementButton.ActualHeight);
      }
    }

    internal List<ElementVM> Elements { get; }
    internal MyButton AddElementButton { get; }
    internal MyButton DeleteLayerButton { get; }
    internal MyButton AddLayerUnderButton { get; }
    internal MyButton MoveLayerUpButton { get; }
    internal MyButton MoveLayerDownButton { get; }
    internal int LayerInd { get; set; }

    internal void OnAnimationEnded() {
      foreach (ElementVM elt in Elements) {
        elt.OnAnimationEnded();
      }

      AddElementButton.OnAnimationEnded();
      DeleteLayerButton.OnAnimationEnded();
      AddLayerUnderButton.OnAnimationEnded();
      MoveLayerUpButton.OnAnimationEnded();
      MoveLayerDownButton.OnAnimationEnded();
    }

    //------------------------ IDraggable --------------------------

    public void Drag(Vector delta) {
      foreach (ElementVM elt in Elements) {
        elt.Drag(delta);
      }
      AddElementButton.Drag(delta);
      DeleteLayerButton.Drag(delta);
      AddLayerUnderButton.Drag(delta);
      MoveLayerUpButton.Drag(delta);
      MoveLayerDownButton.Drag(delta);
    }


    //----------------------------- Private members -------------------------------

    private void OnChildLoaded() {
      children_loaded++;
      if (children_loaded >= total_children) {
        on_loaded.Invoke();
      }
    }


    private Action on_loaded;
    private int children_loaded = 0;
    private int total_children = 0;
  }
}
