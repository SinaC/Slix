using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slix
{
    //http://gamedev.stackexchange.com/questions/55857/car-movement-in-2d-dimension
    public class Car2Wheels
    {
        public Vector2D Position { get; private set; }
        public Vector2D FrontTire { get; private set; }
        public Vector2D BackTire { get; private set; }
        public double Direction { get; private set; }
        public double Speed { get; private set; }
        public double Angle { get; private set; }
        public double WheelBase { get; private set; }

        public void Initialize(double positionX, double positionY, double angle)
        {
            Position = new Vector2D(positionX, positionY);
            Angle = angle;

            WheelBase = 32;
            Direction = angle;
            Speed = 0;
        }

        public void HandleActions(Actions actions)
        {
            if (actions.Accelerate)
                Speed += 10;
            else if (actions.Brake)
                Speed -= 100;
            else
                Speed = 0;
            if (Speed < 0)
                Speed = 0;
            if (Speed > 300)
                Speed = 300;

            if (actions.Left)
                Angle -= 0.03;
            else if (actions.Right)
                Angle += 0.03;
            else
                Angle = 0;
        }

        public void Step(double dt)
        {
            if (Math.Abs(Angle - 0) > 0.00001)
            {
                // break me
            }

            FrontTire = Position + WheelBase / 2 * new Vector2D(Math.Cos(Direction), Math.Sin(Direction));
            BackTire = Position - WheelBase / 2 * new Vector2D(Math.Cos(Direction), Math.Sin(Direction));

            BackTire += Speed * dt * new Vector2D(Math.Cos(Direction), Math.Sin(Direction));
            FrontTire += Speed * dt * new Vector2D(Math.Cos(Direction + Angle), Math.Sin(Direction + Angle));

            Position = (FrontTire + BackTire) / 2;
            Direction = Math.Atan2(FrontTire.Y - BackTire.Y, FrontTire.X - BackTire.X);
        }
    }
}
