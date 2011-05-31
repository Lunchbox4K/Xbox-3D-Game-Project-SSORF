using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SSORFlibrary;


namespace SSORF.Objects
{
    /// <summary>
    /// This class contains all the models and effects for the level.
    /// Including:      Terrain
    ///                 Instanced Models
    ///                 Static Models
    ///                 *Draw Tree
    /// 
    /// Not Including:  Player Models
    ///                 Effect Models (Like Fire)
    /// </summary>
    public class Level
    {

    //-----------------------------------------------------------
    // Members (Properties)
    //-----------------------------------------------------------
    #region Members
        private Game m_rootGame;
        private SSORFlibrary.LevelLayout m_properties;

        private List<StaticModel> m_staticModels;
        public List<StaticModel> StaticModels
        {
            get { return m_staticModels; }
        }

        private List<StaticModel> m_instancedModels;
        public List<StaticModel> InstancedModels
        {
            get { return m_instancedModels; }
        }

        private List<List<Matrix>> m_modelInstances;
        public List<List<Matrix>> ModelInstances
        {
            get { return m_modelInstances; }
        }

        private CheckPoint m_checkPoints;
        /// <summary>
        /// Returned as Static Model. 
        /// Use  (SSORF.Objects.CheckPoint) to cast
        /// </summary>
        public StaticModel getCheckpoints
        {
            get
            {
                return (StaticModel)m_checkPoints;
            }
        }

        private Terrain m_terrain;

        private LocationMap m_locationMap;
        public LocationMap getLocationMap
        {
            get
            {
                return m_locationMap;
            }
        }

        private ModelQuadTree m_drawTree;

        private Ray m_playerSpawns;
        public Ray getSpawns
        {
            get { return m_playerSpawns; }
        }

    #endregion

    //-----------------------------------------------------------
    // Constructor and Content Loading
    //-----------------------------------------------------------
    #region Constructor/Content 

        public  Level(Game game, LevelLayout Properties)
        {
            m_rootGame = game;
            m_properties = Properties.Copy(true); //Deep Copy
            m_staticModels = new List<StaticModel>();
            m_instancedModels = new List<StaticModel>();
            m_modelInstances = new List<List<Matrix>>();
            m_terrain = new Terrain(game.GraphicsDevice, game.Content);
        }


        public void LoadContent()
        {
            m_locationMap =
                m_rootGame.Content.Load<LocationMap>(m_properties.locationMap);
            //Load Terrain
            m_terrain.LoadModel(m_properties.level_heightMap, 
                m_properties.level_textureMap, 
                m_properties.level_textureR, 
                m_properties.level_textureG, 
                m_properties.level_textureB);

            //Load Terrain Draw Effect
            m_terrain.LoadShaders();
            float halfWidth = m_terrain.terrainInfo.HeightmapWidth/2;
            m_drawTree = new ModelQuadTree(m_rootGame,
                new BoundingBox(
                    new Vector3(-halfWidth, 0, -halfWidth),
                    new Vector3( halfWidth, 0,  halfWidth)), 
                m_properties.viewTree_refreshRate);

            //Set Player Spawn
            for (int y = 0; y < m_locationMap.Color.Length; y++)
                for (int x = 0; x < m_locationMap.Color[y].Length; x++)
                    if (m_locationMap.Color[x][y].X == m_properties.playerSpawns)
                    {
                        if (m_locationMap.Color[x][y].Y == 2)
                        {
                            m_playerSpawns = new Ray(new Vector3(x, 0, y), Vector3.Forward);
                            m_playerSpawns.Position.X *= m_locationMap.scale;
                            m_playerSpawns.Position.X -= ((m_locationMap.Color.Length * m_locationMap.scale) / 2);
                            m_playerSpawns.Position.Z *= m_locationMap.scale;
                            m_playerSpawns.Position.Z -= ((m_locationMap.Color.Length * m_locationMap.scale) / 2);
                            if (m_terrain.terrainInfo.IsOnHeightmap(m_playerSpawns.Position))
                            {
                                float height;
                                Vector3 normal;
                                m_terrain.terrainInfo.GetHeightAndNormal
                                    (m_playerSpawns.Position, out height, out normal);
                                m_playerSpawns.Position.Y = height + 10;
                            }
                            else
                            {
                                m_playerSpawns.Position.Y = 0;
                            }
                            y = m_locationMap.Color.Length - 1;
                            x = m_locationMap.Color[0].Length - 1;
                        }
                        else if (m_locationMap.Color[x][y].Y == 3)
                        {
                            m_playerSpawns = new Ray(new Vector3(x, 0, y), Vector3.Forward);
                            m_playerSpawns.Position.X *= m_locationMap.scale;
                            m_playerSpawns.Position.X -= ((m_locationMap.Color.Length * m_locationMap.scale) / 2);
                            m_playerSpawns.Position.Z *= m_locationMap.scale;
                            m_playerSpawns.Position.Z -= ((m_locationMap.Color.Length * m_locationMap.scale) / 2);
                        }
                    }
            m_playerSpawns.Direction = Vector3.Subtract(m_playerSpawns.Position, m_playerSpawns.Direction);

            //Load Check Point Locations
            List<Vector3> gridColors = new List<Vector3>();
            List<Vector3> gridLocations = new List<Vector3>();
            Vector3[] checkPointLocations;
            //Read in all the locations
            for (int y = 0; y < m_locationMap.Color.Length; y++)
                for (int x = 0; x < m_locationMap.Color[y].Length; x++)
                    if (m_locationMap.Color[x][y].X == m_properties.checkpointSpawn)
                    {
                        gridColors.Add(m_locationMap.Color[x][y]);
                        gridLocations.Add(new Vector3(x, 0, y));
                    }
            //Organize the Locations
            checkPointLocations = new Vector3[gridColors.Count];
            byte lastNum = 0;
            for (int i = 0; i < checkPointLocations.Length; i++)
            {
                byte currentNum = 255;
                int index = 0;
                for (int j = 0; j < gridColors.Count; j++)
                {
                    if (gridColors[j].Y < currentNum && gridColors[j].Y > lastNum)
                    {
                        currentNum = (byte)gridColors[i].Y;
                        index = j;
                    }
                }
                lastNum = currentNum;
                
                //Scale Location
                Vector3 tmpLocation = gridLocations[index];

                tmpLocation.X *= m_locationMap.scale;
                tmpLocation.X -= ((m_locationMap.Color.Length * m_locationMap.scale) / 2);
                tmpLocation.Z *= m_locationMap.scale;
                tmpLocation.Z -= ((m_locationMap.Color.Length * m_locationMap.scale) / 2);
                if (m_terrain.terrainInfo.IsOnHeightmap(tmpLocation))
                {
                    float height;
                    Vector3 normal;
                    m_terrain.terrainInfo.GetHeightAndNormal
                        (tmpLocation, out height, out normal);
                    tmpLocation.Y = height + 10;
                }
                else
                {
                    tmpLocation.Y = 0;
                }
                checkPointLocations[i] = tmpLocation;
            }
            

            //Load Check Points
            m_checkPoints = new
                CheckPoint(m_rootGame.Content, m_properties.checkpointAsset, checkPointLocations[0], 1f, Matrix.Identity, m_drawTree);
            for (int i = 1; i < checkPointLocations.Length; i++)
                m_checkPoints.PushCheckPoint(checkPointLocations[i]);
            m_checkPoints.loadCheckpoint();
            m_checkPoints.addToStaticList(ref m_staticModels);

            //Register Check Points w/ View Tree
            //m_checkPoints.registerToTree(m_drawTree);

            //Load Static Models
                float scaledMapSize = m_locationMap.Color.Length * m_locationMap.scale;
                float xOffset = scaledMapSize / 2;
                float zOffSet = scaledMapSize / 2;
            if (m_properties.statics_models != null)
            {
                Vector3 offset = Vector3.Zero;
                int loopit = 0;
                for (int i = 0; i < m_properties.statics_models.Count; i++)
                {
                    loopit = i;
                    for (int x = 0; x < m_locationMap.Color.Length; x++)
                        for (int y = 0; y < m_locationMap.Color[x].Length; y++)
                            if (m_locationMap.Color[y][x].X == m_properties.statics_models[i].asset_colorID)
                            {
                                StaticModel model;
                                Vector3 location = Vector3.Zero;
                                Vector3 normal;
                                location.X = x * m_locationMap.scale - xOffset;
                                location.Z = y * m_locationMap.scale - zOffSet;
                                location.Y = 0;
                                if (m_terrain.terrainInfo.IsOnHeightmap(location))
                                    m_terrain.terrainInfo.GetHeightAndNormal(location, out location.Y, out normal);
                                model = new StaticModel(m_rootGame.Content, m_properties.statics_models[i].asset_location,
                                    location, Matrix.Identity, 1f);
                                //Add each model instance to a List<>
                                m_staticModels.Add(model);
                                m_drawTree.addStaticModel(m_staticModels[loopit]);
                                loopit++;
                            }
                    loopit++;
                }
            }
            if (m_properties.instanced_models != null)
            {
                //Load Instanced Models
                for (int i = 0; i < m_properties.instanced_models.Count; i++)
                {
                    InstancedModel model =
                        new InstancedModel(m_rootGame.Content, m_properties.instanced_models[i].asset_location, 1f, Matrix.Identity);
                    //Add each Instanced Model to a List<>
                    model.LoadModel();
                    m_instancedModels.Add(model);
                    m_drawTree.addInstancedModel(m_instancedModels[i]);
                }
                //Load Instanced Model Locations

                for (int i = 0; i < m_properties.instanced_models.Count; i++)
                {
                    m_modelInstances.Add(new List<Matrix>()); //Add Container for each model
                    for (int x = 0; x < m_locationMap.Color.Length; x++)
                        for (int z = 0; z < m_locationMap.Color[x].Length; z++)
                            if (m_locationMap.Color[z][x].X == m_properties.instanced_models[i].asset_colorID)
                            {
                                Matrix transform = Matrix.Identity;
                                Vector3 location = Vector3.Zero;
                                Vector3 normal = Vector3.Zero;
                                //Scale and Rotate the Object
                                Matrix scale;
                                Matrix.CreateScale(.25f, out scale);
                                Matrix rotation = Matrix.Identity;
                                Matrix.Multiply(ref scale, ref rotation, out transform);
                                location.X = x * m_locationMap.scale - xOffset;
                                location.Z = z * m_locationMap.scale - zOffSet;
                                if (m_terrain.terrainInfo.IsOnHeightmap(location))
                                    m_terrain.terrainInfo.GetHeightAndNormal(location, out location.Y, out normal);
                                //41, 42, 43 -> X, Y, Z Transform
                                transform.M41 = x * m_locationMap.scale - xOffset;
                                transform.M42 = location.Y - 2;
                                transform.M43 = z * m_locationMap.scale - zOffSet;
                                //Add each Matrix Transfrom to a List<>
                                m_modelInstances[i].Add(transform);
                                m_drawTree.addInstanceByID(transform, m_instancedModels[i].ID);
                            }
                }
            }
        }

        public void unload()
        {
            //Unload List
            //-------------
            //  Terrain
            //  Static Models
            //  Instanced Models
            //  Checkpoint Models
            m_terrain.UnloadModel();
            foreach (StaticModel model in m_staticModels)
                model.UnloadModel();
            foreach (InstancedModel model in m_instancedModels)
                model.UnloadModel();
            m_checkPoints.unloadCheckpoint();

        }

        public void disableStaticAsset(int modelID)
        {
            for (int i = 0; i < m_staticModels.Count; i++)
                if (m_staticModels[i].ID == modelID)
                {
                    m_staticModels[i].IsEnabled = false;
                    break;
                }
        }
        public void enableStaticAsset(int modelID)
        {
            for (int i = 0; i < m_staticModels.Count; i++)
                if (m_staticModels[i].ID == modelID)
                {
                    m_staticModels[i].IsEnabled = true;
                    break;
                }
        }

        public void enableCheckpoints()
        {
            //
            //
            //
        }

    #endregion

    //-----------------------------------------------------------
    // Update Draw Functions
    //-----------------------------------------------------------
    #region Update/Draw

        public void update(GameTime gameTime, Matrix View, Matrix Projection)
        {
            m_drawTree.UpdateView(gameTime, new BoundingFrustum(View * Projection));
        }



        public void draw(GameTime gameTime, SpriteBatch spriteBatch, Matrix View, Matrix Projection)
        {
            m_terrain.drawLevel(gameTime, View, Projection);
            m_drawTree.Draw(gameTime, View, Projection);
        }

    #endregion


    //-----------------------------------------------------------
    // Accessors and Mutators (Get & Set the Members)
    //-----------------------------------------------------------
    #region Get/Set

        public TerrainInfo TerrainCollision{get { return m_terrain.terrainInfo; } }

        //public 

    #endregion
    }
}
