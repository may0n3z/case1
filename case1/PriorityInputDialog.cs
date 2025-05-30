using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace case1
{
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
}
