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

//This class will be used to place checkpoints throughout the level 
//and check to see if player has collided with the checkpoint
namespace SSORF.Objects
{
    class CheckPoint : StaticModel
    {
        //base.cont
        private CheckPoint nextPoint;
        private CheckPoint startPoint;
        private ModelQuadTree viewTree;

        private string asset;

        public CheckPoint(ContentManager Content, string modelAsset, Vector3 location, float scale, Matrix orientation,  ModelQuadTree ViewTree)
            : base(Content, modelAsset, location, orientation, scale)
        {
            startPoint = this;
            nextPoint = null;
            asset = modelAsset;
            viewTree = ViewTree;
        }

        public CheckPoint(ContentManager Content, string modelAsset, Vector3 location, float scale, Matrix orientation,
            CheckPoint StartPoint, ModelQuadTree ViewTree)
            : base(Content, modelAsset, location, orientation, scale)
        {
            startPoint = StartPoint;
            nextPoint = null;
            asset = modelAsset;
        }

        public void addToStaticList(ref List<StaticModel> modelList)
        {
            modelList.Add(this);
            if (nextPoint != null)
                nextPoint.addToStaticList(ref modelList);
        }

        public void registerToTree(ModelQuadTree tree)
        {
            tree.addStaticModel((StaticModel)this);
            if (nextPoint != null)
                nextPoint.registerToTree(tree);
        }

        public void PushCheckPoint(Vector3 Location)
        {
            if (nextPoint == null)
                nextPoint = new CheckPoint
                    (base.content, asset, Location, base.scale, base.orientation, viewTree);
            else
                nextPoint.PushCheckPoint(Location);
        }

        //public void setActive()
        //{
        //    isEnabled = true;
        //    if (nextPoint != null)
        //        nextPoint.setActive();
        //}

        public void spinCheckPoints()
        {

        }

        public void loadCheckpoint()
        {
            if (!isLoaded)
            {
                LoadModel();
                base.calcBoundingSpheres();
                viewTree.addStaticModel(this);
                if (nextPoint != null)
                    nextPoint.loadCheckpoint();
                
            }

        }

        public void unloadCheckpoint()
        {
            if (isLoaded)
            {
                UnloadModel();
            }
        }

    }
}
