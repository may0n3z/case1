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
        public Geometry PathGeometry
        {
            get => _pathGeometry;
            set { _pathGeometry = value; OnPropertyChanged(nameof(PathGeometry)); }
        }

        public MainViewModel()
        {
            Points.CollectionChanged += (s, e) => UpdatePath();
        }

        public void AddPoint(DeliveryPoint point)
        {
            Points.Add(point);
            UpdatePath();
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
                StartPoint = new Point(Points[0].X + 10, Points[0].Y + 10), // центр эллипса
                IsClosed = false,
                IsFilled = false
            };

            for (int i = 1; i < Points.Count; i++)
            {
                figure.Segments.Add(new LineSegment(new Point(Points[i].X + 10, Points[i].Y + 10), true));
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
