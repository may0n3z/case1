using System.Collections.Generic;
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
using BestDelivery;
using System;
using System.Collections.Generic;
using System.Linq;

namespace case1
{
    /// <summary>
    /// Interaction logic for MainWindow.Xaml
    /// </summary>
    /// 
    public class Order
    {
        public int ID { get; set; }
        public Point Destination { get; set; }
        public double Priority { get; set; }
    }
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CalculateRoute cr = new CalculateRoute();

            // Получаем массив из BestDelivery
            var bestDeliveryOrders = BestDelivery.OrderArrays.GetOrderArray1();

            // Преобразуем в case1.Order[]
            var orders = bestDeliveryOrders
                .Select(o => new case1.Order
                {
                    ID = o.ID,
                    Destination = new case1.Point { X = o.Destination.X, Y = o.Destination.Y },
                    Priority = o.Priority
                })
                .ToArray();

            // Теперь можно передать в метод
            var (path, cost) = cr.FindMinRoute(orders);

            MessageBox.Show($"Минимальный маршрут: {string.Join(" → ", path.Select(o => o.ID))}\nСтоимость: {cost}");
        }
    }
    public class CalculateRoute
    {
        public (List<Order> path, double totalCost) FindMinRoute(Order[] orders)
        {
            // Найти стартовую точку (ID = -1)
            var start = orders.First(o => o.ID == -1);
            var others = orders.Where(o => o.ID != -1).ToList();

            // Все возможные перестановки остальных точек
            var permutations = GetPermutations(others, others.Count);

            List<Order> bestPath = null;
            double minCost = double.MaxValue;

            foreach (var perm in permutations)
            {
                var path = new List<Order> { start };
                path.AddRange(perm);
                path.Add(start); // Возвращаемся в начало

                double cost = CalculatePathCost(path);

                if (cost < minCost)
                {
                    minCost = cost;
                    bestPath = path;
                }
            }

            return (bestPath, minCost);
        }

        private double CalculatePathCost(List<Order> path)
        {
            double cost = 0;
            double a = 0.5;

            for (int i = 0; i < path.Count - 1; i++)
            {
                var dist = CalculateDistance(path[i].Destination, path[i + 1].Destination);
                var priorityFactor = 1 + a * (path[i].Priority + path[i + 1].Priority) / 2.0;
                cost += dist * priorityFactor;
            }

            return cost;
        }

        private double CalculateDistance(Point p1, Point p2)
        {
            // Здесь ваша реализация расстояния (например, евклидово)
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Генератор всех перестановок списка
        private static IEnumerable<List<Order>> GetPermutations(List<Order> list, int length)
        {
            if (length == 1) return list.Select(t => new List<Order> { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new List<Order> { t2 }).ToList());
        }
    }



    //class CalculateRoute
    //{
    //    public double GetMinOrdersPatch(Order[] orders)
    //    {
    //        double a = 0.5;
    //        List<double> cost = new List<double>();
    //        int n = orders.Length;

    //        for (int i = 0; i < n; i++)
    //        {
    //            for (int j = i + 1; j < n; j++)
    //            {
    //                // Расстояние между i и j
    //                double dist = BestDelivery.RoutingTestLogic.CalculateDistance(orders[i].Destination, orders[j].Destination);

    //                // Учитываем приоритеты обеих точек (например, среднее или сумму)
    //                // В вашем коде учитывался приоритет только orders[i], можно сделать так:
    //                double priorityFactor = 1 + a * (orders[i].Priority + orders[j].Priority) / 2.0;

    //                cost.Add(dist * priorityFactor);
    //            }
    //        }
    //        if (cost.Count == 0)
    //            return 0; // или другое значение, если нет ребер

    //        return cost.Min();
    //    }

    //}
}