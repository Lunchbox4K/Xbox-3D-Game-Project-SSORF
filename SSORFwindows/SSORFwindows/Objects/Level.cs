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
        public List<Vector4> m_checkpoints = new List<Vector4>(10);
        private byte checkID = 10;
        private Terrain m_terrain;

        private byte playerID = 0;
        //w = green component = orientation
        public Vector4  playerStart;
        //player blue component = time limit
        public float timelimit = 0;

        private ModelQuadTree m_drawTree;
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
            //Load Terrain
            m_terrain.LoadModel(m_properties.level_heightMap, 
                m_properties.level_textureMap, 
                m_properties.level_textureR, 
                m_properties.level_textureG, 
                m_properties.level_textureB);
            //Load Terrain Draw Effect
            m_terrain.LoadShaders(m_properties.level_effect);
            float halfWidth = m_terrain.terrainInfo.HeightmapWidth/2;
            m_drawTree = new ModelQuadTree(m_rootGame,
                new BoundingBox(
                    new Vector3(-halfWidth, 0, -halfWidth),
                    new Vector3( halfWidth, 0,  halfWidth)), 
                m_properties.viewTree_refreshRate);

            //Load Static Models
            LocationMap instancedLocation =
                m_rootGame.Content.Load<LocationMap>(m_properties.instances_locationMap);
                float scaledMapSize = instancedLocation.Color.Length * instancedLocation.scale;
                float xOffset = scaledMapSize / 2;
                float zOffSet = scaledMapSize / 2;
              for (int x = 0; x < instancedLocation.Color.Length; x++)
                  for (int y = 0; y < instancedLocation.Color[x].Length; y++)
                  {
                      if (instancedLocation.Color[y][x].X == checkID)
                      {
                          Vector3 position = new Vector3(x * instancedLocation.scale - xOffset,
                              0, y * instancedLocation.scale - zOffSet);

                          float height;
                          Vector3 norm;
                          m_terrain.terrainInfo.GetHeightAndNormal(position, out height, out norm);
                          position.Y = height;
                          m_checkpoints.Add(new Vector4(position, instancedLocation.Color[y][x].Y));
                      }
                      else if (instancedLocation.Color[y][x].X == playerID)
                      {
                          Vector3 position = new Vector3(x * instancedLocation.scale - xOffset,
                              0, y * instancedLocation.scale - zOffSet);

                          float height;
                          Vector3 norm;
                          m_terrain.terrainInfo.GetHeightAndNormal(position, out height, out norm);
                          position.Y = height;

                          playerStart = new Vector4(position, instancedLocation.Color[y][x].Y);
                          timelimit = instancedLocation.Color[y][x].Z;
                      }
                  }
            if (m_properties.statics_models != null)
            {
                Vector3 offset = Vector3.Zero;
                int loopit = 0;
                for (int i = 0; i < m_properties.statics_models.Count; i++)
                {
                    loopit = i;
                    for (int x = 0; x < instancedLocation.Color.Length; x++)
                        for (int y = 0; y < instancedLocation.Color[x].Length; y++)
                        {
                            if (instancedLocation.Color[y][x].X == m_properties.statics_models[i].asset_colorID)
                            {
                                StaticModel model;
                                Vector3 location = Vector3.Zero;
                                Vector3 normal;
                                location.X = x * instancedLocation.scale - xOffset;
                                location.Z = y * instancedLocation.scale - zOffSet;
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
                    for (int x = 0; x < instancedLocation.Color.Length; x++)
                        for (int z = 0; z < instancedLocation.Color[x].Length; z++)
                            if (instancedLocation.Color[z][x].X == m_properties.instanced_models[i].asset_colorID)
                            {
                                Matrix transform = Matrix.Identity;
                                Vector3 location = Vector3.Zero;
                                Vector3 normal = Vector3.Zero;
                                //Scale and Rotate the Object
                                Matrix scale;
                                Matrix.CreateScale(.25f, out scale);
                                Matrix rotation = Matrix.Identity;
                                Matrix.Multiply(ref scale, ref rotation, out transform);
                                location.X = x * instancedLocation.scale - xOffset;
                                location.Z = z * instancedLocation.scale - zOffSet;
                                if (m_terrain.terrainInfo.IsOnHeightmap(location))
                                    m_terrain.terrainInfo.GetHeightAndNormal(location, out location.Y, out normal);
                                //41, 42, 43 -> X, Y, Z Transform
                                transform.M41 = x * instancedLocation.scale - xOffset;
                                transform.M42 = location.Y - 2;
                                transform.M43 = z * instancedLocation.scale - zOffSet;
                                //Add each Matrix Transfrom to a List<>
                                m_modelInstances[i].Add(transform);
                                m_drawTree.addInstanceByID(transform, m_instancedModels[i].ID);
                            }
                }
            }
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
