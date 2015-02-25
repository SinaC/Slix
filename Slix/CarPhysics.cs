using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slix
{
    //http://gamedev.stackexchange.com/questions/23093/creating-sideways-friction-in-a-2d-top-down-racer
    public class CarPhysics
    {
        public double PositionX { get; private set; }
        public double PositionY { get; private set; }
        public double AngleInRadians { get; private set; }
        public double CarSpeed { get; private set; }
        public double SteerAngle { get; private set; }

        public double ForwardVectorX { get; private set; }
        public double ForwardVectorY { get; private set; }
        public double RightVectorX { get; private set; }
        public double RightVectorY { get; private set; }

        public void Initialize(double positionX, double positionY, double angleInRadians)
        {
            PositionX = positionX;
            PositionY = positionY;
            AngleInRadians = angleInRadians;

            CarSpeed = 0;
            SteerAngle = 0;

            ComputeVectors();
        }

        public void HandleActions(Actions actions)
        {
            if (actions.Accelerate)
                CarSpeed += 10;
            if (actions.Brake)
                CarSpeed -= 10;
            if (actions.Left)
                SteerAngle = -5*Math.PI/180;
            if (actions.Right)
                SteerAngle = 5*Math.PI/180;
        }

        public void Step(double dt)
        {
            double velocityX = CarSpeed*ForwardVectorX;
            double velocityY = CarSpeed*ForwardVectorY;

            double dotVelocityForward = Dot(velocityX, velocityY, ForwardVectorX, ForwardVectorY);
            double forwardVelocityX = ForwardVectorX*dotVelocityForward;
            double forwardVelocityY = ForwardVectorY*dotVelocityForward;
            double dotVelocityRight = Dot(velocityX, velocityY, RightVectorX, RightVectorY);
            double rightVelocityX = RightVectorX*dotVelocityRight;
            double rightVelocityY = RightVectorY*dotVelocityRight;
            velocityX = forwardVelocityX + rightVelocityX*0.1;
            velocityY = forwardVelocityY + rightVelocityY*0.1;

            System.Diagnostics.Debug.WriteLine("ForwardVelocity: {0:F10} {1:F10}", forwardVelocityX, forwardVelocityY);
            System.Diagnostics.Debug.WriteLine("RightVelocity: {0:F10} {1:F10}", rightVelocityX, rightVelocityY);
            System.Diagnostics.Debug.WriteLine("ForwardVector: {0:F10} {1:F10}", ForwardVectorX, ForwardVectorY);
            System.Diagnostics.Debug.WriteLine("RightVector: {0:F10} {1:F10}", RightVectorX, RightVectorY);
            System.Diagnostics.Debug.WriteLine("Velocity: {0:F10} {1:F10}", velocityX, velocityY);

            PositionX += velocityX;
            PositionY += velocityY;

            AngleInRadians += SteerAngle;

            ComputeVectors();
        }

        private static double Dot(double x1, double y1, double x2, double y2)
        {
            return x1*x2 + y1*y2;
        }

        private void ComputeVectors()
        {
            ForwardVectorX = Math.Cos(AngleInRadians);
            ForwardVectorY = Math.Sin(AngleInRadians);
            RightVectorX = Math.Cos(AngleInRadians - Math.PI/2);
            RightVectorY = Math.Sin(AngleInRadians - Math.PI/2);
        }
    }
}
