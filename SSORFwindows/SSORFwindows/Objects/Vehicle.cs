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

//Contains 3D model and specs for a scooter
namespace SSORF.Objects
{
    public class Vehicle
    {
        StaticModel geometry;
        float speed = 1.4f;
        float yaw;
        float wheelAngle = 0;
        float wheelMaxAngle = (float).785;
        int wheelBaseLength = 24;
        //vehicle still needs list of specs such as weight, name, etc
        //Also need a way to add upgrades to vehicles

        public void load(ContentManager content, short vehicleID)
        {
            geometry = new StaticModel(content, "Models\\scooter" + vehicleID.ToString(),
                Vector3.Zero, Matrix.Identity, Matrix.Identity);
            geometry.LoadModel();

            //Things to load here:
            //value of wheelMaxAngle - DO IT IN RADIANS
            //value of wheelBaseLength
            //other vehicle specs
            //upgrade specs
        }

        public void setStartingPosition(float startingYaw, Vector3 startingPosition)
        {
            yaw = startingYaw;
            geometry.Orientation = Matrix.CreateRotationY(yaw);
            geometry.Location = startingPosition;
        }

        public void update(GameTime gameTime, float steerValue, float throttleValue, float brakeValue)
        {
            //Get the integral of the vehicle's velocity
            float tempDistance = speed * throttleValue;
            //Find the vehicle's current turning radius
            float turnRadius = wheelBaseLength/(float)Math.Sin(wheelAngle);
            //Now use those to get the vehicle's yaw offset - i love radians
            float tempYaw = tempDistance / turnRadius;

            //Update rotations
            yaw += tempYaw;

            geometry.Orientation = Matrix.CreateRotationY(yaw);

            //Derive and update position
            geometry.Location += geometry.Orientation.Forward * tempDistance * (float)Math.Cos(tempYaw);
            geometry.Location += geometry.Orientation.Left * tempDistance * (float)Math.Sin(tempYaw);

            //@TODO: calculate velocity by end of frame
            //Capture the wheel angle for the next frame's worth of motion
            wheelAngle = steerValue * wheelMaxAngle;

            //TODO: calculate velocity from accel/decel

            //KeyBoard input
            //NOTE: Don't delete this region!! I need to move to test collisions with checkpoints!!!
            //#region
            //if (keyBoardState.current.IsKeyDown(Keys.Up))
            //   geometry.Location += geometry.Orientation.Forward * 1.5f;
            //if (keyBoardState.current.IsKeyDown(Keys.Left))
            //    yaw += 0.1f;
            //if (keyBoardState.current.IsKeyDown(Keys.Right))
            //    yaw -= 0.1f;

            //geometry.Orientation = Matrix.CreateRotationY(yaw);
            //#endregion
        }
 
        //Accessors and Mutators
        public StaticModel Geometry { get { return geometry; } set { geometry = value; } }

        public float Yaw { get { return yaw; } set { yaw = value; } }

    }
}
