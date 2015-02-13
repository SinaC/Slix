using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Slix;

namespace WPF_POC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Dispatcher _uiDispatcher;
        private Actions _currentActions;
        private Car _car;
        private Ellipse _carUI;

        public MainWindow()
        {
            _uiDispatcher = Dispatcher.CurrentDispatcher;

            InitializeComponent();

            _carUI = new Ellipse
                {
                    Width = 20,
                    Height = 10,
                    Fill = new SolidColorBrush(Colors.Black)
                };
            canvasTest.Children.Add(_carUI);

            AddBend(100, 100, 100, Colors.Yellow);
            AddBend(100, 300, 100, Colors.Yellow);
            AddBend(300, 100, 100, Colors.Orange);
            AddBend(300, 300, 100, Colors.Orange);
            AddBend(500, 100, 100, Colors.Red);
            AddBend(500, 300, 100, Colors.Red);

            Task.Factory.StartNew(GameTask);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            OnKey(e.Key, true);
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            OnKey(e.Key, false);
        }

        private void OnKey(Key key, bool isKeyDown)
        {
            switch (key)
            {
                case Key.Left:
                    _currentActions.Left = isKeyDown;
                    break;
                case Key.Right:
                    _currentActions.Right = isKeyDown;
                    break;
                case Key.Up:
                    _currentActions.Accelerate = isKeyDown;
                    break;
                case Key.Down:
                    _currentActions.Brake = isKeyDown;
                    break;
                case Key.Delete:
                    _currentActions.HandBrake = isKeyDown;
                    break;
            }
            ActionUpdated();
        }

        public void AddBend(double x, double y, double radius, Color color)
        {
            Arc arcTop = new Arc
            {
                Center = new Point(0, 0),
                StartAngle = Math.PI / 2,
                EndAngle = -Math.PI / 2,
                Radius = radius,
                Stroke = new SolidColorBrush(color)
            };
            canvasTest.Children.Add(arcTop);
            Canvas.SetLeft(arcTop, x);
            Canvas.SetTop(arcTop, y);

            Arc arcBottom = new Arc
            {
                Center = new Point(0, 0),
                StartAngle = -Math.PI / 2,
                EndAngle = Math.PI / 2,
                Radius = radius,
                Stroke = new SolidColorBrush(color)
            };
            canvasTest.Children.Add(arcBottom);
            Canvas.SetLeft(arcBottom, x);
            Canvas.SetTop(arcBottom, y + radius);
        }

        private void ActionUpdated()
        {
            DisplayActions();
        }

        private void DisplayActions()
        {
            txtInputs.Text = String.Format("{0}{1}{2}{3}{4}",
                                           _currentActions.Left ? 'L' : ' ',
                                           _currentActions.Right ? 'R' : ' ',
                                           _currentActions.Accelerate ? 'A' : ' ',
                                           _currentActions.Brake ? 'B' : ' ',
                                           _currentActions.HandBrake ? 'H' : ' ');
        }

        private void DisplayCar()
        {
            Canvas.SetLeft(_carUI, _car.LocationX / 50);
            Canvas.SetTop(_carUI, _car.LocationY / 50);

            RotateTransform rotateTransform = new RotateTransform(_car.Angle, _carUI.ActualWidth / 2.0, _carUI.ActualHeight / 2.0);
            //RotateTransform rotateTransform = new RotateTransform(_car.Angle, 0, 0);
            _carUI.RenderTransform = rotateTransform;

            txtMaxPower.Text = String.Format("MaxPower: {0:F6}",_car.MaxPower);
            txtHandling.Text = String.Format("Handling: {0:F6}",_car.Handling);
            txtAcceleration.Text = String.Format("Acceleration: {0:F6}",_car.Acceleration);
            txtBraking.Text = String.Format("Braking: {0:F6}",_car.Braking);
            txtHandBrake.Text = String.Format("HandBrake: {0:F6}",_car.HandBrake);
            txtAngle.Text = String.Format("Angle: {0:F6}",_car.Angle);
            txtVelocity.Text = String.Format("Velocity: {0:F6} {1:F6}",_car.VelocityX, _car.VelocityY);
            txtPower.Text = String.Format("Power: {0:F6}",_car.Power);
            txtSteering.Text = String.Format("Steering: {0:F6}", _car.Steering);
        }

        private void ResetCar()
        {
            _car.Initialize(200, 200, 0, 100, 5, 1, 0.25, 2, new GameParameters
            {
                Friction = 0.87
            });
        }

        private void GameTask()
        {
            _car = new Car();
            ResetCar();

            while(true)
            {
                _car.HandleActions(_currentActions);
                _car.Step(0);

                ExecuteOnUIThread(DisplayCar);

                System.Threading.Thread.Sleep(50);
            }
        }

        public static void ExecuteOnUIThread(Action action, DispatcherPriority priority = DispatcherPriority.Render)
        {
            try
            {
                _uiDispatcher.Invoke(action, priority);
            }
            catch (Exception ex)
            {
                //
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ResetCar();
        }
    }
}
