using System;
using Slix;

namespace Console_POC
{
    class Program
    {
        static void DisplayLine(int x, int y, string format, params object[] parameters)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(format, parameters);
        }

        static void DisplayCar(Car car)
        {
            DisplayLine(0, 0, "MaxPower: {0:F4}", car.MaxPower);
            DisplayLine(0, 1, "Handling: {0:F4}", car.Handling);
            DisplayLine(0, 2, "Acceleration: {0:F4}", car.Acceleration);
            DisplayLine(0, 3, "Braking: {0:F4}", car.Braking);
            DisplayLine(0, 4, "HandBrake: {0:F4}", car.HandBrake);
            DisplayLine(0, 5, "Friction: {0:F4}", car.GameParameters.Friction);

            DisplayLine(0, 7, "Angle: {0:F8}", car.Angle);
            DisplayLine(0, 8, "Location: {0:F8}, {1:F8}", car.LocationX, car.LocationY);
            DisplayLine(0, 9, "Velocity: {0:F8}, {1:F8}", car.VelocityX, car.VelocityY);
            DisplayLine(0, 10, "Power: {0:F8}", car.Power);
            DisplayLine(0, 11, "Steering: {0:F8}", car.Steering);
        }

        static void DisplayActions(Actions action)
        {
            DisplayLine(0, 6, "{0}{1}{2}{3}{4}",
                    action.Left ? 'L' : ' ',
                    action.Right ? 'R' : ' ',
                    action.Accelerate ? 'A' : ' ',
                    action.Brake ? 'B' : ' ',
                    action.HandBrake ? 'H' : ' ');
        }

        static bool GetActions(ref Actions actions)
        {
            bool isLeftPressed = NativeKeyboard.IsKeyDown(KeyCode.Left);
            bool isRightPressed = NativeKeyboard.IsKeyDown(KeyCode.Right);
            bool isUpPressed = NativeKeyboard.IsKeyDown(KeyCode.Up);
            bool isDownPressed = NativeKeyboard.IsKeyDown(KeyCode.Down);
            bool isDeletePressed = NativeKeyboard.IsKeyDown(KeyCode.Delete);

            if (isLeftPressed || isRightPressed || isUpPressed || isDownPressed || isDeletePressed)
            {
                if (isLeftPressed)
                    actions.Left = true;
                if (isRightPressed)
                    actions.Right = true;
                if (isUpPressed)
                    actions.Accelerate = true;
                if (isDownPressed)
                    actions.Brake = true;
                if (isDeletePressed)
                    actions.HandBrake = true;
                return true;
            }
            return false;
        }

        static void Main(string[] args)
        {
            Console.SetWindowSize(80, 30);
            Console.BufferWidth = 80;
            Console.BufferHeight = 30;

            Actions actions = new Actions();
            Car car = new Car();

            car.Initialize(0, 0, 0, 5, 10, 1, 0.25, 2, new GameParameters
            {
                Friction = 0.87
            });

            bool stopped = false;
            while (!stopped)
            {
                if (NativeKeyboard.IsKeyDown(KeyCode.X))
                    stopped = true;
                else
                {
                    bool hasActions = GetActions(ref actions);

                    DisplayActions(actions);

                    car.HandleActions(actions);
                    actions.Reset();
                    car.Step(0);
                    DisplayCar(car);
                    System.Threading.Thread.Sleep(50);
                }
            }
        }
    }
}
