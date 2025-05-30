using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BestDelivery;

namespace case1
{
    /// <summary>
    /// Interaction logic for MainWindow.Xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        static CalculateRoute cr = new CalculateRoute();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = MainViewModel.Instance;
            var canvas = sender as Canvas;
            if (canvas == null) return;
            double id = vm.Points.Count > 0 ? vm.Points.Max(p => p.ID) + 1 : 0;
            // Получаем позицию клика относительно Canvas
            System.Windows.Point clickPos = e.GetPosition(canvas);
            // Обратное масштабирование: преобразуем координаты Canvas в "географические"
            double lat = vm.InverseScaleY(clickPos.Y);
            double lon = vm.InverseScaleX(clickPos.X);
            // Добавляем новую точку
            vm.Points.Add(new DeliveryPoint(lat ,lon, 0.5, id));
            NewPatch();
        }

        void CalculatePointsAmount(Order[] orders)
        {
            PathLenght.Text = "";
            List<int> ordersRoute = cr.GetMinOrdersPatch(orders);
            foreach (var item in ordersRoute)
            {
                PathLenght.Text += "Точка " + item.ToString() + "\n";
            }
        }

        void UpdateWeight(Order[] orders)
        {
            WeightLenght.Text = "";
            List<int> ordersRoute = cr.GetMinOrdersPatch(orders);
            Order[] ordersNew = orders.Skip(1).ToArray();
            var depot = orders[0].Destination;
            WeightLenght.Text = BestDelivery.RoutingTestLogic.CalculateRouteCost(ordersRoute, ordersNew.ToList(), depot).ToString();
        }

        private void GetOrder_Click(object sender, RoutedEventArgs e)
        {
            var vm = MainViewModel.Instance;
            var button = sender as Button;
            if (button == null) return;
            int orderNumber;
            if (!int.TryParse(button.Tag?.ToString(), out orderNumber)) return;
            Order[] orders = orderNumber switch
            {
                1 => OrderArrays.GetOrderArray1(),
                2 => OrderArrays.GetOrderArray2(),
                3 => OrderArrays.GetOrderArray3(),
                4 => OrderArrays.GetOrderArray4(),
                5 => OrderArrays.GetOrderArray5(),
                6 => OrderArrays.GetOrderArray6(),      
            };
            PathLenght.Text = "";
            Order[] ordersNew = orders.Skip(1).ToArray();
            UpdateWeight(orders);
            CalculatePointsAmount(orders);
            if (orders == null) return; 
            vm.Points.Clear(); // очищаем старые точки
            foreach (var order in orders)
            {
                vm.AddPoint(new DeliveryPoint(order.Destination.X, order.Destination.Y, order.Priority, order.ID));               
            }
            NewPatch();
        }
        private void NewPath_Click(object sender, RoutedEventArgs e)
        {
            NewPatch();
        }
        public void NewPatch()
        {

            if (this.DataContext is MainViewModel vm)
            {
                Order[] orders = vm.Points.Select(static point => new BestDelivery.Order
                {
                    ID = (int)point.ID,
                    Destination = new BestDelivery.Point { X = point.X, Y = point.Y }, // если тип совпадает!
                    Priority = point.Priority
                }).ToArray();
                updatelines(cr.GetMinOrdersPatch(orders), vm.Points);
                UpdateWeight(orders);
                CalculatePointsAmount(orders);
            }
        }

        void updatelines(List<int> route, ObservableCollection<DeliveryPoint> points)
        {
            // Создаем копию исходной коллекции
            var oldpoints = new ObservableCollection<DeliveryPoint>(points);
            var vm = MainViewModel.Instance;

            // Очищаем исходную коллекцию
            vm.Points.Clear();

            // Добавляем первое значение без изменений
            if (oldpoints.Count > 0)
            {
                vm.AddPoint(new DeliveryPoint(
                    oldpoints[0].X,
                    oldpoints[0].Y,
                    oldpoints[0].Priority,
                    oldpoints[0].ID));
            }
            for (int i = 1; i < route.Count; i++)
            {
                int id = route[i];
                var point = oldpoints.FirstOrDefault(p => p.ID == id);
                if (point != null && point != oldpoints.First() && point != oldpoints.Last())
                {
                    vm.AddPoint(new DeliveryPoint(point.X, point.Y, point.Priority, point.ID));
                }
            }

            // Добавляем последнее значение без изменений
            if (oldpoints.Count > 1)
            {
                vm.AddPoint(new DeliveryPoint(
                    oldpoints[oldpoints.Count - 1].X,
                    oldpoints[oldpoints.Count - 1].Y,
                    oldpoints[oldpoints.Count - 1].Priority,
                    oldpoints[oldpoints.Count - 1].ID));
            }
        }
    }
    class CalculateRoute
    {
        public List<int> GetMinOrdersPatch(Order[] orders)
        {
            int n = orders.Length;
            if (n == 0) return (new List<int>());
            HashSet<int> unvisited = new HashSet<int>(Enumerable.Range(1, n - 1));
            int current = 0;
            double totalCost = 0;
            List<int> route = new List<int> { orders[current].ID }; // начинаем маршрут с ID стартового заказа
            while (unvisited.Count > 0)
            {
                double minCost = double.MaxValue;
                int nextVertex = -1;
                foreach (int candidate in unvisited)
                {
                    double dist = BestDelivery.RoutingTestLogic.CalculateDistance(orders[current].Destination, orders[candidate].Destination);
                    double priorityFactor = 1 * (orders[current].Priority + orders[candidate].Priority) / 2.0;
                    double cost = dist * priorityFactor;
                    if (cost < minCost)
                    {
                        minCost = cost;
                        nextVertex = candidate;
                    }
                }
                if (nextVertex == -1)
                {
                    break;
                }
                totalCost += minCost;
                current = nextVertex;
                unvisited.Remove(nextVertex);
                route.Add(orders[current].ID); // добавляем ID следующего заказа в маршрут
            }
            // Возврат к стартовой вершине
            if (n > 1)
            {
                double distBack = BestDelivery.RoutingTestLogic.CalculateDistance(orders[current].Destination, orders[0].Destination);
                double priorityFactorBack = 1 * (orders[current].Priority + orders[0].Priority) / 2.0;
                totalCost += distBack * priorityFactorBack;
                route.Add(orders[0].ID); // добавляем в конец маршрут возврат к старту
            }
            
            return route;
            
        }

    }
}
