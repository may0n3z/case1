using System.ComponentModel;

public class DeliveryPoint : INotifyPropertyChanged
{
    public double X { get; } // Исходная широта
    public double Y { get; } // Исходная долгота

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

    public DeliveryPoint(double x, double y)
    {
        X = x;
        Y = y;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}
