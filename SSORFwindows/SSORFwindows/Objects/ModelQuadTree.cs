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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ModelQuadTree : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private Game game;

        private ModelQuadTreeNode rootNode;
        private int depth = 0;

        public ModelQuadTree(Game game)
            : base(game)
        {
            this.game = game;
            rootNode = null;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {

            base.Initialize();
        }

        protected override void LoadContent()
        {


            base.LoadContent();
        }

        protected override void UnloadContent()
        {


            base.UnloadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }

    class ModelQuadTreeNode
    {
        private const float Y_SCALE = 1000.0f;
        private const byte NODE_CNT = 4;

        private ModelQuadTreeNode parent;
        private ModelQuadTreeNode[] children;
        private List<StaticModel> models;
        private BoundingBox area;
        private bool isActive;

        public ModelQuadTreeNode(ModelQuadTreeNode Parent, BoundingBox Area)
        {
            parent = Parent;
            isActive = false;
            children = null;
            models = null;
            BoundingBox area = Area;
            area.Max.Y = Y_SCALE;
            area.Min.Y = -Y_SCALE;
        }

        public void pushModel(StaticModel model)
        {
            if (children == null)
                split();
            FindHome(model.getBoundingBox).addModel(model);
        }

        public void drawNodes(GameTime gameTime, Matrix view, Matrix projection)
        {
            if (isActive)
            {
                if (models != null)
                {
                    foreach (StaticModel model in models)
                    {
                        model.drawModel(gameTime, view, projection);
                    }
                }
                if (children != null)
                {
                    foreach (ModelQuadTreeNode child in children)
                    {
                        child.drawNodes(gameTime, view, projection);
                    }
                }
            }
        }

        public void resetNodes()
        {
            isActive = false;
            if (children != null)
                for (byte i = 0; i < NODE_CNT; i++)
                    children[i].resetNodes();
        }
 
        public void update(GameTime gameTime)
        {

        }

        public void checkIfInView(BoundingFrustum viewFrustrum)
        {
            resetNodes();
            if (viewFrustrum.Contains(area) != ContainmentType.Disjoint)
            {
                isActive = true;
                if (children != null)
                {
                    for (byte i = 0; i < NODE_CNT; i++)
                    {
                        children[i].checkIfInView(viewFrustrum);
                    }
                }
            }
        }

        private void split()
        {
            if (children == null)
            {
                children = new ModelQuadTreeNode[NODE_CNT];
                for (byte i = 0; i < NODE_CNT; i++)
                {
                    children[i] = 
                        new ModelQuadTreeNode(this, new BoundingBox());
                }
                setArea(area);
            }
        }

        private void setArea(BoundingBox Area)
        {
            area = Area;
            //Set Childrens Areas
            if ( children != null)
                for (byte i = 0; i < NODE_CNT; i++)
                {
                    children[i].setArea(calcChildArea(Area, i));
                }
        }

        private ModelQuadTreeNode FindHome(BoundingBox Area)
        {
            if (area.Contains(Area) != ContainmentType.Disjoint)
            {
                return this;
            }
            else
            {
                if (children != null)
                {
                    for (byte i = 0; i < NODE_CNT; i++)
                    {
                        if (children[i].area.Contains(Area) != ContainmentType.Disjoint)
                        {
                            return FindHome(Area);
                        }
                    }

                }
            }
            return this;
        }

        private BoundingBox calcChildArea(BoundingBox Area, byte ChildIndex)
        {
            float width = Math.Abs(Area.Max.X - Area.Min.X) / 2;
            float height = Math.Abs(Area.Max.Y - Area.Min.Y) / 2;
            float xOffset;
            float zOffset;
            BoundingBox tmpBox = new BoundingBox();
            switch (ChildIndex)
            {
                case (0):
                    xOffset = Area.Min.X;
                    zOffset = Area.Min.Y;
                    tmpBox.Min = new Vector3(xOffset, zOffset, -Y_SCALE);
                    tmpBox.Max = new Vector3(xOffset + width, zOffset + height, Y_SCALE);
                    return tmpBox;
                case (1):
                    xOffset = Area.Min.X + width;
                    zOffset = Area.Min.Y;
                    tmpBox.Min = new Vector3(xOffset, zOffset, -Y_SCALE);
                    tmpBox.Max = new Vector3(xOffset + width, zOffset + height, Y_SCALE);
                    return tmpBox;
                case (2):
                    xOffset = Area.Min.X;
                    zOffset = Area.Min.Y + height;
                    tmpBox.Min = new Vector3(xOffset, zOffset, -Y_SCALE);
                    tmpBox.Max = new Vector3(xOffset + width, zOffset + height, Y_SCALE);
                    return tmpBox;
                case (3):
                    xOffset = Area.Min.X + width;
                    zOffset = Area.Min.Y + height;
                    tmpBox.Min = new Vector3(xOffset, zOffset, -Y_SCALE);
                    tmpBox.Max = new Vector3(xOffset + width, zOffset + height, Y_SCALE);
                    return tmpBox;
                   
            }
            return tmpBox;
        }

        private void addModel(StaticModel model)
        {
            if (models == null)
                models = new List<StaticModel>();
            models.Add(model);
        }

        private void loadModels()
        {
            foreach (StaticModel model in models)
            {
                model.LoadModel();
            }
        }

        public void unload()
        {
            if (children != null)
            {
                for (byte i = 0; i < NODE_CNT; i++)
                {
                    children[i].unload();
                }
            }
        }
        private void unloadModels()
        {
            foreach (StaticModel model in models)
                model.UnloadModel();
        }

        private void removeModel(StaticModel Model)
        {
            foreach (StaticModel model in models)
            {
                if (Model == model)
                {
                    model.UnloadModel();
                    models.Remove(model);
                }
            }
        }
    }
}
