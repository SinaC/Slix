using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slix
{
    //http://engineeringdotnet.blogspot.be/2010/04/simple-2d-car-physics-in-games.html
    public class Car4Wheels
    {
        public double WheelBase { get; private set; }
        
        public double CarLocationX { get; private set; }
        public double CarLocationY { get; private set; }
        public double FrontWheelX { get; private set; }
        public double FrontWheelY { get; private set; }
        public double BackWheelX { get; private set; }
        public double BackWheelY { get; private set; }
        public double CarHeading { get; private set; }
        public double CarSpeed { get; private set; }
        public double SteerAngle { get; private set; }

        public void Initialize(double locationX, double locationY, double angle)
        {
            CarLocationX = locationX;
            CarLocationY = locationY;
            CarHeading = angle;

            WheelBase = 10;

            FrontWheelX = CarLocationX + WheelBase / 2 * Math.Cos(CarHeading);
            FrontWheelY = CarLocationY + WheelBase / 2 * Math.Sin(CarHeading);

            BackWheelX = CarLocationX - WheelBase / 2 * Math.Cos(CarHeading);
            BackWheelY = CarLocationY - WheelBase / 2 * Math.Sin(CarHeading);

            CarSpeed = 0;
            SteerAngle = 0;
        }

        public void HandleActions(Actions actions)
        {
            if (actions.Accelerate)
                CarSpeed += 10;
            if (actions.Brake)
                CarSpeed -= 10;
            if (actions.Left)
                SteerAngle -= 1*Math.PI/180;
            if (actions.Right)
                SteerAngle += 1 * Math.PI / 180;
        }

        public void Step(double dt)
        {
            FrontWheelX = CarLocationX + WheelBase / 2 * Math.Cos(CarHeading);
            FrontWheelY = CarLocationY + WheelBase / 2 * Math.Sin(CarHeading);

            BackWheelX = CarLocationX - WheelBase / 2 * Math.Cos(CarHeading);
            BackWheelY = CarLocationY - WheelBase / 2 * Math.Sin(CarHeading);

            BackWheelX += CarSpeed * dt * Math.Cos(CarHeading);
            BackWheelY += CarSpeed * dt * Math.Sin(CarHeading);

            FrontWheelX += CarSpeed * dt * Math.Cos(CarHeading + SteerAngle);
            FrontWheelY += CarSpeed * dt * Math.Sin(CarHeading + SteerAngle);

            CarLocationX = (FrontWheelX + BackWheelX) / 2;
            CarLocationY = (FrontWheelY + BackWheelY) / 2;

            CarHeading = Math.Atan2(FrontWheelY - BackWheelY, FrontWheelX - BackWheelX);

            SteerAngle *= 0.9;
        }
    }
}
