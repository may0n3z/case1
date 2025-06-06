using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Reflection;
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
        // Получаем точку при нажатии на Canvas
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = MainViewModel.Instance;
            vm.AddPoint(GetDotFromCanvas(sender,e));
            vm.UpdateScaledPointsAndPath();
        }

        public DeliveryPoint GetDotFromCanvas(object sender, MouseButtonEventArgs e)
        {
            var vm = MainViewModel.Instance;
            var canvas = sender as Canvas;
            //if (canvas == null) return;
            double id = vm.Points.Count > 1 ? vm.Points.Max(p => p.ID) + 1 : 0;
            System.Windows.Point clickPos = e.GetPosition(canvas);
            double lat = vm.InverseScaleY(clickPos.Y);
            double lon = vm.InverseScaleX(clickPos.X);
            // Добавляем новую точку
            double priority;
            PriorityInputDialog priorityInputDialog = new PriorityInputDialog();
            if (priorityInputDialog.ShowDialog() == true)
            {
                priority = priorityInputDialog.Priority;

            }
            else
            {
                priority = 0.0;
            }
            return new DeliveryPoint(lat, lon, priority, id);
        }

        //Обновление списка позиций точек
        void CalculatePointsAmount(Order[] orders)
        {
            PathLenght.Text = "";
            List<int> ordersRoute = cr.GetMinOrdersPatch(orders);
            foreach (var item in ordersRoute)
            {
                PathLenght.Text += "Точка " + item.ToString() + "\n";
            }
        }

        //Обновление общего веса пути
        void UpdateWeight(Order[] orders)
        {
            WeightLenght.Text = "";
            List<int> ordersRoute = cr.GetMinOrdersPatch(orders);
            Order[] ordersNew = orders.Skip(1).ToArray();
            var depot = orders[0].Destination;
            WeightLenght.Text = BestDelivery.RoutingTestLogic.CalculateRouteCost(ordersRoute, ordersNew.ToList(), depot).ToString();
        }
        //Получение и отображение списков заказов по тегам
        private void GetOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!graph.IsEnabled)
                graph.IsEnabled = true;
            UpdateVariables(GetOrdersNumber(sender,e));
        }

        public Order[] GetOrdersNumber(object sender, RoutedEventArgs e)
        {
            var vm = MainViewModel.Instance;
            var button = sender as Button;

            int orderNumber;
            if (!int.TryParse(button.Tag?.ToString(), out orderNumber));
            Order[] orders = orderNumber switch
            {
                1 => OrderArrays.GetOrderArray1(),
                2 => OrderArrays.GetOrderArray2(),
                3 => OrderArrays.GetOrderArray3(),
                4 => OrderArrays.GetOrderArray4(),
                5 => OrderArrays.GetOrderArray5(),
                6 => OrderArrays.GetOrderArray6(),
            };
            return orders;
        } 

        void UpdateVariables(Order[] orders)
        {
            var vm = MainViewModel.Instance;
            PathLenght.Text = "";
            Order[] ordersNew = orders.Skip(1).ToArray();
            UpdateWeight(orders);
            CalculatePointsAmount(orders);
            if (orders == null) return;
            vm.Points.Clear();
            foreach (var order in orders)
            {
                vm.AddPoint(new DeliveryPoint(order.Destination.X, order.Destination.Y, order.Priority, order.ID));
            }
        }
        //Пересчет пути на оптимальный после выставления точек
        private void NewPath_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                Order[] orders = ConvertAllPoints();
                UpdateLines(cr.GetMinOrdersPatch(orders));
                UpdateWeight(orders);
                CalculatePointsAmount(orders);
            }
        }
        public Order[] ConvertAllPoints()
        {
            var vm = MainViewModel.Instance;
            Order[] orders = vm.Points.Select(static point => new BestDelivery.Order
            {
                ID = (int)point.ID,
                Destination = new BestDelivery.Point { X = point.X, Y = point.Y },
                Priority = point.Priority
            }).ToArray();
            return orders;
        }
        //Переставление точек для отображения
        void UpdateLines(List<int> route)
        {
            //копия исходной коллекции
            var vm = MainViewModel.Instance;
            var oldpoints = new ObservableCollection<DeliveryPoint>(vm.Points);
            vm.Points.Clear();

            if (oldpoints.Count > 0)
            {
                vm.AddPoint(new DeliveryPoint(
                    oldpoints[0].X,
                    oldpoints[0].Y,
                    oldpoints[0].Priority,
                    oldpoints[0].ID));
            }
            for (int i = 0; i < route.Count; i++)
            {
                int id = route[i];
                var point = oldpoints.FirstOrDefault(p => p.ID == id);
                if (point != null && point.ID != -1)
                {
                    vm.AddPoint(new DeliveryPoint(point.X, point.Y, point.Priority, point.ID));
                }
            }
        }
    }
}
