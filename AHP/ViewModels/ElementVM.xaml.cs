using Database.DB;
using Database.MathModel;
using AHP.ViewModels.ElementState;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AHP.ViewModels
{

  /// <summary>
  /// Interaction logic for Element.xaml
  /// </summary>
  public partial class ElementVM : UserControl, INotifyPropertyChanged, IPlaceable, IDraggable, IDragTarget
  {
    public ElementVM(Canvas canvas, GraphVM graph_vm, Element elt) {
      InitializeComponent();
      this.graph_vm = graph_vm;
      this.element = elt;

      Opacity = 0;

      DataContext = this;

      canvas.Children.Add(this);
    }


    //----------------------------- API -------------------------

    internal Element Element => element;

    internal void OnAnimationEnded() {
      if (DestPos != null) {
        X = DestPos.Value.X;
        Y = DestPos.Value.Y;
        DestPos = null;
      }
    }

    internal Point? PosBeforeDrag { get; set; } = null;

    internal double DraggedCenterX => X + ActualWidth / 2;

    public ElementStateBase State
    {
      get => state;
      set
      {
        state = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectionsButtonVisibility)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
      }
    }

    internal void UpdateTitle() {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
    }

    //----------------------------- IDragTarget -------------------------

    public double DragTargetCenterX => PosBeforeDrag.HasValue ? ( PosBeforeDrag.Value.X + ActualWidth / 2 ) : ( X + ActualWidth / 2 );

    public Point DragTargetLeft
    {
      get
      {
        Point pos = PosBeforeDrag ?? Pos;
        pos.X -= Margin.Left;
        pos.Y += ActualHeight / 2;
        return pos;
      }
    }

    public Point DragTargetCenter
    {
      get
      {
        Point pos = PosBeforeDrag ?? Pos;
        pos.X += ActualWidth / 2;
        pos.Y += ActualHeight / 2;
        return pos;
      }
    }

    public Point DragTargetRight
    {
      get
      {
        Point pos = PosBeforeDrag ?? Pos;
        pos.X += ActualWidth + Margin.Right;
        pos.Y += ActualHeight / 2;
        return pos;
      }
    }

    public double DragTargetHeight => ActualHeight;


    //----------------------------- GUI -------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public string Title
    {
      get => element.ScaleValue?.GetElementTitle() ?? element.Title;
      set
      {
        element.Title = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
      }
    }

    public Brush OutlineBrush
    {
      get => outlineBrush;
      set
      {
        outlineBrush = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutlineBrush)));
      }
    }

    public Brush DeleteButtonBrush
    {
      get => deleteButtonBrush;
      set
      {
        deleteButtonBrush = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeleteButtonBrush)));
      }
    }

    public Brush ConnectionsButtonBrush
    {
      get => connectionsButtonBrush;
      set
      {
        connectionsButtonBrush = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectionsButtonBrush)));
      }
    }

    public Visibility DeleteButtonVisibility
    {
      get => deleteButtonVisibility;
      set
      {
        deleteButtonVisibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeleteButtonVisibility)));
      }
    }

    public Visibility ConnectionsButtonVisibility => State is ElementAlternative ? Visibility.Hidden : Visibility.Visible;


    //----------------------------- IPlaceable -------------------------

    public double X
    {
      get => x;
      set
      {
        x = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X)));
      }
    }

    public double Y
    {
      get => y;
      set
      {
        y = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y)));
      }
    }

    public Point? DestPos { get; set; } = null;

    public Point Pos => new Point(X, Y);

    public bool IsJustAdded { get; set; } = true;

    //------------------------ IDraggable --------------------------

    public void Drag(Vector delta) {
      X += delta.X;
      Y += delta.Y;
      Console.WriteLine(Pos.ToString());
    }


    //----------------------------- Events -------------------------------

    private void UserControl_Loaded(object sender, RoutedEventArgs e) {
      SizeChanged += UserControl_SizeChanged;
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
      graph_vm.ElementSizeChanged(this, e);
    }


    private void Border_MouseEnter(object sender, MouseEventArgs e) {
      OutlineBrush = Brushes.GreenYellow;
      border.MouseLeave += Border_MouseLeave;
      border.MouseDown += Border_MouseDown;
    }

    private void Border_MouseLeave(object sender, MouseEventArgs e) {
      OutlineBrush = Brushes.Transparent;
      border.MouseDown -= Border_MouseDown;
      border.MouseLeave -= Border_MouseLeave;
      border.MouseUp -= Border_MouseUp;
      graph_vm.StopDragElement();
    }

    private void Border_MouseDown(object sender, MouseButtonEventArgs e) {
      if (e.ChangedButton == MouseButton.Left) {
        e.Handled = true;
        if (!graph_vm.StartDragElement(this, e)) return;
        border.MouseUp += Border_MouseUp;
        border.MouseLeave -= Border_MouseLeave;
        border.MouseEnter -= Border_MouseEnter;
      }
    }

    private void Border_MouseUp(object sender, MouseButtonEventArgs e) {
      if (e.ChangedButton == MouseButton.Left) {
        e.Handled = true;
        OutlineBrush = Brushes.GreenYellow;
        graph_vm.StopDragElement();
        border.MouseUp -= Border_MouseUp;
        border.MouseLeave += Border_MouseLeave;
        border.MouseEnter += Border_MouseEnter;
      }
    }


    private void UserControl_MouseEnter(object sender, MouseEventArgs e) {
      DeleteButtonVisibility = Visibility.Visible;
    }

    private void UserControl_MouseLeave(object sender, MouseEventArgs e) {
      DeleteButtonVisibility = Visibility.Hidden;
    }


    private void DeleteButton_MouseEnter(object sender, MouseEventArgs e) {
      DeleteButtonBrush = Brushes.GreenYellow;
      delete_btn_canv.MouseLeave += DeleteButton_MouseLeave;
      delete_btn_canv.MouseDown += DeleteButton_MouseDown;
    }

    private void DeleteButton_MouseLeave(object sender, MouseEventArgs e) {
      DeleteButtonBrush = Brushes.Red;
      delete_btn_canv.MouseDown -= DeleteButton_MouseDown;
      delete_btn_canv.MouseLeave -= DeleteButton_MouseLeave;
      delete_btn_canv.MouseUp -= DeleteButton_MouseUp;
    }

    private void DeleteButton_MouseDown(object sender, MouseButtonEventArgs e) {
      if (e.ChangedButton == MouseButton.Left) {
        e.Handled = true;
        delete_btn_canv.MouseUp += DeleteButton_MouseUp;
      }
    }

    private void DeleteButton_MouseUp(object sender, MouseButtonEventArgs e) {
      if (e.ChangedButton == MouseButton.Left) {
        e.Handled = true;
        delete_btn_canv.MouseUp -= DeleteButton_MouseUp;
        graph_vm.DeleteElement(this);
      }
    }


    private void ConnectionsCanvas_MouseEnter(object sender, MouseEventArgs e) {
      ConnectionsButtonBrush = Brushes.GreenYellow;
      connections_canv.MouseLeave += ConnectionsCanvas_MouseLeave;
      connections_canv.MouseDown += ConnectionsCanvas_MouseDown;
    }

    private void ConnectionsCanvas_MouseLeave(object sender, MouseEventArgs e) {
      ConnectionsButtonBrush = Brushes.Green;
      connections_canv.MouseDown -= ConnectionsCanvas_MouseDown;
      connections_canv.MouseLeave -= ConnectionsCanvas_MouseLeave;
      connections_canv.MouseUp -= ConnectionsCanvas_MouseUp;
    }

    private void ConnectionsCanvas_MouseDown(object sender, MouseButtonEventArgs e) {
      if (e.ChangedButton == MouseButton.Left) {
        e.Handled = true;
        connections_canv.MouseUp += ConnectionsCanvas_MouseUp;
      }
    }

    private void ConnectionsCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
      if (e.ChangedButton == MouseButton.Left) {
        e.Handled = true;
        connections_canv.MouseUp -= ConnectionsCanvas_MouseUp;
        graph_vm.EditElement(this);
      }
    }



    private GraphVM graph_vm;
    private Element element;
    private double x;
    private double y;
    private Brush outlineBrush = Brushes.Transparent;
    private Brush deleteButtonBrush = Brushes.Red;
    private Brush connectionsButtonBrush = Brushes.Green;
    private Visibility deleteButtonVisibility = Visibility.Hidden;
    private ElementStateBase state;

    private Brush elementStateBrush = Brushes.Transparent;
  }
}
