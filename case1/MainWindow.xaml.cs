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
                priority = priorityInputDialog.Priority;
                
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
        public double Priority { get; private set; } = 0;
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
            if (double.TryParse(inputTextBox.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value))
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
            if (n == 0) return new List<int>();
            if (n == 1) return new List<int> { orders[0].ID, orders[0].ID };
            double[,] dist = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    dist[i, j] = (i == j) ? 0 : BestDelivery.RoutingTestLogic.CalculateDistance(orders[i].Destination, orders[j].Destination);
            var mstEdges = BuildMST(dist, n);
            var oddVertices = FindOddDegreeVertices(mstEdges, n);
            var matchingEdges = MinimumWeightPerfectMatching(oddVertices, dist);
            var multigraph = CombineGraphs(mstEdges, matchingEdges);
            var eulerianCircuit = FindEulerianCircuit(multigraph, n);
            var hamiltonianCycle = EulerToHamiltonian(eulerianCircuit);
            List<int> route = hamiltonianCycle.Select(i => orders[i].ID).ToList();
            route.Add(orders[hamiltonianCycle[0]].ID);
            return route;
        }
        private List<(int, int)> BuildMST(double[,] dist, int n)
        {
            var mstEdges = new List<(int, int)>();
            bool[] inMST = new bool[n];
            double[] key = new double[n];
            int[] parent = new int[n];
            for (int i = 0; i < n; i++)
            {
                key[i] = double.MaxValue;
                parent[i] = -1;
            }
            key[0] = 0;

            for (int count = 0; count < n - 1; count++)
            {
                int u = -1;
                double minKey = double.MaxValue;
                for (int v = 0; v < n; v++)
                {
                    if (!inMST[v] && key[v] < minKey)
                    {
                        minKey = key[v];
                        u = v;
                    }
                }

                inMST[u] = true;

                for (int v = 0; v < n; v++)
                {
                    if (!inMST[v] && dist[u, v] < key[v])
                    {
                        key[v] = dist[u, v];
                        parent[v] = u;
                    }
                }
            }

            for (int v = 1; v < n; v++)
            {
                mstEdges.Add((parent[v], v));
            }

            return mstEdges;
        }
        private List<int> FindOddDegreeVertices(List<(int, int)> edges, int n)
        {
            int[] degree = new int[n];
            foreach (var (u, v) in edges)
            {
                degree[u]++;
                degree[v]++;
            }
            var oddVertices = new List<int>();
            for (int i = 0; i < n; i++)
                if (degree[i] % 2 == 1)
                    oddVertices.Add(i);
            return oddVertices;
        }
        private List<(int, int)> MinimumWeightPerfectMatching(List<int> oddVertices, double[,] dist)
        {
            var matchingEdges = new List<(int, int)>();
            var used = new HashSet<int>();
            oddVertices.Sort(); 

            foreach (int u in oddVertices)
            {
                if (used.Contains(u)) continue;
                double minDist = double.MaxValue;
                int minV = -1;
                foreach (int v in oddVertices)
                {
                    if (u != v && !used.Contains(v))
                    {
                        if (dist[u, v] < minDist)
                        {
                            minDist = dist[u, v];
                            minV = v;
                        }
                    }
                }
                if (minV != -1)
                {
                    matchingEdges.Add((u, minV));
                    used.Add(u);
                    used.Add(minV);
                }
            }
            return matchingEdges;
        }

        private Dictionary<int, List<int>> CombineGraphs(List<(int, int)> mstEdges, List<(int, int)> matchingEdges)
        {
            var graph = new Dictionary<int, List<int>>();
            void AddEdge(int u, int v)
            {
                if (!graph.ContainsKey(u)) graph[u] = new List<int>();
                if (!graph.ContainsKey(v)) graph[v] = new List<int>();
                graph[u].Add(v);
                graph[v].Add(u);
            }

            foreach (var (u, v) in mstEdges)
                AddEdge(u, v);
            foreach (var (u, v) in matchingEdges)
                AddEdge(u, v);

            return graph;
        }

        private List<int> FindEulerianCircuit(Dictionary<int, List<int>> graph, int n)
        {
            var circuit = new List<int>();
            var stack = new Stack<int>();
            var current = 0;
            stack.Push(current);

            var localGraph = new Dictionary<int, List<int>>();
            foreach (var kvp in graph)
                localGraph[kvp.Key] = new List<int>(kvp.Value);

            while (stack.Count > 0)
            {
                current = stack.Peek();
                if (localGraph.ContainsKey(current) && localGraph[current].Count > 0)
                {
                    int next = localGraph[current][0];
                    localGraph[current].RemoveAt(0);
                    localGraph[next].Remove(current);
                    stack.Push(next);
                }
                else
                {
                    circuit.Add(current);
                    stack.Pop();
                }
            }

            circuit.Reverse();
            return circuit;
        }

        private List<int> EulerToHamiltonian(List<int> eulerCircuit)
        {
            var visited = new HashSet<int>();
            var path = new List<int>();
            foreach (var v in eulerCircuit)
            {
                if (!visited.Contains(v))
                {
                    visited.Add(v);
                    path.Add(v);
                }
            }
            return path;
        }
    }
}
