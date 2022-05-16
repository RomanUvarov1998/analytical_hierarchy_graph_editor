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
  /// Interaction logic for PosCursor.xaml
  /// </summary>
  public partial class PosCursor : UserControl, INotifyPropertyChanged, IPlaceable
  {
    public PosCursor() {
      InitializeComponent();
      DataContext = this;
    }


    //----------------------------- API -------------------------

    internal void SetVisible(bool is_visible) {
      CursorVisibility = is_visible ? Visibility.Visible : Visibility.Hidden;
    }

    internal void Redraw() {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Upper)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpperLeft)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpperRight)));

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Downer)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownerLeft)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownerRight)));
    }

    internal double CursorHeight
    {
      get => cursorHeight;
      set
      {
        cursorHeight = value;
        Redraw();
      }
    }

    //----------------------------- GUI -------------------------

    public event PropertyChangedEventHandler PropertyChanged;

    public Point Upper =>       new Point(0,        CursorHeight / 2);
    public Point UpperLeft =>   new Point(-PART_DX, CursorHeight / 2 + PART_DY);
    public Point UpperRight =>  new Point(PART_DX,  CursorHeight / 2 + PART_DY);

    public Point Downer =>      new Point(0,        -CursorHeight / 2);
    public Point DownerLeft =>  new Point(-PART_DX, -CursorHeight / 2 - PART_DY);
    public Point DownerRight => new Point(PART_DX,  -CursorHeight / 2 - PART_DY);

    public Visibility CursorVisibility
    {
      get => cursorVisibility;
      set
      {
        cursorVisibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CursorVisibility)));
      }
    }


    //----------------------------- IPlaceable -------------------------

    public double X
    {
      get => x;
      set
      {
        x = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X)));
        Redraw();
      }
    }

    public double Y
    {
      get => y;
      set
      {
        y = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y)));
        Redraw();
      }
    }

    public Point? DestPos { get; set; }

    public bool IsJustAdded { get; set; }


    //----------------------------- Private members -------------------------------

    private double x;
    private double y;
    private double cursorHeight = 100;
    private Visibility cursorVisibility = Visibility.Hidden;
    private const double PART_DX = 5;
    private const double PART_DY = 3;
  }
}
