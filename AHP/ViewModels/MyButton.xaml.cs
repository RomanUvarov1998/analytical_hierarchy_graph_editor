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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AHP.ViewModels
{
  /// <summary>
  /// Interaction logic for AddElementButton.xaml
  /// </summary>
  public partial class MyButton : UserControl, INotifyPropertyChanged, IPlaceable, IDraggable, IDragTarget
  {
    public MyButton(Canvas canvas, ButtonType btn_type, Action<MyButton> cbk) {
      InitializeComponent();

      switch (btn_type) {
        case ButtonType.AddElement:     BtnToolTip = "Добавить элемент";  break;
        case ButtonType.AddLayer:       BtnToolTip = "Добавить слой";  break;
        case ButtonType.DeleteLayer:    BtnToolTip = "Удалить слой";   break;
    }

      this.cbk = cbk;
      ButtonActionType = btn_type;

      Opacity = 0;

      DataContext = this;

      canvas.Children.Add(this);
    }


    //------------------- API -------------------------

    internal void OnAnimationEnded() {
      if (DestPos != null) {
        X = DestPos.Value.X;
        Y = DestPos.Value.Y;
        DestPos = null;
      }
    }

    //------------------- GUI -------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public Brush OutlineBrush
    {
      get => outline_brush;
      set
      {
        outline_brush=value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutlineBrush)));
      }
    }

    public ButtonType ButtonActionType { get; }

    public string BtnToolTip { get; }


    //------------------- IPlaceable -------------------------

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
    }

    //----------------------------- IDragTarget -------------------------

    public double DragTargetCenterX => X + ActualWidth / 2;

    public Point DragTargetLeft
    {
      get
      {
        Point pos = Pos;
        pos.X -= Margin.Left;
        pos.Y += ActualHeight / 2;
        return pos;
      }
    }

    public Point DragTargetCenter
    {
      get
      {
        Point pos = Pos;
        pos.X += ActualWidth / 2;
        pos.Y += ActualHeight / 2;
        return pos;
      }
    }

    public Point DragTargetRight => DragTargetLeft;

    public double DragTargetHeight => ActualHeight;


    //----------------------------- Events -------------------------------

    private void UC_MouseEnter(object sender, MouseEventArgs e) {
      OutlineBrush = Brushes.GreenYellow;
      MouseLeave += UC_MouseLeave;
      MouseDown += UC_MouseDown;
    }

    private void UC_MouseLeave(object sender, MouseEventArgs e) {
      OutlineBrush = Brushes.Transparent;
      MouseDown -= UC_MouseDown;
      MouseLeave -= UC_MouseLeave;
      MouseUp -= UC_MouseUp;
    }

    private void UC_MouseDown(object sender, MouseButtonEventArgs e) {
      if (e.ChangedButton == MouseButton.Left) {
        e.Handled = true;
        OutlineBrush = Brushes.Green;
        MouseUp += UC_MouseUp;
      }
    }

    private void UC_MouseUp(object sender, MouseButtonEventArgs e) {
      if (e.ChangedButton == MouseButton.Left) {
        e.Handled = true;
        OutlineBrush = Brushes.GreenYellow;
        cbk?.Invoke(this);
        MouseUp -= UC_MouseUp;
      }
    }


    //----------------------------- Private members -------------------------------

    private double x;
    private double y;
    private Brush outline_brush = Brushes.Transparent;
    private Action<MyButton> cbk;
  }
}
