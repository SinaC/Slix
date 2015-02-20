using System;

namespace Slix
{
    //http://www.asawicki.info/Mirror/Car%20Physics%20for%20Games/Car%20Physics%20for%20Games.html
    //http://ploobs.com.br/?p=2278
    public class CarPhysics2
    {
        public double Mass { get; private set; }
        public double Drag { get; private set; } // air-resistance aka aerodynamic drag //  0.4257
        public double RollingResistance { get; private set; } // rolling-resistance (should 30 times drag) // 12.8
        public double Handling { get; private set; }
        public double WheelBase { get; private set; }
        public double MaxGrip { get; private set; } // maximum (normalised) friction force, =diameter of friction circle
        public double CARear { get; private set; } // rear cornering stiffness
        public double CAFront { get; private set; } // front cornering stiffness
        public bool FrontSlip { get; private set; }
        public bool RearSlip { get; private set; }
        public double Inertia { get; private set; } // kg*m

        public double EngineForce { get; private set; }
        public double Angle { get; private set; }
        public double Steering { get; private set; }
        public Vector2D NormalizedDirection { get; private set; }
        public Vector2D Velocity { get; private set; }
        public Vector2D Force { get; private set; }
        public Vector2D Position { get; private set; }
        public Vector2D FTraction { get; private set; }
        public Vector2D FDrag { get; private set; }
        public Vector2D FRollingResistance { get; private set; }
        public Vector2D FLongitudinal { get; private set; }
        public double TurnCircleRadius  { get; private set; }
        public double AngularVelocity { get; private set; }
        public double Speed
        {
            get { return Velocity.Length; }
        }

        public void Initialize(double x, double y, double angle)
        {
            Position = new Vector2D(x, y);
            Angle = angle;
            ComputeNormalizedDirection();
            Velocity = new Vector2D();
            EngineForce = 0;
            Steering = 0;
            TurnCircleRadius = 0;
            AngularVelocity = 0;

            Mass = 500; // Kg
            Drag = 0.4257;
            RollingResistance = 12.8;
            WheelBase = 4; // Meters
            Handling = 10*Math.PI/180; // Radians
            MaxGrip = 2.0;
            CARear = -5.2;
            CAFront = -5.0;
            FrontSlip = true;
            RearSlip = false;
            Inertia = 500;
        }

        public void HandleActions(Actions actions)
        {
            if (actions.Accelerate)
                EngineForce = 1000;
            else if (actions.Brake)
                EngineForce = -500;
            else
                EngineForce = 0;

            if (actions.Left)
                Steering = -Handling;
            else if (actions.Right)
                Steering = Handling;
            else
                Steering = 0;
        }

        public void Step2(double dt)
        {
            // convention X: front Y: right

            // Car velocity in car reference
            // Transform velocity in world reference frame to velocity in car reference frame
            // 2D rotation of the velocity vector by car orientation ...
            Vector2D velocity = Vector2D.Rotate(Velocity, Angle);

            // Lateral force on wheels
            // Resulting velocity of the wheels as result of the yaw rate of the car body
            // v = yawrate * r where r is distance of wheel to CG (approx. half wheel base)
            // yawrate (ang.velocity) must be in rad/s
            double yawSpeed = WheelBase * 0.5 * AngularVelocity;

            double rotAngle;
            if (Math.Abs(velocity.X) < 0.000001) // TODO: fix singularity
                rotAngle = 0;
            else
                rotAngle = Math.Atan2(yawSpeed, velocity.X);

            // Calculate the side slip angle of the car (a.k.a. beta)
            double slideSlip;
            if (Math.Abs(velocity.X) < 0.000001) // TODO: fix singularity
                slideSlip = 0;
            else
                slideSlip = Math.Atan2(velocity.Y, velocity.X);

            // Calculate slip angles for front and rear wheels (a.k.a. alpha)
            double slipAngleFront = slideSlip + rotAngle - Steering;
            double slipAngleRear = slideSlip - rotAngle;

            // Weight per axle = half car mass times 1G (=9.8m/s^2) 
            double weight = Mass * 9.8f * 0.5f;

            // lateral force on front wheels = (Ca * slip angle) capped to friction circle * load
            double fLatFrontY = (CAFront * slipAngleFront);
            fLatFrontY = Math.Min(MaxGrip, fLatFrontY);
            fLatFrontY = Math.Max(-MaxGrip, fLatFrontY);
            fLatFrontY *= (float)weight;
            if (FrontSlip)
                fLatFrontY *= 0.5f;
            double fLatFrontX = 0;

            // lateral force on rear wheels
            double fLatRearY = (CARear * slipAngleRear);
            fLatRearY = Math.Min(MaxGrip, fLatRearY);
            fLatRearY = Math.Max(-MaxGrip, fLatRearY);
            fLatRearY *= (float)weight;
            if (RearSlip)
                fLatRearY *= 0.5f;
            double fLatRearX = 0;

            // longtitudinal force on rear wheels - very simple traction model
            //double fTractionX = 100 * (car.throttle - car.brake * Math.Sign(velocity.X));
            double fTractionX = EngineForce;
            if (RearSlip)
                fTractionX *= 0.5f;
            double fTractionY = 0;

            // Forces and torque on body

            // drag and rolling resistance
            double speed = Velocity.Length;
            Vector2D resistance = -RollingResistance*velocity - Drag*velocity*speed;

            // sum forces
            double forceX = (fTractionX + Math.Sin(Steering) * fLatFrontX + fLatRearX + resistance.X);
            double forceY = (fTractionY + Math.Cos(Steering) * fLatFrontY + fLatRearY + resistance.Y);
            Force = new Vector2D(forceX, forceY);

            // torque on body from lateral forces
            double torque = WheelBase * 0.5 * fLatFrontX - WheelBase * 0.5 * fLatRearY; // b*fLatFrontX + c*fLatRearY

            // Acceleration

            // Newton F = m.a, therefore a = F/m
            Vector2D acceleration = Force/Mass;

            double angularAcceleration = torque / Inertia;

            // Velocity and position

            // transform acceleration from car reference frame to world reference frame
            Vector2D accelerationWorld = Vector2D.Rotate(acceleration, Angle);

            // velocity is integrated acceleration
            Velocity += accelerationWorld*dt;

            // position is integrated velocity
            Position += Velocity*dt;

            // Angular velocity and heading

            // integrate angular acceleration to get angular velocity
            AngularVelocity += angularAcceleration * dt;

            // integrate angular velocity to get angular orientation
            Angle += AngularVelocity*dt;
        }

        public void Step(double dt)
        {
            double speed = Velocity.Length;
            FDrag = -Drag*speed*Velocity;
            FRollingResistance = -RollingResistance*Velocity;
            FTraction = EngineForce*NormalizedDirection;
            FLongitudinal = FTraction + FDrag + FRollingResistance;

            Force = FLongitudinal;

            Vector2D a = Force/Mass;
            Velocity += a*dt;

            if (Math.Abs(Steering) > 0)
            {
                AngularVelocity = Steering * speed / WheelBase;
                double theta = AngularVelocity * dt;
                Angle += theta;
                double tempX = Velocity.X * Math.Cos(theta) - Velocity.Y * Math.Sin(theta);
                double tempY = Velocity.X * Math.Sin(theta) + Velocity.Y * Math.Cos(theta);
                Velocity.X = tempX;
                Velocity.Y = tempY;
                ComputeNormalizedDirection();
            }

            Position += Velocity * dt;
        }

        private void ComputeNormalizedDirection()
        {
            NormalizedDirection = new Vector2D(Math.Cos(Angle), Math.Sin(Angle));
        }
    }

    public class Vector2D
    {
        public static Vector2D NullObject = new Vector2D(0, 0);

        public double X { get; set; }
        public double Y { get; set; }

        public Vector2D() : this(0,0)
        {
        }

        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector2D operator -(Vector2D v)
        {
            return new Vector2D(-v.X, -v.Y);
        }

        public static Vector2D operator *(Vector2D v, double d)
        {
            return new Vector2D(v.X * d, v.Y * d);
        }

        public static Vector2D operator *(double d, Vector2D v)
        {
            return new Vector2D(v.X * d, v.Y * d);
        }

        public static Vector2D operator /(Vector2D v, double d)
        {
            return new Vector2D(v.X / d, v.Y / d);
        }

        public static Vector2D operator +(Vector2D v, Vector2D w)
        {
            return new Vector2D(v.X + w.X, v.Y + w.Y);
        }

        public static Vector2D operator -(Vector2D v, Vector2D w)
        {
            return new Vector2D(v.X - w.X, v.Y - w.Y);
        }

        public static Vector2D Rotate(Vector2D v, double angle)
        {
            double cs = Math.Cos(angle);
            double sn = Math.Sin(angle);
            double x = sn * v.X + cs * v.Y;
            double y = cs * v.X - sn * v.Y;
            return new Vector2D(x, y);
        }

        public void Normalize()
        {
            double invLength = 1.0 / Length;
            X *= invLength;
            Y *= invLength;
        }

        public double Length
        {
            get { return Math.Sqrt(Length2); }
        }

        public double Length2
        {
            get { return X*X + Y*Y; }
        }

        public static double Dot(Vector2D v, Vector2D w)
        {
            return v.X*w.X + v.Y*w.Y;
        }

        public override string ToString()
        {
            return String.Format("{0:F6};{1:F7}", X, Y);
        }
    }
}
