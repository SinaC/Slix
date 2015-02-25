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
        //private Car _car;
        //private CarSkid _car;
        //private CarRobot _car;
        //private Car4Wheels _car;
        //private CarPhysics _car;
        //private Car2Wheels _car;
        //private CarWheels _car;
        private CarWheels _car;
        private FrameworkElement _carUI;
        private List<string> _datas = new List<string>();

        public MainWindow()
        {
            _uiDispatcher = Dispatcher.CurrentDispatcher;

            InitializeComponent();

            //_carUI = new Ellipse
            //    {
            //        Width = 20,
            //        Height = 10,
            //        Fill = new SolidColorBrush(Colors.Black)
            //    };
            _carUI = new Rectangle
            {
                Width = 10,
                Height = 20,
                Stroke = new SolidColorBrush(Colors.Black)
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
            double posX = _car.Position.X;
            double posY = _car.Position.Y;

            //Canvas.SetLeft(_carUI, _car.PositionX);
            //Canvas.SetTop(_carUI, _car.PositionY);
            //Canvas.SetLeft(_carUI, _car.PositionX);
            //Canvas.SetTop(_carUI, _car.PositionY);
            //Canvas.SetLeft(_carUI, _car.LocationX / 50);
            //Canvas.SetTop(_carUI, _car.LocationY / 50);
            //Canvas.SetLeft(_carUI, _car.CarLocationX / 100);
            //Canvas.SetTop(_carUI, _car.CarLocationY / 100);
            Canvas.SetLeft(_carUI, posX - _carUI.ActualWidth / 2.0);
            Canvas.SetTop(_carUI, posY - _carUI.ActualHeight / 2.0);

            //RotateTransform rotateTransform = new RotateTransform(_car.Angle, _carUI.ActualWidth / 2.0, _carUI.ActualHeight / 2.0);
            //RotateTransform rotateTransform = new RotateTransform(_car.Angle*180.0/Math.PI, _carUI.ActualWidth / 2.0, _carUI.ActualHeight / 2.0);
            //RotateTransform rotateTransform = new RotateTransform(_car.CarHeading * 180.0 / Math.PI, _carUI.ActualWidth / 2.0, _carUI.ActualHeight / 2.0);
            //RotateTransform rotateTransform = new RotateTransform(_car.AngleInRadians * 180.0 / Math.PI, _carUI.ActualWidth / 2.0, _carUI.ActualHeight / 2.0);
            //RotateTransform rotateTransform = new RotateTransform(_car.Angle, 0, 0);
            //RotateTransform rotateTransform = new RotateTransform(_car.Direction * 180.0 / Math.PI, _carUI.ActualWidth / 2.0, _carUI.ActualHeight / 2.0);
            RotateTransform rotateTransform = new RotateTransform(_car.Angle * 180.0 / Math.PI, _carUI.ActualWidth / 2.0, _carUI.ActualHeight / 2.0);
            _carUI.RenderTransform = rotateTransform;

            //txtMaxPower.Text = String.Format("MaxPower: {0:F6}",_car.MaxPower);
            //txtHandling.Text = String.Format("Handling: {0:F6}",_car.Handling);
            //txtAcceleration.Text = String.Format("Acceleration: {0:F6}",_car.Acceleration);
            //txtBraking.Text = String.Format("Braking: {0:F6}",_car.Braking);
            //txtHandBrake.Text = String.Format("HandBrake: {0:F6}",_car.HandBrake);
            //txtAngle.Text = String.Format("Angle: {0:F6}",_car.Angle);
            //txtVelocity.Text = String.Format("Velocity: {0:F6} {1:F6}",_car.VelocityX, _car.VelocityY);
            //txtPower.Text = String.Format("Power: {0:F6}",_car.Power);
            //txtSteering.Text = String.Format("Steering: {0:F6}", _car.Steering);

            //txtHandling.Text = String.Format("TurnSpeed: {0:F6}", _car.TurnSpeed);
            //txtAcceleration.Text = String.Format("Power: {0:F6}", _car.MaxPower);
            //txtAngle.Text = String.Format("Angle: {0:F6}", _car.Angle);
            //double velocity = Math.Sqrt(_car.VelocityX*_car.VelocityX + _car.VelocityY*_car.VelocityY);
            //txtVelocity.Text = String.Format("Velocity: {0:F6} -> {1:F6} {2:F6}", velocity, _car.VelocityX, _car.VelocityY);
            //txtSteering.Text = String.Format("AngularVelocity: {0:F6}", _car.AngularVelocity);

            //txtHandling.Text = String.Format("Handling: {0:F6}", _car.Handling);
            //txtAcceleration.Text = String.Format("Acceleration: {0:F6}", _car.MaxAcceleration);
            //txtMaxPower.Text = String.Format("MaxForwardSpeed: {0:F6}", _car.MaxForwardSpeed);
            //txtBraking.Text = String.Format("MaxBackwardSpeed: {0:F6}", _car.MaxBackwardSpeed);
            //txtAngle.Text = String.Format("Angle: {0:F6}", _car.Angle);
            //txtVelocity.Text = String.Format("Velocity: {0:F6} -> {1:F6}", _car.CurrentSpeed, _car.DesiredSpeed);

            //txtAngle.Text = String.Format("Angle: {0:F6}", _car.Angle);
            //txtVelocity.Text = String.Format("Speed: {0:F6}", _car.Speed);
            //txtSteering.Text = String.Format("Direction: {0:F6}", _car.Direction);

            _datas = new List<string>
                {
                    String.Format("Steering: {0:F6}", _car.Steering),
                    String.Format("Throttle: {0:F6}", _car.Throttle),
                    String.Format("Brakes: {0:F6}", _car.Brakes),
                    String.Format("Position: {0}", _car.Position),
                    String.Format("Velocity: {0}", _car.Velocity),
                    String.Format("Speed: {0:F6}", _car.Speed),
                    String.Format("Forces: {0}", _car.Forces),
                    String.Format("Mass: {0:F6}", _car.Mass),
                    String.Format("Angle: {0:F6}", _car.Angle),
                    String.Format("AngularVelocity: {0:F6}", _car.AngularVelocity),
                    String.Format("Torque: {0:F6}", _car.Torque),
                    String.Format("Inertia: {0:F6}", _car.Inertia),
                };
            lstDatas.ItemsSource = _datas;

            //Ellipse myEllipse = new Ellipse
            //    {
            //        Fill = new SolidColorBrush(Colors.White),
            //        StrokeThickness = 1,
            //        Stroke = Brushes.White,
            //        Width = 2,
            //        Height = 2
            //    };
            //Canvas.SetTop(myEllipse, posY);
            //Canvas.SetLeft(myEllipse, posX);
            //canvasTest.Children.Add(myEllipse);
        }

        private void ResetCar()
        {
            ExecuteOnUIThread(() =>
                {
                    List<Ellipse> dots = canvasTest.Children.OfType<Ellipse>().ToList();
                    foreach (Ellipse dot in dots)
                        canvasTest.Children.Remove(dot);
                });

            //_car.Initialize(200, 200, 0, 100, 5, 1, 0.25, 2, new GameParameters
            //{
            //    Friction = 0.87
            //});
            //_car.Initialize(2, 2, 0, 0.9, 0.1, 1, 3 * Math.PI / 180.0, 1.5);
            //_car.Initialize(200, 200, 0, 10, 500, 100, 15*Math.PI/180.0, 0.9);
            //_car.Initialize(200,200,0);
            //_car.Initialize(new Vector2D(200, 200), 0);
            _car.Initialize(new Vector2D(3, 8) / 2, 5);
            _car.SetLocation(new Vector2D(100, 100), 0);
            //_car.Initialize(new Vector2DFloat(3, 8) / 2, 5);
            //_car.SetLocation(new Vector2DFloat(0, 0), 0);
        }

        private void GameTask()
        {
            //_car = new Car();
            //_car = new CarSkid();
            //_car = new CarRobot();
            //_car = new Car4Wheels();
            //_car = new CarPhysics();
            //_car = new Car2Wheels();
            _car = new CarWheels();
            //_car = new CarWheelsFloat();
            ResetCar();

            while(true)
            {
                _car.HandleActions(_currentActions);
                //_car.Step(0.05);
                _car.Step(0.05f);

                ExecuteOnUIThread(DisplayCar);

                System.Threading.Thread.Sleep(10);
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
