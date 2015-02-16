using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slix
{
    public class CarRobot
    {
        private const double Tolerance = 0.0001;

        public double MaxAcceleration { get; private set; }
        public double MaxForwardSpeed { get; private set; }
        public double MaxBackwardSpeed { get; private set; }
        public double Handling { get; private set; }
        public double Drag { get; private set; }

        public double LocationX { get; private set; }
        public double LocationY { get; private set; }
        public double CurrentSpeed { get; private set; }
        public double DesiredSpeed { get; private set; }
        public double Angle { get; private set; }

        public void Initialize(double positionX, double positionY, double angle, double maxAcceleration, double maxForwardSpeed, double maxBackwardSpeed, double handling, double drag)
        {
            LocationX = positionX;
            LocationY = positionY;
            Angle = angle;

            MaxAcceleration = maxAcceleration;
            MaxForwardSpeed = maxForwardSpeed;
            MaxBackwardSpeed = maxBackwardSpeed;
            Handling = handling;

            CurrentSpeed = 0;
            DesiredSpeed = 0;
        }

        public void HandleActions(Actions actions)
        {
            if (actions.Accelerate)
                DesiredSpeed = MaxForwardSpeed;
            if (actions.Brake)
                DesiredSpeed = -MaxBackwardSpeed;
            if (actions.Left)
                Angle -= Handling;
            if (actions.Right)
                Angle += Handling;
            //if (!actions.Accelerate && !actions.Brake)
            //    DesiredSpeed *= Drag;
        }

        public void Step(double dt)
        {
            double newLocationX;
            double newLocationY;
            double newSpeed;

            double delta = dt * MaxAcceleration; // speed increase due to acceleration v = a.t
            double travelledDistanceDueToAcceleration = 0.5 * delta * dt; // distance increase due to acceleration d = 0.5.a.t.t
            double travelledDistance = dt * CurrentSpeed; // relocates due to its speed d = v.t

            double speedDiff = DesiredSpeed - CurrentSpeed; // difference between desired and current speed
            if (Math.Abs(speedDiff) < Tolerance) // desired speed reached
            {
                newLocationX = LocationX + travelledDistance * Math.Cos(Angle); // increase along x axis
                newLocationY = LocationY + travelledDistance * Math.Sin(Angle); // increase along y axis
                newSpeed = CurrentSpeed;
            }
            else
            {
                if (speedDiff > 0) // acceleration
                {
                    double nextSpeed = CurrentSpeed + delta; // next speed
                    if (nextSpeed > DesiredSpeed) // overstep
                    {
                        double t1 = speedDiff / MaxAcceleration; // time when we would overstep
                        double t2 = dt - t1; // remaining time to finish the step
                        double compositeTravelledDistance = CurrentSpeed * t1 + 0.5 * MaxAcceleration * t1 * t1 + DesiredSpeed * t2; // travelled distance using current speed and desired speed
                        newLocationX = LocationX + compositeTravelledDistance * Math.Cos(Angle); // relocation along x axis
                        newLocationY = LocationY + compositeTravelledDistance * Math.Sin(Angle); // relocation along y axis
                        newSpeed = DesiredSpeed; // no speed overstepping
                    }
                    else // no overstep
                    {
                        newLocationX = LocationX + (travelledDistance + travelledDistanceDueToAcceleration) * Math.Cos(Angle); // relocation along x axis
                        newLocationY = LocationY + (travelledDistance + travelledDistanceDueToAcceleration) * Math.Sin(Angle); // relocation along y axis
                        newSpeed = nextSpeed; // speed at the end of time step
                    }
                }
                else // deacceleration
                {
                    double nextSpeed = CurrentSpeed - delta; // next speed
                    if (nextSpeed < DesiredSpeed) // overstep
                    {
                        double t1 = -speedDiff / MaxAcceleration; // time when we would overstep (sign change is faster than abs)
                        double t2 = dt - t1; // remaining time to finish the step
                        double compositeTravelledDistance = CurrentSpeed * t1 - 0.5 * MaxAcceleration * t1 * t1 + DesiredSpeed * t2; // travelled distance using current speed and desired speed
                        newLocationX = LocationX + compositeTravelledDistance * Math.Cos(Angle); // relocation along x axis
                        newLocationY = LocationY + compositeTravelledDistance * Math.Sin(Angle); // relocation along y axis
                        newSpeed = DesiredSpeed; // no speed overstepping
                    }
                    else
                    {
                        newLocationX = LocationX + (travelledDistance - travelledDistanceDueToAcceleration) * Math.Cos(Angle); // relocation along x axis
                        newLocationY = LocationY + (travelledDistance - travelledDistanceDueToAcceleration) * Math.Sin(Angle); // relocation along y axis
                        newSpeed = nextSpeed; // speed at the end of time step
                    }
                }
            }

            LocationX = newLocationX;
            LocationY = newLocationY;
            CurrentSpeed = newSpeed;
        }
    }


    // Extract from JRobots, compute new location from previous location, desired speed, current speed and angle
    /*
    double delta = dt * MaxAcceleration; // speed increase due to acceleration v = a.t
        double travelledDistanceDueToAcceleration = 0.5 * delta * dt; // distance increase due to acceleration d = 0.5.a.t.t

        double travelledDistance = dt * CurrentSpeed; // relocates due to its speed d = v.t
        double speedDiff = DesiredSpeed - CurrentSpeed; // difference between desired and current speed
        if (System.Math.Abs(speedDiff) < Tolerance) // desired speed reached
        {
            newLocationX = LocationX + travelledDistance * Math.Cos(Angle); // increase along x axis
            newLocationY = LocationY + travelledDistance * Math.Sin(Angle); // increase along y axis
            newSpeed = CurrentSpeed;
        }
        else
        {
            if (speedDiff > 0) // acceleration
            {
                double nextSpeed = CurrentSpeed + delta; // next speed
                if (nextSpeed > DesiredSpeed) // overstep
                {
                    double t1 = speedDiff / MaxAcceleration; // time when we would overstep
                    double t2 = dt - t1; // remaining time to finish the step
                    double compositeTravelledDistance = CurrentSpeed * t1 + 0.5 * MaxAcceleration * t1 * t1 + DesiredSpeed * t2; // travelled distance using current speed and desired speed
                    newLocationX = LocationX + compositeTravelledDistance * Math.Cos(Angle); // relocation along x axis
                    newLocationY = LocationY + compositeTravelledDistance * Math.Sin(Angle); // relocation along y axis
                    newSpeed = DesiredSpeed; // no speed overstepping
                }
                else // no overstep
                {
                    newLocationX = LocationX + (travelledDistance + travelledDistanceDueToAcceleration) * Math.Cos(Angle); // relocation along x axis
                    newLocationY = LocationY + (travelledDistance + travelledDistanceDueToAcceleration) * Math.Sin(Angle); // relocation along y axis
                    newSpeed = nextSpeed; // speed at the end of time step
                }
            }
            else // deacceleration
            {
                double nextSpeed = CurrentSpeed - delta; // next speed
                if (nextSpeed < DesiredSpeed) // overstep
                {
                    double t1 = -speedDiff / MaxAcceleration; // time when we would overstep (sign change is faster than abs)
                    double t2 = dt - t1; // remaining time to finish the step
                    double compositeTravelledDistance = CurrentSpeed * t1 - 0.5 * MaxAcceleration * t1 * t1 + DesiredSpeed * t2; // travelled distance using current speed and desired speed
                    newLocationX = LocationX + compositeTravelledDistance * Math.Cos(Angle); // relocation along x axis
                    newLocationY = LocationY + compositeTravelledDistance * Math.Sin(Angle); // relocation along y axis
                    newSpeed = DesiredSpeed; // no speed overstepping
                }
                else
                {
                    newLocationX = LocationX + (travelledDistance - travelledDistanceDueToAcceleration) * Math.Cos(Angle); // relocation along x axis
                    newLocationY = LocationY + (travelledDistance - travelledDistanceDueToAcceleration) * Math.Sin(Angle); // relocation along y axis
                    newSpeed = nextSpeed; // speed at the end of time step
                }
            }
        }

        // Check collision with wall
        bool hit = false;
        if (newLocationX < 0)
        {
            hit = true;
            if (System.Math.Abs(Math.Cos(Angle)) >= Tolerance)
                newLocationY = newLocationY - newLocationX * Math.Sin(Angle) / Math.Cos(Angle);
            newLocationX = 0;
            newSpeed = 0;
        }
        else if (newLocationX > ArenaSize)
        {
            hit = true;
            if (System.Math.Abs(Math.Cos(Angle)) >= Tolerance)
                newLocationY = newLocationY + (ArenaSize - newLocationX) * Math.Sin(Angle) / Math.Cos(Angle);
            newLocationX = ArenaSize;
            newSpeed = 0;
        }
        if (newLocationY < 0)
        {
            hit = true;
            if (System.Math.Abs(Math.Sin(Angle)) >= Tolerance)
                newLocationX = newLocationX - newLocationY * Math.Cos(Angle) / Math.Sin(Angle);
            newLocationY = 0;
            newSpeed = 0;
        }
        else if (newLocationY > ArenaSize)
        {
            hit = true;
            if (System.Math.Abs(Math.Sin(Angle)) >= Tolerance)
                newLocationX = newLocationX + (ArenaSize - newLocationY) * Math.Cos(Angle) / Math.Sin(Angle);
            newLocationY = ArenaSize;
            newSpeed = 0;
        }

        return hit;
*/
}
