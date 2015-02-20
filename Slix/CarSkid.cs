using System;

namespace Slix
{
    //http://gamedev.stackexchange.com/questions/1796/vehicle-physics-with-skid
    public sealed class CarSkid
    {
        public double Drag { get; private set; } // How fast the car slows down
        public double AngularDrag { get; private set; } // How fast the car stops spinning
        public double TurnSpeed { get; private set; } // How fast to turn
        public double HandBrakeMultiplicativeTurnSpeed { get; private set; }
        public double Power { get; private set; } //

        public double Velocity { get; private set; }
        public double PositionX { get; private set; } // Where the car is
        public double PositionY { get; private set; }
        public double VelocityX { get; private set; } // Speed on each axis
        public double VelocityY { get; private set; }
        public double Angle { get; private set; } // The rotation of the car, in radians
        public double AngularVelocity { get; private set; } // Speed the car is spinning, in radians

        public void Initialize(double positionX, double positionY, double angle, double drag, double angularDrag, double power, double turnSpeed, double handBrakeMultiplicativeTurnSpeed)
        {
            PositionX = positionX;
            PositionY = positionY;
            Angle = angle;

            Drag = drag;
            AngularDrag = angularDrag;
            TurnSpeed = turnSpeed;
            HandBrakeMultiplicativeTurnSpeed = handBrakeMultiplicativeTurnSpeed;
            Power = power;

            Velocity = 0;
            VelocityX = 0;
            VelocityY = 0;
            AngularVelocity = 0;

            // when not turning, max velocity may be computed the following way
            //  V(n+1) = (Vn + Power) * Drag
            //  Vn = SUM(1, INF, Power * Drag^n)
            //     = Power * SUM(1, INF, Drag^n)
            //     = Power * (SUM(0, INF, Drag^n) - 1) with Drag != 0
            //  SUM(0, INF, r^n) converges to 1/(1-r) if |r| < 1       http://en.wikipedia.org/wiki/Geometric_series#Formula
            //     = Power * (1/(1-Drag) - 1)
            //     = (Power*Drag)/(1-Drag)
        }

        public void HandleActions(Actions actions)
        {
            // Accelerate/Brake
            if (actions.Accelerate)
            {
                VelocityX += Math.Cos(Angle) * Power;
                VelocityY += Math.Sin(Angle) * Power;
            }
            if (actions.Brake)
            {
                VelocityX -= Math.Cos(Angle) * Power;
                VelocityY -= Math.Sin(Angle) * Power;
            }

            // Steer (+ handbrake)
            if (actions.Left)
                if (actions.HandBrake)
                    AngularVelocity -= TurnSpeed*HandBrakeMultiplicativeTurnSpeed;
                else
                    AngularVelocity -= TurnSpeed;
            if (actions.Right)
                if (actions.HandBrake)
                    AngularVelocity += TurnSpeed*HandBrakeMultiplicativeTurnSpeed;
                else
                    AngularVelocity += TurnSpeed;

            // Handbrake
            if (actions.HandBrake && !actions.Left && !actions.Right)
            {
                VelocityX -= Math.Cos(Angle) * Power;
                VelocityY -= Math.Sin(Angle) * Power;
            }
        }

        public void Step(double dt)
        {
            PositionX += VelocityX;
            PositionY += VelocityY;
            VelocityX *= Drag;
            VelocityY *= Drag;

            Angle += AngularVelocity;
            AngularVelocity *= AngularDrag;
        }
    }
}
