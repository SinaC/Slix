using System;

namespace Slix
{
    public sealed class Car
    {
        // Will not be vary while running
        public GameParameters GameParameters { get; private set; }
        public double MaxPower { get; private set; }
        public double Handling { get; private set; }
        public double Acceleration { get; private set; }
        public double Braking { get; private set; }
        public double HandBrake { get; private set; }
        
        // Will vary while running
        public double Angle { get; private set; } // in degrees
        public double PositionX { get; private set; }
        public double PositionY { get; private set; }
        public double VelocityX { get; private set; }
        public double VelocityY { get; private set; }
        public double Power { get; private set; }
        public double Steering { get; private set; }

        public void Initialize(double positionX, double positionY, double angle, double maxPower, double handling, double acceleration, double braking, double handBrake, GameParameters gameParameters)
        {
            PositionX = positionX;
            PositionY = positionY;
            Angle = angle;
            MaxPower = maxPower;
            Handling = handling;
            Acceleration = acceleration;
            Braking = braking;
            HandBrake = handBrake;
            GameParameters = gameParameters;

            VelocityX = 0;
            VelocityY = 0;
            Power = 0;
            Steering = 0;
        }

        public void HandleActions(Actions actions)
        {
            // Steer left ?
            if (actions.Left)
                Angle -= Steering;
            // Steer right ?
            if (actions.Right)
                Angle += Steering;
            // Accelerate ?
            if (actions.Accelerate && !actions.Brake)
            {
                if (Power < MaxPower)
                    Power += Acceleration;
                if (Power > MaxPower)
                    Power = MaxPower;
            }
            // Decelerate
            if ((actions.Accelerate && actions.Brake)
                || (!actions.Accelerate && !actions.Brake))
                Power *= GameParameters.Friction;
            // Brake/reverse
            if (actions.Brake && !actions.Accelerate)
            {
                if (Power > -MaxPower)
                    Power -= Braking;
                if (Power < -MaxPower)
                    Power = -MaxPower;
            }
            // Handbrake
            if (actions.HandBrake && !actions.Left && !actions.Right)
            {
                if (Power > 0)
                    Power -= HandBrake;
                // No reverse
                if (Power < 0)
                    Power = 0;
            }
            // Decrease angle if sliding left
            if (actions.HandBrake && actions.Left)
                Angle -= Steering*0.5;
            // Decrease angle if sliding right
            if (actions.HandBrake && actions.Right)
                Angle += Steering*0.5;
        }

        public void Step(double dt) // TODO: use dt
        {
            // compute dx, dy
            double dx = Math.Cos(Angle*Math.PI/180.0);
            double dy = Math.Sin(Angle*Math.PI/180.0);

            // add power to velocity
            VelocityX += dx*Power;
            VelocityY += dy*Power;

            // apply friction with grip
            double grip = Math.Abs(Math.Atan2(PositionY - VelocityY, PositionX - VelocityX)) * 0.01;
            VelocityX *= GameParameters.Friction - grip;
            VelocityY *= GameParameters.Friction - grip;

            //// turn quickier when going faster
            //Steering = Handling*Math.Abs(Power)/MaxPower;
            Steering = Handling;

            // add velocity and compute new position
            PositionX += VelocityX;
            PositionY += VelocityY;

            // TODO: environnement ? sand, oil, ...
            // TODO: check collision and update position according to collision
            // Sample: left wall
            //if (newLocationX < 0)
            //{
            //    hit = true;
            //    if (System.Math.Abs(_cosDriveAngle) >= Tolerance)
            //        newLocationY = newLocationY - newLocationX * _sinDriveAngle / _cosDriveAngle;
            //    newLocationX = 0;
            //    newSpeed = 0;
            //}
        }
    }
}
