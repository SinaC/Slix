using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace Slix
{
    public class Tire
    {
        internal readonly Body Body;

        private float _maxForwardSpeed;
        private float _maxBackwardSpeed;
        private float _maxDriveForce;
        private float _maxLateralImpulse;
        private float _currentTraction;

        public Tire(World world)
        {
            Body = new Body(world)
            {
                BodyType = BodyType.Dynamic
            };
            PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(-0.5f, -1.25f), 1);
            Body.CreateFixture(shape);
            Body.UserData = this;

            _currentTraction = 1;
        }

        public void Initialize(float maxForwardSpeed, float maxBackwardSpeed, float maxDriveForce, float maxLateralImpulse)
        {
            _maxForwardSpeed = maxForwardSpeed;
            _maxBackwardSpeed = maxBackwardSpeed;
            _maxDriveForce = maxDriveForce;
            _maxLateralImpulse = maxLateralImpulse;
        }

        private void UpdateTraction()
        {
            _currentTraction = 1;
        }

        private Vector2 GetLateralVelocity()
        {
            Vector2 currentRightNormal = Body.GetWorldVector(new Vector2(1, 0));
            return Vector2.Dot(currentRightNormal, Body.LinearVelocity) * currentRightNormal;
        }

        private Vector2 GetForwardVelocity()
        {
            Vector2 currentForwardNormal = Body.GetWorldVector(new Vector2(0, 1));
            return Vector2.Dot(currentForwardNormal, Body.LinearVelocity) * currentForwardNormal;
        }

        public void UpdateFriction()
        {
            //lateral linear velocity
            Vector2 impulse = Body.Mass * -GetLateralVelocity();
            if (impulse.Length() > _maxLateralImpulse)
                impulse *= _maxLateralImpulse / impulse.Length();
            Body.ApplyLinearImpulse(_currentTraction * impulse, Body.WorldCenter);

            //angular velocity
            Body.ApplyAngularImpulse(_currentTraction * 0.1f * Body.Inertia * -Body.AngularVelocity);

            //forward linear velocity
            Vector2 currentForwardNormal = GetForwardVelocity();
            float currentForwardSpeed = currentForwardNormal.Length();
            float dragForceMagnitude = -2 * currentForwardSpeed;
            Body.ApplyForce(_currentTraction * dragForceMagnitude * currentForwardNormal, Body.WorldCenter);
        }

        public void HandleActions(Actions actions)
        {
            UpdateDrive(actions);
            UpdateTurn(actions);
        }

        private void UpdateDrive(Actions actions)
        {
            //find desired speed
            float desiredSpeed = 0;
            if (actions.Accelerate)
                desiredSpeed = _maxForwardSpeed;
            else if (actions.Brake)
                desiredSpeed = _maxBackwardSpeed;

            //find current speed in forward direction
            Vector2 currentForwardNormal = Body.GetWorldVector(new Vector2(0, 1));
            float currentSpeed = Vector2.Dot(GetForwardVelocity(), currentForwardNormal);

            //apply necessary force
            float force;
            if (desiredSpeed > currentSpeed)
                force = _maxDriveForce;
            else if (desiredSpeed < currentSpeed)
                force = -_maxDriveForce;
            else
                return;
            Body.ApplyForce(_currentTraction * force * currentForwardNormal, Body.WorldCenter);
        }

        private void UpdateTurn(Actions actions)
        {
            float desiredTorque = 0;
            if (actions.Left)
                desiredTorque = 15;
            else if (actions.Right)
                desiredTorque = -15;
            Body.ApplyTorque(desiredTorque);
        }
    }

    public class CarFarseerPhysics
    {
        private readonly Body _body;
        private readonly List<Tire> _tires;
        private readonly RevoluteJoint _frontLeftJoint;
        private readonly RevoluteJoint _frontRightJoint;

        public CarFarseerPhysics(World world)
        {
            // car body
            _body = new Body(world)
                {
                    BodyType = BodyType.Dynamic,
                    AngularDamping = 3
                };
            Vertices vertices = new Vertices
                {
                    new Vector2(1.5f, 0),
                    new Vector2(3, 2.5f),
                    new Vector2(2.8f, 5.5f),
                    new Vector2(1, 10),
                    new Vector2(-1, 10),
                    new Vector2(-2.8f, 5.5f),
                    new Vector2(-3, 2.5f),
                    new Vector2(-1.5f, 0),
                };
            PolygonShape shape = new PolygonShape(vertices, 0.1f);
            _body.CreateFixture(shape);
            _body.UserData = this;

            // tires
            _tires = new List<Tire>(4);
            const float maxForwardSpeed = 250;
            const float maxBackwardSpeed = -40;
            const float backTireMaxDriveForce = 300;
            const float frontTireMaxDriveForce = 500;
            const float backTireMaxLateralImpulse = 8.5f;
            const float frontTireMaxLateralImpulse = 7.5f;

            // back left tire
            Tire backLeftTire = new Tire(world);
            backLeftTire.Initialize(maxForwardSpeed, maxBackwardSpeed, backTireMaxDriveForce, backTireMaxLateralImpulse);
            RevoluteJoint backLeftJoint = new RevoluteJoint(_body, backLeftTire.Body, new Vector2(-3, 0.75f));
            world.AddJoint(backLeftJoint);
            _tires.Add(backLeftTire);

            // back right tire
            Tire backRightTire = new Tire(world);
            backRightTire.Initialize(maxForwardSpeed, maxBackwardSpeed, backTireMaxDriveForce, backTireMaxLateralImpulse);
            RevoluteJoint backRightJoint = new RevoluteJoint(_body, backRightTire.Body, new Vector2(3, 0.75f));
            world.AddJoint(backRightJoint);
            _tires.Add(backRightTire);

            // front left tire
            Tire frontLeftTire = new Tire(world);
            frontLeftTire.Initialize(maxForwardSpeed, maxBackwardSpeed, frontTireMaxDriveForce, frontTireMaxLateralImpulse);
            _frontLeftJoint = new RevoluteJoint(_body, frontLeftTire.Body, new Vector2(-3, 8.5f));
            world.AddJoint(_frontLeftJoint);
            _tires.Add(frontLeftTire);

            // front right tire
            Tire frontRightTire = new Tire(world);
            frontRightTire.Initialize(maxForwardSpeed, maxBackwardSpeed, frontTireMaxDriveForce, frontTireMaxLateralImpulse);
            _frontRightJoint = new RevoluteJoint(_body, frontRightTire.Body, new Vector2(3, 8.5f));
            world.AddJoint(_frontRightJoint);
            _tires.Add(frontRightTire);
        }

        public void HandleActions(Actions actions)
        {
            foreach (Tire tire in _tires)
                tire.UpdateFriction();
            foreach(Tire tire in _tires)
                tire.HandleActions(actions);

            //control steering
            const float lockAngle = 35 * (float)Math.PI/180.0f;
            const float turnSpeedPerSec = 160 * (float)Math.PI/180.0f; //from lock to lock in 0.5 sec
            const float turnPerTimeStep = turnSpeedPerSec / 60.0f;
            float desiredAngle = 0;

            if (actions.Left)
                 desiredAngle = lockAngle;
            else if (actions.Right)
                desiredAngle = -lockAngle;

            float angleNow = _frontLeftJoint.JointAngle;
            float angleToTurn = desiredAngle - angleNow;
            angleToTurn = MathUtils.Clamp(angleToTurn, -turnPerTimeStep, turnPerTimeStep);
            float newAngle = angleNow + angleToTurn;
            _frontLeftJoint.SetLimits(newAngle, newAngle);
            _frontRightJoint.SetLimits(newAngle, newAngle);
        }
    }
}
