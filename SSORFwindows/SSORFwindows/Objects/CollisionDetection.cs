using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace SSORF.Objects
{

    class CollisionDetection
    {
    //----------------------------------------------------------------------------------
    // 
    //----------------------------------------------------------------------------------
    #region Members     

        //Static Collision Data
        private List<int> static_IDs;
        private List<BoundingSphere> static_bSphere;
        //private List<BoundingSphere[]> static_meshSpheres;
        //private List<Vector3> static_velocity;

        //Instanced Collision
        private List<int> inst_IDs;
        private List<BoundingSphere> inst_baseBSphere;
        //private List<BoundingSphere[]> inst_baseMeshSphere;
        private List<List<Vector3>> inst_locations;
        //private List<List<Vector3>> inst_velocity;

        private List<BoundingSphere> player_bSphere;
        //private List<BoundingSphere[]> player_mSpheres;
        private List<Vector3> player_Velocity;

        //Collision Set
        private List<Collision> collisions;

        //Threading
        private int sleepTime = 10;
        private Thread collisionDataThread = null;
        private volatile bool isStopping = false;
        private object collisionDataLock = new object();

        #endregion 

    //----------------------------------------------------------------------------------
    // 
    //----------------------------------------------------------------------------------
    #region Constructor

        public CollisionDetection()
        {
            static_IDs = new List<int>();
            static_bSphere = new List<BoundingSphere>();
            //static_meshSpheres = new List<BoundingSphere[]>();

            inst_IDs = new List<int>();
            inst_baseBSphere = new List<BoundingSphere>();
            //inst_baseMeshSphere = new List<BoundingSphere[]>();
            inst_locations = new List<List<Vector3>>();

            collisions = new List<Collision>();
        }

    #endregion

    //----------------------------------------------------------------------------------
    // Update/Add Model Spheres
    //----------------------------------------------------------------------------------
    #region Update

        public void setModels(List<StaticModel> staticModelCollection, 
            List<StaticModel> instanceModelCollection, List<List<Matrix>> modelInstanceList)
        {
            lock (collisionDataLock) //Waits for thread to unlock
            {
                //Set Static Model Collection
                    setStaticModels(staticModelCollection);
                //Set Instanced Model Collection
                    setInstancedModels(instanceModelCollection);
                //Set Instanced Model Location Collection
                    setModelInstances(modelInstanceList);           
            }
        }

        private void setStaticModels(List<StaticModel> staticModels)
        {
            //Check List Length
            if (staticModels == null)
            {
                static_IDs = null;
                static_bSphere = null;
                //static_velocity = null;
                return;
            }
            if (static_IDs == null || static_IDs.Count != staticModels.Count)
            {
                static_IDs = new List<int>();
                static_bSphere = new List<BoundingSphere>();
                //static_velocity = new List<Vector3>();
                for (int i = 0; i < staticModels.Count; i++)
                {
                    static_IDs.Add(staticModels[i].ID);
                    static_bSphere.Add(staticModels[i].GetBoundingSphere);
                    //static_velocity.Add(staticModels[i].Velocity);
                }
            }
            else
            {
                for (int i = 0; i < static_bSphere.Count; i++)
                {
                    BoundingSphere tmp = staticModels[i].GetBoundingSphere;
                    static_bSphere[i] = new BoundingSphere(tmp.Center, tmp.Radius);
                    //static_velocity[i] = staticModels[i].Velocity;
                }
            }
        }

        private void setInstancedModels(List<StaticModel> instancedModels)
        {
            if (instancedModels == null)
            {
                inst_IDs = null;
                inst_baseBSphere = null;
                return;
            }
            //Check List Length
            if (inst_IDs == null || inst_IDs.Count != instancedModels.Count)
            {
                inst_IDs = new List<int>();
                inst_baseBSphere = new List<BoundingSphere>();
                for (int i = 0; i < instancedModels.Count; i++)
                {
                    inst_IDs.Add(instancedModels[i].ID);
                    inst_baseBSphere.Add(instancedModels[i].GetBoundingSphere);
                }
            }
            else
            {
                for (int i = 0; i < inst_baseBSphere.Count; i++)
                {
                    BoundingSphere tmp = instancedModels[i].GetBoundingSphere;
                    inst_baseBSphere[i] = new BoundingSphere(tmp.Center, tmp.Radius);
                }
            }
        }

        private void setModelInstances(List<List<Matrix>> InstanceLocations)
        {
            if (InstanceLocations == null)
            {
                inst_locations = null;
                //inst_velocity = null;
                return;
            } 
            inst_locations = new List<List<Vector3>>();
            for (int i = 0; i < InstanceLocations.Count; i++)
            {
                inst_locations.Add(new List<Vector3>());
                for (int j = 0; j < InstanceLocations[i].Count; j++)
                {
                    Vector3 tmpVect = new Vector3();
                    tmpVect.X = InstanceLocations[i][j].M41;
                    tmpVect.Y = InstanceLocations[i][j].M42;
                    tmpVect.Z = InstanceLocations[i][j].M43;
                    inst_locations[i].Add(tmpVect);
                }
            }
        }

        public void setPlayerModels(List<StaticModel> playerModels)
        {
            //Check List Length
            if (playerModels == null)
            {
                player_bSphere = null;
                player_Velocity = null;
                return;
            }
            if (player_bSphere == null || player_bSphere.Count != playerModels.Count)
            {
                player_bSphere = new List<BoundingSphere>();
                player_Velocity = new List<Vector3>();
                for (int i = 0; i < playerModels.Count; i++)
                {
                    player_bSphere.Add(playerModels[i].GetBoundingSphere);
                    player_Velocity.Add(playerModels[i].Velocity);
                }
            }
            else
            {
                for (int i = 0; i < playerModels.Count; i++)
                {
                    BoundingSphere tmp = new BoundingSphere();
                    tmp.Radius = playerModels[i].GetBoundingSphere.Radius;
                    tmp.Center = playerModels[i].GetBoundingSphere.Center;
                    tmp.Center += playerModels[i].Location;
                    player_bSphere[i] = tmp;
                    player_Velocity[i] = playerModels[i].Velocity;
                }
            }
        }

    #endregion

        //----------------------------------------------------------------------------------
        // Update/Add Model Spheres
        //----------------------------------------------------------------------------------
        #region Threading


        public void threadedUpdate()                                                                                                                                                     
        {
#if XBOX
            collisionDataThread.SetProcessorAffinity(3);
#endif
            while (!isStopping)
            {
                lock (collisionDataLock) //Data lock for threading this function
                {
                    //===
                    // Update Collision Collection
                    //===
                    collisions = null;
                    collisions = new List<Collision>();
                    //For each Static model compare collisions
                    if (player_bSphere != null && player_bSphere.Count > 0)
                        for (int i = 0; i < player_bSphere.Count; i++)
                        {
                            //Compare with static Models
                            for (int j = 0; j < static_IDs.Count; j++)
                            {
                                if (static_bSphere[i].Intersects(static_bSphere[j]))
                                {
                                    Collision tmpCollision = new Collision();
                                    tmpCollision.modelA_ID = i;
                                    tmpCollision.modelB_ID = static_IDs[j];
                                    tmpCollision.player1Location = player_bSphere[0].Center;
                                    collisions.Add(tmpCollision);
                                }
                            }
                            //Compare with instanced models
                            if (inst_IDs != null && inst_IDs.Count > 0)
                            {
                                for (int j = 0; j < inst_IDs.Count; j++)
                                {
                                    //Compare with each instanced model transform
                                    if ( inst_locations[j] != null || inst_locations[j].Count > 0)
                                        for (int t = 0; t < inst_locations[j].Count; t++)
                                        {
                                            BoundingSphere sphereB =
                                                CalcNewBoundingSphereLocation(inst_baseBSphere[j], inst_locations[j][t]);
                                            if (static_bSphere[i].Intersects(sphereB))
                                            {
                                                Collision tmpCollision = new Collision();
                                                tmpCollision.modelA_ID = i;
                                                tmpCollision.modelB_ID = inst_IDs[j];
                                                tmpCollision.player1Location = player_bSphere[0].Center;
                                                collisions.Add(tmpCollision);
                                            }
                                        }
                                }
                            }
                        }
                }
                Thread.Sleep(sleepTime);
            }
        }

        public void start()
        {
            collisionDataThread = new Thread(threadedUpdate);

            collisionDataThread.Start();
        }

        public void stop()
        {
            try
            {
                isStopping = true;
                if (collisionDataThread != null)
                {
                    collisionDataThread.Join();
                    collisionDataThread = null;
                }
            }
            finally
            {
                isStopping = false;
            }
        }

    #endregion

        public static BoundingSphere CalcNewBoundingSphereLocation(BoundingSphere sphere, Vector3 location)
        {
            BoundingSphere returnable = new BoundingSphere();
            returnable.Radius = sphere.Radius;
            returnable.Center.X = location.X;
            returnable.Center.Y = location.Y;
            returnable.Center.Z = location.Z;
            return returnable;
        }

        public Collision[] waitToGetCollisions
        {
            get
            {
                lock (collisionDataLock)
                {
                    return collisions.ToArray();
                }
            }
        }
    }


    public struct Collision
    {
        public int modelA_ID;
        public int modelB_ID;
        public Vector3 player1Location;
    }
}
