using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace case1
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DeliveryPoint> Points { get; set; } = new();

        private Geometry _pathGeometry;
        private const double CanvasWidth = 800;
        private const double CanvasHeight = 700;

        private double minLat = double.MaxValue;
        private double maxLat = double.MinValue;
        private double minLon = double.MaxValue;
        private double maxLon = double.MinValue;

        public Geometry PathGeometry
        {
            get => _pathGeometry;
            set { _pathGeometry = value; OnPropertyChanged(nameof(PathGeometry)); }
        }
        public static MainViewModel Instance { get; private set; }
        public MainViewModel()
        {
            Points = new ObservableCollection<DeliveryPoint>
            {
            };
            Instance = this;
            UpdatePath();
            Points.CollectionChanged += (s, e) => UpdatePath();
        }

        public void AddPoint(DeliveryPoint point)
        {
            // Обновляем минимумы и максимумы
            if (point.X < minLat) minLat = point.X;
            if (point.X > maxLat) maxLat = point.X;
            if (point.Y < minLon) minLon = point.Y;
            if (point.Y > maxLon) maxLon = point.Y;

            Points.Add(point);
            UpdateScaledPointsAndPath();
        }
        public double InverseScaleX(double canvasX)
        {
            double lonRange = maxLon - minLon;
            if (lonRange == 0) lonRange = 1;
            return (canvasX / CanvasWidth) * lonRange + minLon;
        }

        public double InverseScaleY(double canvasY)
        {
            double latRange = maxLat - minLat;
            if (latRange == 0) latRange = 1;
            // Обратите внимание на инверсию Y
            return ((CanvasHeight - canvasY) / CanvasHeight) * latRange + minLat;
        }


        private void UpdateScaledPointsAndPath()
        {
            if (Points.Count == 0)
            {
                PathGeometry = null;
                return;
            }

            // Создаём новую коллекцию с масштабированными точками
            var scaledPoints = Points.Select(p => new Point(ScaleX(p.Y), ScaleY(p.X))).ToList();

            // Строим PathGeometry по масштабированным точкам
            var figure = new PathFigure
            {
                StartPoint = scaledPoints[0],
                IsClosed = false,
                IsFilled = false
            };

            for (int i = 1; i < scaledPoints.Count; i++)
            {
                figure.Segments.Add(new LineSegment(scaledPoints[i], true));
            }

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            PathGeometry = geometry;

            // Обновляем позиции точек в коллекции для отображения (если DeliveryPoint поддерживает изменение координат)
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].CanvasX = scaledPoints[i].X -4;
                Points[i].CanvasY = scaledPoints[i].Y -4;
            }
        }

        private double ScaleX(double lon)
        {
            double lonRange = maxLon - minLon;
            if (lonRange == 0) lonRange = 1;
            return (lon - minLon) / lonRange * CanvasWidth;
        }

        private double ScaleY(double lat)
        {
            double latRange = maxLat - minLat;
            if (latRange == 0) latRange = 1;
            return CanvasHeight - (lat - minLat) / latRange * CanvasHeight;
        }


        private void UpdatePath()
        {
            if (Points.Count == 0)
            {
                PathGeometry = null;
                return;
            }

            var figure = new PathFigure
            {
                StartPoint = new Point(Points[0].X, Points[0].Y ), // центр эллипса
                IsClosed = false,
                IsFilled = false
            };

            for (int i = 1; i < Points.Count; i++)
            {
                figure.Segments.Add(new LineSegment(new Point(Points[i].X , Points[i].Y ), true));
            }         
            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            
            PathGeometry = geometry;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

