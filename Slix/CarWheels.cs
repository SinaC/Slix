using System;

namespace Slix
{
    //http://www.gamedev.net/page/resources/_/technical/math-and-physics/2d-car-physics-r2443
    public class CarWheels : RigidBody
    {
        public double Steering { get; private set; }
        public double Throttle { get; private set; }
        public double Brakes { get; private set; }

        private class Wheel
        {
            private static readonly Vector2D ForwardVector = new Vector2D(0, 1);
            private static readonly Vector2D SideVector = new Vector2D(-1, 0);

            public Vector2D Position { get; private set; }
            public Vector2D ForwardAxis { get; private set; }
            public Vector2D SideAxis { get; private set; }

            public double WheelTorque { get; private set; }
            public double WheelSpeed { get; private set; }

            public readonly double WheelInertia;
            public readonly double WheelRadius;

            public Wheel(Vector2D position, double radius)
            {
                Position = position;
                WheelRadius = radius;

                WheelSpeed = 0;
                WheelInertia = WheelRadius*WheelRadius; // fake value
                SetSteeringAngle(0);
            }

            public void SetSteeringAngle(double newAngle)
            {
                ForwardAxis = Vector2D.Rotate(ForwardVector, newAngle);
                SideAxis = Vector2D.Rotate(SideVector, newAngle);
            }

            public void AddTransmissionTorque(double newValue)
            {
                WheelTorque += newValue;
            }

            public Vector2D CalculateForce(Vector2D relativeGroundSpeed, double dt)
            {
                //calculate speed of tire patch at ground
                Vector2D patchSpeed = ForwardAxis*(WheelSpeed*WheelRadius);

                //get velocity difference between ground and patch
                Vector2D velocityDifference = relativeGroundSpeed - patchSpeed;

                //project ground speed onto side and forward axis
                double forwardMag;
                Vector2D sideVelocity = velocityDifference.Project(SideAxis);
                Vector2D forwardVelocity = velocityDifference.Project(ForwardAxis, out forwardMag);

                //calculate super fake friction forces
                //calculate response force
                Vector2D responseForce = -(sideVelocity*2 + forwardVelocity);

                //calculate torque on wheel
                WheelTorque += forwardMag*WheelRadius;

                //System.Diagnostics.Debug.WriteLine("Forward:{0}", forwardVelocity);
                //System.Diagnostics.Debug.WriteLine("Side:{0}", sideVelocity);
                //System.Diagnostics.Debug.WriteLine("Torque:{0}", WheelTorque);

                //integrate total torque into wheel
                WheelSpeed += WheelTorque/WheelInertia*dt;

                //clear our transmission torque accumulator
                WheelTorque = 0;

                //return force acting on body
                return responseForce;
            }
        }

        private readonly Wheel[] _wheels = new Wheel[4];

        public void HandleActions(Actions actions)
        {
            //if (actions.Accelerate)
            //{
            //    if (actions.HandBrake)
            //        SetThrottle(+0.75);
            //    else
            //        SetThrottle(+1);
            //}
            //else
            //    SetThrottle(0);
            //if (actions.Brake)
            //    SetBrakes(+1);
            //else
            //    SetBrakes(0);
            //if (actions.Left)
            //{
            //    if (actions.HandBrake)
            //        SetSteering(-1.25);
            //    else
            //        SetSteering(-1);
            //}
            //else if (actions.Right)
            //{
            //    if (actions.HandBrake)
            //        SetSteering(+1.25);
            //    else
            //        SetSteering(+1);
            //}
            //else
            //    SetSteering(0);

            //if (actions.Accelerate)
            //    SetThrottle(+1);
            //else
            //    SetThrottle(0);
            //if (actions.Brake)
            //    SetBrakes(+1);
            //else
            //    SetBrakes(0);
            //if (actions.Left)
            //    SetSteering(-1);
            //else if (actions.Right)
            //    SetSteering(+1);
            //else
            //    SetSteering(0);

            double throttle = 0;
            double brakes = 0;
            double steering = 0;

            if (actions.Brake)
                brakes = +1;
            else if (actions.Accelerate)
                throttle = +1;
            if (actions.Left)
                steering = -1;
            else if (actions.Right)
                steering = +1;

            SetSteering(steering);
            SetThrottle(throttle);
            SetBrakes(brakes);

            //SetThrottle(+1);
            //SetBrakes(+1);
            //SetSteering(-0.5);
        }

        public override void Initialize(Vector2D halfSize, double mass)
        {
            //front wheels
            _wheels[0] = new Wheel(new Vector2D(halfSize.X, halfSize.Y), 0.5f);
            _wheels[1] = new Wheel(new Vector2D(-halfSize.X, halfSize.Y), 0.5f);

            //rear wheels
            _wheels[2] = new Wheel(new Vector2D(halfSize.X, -halfSize.Y), 0.5f);
            _wheels[3] = new Wheel(new Vector2D(-halfSize.X, -halfSize.Y), 0.5f);

            base.Initialize(halfSize, mass);
        }

        public override void Step(double dt)
        {
            const double drag = 0.4257;
            const double rollingResistance = 12.8;
            
            System.Diagnostics.Debug.WriteLine("STEP:{0:F10}", dt);

            foreach (Wheel wheel in _wheels)
            {
                Vector2D worldWheelOffset = RelativeToWorld(wheel.Position);
                Vector2D worldGroundVel = PointVelocity(worldWheelOffset);
                Vector2D relativeGroundSpeed = WorldToRelative(worldGroundVel);
                Vector2D relativeResponseForce = wheel.CalculateForce(relativeGroundSpeed, dt);
                Vector2D worldResponseForce = RelativeToWorld(relativeResponseForce);

                //System.Diagnostics.Debug.WriteLine("WHEEL:{0}", Array.IndexOf(_wheels, wheel));
                //System.Diagnostics.Debug.WriteLine("worldWheelOffset: {0}", worldWheelOffset);
                //System.Diagnostics.Debug.WriteLine("worldGroundVel: {0}", worldGroundVel);
                //System.Diagnostics.Debug.WriteLine("relativeGroundSpeed: {0}", relativeGroundSpeed);
                //System.Diagnostics.Debug.WriteLine("relativeResponseForce: {0}", relativeResponseForce);
                //System.Diagnostics.Debug.WriteLine("worldResponseForce: {0}", worldResponseForce);

                AddForce(worldResponseForce, worldWheelOffset);
            }

            // Friction
            Vector2D fDrag = -drag * Velocity.Length * Velocity;
            Vector2D fRollingResistance = -rollingResistance * Velocity;
            AddForce(fDrag, Velocity);
            AddForce(fRollingResistance, Velocity);

            base.Step(dt);

            System.Diagnostics.Debug.WriteLine("Position:{0}", Position);
            System.Diagnostics.Debug.WriteLine("Angle:{0}", Angle);
            System.Diagnostics.Debug.WriteLine("Speed:{0:F6}", Speed);
        }

        private void SetSteering(double steering)
        {
            //const double steeringLock = Math.PI/4;
            const double steeringLock = Math.PI / 24;

            Steering = steering;

            //apply steering angle to front wheels
            _wheels[0].SetSteeringAngle(steering*steeringLock);
            _wheels[1].SetSteeringAngle(steering*steeringLock);
        }

        private void SetThrottle(double throttle, bool allWheel = false)
        {
            const double torque = 500.0;

            Throttle = throttle;

            //apply transmission torque to back wheels
            _wheels[2].AddTransmissionTorque(throttle*torque);
            _wheels[3].AddTransmissionTorque(throttle*torque);

            // and optionally to front wheels
            if (allWheel)
            {
                _wheels[0].AddTransmissionTorque(throttle*torque);
                _wheels[1].AddTransmissionTorque(throttle*torque);
            }
        }

        private void SetBrakes(double brakes)
        {
            const double brakeTorque = 0.25;

            Brakes = brakes;

            //apply brake torque opposing wheel velocity
            foreach (Wheel wheel in _wheels)
            {
                double wheelVelocity = wheel.WheelSpeed;
                wheel.AddTransmissionTorque(-wheelVelocity*brakeTorque*brakes);
            }
        }
    }
}
