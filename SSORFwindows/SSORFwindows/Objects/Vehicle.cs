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
        SimpleModel geometry;
        Matrix rotationMtx;
        Vector3 position = Vector3.Zero;
        Vector3 lookVector = Vector3.Zero;
        float speed = 1.4f;
        float yaw = 0.0f;
        float wheelAngle = 0;
        float wheelMaxAngle = (float).785;
        int wheelBaseLength = 24;
        //vehicle still needs list of specs such as weight, name, etc
        //Also need a way to add upgrades to vehicles

        public void load(ContentManager content, short vehicleID)
        {
            geometry = new SimpleModel();
            geometry.Mesh = content.Load<Model>("Models\\scooter" + vehicleID.ToString());

            //Things to load here:
            //value of wheelMaxAngle - DO IT IN RADIANS
            //value of wheelBaseLength
            //other vehicle specs
            //upgrade specs
        }


        public void update(GameTime gameTime)
        {
            //TODO: calculate velocity from accel/decel
            //Get the integral of the vehicle's velocity
            float tempDistance = speed * gamePadState.current.Triggers.Right;
            //Find the vehicle's current turning radius
            float turnRadius = wheelBaseLength/(float)Math.Sin(wheelAngle);
            //Now use those to get the vehicle's yaw offset - i love radians
            float tempYaw = tempDistance / turnRadius;

            //Update rotations
            yaw += tempYaw;

            rotationMtx = Matrix.CreateRotationY(yaw);

            //Derive and update position
            position += rotationMtx.Forward * tempDistance * (float)Math.Cos(tempYaw);
            position += rotationMtx.Left * tempDistance * (float)Math.Sin(tempYaw);

            //@TODO: calculate velocity by end of frame
            //Capture the wheel angle for the next frame's worth of motion
            wheelAngle = gamePadState.current.ThumbSticks.Left.X * wheelMaxAngle;

            //KeyBoard input to go forward
            if (keyBoardState.current.IsKeyDown(Keys.Up))
                position += rotationMtx.Forward * 1.5f;

            geometry.WorldMtx = rotationMtx * Matrix.CreateTranslation(position);
        }
 
        //Accessors and Mutators
        public SimpleModel Geometry { get { return geometry; } set { geometry = value; } }
    
        public Matrix RotationMtx { get { return rotationMtx; } }

        public Vector3 Position { get { return position; } set { position = value; } }

        public float Yaw { get { return yaw; } set { yaw = value; } }

    }
}
