using System;

namespace Slix
{
    //http://www.asawicki.info/Mirror/Car%20Physics%20for%20Games/Car%20Physics%20for%20Games.html
    //http://ploobs.com.br/?p=2278
    //http://entertain.univie.ac.at/~hlavacs/publications/car_model06.pdf
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
        public double Width { get; private set; }
        public double TyreRadius { get; private set; } // Rw
        public double TyreInertia { get; private set; } // Iw

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

            TyreRadius = 0.33; // m
            TyreInertia = 2*4.1; // kg m²
            Width = 2; // m
            Mass = 1500; // Kg
            Drag = 0.4257;
            RollingResistance = 12.8;
            WheelBase = 4; // Meters
            Handling = 1*Math.PI/180; // Radians
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

        //http://entertain.univie.ac.at/~hlavacs/publications/car_model06.pdf
        private struct CarStepInput
        {
            public double Beta;
            public double EngineFrontTorque;
            public double EngineRearTorque;
        }

        public void Step(double dt)
        {
            CarStepInput oldInput = new CarStepInput(); // TO DEFINE
            CarStepInput input = new CarStepInput(); // TO DEFINE

            double beta = oldInput.Beta;
            double deltaBeta = input.Beta - oldInput.Beta;
            double engineFrontTorque = oldInput.EngineFrontTorque;
            double engineRearTorque = oldInput.EngineRearTorque;
            double tanBeta = Math.Tan(beta);

            double speed = Velocity.Length;
            double omega = tanBeta*speed/WheelBase;

            Vector2D centripetalFrontForce;
            Vector2D centripetalRearForce;
            if (Math.Abs(beta - 0) > 0.00001)
            {
                double subValue = -Mass*Square(speed)*Square(tanBeta)/WheelBase;
                centripetalFrontForce = new Vector2D(subValue/(2*tanBeta), subValue*0.5);
                centripetalRearForce = new Vector2D(subValue/(2*tanBeta), 0);
            }
            else
            {
                centripetalFrontForce = new Vector2D(0, 0);
                centripetalRearForce = new Vector2D(0, 0);
            }

            double ic = Mass*(Square(Width) + Square(WheelBase))/12; // moment of inertia
            double ib = ic + Mass*Square(WheelBase)/4; // moment of inertia
            double fr = ib*tanBeta/Square(WheelBase);

            double fcpfy = -Mass*Square(speed)*Square(tanBeta)/(2*WheelBase);
            //????
            //double fTotSummand1 = Mass*Square(TyreRadius)*(fcpfy - fr*speed/Square(Math.Cos(beta)))/(2*TyreInertia + (Mass + fr*tanBeta)*Square(TyreRadius)); // rotational + centripetal
            //double fTotSummand2 = Mass * TyreRadius * (Math.Cos(beta) * centripetalFrontForce + centripetalRearForce) / (2 * TyreInertia + (Mass + fr * tanBeta) * Square(TyreRadius)); // engine + torque
            //double fTot = fTotSummand1 + fTotSummand2;
        }

        //http://www.asawicki.info/Mirror/Car%20Physics%20for%20Games/Car%20Physics%20for%20Games.html
        //public void Step(double dt)
        //{
        //    double speed = Velocity.Length;
        //    FDrag = -Drag*speed*Velocity;
        //    FRollingResistance = -RollingResistance*Velocity;
        //    FTraction = EngineForce*NormalizedDirection;
        //    FLongitudinal = FTraction + FDrag + FRollingResistance;

        //    Force = FLongitudinal;

        //    Vector2D a = Force/Mass;
        //    Velocity += a*dt;

        //    if (Math.Abs(Steering) > 0)
        //    {
        //        AngularVelocity = Steering * speed / WheelBase;
        //        double theta = AngularVelocity * dt;
        //        Angle += theta;
        //        double tempX = Velocity.X * Math.Cos(theta) - Velocity.Y * Math.Sin(theta);
        //        double tempY = Velocity.X * Math.Sin(theta) + Velocity.Y * Math.Cos(theta);
        //        Velocity.X = tempX;
        //        Velocity.Y = tempY;
        //        ComputeNormalizedDirection();
        //    }

        //    Position += Velocity * dt;
        //}

        //http://ploobs.com.br/?p=2278
        //public void Step(double dt)
        //{
        //    if (Math.Abs(Steering - 0.0) > 0.0001)
        //    {
        //        int breakme = 1;
        //    }

        //    // convention X: front Y: right

        //    // Car velocity in car reference
        //    // Transform velocity in world reference frame to velocity in car reference frame
        //    // 2D rotation of the velocity vector by car orientation ...
        //    Vector2D velocity = Vector2D.Rotate(Velocity, Angle);

        //    // Lateral force on wheels
        //    // Resulting velocity of the wheels as result of the yaw rate of the car body
        //    // v = yawrate * r where r is distance of wheel to CG (approx. half wheel base)
        //    // yawrate (ang.velocity) must be in rad/s
        //    double yawSpeed = WheelBase * 0.5 * AngularVelocity;

        //    double rotAngle;
        //    if (Math.Abs(velocity.X) < 0.000001) // TODO: fix singularity
        //        rotAngle = 0;
        //    else
        //        rotAngle = Math.Atan2(yawSpeed, velocity.X);

        //    // Calculate the side slip angle of the car (a.k.a. beta)
        //    double slideSlip;
        //    if (Math.Abs(velocity.X) < 0.000001) // TODO: fix singularity
        //        slideSlip = 0;
        //    else
        //        slideSlip = Math.Atan2(velocity.Y, velocity.X);

        //    // Calculate slip angles for front and rear wheels (a.k.a. alpha)
        //    double slipAngleFront = slideSlip + rotAngle - Steering;
        //    double slipAngleRear = slideSlip - rotAngle;

        //    // Weight per axle = half car mass times 1G (=9.8m/s^2) 
        //    double weight = Mass * 9.8f * 0.5f;

        //    // lateral force on front wheels = (Ca * slip angle) capped to friction circle * load
        //    double fLateralFrontY = (CAFront * slipAngleFront);
        //    fLateralFrontY = Math.Max(-MaxGrip, Math.Min(MaxGrip, fLateralFrontY));
        //    fLateralFrontY *= weight;
        //    if (FrontSlip)
        //        fLateralFrontY *= 0.5f;
        //    double fLateralFrontX = 0;

        //    // lateral force on rear wheels
        //    double fLateralRearY = (CARear * slipAngleRear);
        //    fLateralRearY = Math.Max(-MaxGrip,Math.Min(MaxGrip, fLateralRearY));
        //    fLateralRearY *= weight;
        //    if (RearSlip)
        //        fLateralRearY *= 0.5f;
        //    double fLateralRearX = 0;

        //    // longtitudinal force on rear wheels - very simple traction model
        //    //double fTractionX = 100 * (car.throttle - car.brake * Math.Sign(velocity.X));
        //    double fTractionX = EngineForce;
        //    if (RearSlip)
        //        fTractionX *= 0.5f;
        //    double fTractionY = 0;

        //    // Forces and torque on body

        //    // drag and rolling resistance
        //    double speed = Velocity.Length;
        //    //resistanceX = -(RollingResistance * velocity.X + Drag * velocity.X * Math.Abs(velocity.X));
        //    //resistanceY = -(RESISTANCE * velocity.Y + DRAG * velocity.Y * Math.Abs(velocity.Y));
        //    Vector2D resistance = -RollingResistance*velocity - Drag*velocity*speed;

        //    // sum forces
        //    double forceX = (fTractionX + Math.Sin(Steering) * fLateralFrontX + fLateralRearX + resistance.X);
        //    double forceY = (fTractionY + Math.Cos(Steering) * fLateralFrontY + fLateralRearY + resistance.Y);
        //    Force = new Vector2D(forceX, forceY);

        //    // torque on body from lateral forces
        //    double torque = WheelBase * 0.5 * fLateralFrontY - WheelBase * 0.5 * fLateralRearY; // b*fLateralFrontY - c*fLateralRearY

        //    // Acceleration

        //    // Newton F = m.a, therefore a = F/m
        //    Vector2D acceleration = Force/Mass;

        //    double angularAcceleration = torque / Inertia;

        //    // Velocity and position

        //    // transform acceleration from car reference frame to world reference frame
        //    Vector2D accelerationWorld = Vector2D.Rotate(acceleration, Angle);

        //    // velocity is integrated acceleration
        //    Velocity += accelerationWorld*dt;

        //    // position is integrated velocity
        //    Position += Velocity*dt;

        //    // Angular velocity and heading

        //    // integrate angular acceleration to get angular velocity
        //    AngularVelocity += angularAcceleration * dt;

        //    // integrate angular velocity to get angular orientation
        //    Angle += AngularVelocity*dt;

        //    System.Diagnostics.Debug.WriteLine("S:{0:F10} FLY:{1:F10} FRY:{2:F10} FTX:{3:F10} T:{4:F10} AA:{5:F10}", Steering, fLateralFrontY, fLateralRearY, fTractionX, torque, angularAcceleration);
        //    System.Diagnostics.Debug.WriteLine("V:{0} AW:{1} AV:{2:F10} A: {3:F10}", Velocity, accelerationWorld, AngularVelocity, Angle);
        //}

        private void ComputeNormalizedDirection()
        {
            NormalizedDirection = new Vector2D(Math.Cos(Angle), Math.Sin(Angle));
        }

        private double Square(double v)
        {
            return v*v;
        }
    }
}
