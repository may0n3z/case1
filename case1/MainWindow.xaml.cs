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
using BestDelivery;

namespace case1
{
    /// <summary>
    /// Interaction logic for MainWindow.Xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private int _pointCounter = 1;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm == null) return;

            // Получаем позицию клика относительно Canvas
            var pos = e.GetPosition(sender as UIElement);

            // Добавляем новую точку с уникальным именем
            vm.AddPoint(new DeliveryPoint($"Точка {_pointCounter++}", pos.X - 10, pos.Y - 10));
        }

        private void Get1Order_Click(object sender, RoutedEventArgs e)
        {
            BestDelivery.OrderArrays.GetOrderArray1();

        }

        //public static void MathMetod()
        //{
        //    BestDelivery.Point depot = new BestDelivery.Point { X = 55.75, Y = 37.61 }; // Москва
        //    Order[] orders =
        //    {
        //        new Order { ID = 1, Destination = new BestDelivery.Point { X = 55.76, Y = 37.62 }, Priority = 1.0 }, 
        //        new Order { ID = 2, Destination = new BestDelivery.Point { X = 90.74, Y = 37.60 }, Priority = 0.8 }, 
        //        new Order { ID = 3, Destination = new BestDelivery.Point { X = 55.73, Y = 37.63 }, Priority = 0.9 }  
        //    };
        //    int[] route = { -1, 1, 2, 3, -1 }; // Пример маршрута: склад -> заказ 1 -> заказ 2 -> заказ 3 -> склад
        //    double routeCost;

        //    if (RoutingTestLogic.TestRoutingSolution(depot, orders, route, out routeCost))
        //    {
        //        Console.WriteLine($"Стоимость маршрута: {routeCost}");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Проверка маршрутизации завершилась неудачей");
        //    }
        //}
    }

}