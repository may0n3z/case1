using System.ComponentModel;
using System.Drawing;

public class DeliveryPoint : INotifyPropertyChanged
{
    public double X { get; } // Исходная широта
    public double Y { get; } // Исходная долгота
    public double Priority { get; }
    public double ID { get; }

    private double _canvasX;
    public double CanvasX
    {
        get => _canvasX;
        set { _canvasX = value; OnPropertyChanged(nameof(CanvasX)); }
    }

    private double _canvasY;
    public double CanvasY
    {
        get => _canvasY;
        set { _canvasY = value; OnPropertyChanged(nameof(CanvasY)); }
    }

    public DeliveryPoint(double x, double y, double priority, double id )
    {
        X = x;
        Y = y;
        Priority = priority;
        ID = id;
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}
