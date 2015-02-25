using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slix
{
    //http://www.gamedev.net/page/resources/_/technical/math-and-physics/2d-car-physics-r2443
    public class RigidBody
    {
        // Linear properties
        public Vector2D Position { get; private set; }
        public Vector2D Velocity { get; private set; }
        public Vector2D Forces { get; private set; }
        public double Mass { get; private set; }

        // Angular properties
        public double Angle { get; private set; }
        public double AngularVelocity { get; private set; }
        public double Torque { get; private set; }
        public double Inertia { get; private set; }

        // Graphical properties
        public Vector2D HalfSize { get; private set; }

        //
        public double Speed { get { return Velocity.Length; } }
 
        public RigidBody()
        {
            Position = new Vector2D();
            Velocity = new Vector2D();
            Forces = new Vector2D();
            Mass = 1;

            Angle = 0;
            AngularVelocity = 0;
            Torque = 0;
            Inertia = 1;

            HalfSize = new Vector2D();
        }

        public void SetLocation(Vector2D position, double angle)
        {
            Position = position;
            Angle = angle;
        }

        public virtual void Initialize(Vector2D halfSize, double mass)
        {
            HalfSize = halfSize;
            Mass = mass;

            Inertia = (halfSize.X * halfSize.X) * (halfSize.Y * halfSize.Y) * Mass / 12; // http://howard.nebrwesleyan.edu/hhmi/fellows/pgomez/inertforms.html
            AngularVelocity = 0;
            Torque = 0;
            Position = new Vector2D();
            Velocity = new Vector2D();
            Forces = new Vector2D();
        }

        public virtual void Step(double dt)
        {
            //integrate physics

            //linear
            // F = m.a -> a = F/m
            Vector2D acceleration = Forces / Mass;
            // V = V + a.t
            Velocity += acceleration * dt;
            // P = P + a.t
            Position += Velocity * dt;

            //angular
            // AngA = torque / inertia
            double angularAcceleration = Torque / Inertia;
            // AngV = AngV + AngA.t
            AngularVelocity += angularAcceleration * dt;
            // Angle = Angle + AngV.t
            Angle += AngularVelocity * dt;

            //clear
            Forces = new Vector2D(0, 0); //clear forces
            Torque = 0; //clear torque
        }

        //velocity of a point on body
        public Vector2D PointVelocity(Vector2D worldOffset)
        {
            Vector2D tangent = new Vector2D(-worldOffset.Y, worldOffset.X);
            return tangent*AngularVelocity + Velocity;
        }

        //
        public void AddForce(Vector2D worldForce, Vector2D worldOffset)
        {
            //add linear force
            Forces += worldForce;
            //and its associated torque
            double torque = Vector2D.CrossProduct(worldOffset, worldForce);
            Torque += torque;

            //System.Diagnostics.Debug.WriteLine("Forces: {0}", Forces);
            //System.Diagnostics.Debug.WriteLine("Torque: {0:F10} --- {1:F10} --- {2} {3}", Torque, torque, worldForce, worldOffset);
        }

        //take a relative vector and make it a world vector
        public Vector2D RelativeToWorld(Vector2D relative)
        {
            return Vector2D.Rotate(relative, Angle);
        }

        //take a world vector and make it a relative vector
        public Vector2D WorldToRelative(Vector2D world)
        {
            return Vector2D.Rotate(world, -Angle);
        }
    }
}
