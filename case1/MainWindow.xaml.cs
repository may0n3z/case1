using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
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
            var canvas = sender as Canvas;
            if (canvas == null) return;
            double id = vm.Points.Count > 1 ? vm.Points.Max(p => p.ID) + 1 : 0;        
            System.Windows.Point clickPos = e.GetPosition(canvas);
            double lat = vm.InverseScaleY(clickPos.Y);
            double lon = vm.InverseScaleX(clickPos.X);
            // Добавляем новую точку
            double priority;
            PriorityInputDialog priorityInputDialog = new PriorityInputDialog();
            if (priorityInputDialog.ShowDialog() == true)
            {
                priority = Convert.ToDouble(priorityInputDialog.InputValue);
                
            }
            else
            {
                priority = 0.0;
            }
            vm.AddPoint(new DeliveryPoint(lat ,lon, priority, id));
            vm.UpdateScaledPointsAndPath();
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
                vm = MainViewModel.Instance;
                Order[] orders = vm.Points.Select(static point => new BestDelivery.Order
                {
                    ID = (int)point.ID,
                    Destination = new BestDelivery.Point { X = point.X, Y = point.Y },
                    Priority = point.Priority
                }).ToArray();

                UpdateLines(cr.GetMinOrdersPatch(orders));
                UpdateWeight(orders);
                CalculatePointsAmount(orders);
            }
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
    public class PriorityInputDialog : Window
    {
        private TextBox inputTextBox;
        private Button okButton;
        public string InputValue => inputTextBox.Text;
        public double? Priority { get; private set; } = null;
        public PriorityInputDialog()
        {
            this.Title = "Введите приоритет";
            this.Width = 300;
            this.Height = 150;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ResizeMode = ResizeMode.NoResize;

            var panel = new StackPanel { Margin = new Thickness(10) };

            var prompt = new TextBlock
            {
                Text = "Выберите приоритет от 0.0 до 1.0:",
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(prompt);

            inputTextBox = new TextBox { Height = 25 };
            panel.Children.Add(inputTextBox);

            okButton = new Button
            {
                Content = "ОК",
                Width = 70,
                Height = 25,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                IsDefault = true
            };
            okButton.Click += OkButton_Click;
            panel.Children.Add(okButton);

            this.Content = panel;
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(inputTextBox.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value))
            {
                if (value >= 0.0 && value <= 1.0)
                {
                    Priority = value;
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Значение должно быть в диапазоне от 0.0 до 1.0.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректное число.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
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
