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
namespace case1
{
    /// <summary>
    /// Interaction logic for MainWindow.Xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //BestDelivery.Point point1 = new BestDelivery.Point() { X = 55.33, Y= 44.33};
            //BestDelivery.Point point2 = new BestDelivery.Point() { X = 54.33, Y = 34.33 };
            //List<int> route = new List<int>{ -1, 1, 2, 3, -1 };
            //double data = BestDelivery.RoutingTestLogic.CalculateDistance(point1,point2);
            //List<Order> orders = new List<Order>{
            //new Order { ID = 1, Destination = new BestDelivery.Point { X = 55.76, Y = 37.62 }, Priority = 1.0 }, // Заказ 1
            //new Order { ID = 2, Destination = new BestDelivery.Point { X = 90.74, Y = 37.60 }, Priority = 0.8 }, // Заказ 2
            //new Order { ID = 3, Destination = new BestDelivery.Point { X = 55.73, Y = 37.63 }, Priority = 0.9 }  // Заказ 3
            //};

            //var data3 = OrderArrays.GetOrderArray6();
            //var data2 = BestDelivery.RoutingTestLogic.CalculateRouteCost(route, orders, point1);
            //testbox.Text = data2.ToString();


            CalculateRoute cr = new CalculateRoute();
            cr.GetMinOrdersPatch(BestDelivery.OrderArrays.GetOrderArray1());
        }


        
    }
    class CalculateRoute
    {
        //public double GetMinOrdersPatch(Order[] orders)
        //{
        //    double a = 0.5;
        //    List<double> cost = new List<double>();
        //    var listorders = orders.ToList();
        //    Order first = listorders[0];
        //    for (int i = 0; listorders.Count > 0; i++)
        //    {


        //        for (int j = 0; j < listorders.Count; j++)
        //        {
        //            if (i == j)
        //            {
        //                continue;
        //            }
        //            else
        //            {
        //                cost.Add(BestDelivery.RoutingTestLogic.CalculateDistance(orders[i].Destination, orders[j].Destination) * (1 + a * orders[i].Priority));
        //            }
        //            if (listorders.Count < 1)
        //            {
        //                cost.Add(BestDelivery.RoutingTestLogic.CalculateDistance(orders[j].Destination, first.Destination) * (1 + a * orders[i].Priority));
        //            }
        //        }
        //        listorders.Remove(orders[i]);
        //    }
        //    //должен учитывать что метот перевызывается и не брать 1 и те же точки   
        //    return cost.Min();
        //}
        public double GetMinOrdersPatch(Order[] orders)
        {
            double a = 0.5;
            List<double> cost = new List<double>();
            int n = orders.Length;

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    // Расстояние между i и j
                    double dist = BestDelivery.RoutingTestLogic.CalculateDistance(orders[i].Destination, orders[j].Destination);

                    // Учитываем приоритеты обеих точек (например, среднее или сумму)
                    // В вашем коде учитывался приоритет только orders[i], можно сделать так:
                    double priorityFactor = 1 + a * (orders[i].Priority + orders[j].Priority) / 2.0;

                    cost.Add(dist * priorityFactor);
                }
            }
            if (cost.Count == 0)
                return 0; // или другое значение, если нет ребер

            return cost.Min();
        }

    }
}