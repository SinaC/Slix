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
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        private static Dispatcher _uiDispatcher;
        private Actions _currentActions;
        private CarPhysics2 _car;
        private Ellipse _carUI;

        private List<string> _datas = new List<string>();

        public MainWindow2()
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

            Line line = new Line
            {
                X1 = -radius,
                Y1 = - radius,
                X2 = - radius,
                Y2 = + radius+radius,
                Stroke = new SolidColorBrush(color)
            };
            canvasTest.Children.Add(line);
            Canvas.SetLeft(line, x+radius);
            Canvas.SetTop(line, y);
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
            Canvas.SetLeft(_carUI, _car.Position.X/10);
            Canvas.SetTop(_carUI, _car.Position.Y/10);

            RotateTransform rotateTransform = new RotateTransform(_car.Angle * 180.0 / Math.PI, _carUI.ActualWidth / 2.0, _carUI.ActualHeight / 2.0);
            _carUI.RenderTransform = rotateTransform;

            _datas = new List<string>
                {
                    String.Format("Mass: {0:F6}", _car.Mass),
                    String.Format("Drag: {0:F6}", _car.Drag),
                    String.Format("RR: {0:F6}", _car.RollingResistance),
                    String.Format("Engine: {0:F6}", _car.EngineForce),
                    String.Format("Speed: {0:F6}", _car.Speed),
                    String.Format("Angle: {0:F6}", _car.Angle),
                    String.Format("Drag: {0:F6}", _car.Drag),
                    String.Format("Direction: {0}", _car.NormalizedDirection ?? Vector2D.NullObject),
                    String.Format("V: {0}", _car.Velocity ?? Vector2D.NullObject),
                    String.Format("F: {0}", _car.Force ?? Vector2D.NullObject),
                    String.Format("P: {0}", _car.Position ?? Vector2D.NullObject),
                    String.Format("Ftraction: {0}", _car.FTraction ?? Vector2D.NullObject),
                    String.Format("Fdrag: {0}", _car.FDrag ?? Vector2D.NullObject),
                    String.Format("Frr: {0}", _car.FRollingResistance ?? Vector2D.NullObject),
                    String.Format("Flong: {0}", _car.FLongitudinal ?? Vector2D.NullObject),
                    String.Format("Steering: {0:F6}", _car.Steering),
                    String.Format("Turn: {0:F6}", _car.TurnCircleRadius),
                    String.Format("AV: {0:F6}", _car.AngularVelocity),
                };
            lstDatas.ItemsSource = _datas;
        }

        private void ResetCar()
        {
            _car.Initialize(0,0,0);
        }

        private void GameTask()
        {
            _car = new CarPhysics2();
            ResetCar();

            while(true)
            {
                _car.HandleActions(_currentActions);
                _car.Step(1);

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
