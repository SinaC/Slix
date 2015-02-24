using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slix
{
    public class CarPacejka
    {
        // Lateral force coefficients
        private double _a0, _a1, _a2, _a3, _a4, _a5, _a6, _a7, _a8, _a9, _a10, _a111, _a112, _a12, _a13;

        // Longitudinal force coefficients
        private double _b0, _b1, _b2, _b3, _b4, _b5, _b6, _b7, _b8, _b9, _b10;

        // Aligning moment coefficients
        private double _c0, _c1, _c2, _c3, _c4, _c5, _c6, _c7, _c8, _c9, _c10, _c11, _c12, _c13, _c14, _c15, _c16, _c17;

        // - load - how hard is the tire pressing against the ground
        // - slip angle - how big is the angle between tire angle and actual heading - for example when your wheels are turned all the way out but you are going straight because of excessive speed, there is a big slip angle (you are understeering)
        // - slip ratio - how much faster or slower is the wheel turning relative to actual vehicle speed. for example, the slip ratio is positive when there is wheelspin

        // Input parameters
        private double _camber; // Angle of tire vs. surface (in degrees)
        private double _slipAngle; // Slip angle (in degrees)
        private double _slipPercentage; // Percentage slip ratio (in %)
        private double _load; // Normal force (in kN)

        // Output
        public double LongitudinalForce { get; set; }
        public double LateralForce { get; set; }
        public double AligningForce { get; set; }
        public double LongitudinalStiffness { get; private set; } // Longitudinal tire stiffness
        public double LatStiffness { get; private set; } // Lateral or cornering 
        public double MaxLongitudinalForce { get; set; } // Max available tire longitudinal force (friction ellipse)
        public double MaxLateralForce { get; set; } // Max available tire lateral force (friction ellipse)

        public void Initialize()
        {
            // Longitudinal coefficients
            _a0 = 1.3;
            _a0 = 1.3;
            _a1 = -49.0;
            _a2 = 1216.0;
            _a3 = 1632.0;
            _a4 = 11.0;
            _a5 = 0.006;
            _a6 = -0.04;
            _a7 = -0.4;
            _a8 = 0.003;
            _a9 = -0.002;
            _a10 = 0.0;
            _a111 = -11.0;
            _a112 = 0.045;
            _a12 = 0.0;
            _a13 = 0.0;

            // Lateral coefficients
            _b0 = 1.57;
            _b1 = -48.0;
            _b2 = 1338.0;
            _b3 = 6.8;
            _b4 = 444.0;
            _b5 = 0.0;
            _b6 = 0.0034;
            _b7 = -0.008;
            _b8 = 0.66;
            _b9 = 0.0;
            _b10 = 0.0;

            // Aligning moment coefficients
            _c0 = 2.46;
            _c1 = -2.77;
            _c2 = -2.9;
            _c3 = 0.0;
            _c4 = -3.6;
            _c5 = -0.1;
            _c6 = 0.0004;
            _c7 = 0.22;
            _c8 = -2.31;
            _c9 = 3.87;
            _c10 = 0.0007;
            _c11 = -0.05;
            _c12 = -0.006;
            _c13 = 0.33;
            _c14 = -0.04;
            _c15 = -0.4;
            _c16 = 0.092;
            _c17 = 0.0114;

            _camber = 0.0;
            _slipAngle = 0.0;
            _slipPercentage = 0.0;

            LongitudinalForce = LateralForce = _load = AligningForce = 0.0;
            LongitudinalStiffness = 0.0;
            LatStiffness = 0.0;
            MaxLongitudinalForce = 0.0;
            MaxLateralForce = 0.0;
        }

        public void SetCamber(double camberDegrees)
        {
            _camber = camberDegrees;
        }

        public void SetSlipAngle(double slipAngleDegrees)
        {
            _slipAngle = slipAngleDegrees;
        }

        public void SetSlipRatio(double slipRatio)
        {
            _slipPercentage = slipRatio*100.0;
        }

        public void SetLoad(double loadForce)
        {
            _load = loadForce/1000.0;
        }

        // Calculates longitudinal force
        // From G. Genta's book, page 63
        // Note that the units are inconsistent:
        //   load is in kN
        //   slipRatio is in percentage (=slipRatio*100=slipPercentage)
        //   camber and slipAngle are in degrees
        // Resulting forces are better defined:
        //   longitudinalForce, lateralForce are in N
        //   aligningForce     is in Nm
        protected double CalculateLongitudinalForce()
        {
            double B, C, D, E;
            double longitudinalForce;
            double Sh, Sv;
            double uP;
            double loadSquared;

            // Calculate derived coefficients
            loadSquared = _load * _load;
            C = _b0;
            uP = _b1 * _load + _b2;
            D = uP * _load;

            // Avoid div by 0
            if ((C > -0.00001f && C < 0.00001f) || (D > -0.00001f && D < 0.00001f))
                B = 99999.0;
            else
                B = ((_b3 * loadSquared + _b4 * _load) * Math.Exp(-_b5 * _load)) / (C * D);

            E = _b6 * loadSquared + _b7 * _load + _b8;
            Sh = _b9 * _load + _b10;
            Sv = 0;

            // Notice that product BCD is the longitudinal tire stiffness
            LongitudinalStiffness = B * C * D;

            // Remember the max longitudinal force
            MaxLongitudinalForce = D + Sv;

            // Calculate result force
            longitudinalForce = D * Math.Sin(C * Math.Atan(B * (1.0f - E) * (_slipPercentage + Sh) + E * Math.Atan(B * (_slipPercentage + Sh)))) + Sv;

            return longitudinalForce;
        }

        // Calculates lateral force
        // Note that BCD is the cornering stiffness, and
        // Sh and Sv account for ply steer and conicity forces
        protected double CalculateLateralForce()
        {
            double B, C, D, E;
            double lateralForce;
            double Sh, Sv;
            double uP;

            // Calculate derived coefficients
            C = _a0;
            uP = _a1 * _load + _a2;
            D = uP * _load;
            E = _a6 * _load + _a7;

            // Avoid div by 0
            if ((C > -0.00001f && C < 0.00001f) || (D > -0.00001f && D < 0.00001f))
                B = 99999.0;
            else
            {
                if (_a4 > -0.00001f && _a4 < 0.00001f)
                    B = 99999.0;
                else
                {
                    // Notice that product BCD is the lateral stiffness (=cornering)
                    LatStiffness = _a3 * Math.Sin(2 * Math.Atan(_load / _a4)) * (1 - _a5 * Math.Abs(_camber));
                    B = LatStiffness / (C * D);
                }
            }

            Sh = _a8 * _camber + _a9 * _load + _a10;
            Sv = (_a111 * _load + _a112) * _camber * _load + _a12 * _load + _a13;

            // Remember maximum lateral force
            MaxLateralForce = D + Sv;

            // Calculate result force
            lateralForce = D * Math.Sin(C * Math.Atan(B * (1.0f - E) * (_slipAngle + Sh) + E * Math.Atan(B * (_slipAngle + Sh)))) + Sv;

            return lateralForce;
        }

        protected double CalculateAligningForce()
        {
            double aligningForce;
            double B, C, D, E, Sh, Sv;
            double loadSquared;

            // Calculate derived coefficients
            loadSquared = _load * _load;
            C = _c0;
            D = _c1 * loadSquared + _c2 * _load;
            E = (_c7 * loadSquared + _c8 * _load + _c9) * (1 - _c10 * Math.Abs(_camber));

            if ((C > -0.00001f && C < 0.00001f) || (D > -0.00001f && D < 0.00001f))
                B = 99999.0;
            else
                B = ((_c3 * loadSquared + _c4 * _load) * (1 - _c6 * Math.Abs(_camber)) * Math.Exp(-_c5 * _load)) / (C * D);

            Sh = _c11 * _camber + _c12 * _load + _c13;
            Sv = (_c14 * loadSquared + _c15 * _load) * _camber + _c16 * _load + _c17;

            aligningForce = D * Math.Sin(C * Math.Atan(B * (1.0f - E) * (_slipAngle + Sh) + E * Math.Atan(B * (_slipAngle + Sh)))) + Sv;

            return aligningForce;
        }

        public void Calculate()
        {
            // Calculate long. force (and long. stiffness plus max long. force)
            LongitudinalForce = CalculateLongitudinalForce();

            // Calculate lateral force, cornering stiffness and max lateral force
            LateralForce = CalculateLateralForce();

            // Aligning moment (force feedback)
            AligningForce = CalculateAligningForce();
        }

        // Calculates maximum force that the tire can produce
        // If the longitudinal and lateral force combined exceed this,
        // a violation of the friction circle (max total tire force) is broken.
        // In that case, reduce the lateral force, since the longitudinal force
        // is more prominent (this simulates a locked braking wheel, which
        // generates no lateral force anymore but does maximum longitudinal force).
        public double GetMaxForce()
        {
            double uP = _b1 * _load + _b2;

            return uP * _load;
        }
    }
}
