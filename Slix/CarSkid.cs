using System;

namespace Slix
{
    //http://gamedev.stackexchange.com/questions/1796/vehicle-physics-with-skid
    public sealed class CarSkid
    {
        public double Drag { get; private set; } // How fast the car slows down
        public double AngularDrag { get; private set; } // How fast the car stops spinning
        public double TurnSpeed { get; private set; } // How fast to turn
        public double HandBrakeAdditionalTurnSpeed { get; private set; }
        public double Acceleration { get; private set; } // How fast the car can accelerate

        public double Velocity { get; private set; }
        public double PositionX { get; private set; } // Where the car is
        public double PositionY { get; private set; }
        public double VelocityX { get; private set; } // Speed on each axis
        public double VelocityY { get; private set; }
        public double Angle { get; private set; } // The rotation of the car, in radians
        public double AngularVelocity { get; private set; } // Speed the car is spinning, in radians

        public void Initialize(double positionX, double positionY, double angle, double drag, double angularDrag, double turnSpeed, double power, double handBrakeAdditionalTurnSpeed)
        {
            PositionX = positionX;
            PositionY = positionY;
            Angle = angle;

            Drag = drag;
            AngularDrag = angularDrag;
            TurnSpeed = turnSpeed;
            HandBrakeAdditionalTurnSpeed = handBrakeAdditionalTurnSpeed;
            Acceleration = power;

            Velocity = 0;
            VelocityX = 0;
            VelocityY = 0;
            AngularVelocity = 0;
        }

        public void HandleActions(Actions actions)
        {
            // Accelerate/Brake
            if (actions.Accelerate)
            {
                VelocityX += Math.Cos(Angle) * Acceleration;
                VelocityY += Math.Sin(Angle) * Acceleration;
            }
            if (actions.Brake)
            {
                VelocityX -= Math.Cos(Angle) * Acceleration;
                VelocityY -= Math.Sin(Angle) * Acceleration;
            }

            // Steer
            if (actions.Left)
                AngularVelocity -= TurnSpeed;
            if (actions.Right)
                AngularVelocity += TurnSpeed;

            // Handbrake
            if (actions.HandBrake && !actions.Left && !actions.Right)
            {
                VelocityX -= Math.Cos(Angle) * Acceleration;
                VelocityY -= Math.Sin(Angle) * Acceleration;
            }
            if (actions.HandBrake && actions.Left)
                AngularVelocity -= TurnSpeed * HandBrakeAdditionalTurnSpeed;
            if (actions.HandBrake && actions.Right)
                AngularVelocity += TurnSpeed * HandBrakeAdditionalTurnSpeed;
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
