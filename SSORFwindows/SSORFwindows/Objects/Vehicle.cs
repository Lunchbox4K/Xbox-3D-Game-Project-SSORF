using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SSORF.Management;

//Contains 3D model and specs for a scooter
namespace SSORF.Objects
{
    public class Vehicle
    {
        StaticModel geometry;
        //Units for simulation:
        //Torque:       Newton-meters
        //Force:        Newtons
        //Weight:       Kilograms
        //Speed:        Meters/Sec
        //Distance:     Meters
        //Angles:       Radians
        //Grip:         Meters/Sec^2
        SSORFlibrary.ScooterData mySpecs;
        const float meterToInchScale = 39.37f;
        const float ampToNetwonMeterScale = .035f;
        float speed;
        float yaw;
        float wheelAngle;

        public void load(ContentManager content, SSORFlibrary.ScooterData VehicleSpecs, upgradeSpecs Upgrades)
        {
            mySpecs = new SSORFlibrary.ScooterData();
            mySpecs.Copy(VehicleSpecs);
            mySpecs.outputPower += Upgrades.power;
            mySpecs.outputPower *= ampToNetwonMeterScale;              //Scaling from amps to newton-meters here
            mySpecs.weight += Upgrades.weight;
            geometry = new StaticModel(content, "Models\\scooter" + VehicleSpecs.IDnum.ToString(),
                Vector3.Zero, Matrix.Identity, Matrix.Identity);
            geometry.LoadModel();
        }

        public void setNormal(TerrainInfo terrainInfo)
        {
            float height;
            Vector3 normal;
            Vector3 location;

            if(terrainInfo.IsOnHeightmap(geometry.Location))
            {
                terrainInfo.GetHeightAndNormal(geometry.Location, out height, out normal);
                location = geometry.Location;
                location.Y = height;
                geometry.Location = location;
                geometry.Orientation = Matrix.CreateRotationZ((-normal.X * (float)Math.Cos(yaw)) + (normal.Z * (float)Math.Sin(yaw)));  //Get roll
                geometry.Orientation *= Matrix.CreateRotationX((normal.Z * (float)Math.Cos(yaw)) + (normal.X * (float)Math.Sin(yaw)));  //Get pitch
                geometry.Orientation *= Matrix.CreateRotationY(yaw);    //Get yaw
            }
        }

        public void setStartingPosition(float startingYaw, Vector3 startingPosition, float startingSpeed)
        {
            yaw = startingYaw;
            geometry.Orientation = Matrix.CreateRotationY(yaw);
            geometry.Location = startingPosition;
            speed = startingSpeed;
        }

        public void update(GameTime gameTime, float steerValue, float throttleValue, float brakeValue)
        {
            //Get the integral of the vehicle's velocity
            float tempDistance = speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //Find the vehicle's current turning radius
            float turnRadius = mySpecs.wheelBaseLength / (float)Math.Tan(wheelAngle);
            //Model the effects of ground angle and velocity on grip
            float gripForce = (float)Math.Pow(speed / turnRadius, 2) * Math.Abs(turnRadius);                //Raw centrifugal force, m/sec^2
            gripForce *= mySpecs.weight;        //Lateral force, N
            gripForce *= (float)Math.Sqrt(1 - Math.Pow(Math.Abs(geometry.Orientation.Right.Y), 2));          //Compensated for ground angle
            //Clamp turning radius to maximum grip
            if (mySpecs.gripRating < gripForce/9.8)
            {
                if (turnRadius < 0)
                    turnRadius = (float)Math.Pow(speed, 2) / -mySpecs.gripRating;
                else
                    turnRadius = (float)Math.Pow(speed, 2) / mySpecs.gripRating;
            }
            //Now use those to get the vehicle's yaw offset
            float deltaYaw = tempDistance / turnRadius;
            //Update rotations
            yaw += deltaYaw * (float)Math.Sqrt(1 - Math.Pow(geometry.Orientation.Left.Y, 2));
            //Derive and update position
            geometry.Location += geometry.Orientation.Forward * tempDistance * (float)Math.Cos(deltaYaw) * meterToInchScale * (float)Math.Sqrt(1 - Math.Pow(geometry.Orientation.Forward.Y, 2));
            geometry.Location += geometry.Orientation.Left * tempDistance * (float)Math.Sin(deltaYaw) * meterToInchScale * (float)Math.Sqrt(1 - Math.Pow(geometry.Orientation.Left.Y, 2));
            //Capture the wheel angle for the next frame's worth of motion
            wheelAngle = steerValue * mySpecs.wheelMaxAngle;
            //Swap brake/power if the player is reversing
            if (speed < 0)
            {
                float temp = throttleValue;
                throttleValue = brakeValue;
                brakeValue = temp;
            }
            //Now get the effects of various forces on speed for next frame
            float longForce = (mySpecs.outputPower / mySpecs.wheelRadius) * throttleValue;      //Power effect
            if (mySpecs.gripRating < longForce)       //wheelspin modeling, incomplete
                longForce = mySpecs.gripRating * 2;
            longForce -= (mySpecs.brakePower / mySpecs.wheelRadius) * brakeValue;               //Brake effect
            //Calculate drag
            float dragForce = mySpecs.coefficientDrag * mySpecs.frontalArea * .6f * (float)Math.Pow(speed, 2);
            longForce -= dragForce;
            //Flip power/brake/drag if reversing
            if (speed < 0)
            {
                longForce *= -1;
                longForce += mySpecs.rollingResistance;
            }
            else
                longForce -= mySpecs.rollingResistance;
            //Incorporate force of gravity
            longForce -= (geometry.Orientation.Forward.Y * mySpecs.weight) / 9.8f;
            //Modify force for inertia
            float deltaV = (longForce) / mySpecs.weight;
            speed += deltaV;
            UpdateAudio(throttleValue);
        }

        public void UpdateAudio(float throttleValue)
        {
            AudioManager.getEngineSounds().SetVariable("throttleValue", throttleValue + speed);
            AudioManager.getEngineSounds().SetVariable("Speed", speed);
            if (throttleValue != 0)
            {

                if (AudioManager.getEngineSounds().IsPaused)
                    AudioManager.getEngineSounds().Resume();
                else if (AudioManager.getEngineSounds().IsStopped)
                {
                    AudioManager.resetEngineSounds();
                    AudioManager.getEngineSounds().Play();
                }
                else if (AudioManager.getEngineSounds().IsPlaying == false &&
                    AudioManager.getEngineSounds().IsPrepared)
                    AudioManager.getEngineSounds().Play();
            }
            else
            {
                AudioManager.getEngineSounds().Stop(AudioStopOptions.AsAuthored);
            }
        }
 
        //Accessors and Mutators
        public StaticModel Geometry { get { return geometry; } set { geometry = value; } }

        public float Speed { get { return speed; } set { speed = value; } }

        public float Yaw { get { return yaw; } set { yaw = value; } }

    }
}
