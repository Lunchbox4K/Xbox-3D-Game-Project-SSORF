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
    public enum ChildNodeLocation
    {
        UpperLeft = 0,
        UpperRight,
        LowerLeft,
        LowerRight
    }

    public class ModelQuadTreeNode
    {
        //--------------------------------------------------------------------------------------------------------------------------------
        //  CONSTANTS
        //--------------------------------------------------------------------------------------------------------------------------------
        public const float Y_SCALE = 200f;
        public const byte NODE_CNT = 4;
        public const byte MAX_DEPTH = 6;

        private const bool DEBUG_MODE = true;

        //--------------------------------------------------------------------------------------------------------------------------------
        //  Properties
        //--------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        private int depth;
        private ModelQuadTreeNode parent;
        private List<ModelQuadTreeNode> children;
        private List<StaticModel> models;
        private static List<InstancedModel> instancedModels;
        private List<List<Matrix>> modelInstances;

        private BoundingBox area;
        private bool isActive;

        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------
        //  Constructor Content Loading
        //--------------------------------------------------------------------------------------------------------------------------------
        #region Constructor / Content Loading

        public ModelQuadTreeNode(ModelQuadTreeNode Parent, BoundingBox Area)
        {
            if (Parent == null)
                throw new ArgumentNullException("Parent cant be null!");
            parent = Parent;
            isActive = false;
            children = null;
            models = null;
            area = Area;
            depth = parent.Depth + 1;
           
        }

        public ModelQuadTreeNode(BoundingBox StartingArea)
        {
            parent = null;
            depth = 0;
            isActive = false;
            children = null;
            models = null;
            area = StartingArea;
            //Since we only want to check on a 2D plane we will make the Y space
            //very large so evrything will be inside.
            area.Max.Y = Y_SCALE;
            area.Min.Y = -Y_SCALE;

            setChildrenNodes();
        }

        #endregion
        //--------------------------------------------------------------------------------------------------------------------------------
        //  Public Functions
        //--------------------------------------------------------------------------------------------------------------------------------
        #region Public Functions

        public void pushStaticModel(StaticModel model)
        {
            if (!model.IsLoaded)
            {
                //Load model if not loaded
                try
                {
                    model.LoadModel();
                }
                catch
                {
                    throw new Exception("Failed to load model");
                }
            }
            findHome(model.GetBoundingSphere).addStaticModel(model);
        }

        public void pushInstancedModel(StaticModel model)
        {
            if (!model.IsLoaded)
            {
                try
                {
                    model.LoadModel();
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }            
            }
            this.addInstancedModel((InstancedModel)model);
        }

        public void pushModelInstanceByID(Matrix transform, int modelID)
        {
            //Locate Model
            int modelIndex = 0;
            for (int i = 0; i < instancedModels.Count; i++ )
                if (instancedModels[i].ID == modelID)
                {
                    modelIndex = i;
                    pushModelInstanceByIndex(transform, modelIndex);
                    break;
                }
        }

        public void pushModelInstanceByIndex(Matrix transform, int index)
        {
            if (index >= instancedModels.Count)
                throw new IndexOutOfRangeException
                    ("Index out of range of total Instanced Models in this Node!");
            //Find Home and Add Instance to it
            findHome(instancedModels[index].GetBoundingSphereTransform(transform)).
                addModelInstance(transform, index);
        }

        public void resetActiveNodes()
        {
            if (isActive)
            {
                isActive = false;

            }
            if (children != null)
            {
                for (byte i = 0; i < NODE_CNT; i++)
                {
                    children[i].resetActiveNodes();
                }
            }
        }

        public void unLoad()
        {
            if (children != null)
                for (byte i = 0; i < NODE_CNT; i++)
                    children[i].unLoad();
            foreach (StaticModel model in models)
                model.UnloadModel();
            models = null;
            children = null;
        }

        public void setVisibleNodesActive(BoundingFrustum view)
        {
            if (view.Contains(area) != ContainmentType.Disjoint)
            {
                isActive = true;
                if (children != null)
                    for (byte i = 0; i < NODE_CNT; i++)
                        children[i].setVisibleNodesActive(view);
            }
            else
            {
                isActive = false;
            }
        }

        public void drawNodes(GameTime gameTime, GraphicsDevice graphics, Matrix view, Matrix projection)
        {
            if (isActive)
            {
                drawStaticModels(gameTime, view, projection);
                drawInstancedModels(gameTime, graphics, view, projection);
                if (children != null)
                    for (byte i = 0; i < NODE_CNT; i++)
                        children[i].drawNodes(gameTime, graphics, view, projection);
            }
        }

        private void drawStaticModels(GameTime gameTime, Matrix view, Matrix projection)
        {
            if (models != null)
            {
                for (byte i = 0; i < models.Count; i++)
                {
                    models[i].drawModel(gameTime, view, projection);
                }
            }
        }

        private void drawInstancedModels(GameTime gameTime, GraphicsDevice graphics, Matrix view, Matrix projection)
        {
            if (instancedModels != null && modelInstances != null)
                for (byte i = 0; i < instancedModels.Count; i++)
                    if (modelInstances[i] != null && modelInstances[i].Count > 0)
                    {
                        if (instancedModels[i].Locations != modelInstances[i])
                            instancedModels[i].Locations = modelInstances[i];
                        instancedModels[i].draw(gameTime, graphics, view, projection);
                    }

        }

        #endregion
        //--------------------------------------------------------------------------------------------------------------------------------
        //  Private Helper Functions
        //--------------------------------------------------------------------------------------------------------------------------------
        #region Private Helpers

        public ModelQuadTreeNode findHome(BoundingSphere modelSphere)
        {
            if (area.Contains(modelSphere) == ContainmentType.Contains)
            {
                if (children != null)
                    for (byte i = 0; i < NODE_CNT; i++)
                        if (children[i].BoundingBox.Contains(modelSphere)
                            != ContainmentType.Disjoint)
                            return children[i].findHome(modelSphere);
                return this;
            }
            else if (area.Contains(modelSphere) == ContainmentType.Intersects)
            {
                if (children != null)
                    for (byte i = 0; i < NODE_CNT; i++)
                        if (children[i].BoundingBox.Contains(modelSphere)
                            != ContainmentType.Disjoint)
                            return children[i].findHome(modelSphere);
                return this;
            }
            else
            {
                if (parent != null)
                    return parent;
                else
                    return this;
            }
        }

        private void setChildrenNodes()
        {
            if (depth < MAX_DEPTH && children == null)
            {
                splitNode();
                for (byte i = 0; i < NODE_CNT; i++)
                {
                    children[i].setChildrenNodes();
                }
            }
        }

        private void splitNode()
        {
            if (children != null)
                throw new NullReferenceException("");
            children = new List<ModelQuadTreeNode>();
            for(byte i = 0; i < NODE_CNT; i++)
            {
                BoundingBox area = calcChildsArea(i);
                children.Add(new ModelQuadTreeNode(this,area));
            }
        }

        private BoundingBox calcChildsArea(byte index)
        {
            //Check 
            if (children == null)
                throw new
                    NullReferenceException("Children cant be null!");
            //Size
            float childWidth = (Math.Abs(area.Max.X - area.Min.X) / 2);
            float childHeight = (Math.Abs(area.Max.Z - area.Min.Z) / 2);
            switch (index)
            {
                case 0:
                    return new BoundingBox(
                        new Vector3(area.Min.X, -Y_SCALE, area.Min.Z),
                        new Vector3(area.Min.X + childWidth, Y_SCALE, area.Min.Z + childHeight));
                case 1:
                    return new BoundingBox(
                        new Vector3(area.Min.X + childWidth, -Y_SCALE, area.Min.Z),
                        new Vector3(area.Max.X, Y_SCALE, area.Min.Z + childHeight));
                case 2:
                    return new BoundingBox(
                        new Vector3(area.Min.X, -Y_SCALE, area.Min.Z + childHeight),
                        new Vector3(area.Min.X + childWidth, Y_SCALE, area.Max.Z));
                case 3:
                    return new BoundingBox(
                        new Vector3(area.Min.X + childWidth, -Y_SCALE, area.Min.Z + childHeight),
                        new Vector3(area.Max.X, Y_SCALE, area.Max.Z));
                default:
                    return new BoundingBox(
                        new Vector3(area.Min.X, -Y_SCALE, area.Min.Z),
                        new Vector3(area.Max.X, Y_SCALE, area.Max.Z));
            }
        }

        private void addStaticModel(StaticModel model)
        {
            if (models == null)
                models = new List<StaticModel>();
            models.Add(model);
        }

        private void addInstancedModel(InstancedModel model)
        {
            if (instancedModels == null)
                instancedModels = new List<InstancedModel>();
            instancedModels.Add(model);
        }

        private void addModelInstance(Matrix instance, int index)
        {
            //Check Index Range
            if (index >= instancedModels.Count)
                throw new IndexOutOfRangeException("Index out of range!");
            if (modelInstances == null)
                modelInstances = new List<List<Matrix>>();
            while (index >= modelInstances.Count)
                modelInstances.Add(new List<Matrix>());
            modelInstances[index].Add(instance); 
        }
        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------
        //  Accessors / Mutators
        //--------------------------------------------------------------------------------------------------------------------------------
        #region Accessors/Mutators

        public StaticModel getStaticModel(int index)
        {
            if (models != null && models.Count > index)
            {
                return models[index];
            }
            return null;
        }

        public int Depth
        {
            get { return depth; }
        }

        public BoundingBox BoundingBox
        {
            get { return area; }
        }

        #endregion
    }


    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ModelQuadTree
    {
    //--------------------------------------------------------------------------------------------------------------------------------
    //  Properties
    //--------------------------------------------------------------------------------------------------------------------------------
    #region Properies

        private ModelQuadTreeNode rootNode = null;
        private byte updateRate;
        private byte updateLoop;
        private Game rootGame;

    #endregion


    //--------------------------------------------------------------------------------------------------------------------------------
    //  Constructor and Loading/Unloading
    //--------------------------------------------------------------------------------------------------------------------------------
    #region Loading/Unloading

        public ModelQuadTree(Game game, BoundingBox area, byte UpdateRate)
        {
            rootGame = game;
            updateRate = UpdateRate;
            updateLoop = 0;
            rootNode = new
                ModelQuadTreeNode(area);
        }
        public void unLoad()
        {
            rootNode.unLoad(); //Unloads All
        }

    #endregion
    //--------------------------------------------------------------------------------------------------------------------------------
    //  Add/Remove Model Functions
    //--------------------------------------------------------------------------------------------------------------------------------
    #region Add/Remove Models
        public void addStaticModel(StaticModel model)
        {
            rootNode.pushStaticModel(model);
        }
        public void addInstancedModel(StaticModel model)
        {
            rootNode.pushInstancedModel(model);
        }
        public void addInstanceByID(Matrix Transform, int ID)
        {
            rootNode.pushModelInstanceByID(Transform, ID);
        }
    #endregion
    //--------------------------------------------------------------------------------------------------------------------------------
    //  Update View / Draw Functions
    //--------------------------------------------------------------------------------------------------------------------------------
    #region Update / Draw
        public void UpdateView(GameTime gameTime, BoundingFrustum ViewArea)
        {
            if (updateLoop == updateRate)
            {
                rootNode.setVisibleNodesActive(ViewArea);
                updateLoop = 0;
            }
            else
                updateLoop++;
        }

        public void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            rootNode.drawNodes(gameTime, rootGame.GraphicsDevice, view, projection);
        }
    #endregion
    }
}
