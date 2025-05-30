using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BestDelivery;

namespace case1
{
    /// <summary>
    /// Interaction logic for MainWindow.Xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        //private int _pointCounter = 1;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = MainViewModel.Instance;
            var canvas = sender as Canvas;
            if (canvas == null) return;

            // Получаем позицию клика относительно Canvas
            System.Windows.Point clickPos = e.GetPosition(canvas);

            // Обратное масштабирование: преобразуем координаты Canvas в "географические"
            double lat = vm.InverseScaleY(clickPos.Y);
            double lon = vm.InverseScaleX(clickPos.X);
            // Добавляем новую точку
            vm.AddPoint(new DeliveryPoint(lat, lon));

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

            CalculateRoute cr = new CalculateRoute();
            List<int> ordersRoute = cr.GetMinOrdersPatch(orders);
            var depot = orders[0].Destination;
            WeightLenght.Text = BestDelivery.RoutingTestLogic.CalculateRouteCost(ordersRoute, ordersNew.ToList(), depot).ToString();
            foreach (var item in ordersRoute)
            {
                PathLenght.Text += "Точка "+item.ToString() +"\n";
            }
            if (orders == null) return;
            vm.Points.Clear(); // очищаем старые точки
            foreach (var order in orders)
            {
                vm.AddPoint(new DeliveryPoint(order.Destination.X, order.Destination.Y));
            }
        }

        private void NewPath_Click(object sender, RoutedEventArgs e)
        {
            //CalculateRoute cr = new CalculateRoute();
            //if (this.DataContext is MainViewModel vm)
            //{
            //    Order[] orders = vm.Points
            //        .Select(point => new Order {}).ToArray();
            //    List<int> ordersRoute = cr.GetMinOrdersPatch(orders);
            //    Order[] ordersNew = orders.Skip(1).ToArray();
            //    var depot = orders[0].Destination;
            //    WeightLenght.Text = BestDelivery.RoutingTestLogic.CalculateRouteCost(ordersRoute, ordersNew.ToList(), depot).ToString();
            //}
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
