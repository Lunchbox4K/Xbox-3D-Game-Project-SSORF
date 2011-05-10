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

namespace SSORF.Objects
{
    class ModelCollection
    {
        private StaticModel geometry;
        private Vector3[] coordinates;
        private short numModels;

        public ModelCollection(StaticModel model, short numClones, Vector3[] locations)
        {
            coordinates = new Vector3[numClones];
            coordinates = locations;
            geometry = model;
            numModels = numClones;
        }

        public bool CheckCollision(short whichModel, StaticModel otherModel)
        {
            geometry.Location = coordinates[whichModel];
            return geometry.TemporaryCollisionDetection(otherModel);
        }

        public void draw(GameTime gameTime, ThirdPersonCamera camera)
        { 
            for(int i = 0; i < numModels; i++)
            {
                geometry.Location = coordinates[i];
                geometry.drawModel(gameTime, camera.ViewMtx, camera.ProjMtx);
            }
        
        }

        public void draw(GameTime gameTime, ThirdPersonCamera camera, short start, short end)
        {
            if (end >= numModels)
                end = (short)(numModels - 1);
            for (int i = start; i <= end; i++)
            {
                geometry.Location = coordinates[i];
                geometry.drawModel(gameTime, camera.ViewMtx, camera.ProjMtx);
            }
        }

        public StaticModel Geometry
        {
            get { return geometry; }
            set{ geometry = value; }
        }
    }
}
