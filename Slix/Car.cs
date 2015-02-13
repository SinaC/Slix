using System;

namespace Slix
{
    public sealed class Car
    {
        private double _previousLocationX;
        private double _previousLocationY;

        // Will not be vary while running
        public GameParameters GameParameters { get; private set; }
        public double MaxPower { get; private set; }
        public double Handling { get; private set; }
        public double Acceleration { get; private set; }
        public double Braking { get; private set; }
        public double HandBrake { get; private set; }
        
        // Will vary while running
        public double Angle { get; private set; } // in degrees
        public double LocationX { get; private set; }
        public double LocationY { get; private set; }
        public double VelocityX { get; private set; }
        public double VelocityY { get; private set; }
        public double Power { get; private set; }
        public double Steering { get; private set; }

        public void Initialize(double locationX, double locationY, double angle, double maxPower, double handling, double acceleration, double braking, double handBrake, GameParameters gameParameters)
        {
            LocationX = locationX;
            LocationY = locationY;
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
            _previousLocationX = LocationX;
            _previousLocationY = LocationY;

            // compute dx, dy
            double dx = Math.Cos(Angle*Math.PI/180.0);
            double dy = Math.Sin(Angle*Math.PI/180.0);

            // add power to velocity
            VelocityX += dx*Power;
            VelocityY += dy*Power;

            // apply friction with grip
            double grip = Math.Abs(Math.Atan2(LocationY - VelocityY, LocationX - VelocityX)) * 0.01;
            VelocityX *= GameParameters.Friction - grip;
            VelocityY *= GameParameters.Friction - grip;

            //// turn quickier when going faster
            //Steering = Handling*Math.Abs(Power)/MaxPower;
            Steering = Handling;

            // add velocity and compute new position
            LocationX += VelocityX;
            LocationY += VelocityY;

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

        // Extract from JRobots, compute new location from previous location, desired speed, current speed and angle
        /*
        double delta = dt * ParametersSingleton.MaxAcceleration; // speed increase due to acceleration v = a.t
            double travelledDistanceDueToAcceleration = 0.5 * delta * dt; // distance increase due to acceleration d = 0.5.a.t.t

            double travelledDistance = dt * _currentSpeed; // relocates due to its speed d = v.t
            double speedDiff = _desiredSpeed - _currentSpeed; // difference between desired and current speed
            if (System.Math.Abs(speedDiff) < Tolerance) // desired speed reached
            {
                newLocationX = _locX + travelledDistance * _cosDriveAngle; // increase along x axis
                newLocationY = _locY + travelledDistance * _sinDriveAngle; // increase along y axis
                newSpeed = _currentSpeed;
            }
            else
            {
                if (speedDiff > 0) // acceleration
                {
                    double nextSpeed = _currentSpeed + delta; // next speed
                    if (nextSpeed > _desiredSpeed) // overstep
                    {
                        double t1 = speedDiff / ParametersSingleton.MaxAcceleration; // time when we would overstep
                        double t2 = dt - t1; // remaining time to finish the step
                        double compositeTravelledDistance = _currentSpeed * t1 + 0.5 * ParametersSingleton.MaxAcceleration * t1 * t1 + _desiredSpeed * t2; // travelled distance using current speed and desired speed
                        newLocationX = _locX + compositeTravelledDistance * _cosDriveAngle; // relocation along x axis
                        newLocationY = _locY + compositeTravelledDistance * _sinDriveAngle; // relocation along y axis
                        newSpeed = _desiredSpeed; // no speed overstepping
                    }
                    else // no overstep
                    {
                        newLocationX = _locX + (travelledDistance + travelledDistanceDueToAcceleration) * _cosDriveAngle; // relocation along x axis
                        newLocationY = _locY + (travelledDistance + travelledDistanceDueToAcceleration) * _sinDriveAngle; // relocation along y axis
                        newSpeed = nextSpeed; // speed at the end of time step
                    }
                }
                else // deacceleration
                {
                    double nextSpeed = _currentSpeed - delta; // next speed
                    if (nextSpeed < _desiredSpeed) // overstep
                    {
                        double t1 = -speedDiff / ParametersSingleton.MaxAcceleration; // time when we would overstep (sign change is faster than abs)
                        double t2 = dt - t1; // remaining time to finish the step
                        double compositeTravelledDistance = _currentSpeed * t1 - 0.5 * ParametersSingleton.MaxAcceleration * t1 * t1 + _desiredSpeed * t2; // travelled distance using current speed and desired speed
                        newLocationX = _locX + compositeTravelledDistance * _cosDriveAngle; // relocation along x axis
                        newLocationY = _locY + compositeTravelledDistance * _sinDriveAngle; // relocation along y axis
                        newSpeed = _desiredSpeed; // no speed overstepping
                    }
                    else
                    {
                        newLocationX = _locX + (travelledDistance - travelledDistanceDueToAcceleration) * _cosDriveAngle; // relocation along x axis
                        newLocationY = _locY + (travelledDistance - travelledDistanceDueToAcceleration) * _sinDriveAngle; // relocation along y axis
                        newSpeed = nextSpeed; // speed at the end of time step
                    }
                }
            }

            // Check collision with wall
            bool hit = false;
            if (newLocationX < 0)
            {
                hit = true;
                if (System.Math.Abs(_cosDriveAngle) >= Tolerance)
                    newLocationY = newLocationY - newLocationX * _sinDriveAngle / _cosDriveAngle;
                newLocationX = 0;
                newSpeed = 0;
            }
            else if (newLocationX > ParametersSingleton.ArenaSize)
            {
                hit = true;
                if (System.Math.Abs(_cosDriveAngle) >= Tolerance)
                    newLocationY = newLocationY + (ParametersSingleton.ArenaSize - newLocationX) * _sinDriveAngle / _cosDriveAngle;
                newLocationX = ParametersSingleton.ArenaSize;
                newSpeed = 0;
            }
            if (newLocationY < 0)
            {
                hit = true;
                if (System.Math.Abs(_sinDriveAngle) >= Tolerance)
                    newLocationX = newLocationX - newLocationY * _cosDriveAngle / _sinDriveAngle;
                newLocationY = 0;
                newSpeed = 0;
            }
            else if (newLocationY > ParametersSingleton.ArenaSize)
            {
                hit = true;
                if (System.Math.Abs(_sinDriveAngle) >= Tolerance)
                    newLocationX = newLocationX + (ParametersSingleton.ArenaSize - newLocationY) * _cosDriveAngle / _sinDriveAngle;
                newLocationY = ParametersSingleton.ArenaSize;
                newSpeed = 0;
            }

            return hit;
    */
    }
}
