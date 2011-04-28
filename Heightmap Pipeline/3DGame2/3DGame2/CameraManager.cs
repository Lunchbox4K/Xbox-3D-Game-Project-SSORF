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
using Microsoft.Xna.Framework.Net;


namespace _3dOnlineGame
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class CameraManager : Microsoft.Xna.Framework.GameComponent
    {
        public const int MAX_LOCAL_PLAYERS = 4;

        #region Properties
        private GraphicsDevice graphicsDevice;
        private Viewport defViewPort;
        private Viewport[] viewPorts = null;
        private Vector3[] cLocation = null;
        private Vector3[] cDirection = null;    
        private Matrix[] cProjection = null;
        private Matrix[] cView = null;
        #endregion

        #region Constructors
        public CameraManager(Game game)
            : base(game)
        {
        }
        public void setGraphicsDevice(GraphicsDevice gdevice)
        {
            graphicsDevice = gdevice;
            defViewPort = gdevice.Viewport;
        }

        #endregion

        #region Initialize / Update


        public override void Initialize()
        {
            

            base.Initialize();
        }
        public override void Update(GameTime gameTime)
        {
            if (LocalNetworkGamer.SignedInGamers.Count > 0)
            {
                if (cView == null || cView.Length < 
                    LocalNetworkGamer.SignedInGamers.Count)
                {
                    setScreenSplits();
                }
                updateView();
            }

            base.Update(gameTime);
        }

        #endregion

        public void setScreenSplits()
        {
            if (LocalNetworkGamer.SignedInGamers.Count > MAX_LOCAL_PLAYERS)
                throw new IndexOutOfRangeException
                    ("Local Player Count Can't be above 4!");
            else
                setScreenSplits(LocalNetworkGamer.SignedInGamers.Count);
        }
        private void setScreenSplits(int localCount)
        {
            //Backup Important Camera Info
            Vector3[] tmpLocation;
            Vector3[] tmpDirection;
            if (cLocation != null)
            {
                tmpLocation = cLocation;
                tmpDirection = cDirection;
                cLocation = new Vector3[localCount];
                cDirection = new Vector3[localCount];
                for (int i = 0; i < tmpLocation.Length; i++)
                {
                    cLocation[i] = tmpLocation[i];
                    cDirection[i] = tmpDirection[i];
                }
                for (int i = tmpLocation.Length; i < localCount; i++)
                {
                    setDefaultCameraLocation(i);
                }
            }
            else
            {
                cLocation = new Vector3[localCount];
                cDirection = new Vector3[localCount];
                for (int i = 0; i < localCount; i++)
                {
                    setDefaultCameraLocation(i);
                }
            }
            cView = new Matrix[localCount];
            cProjection = new Matrix[localCount];
            viewPorts = new Viewport[localCount];

            updateViewPorts(localCount);
            updateView();
            updateProjection();
        }
        private void setDefaultCameraLocation(int index)
        {
            cLocation[index] = new Vector3(0, 1, -5);
                cDirection[index] = Vector3.Forward;
        }
        private void updateViewPorts(int localCount)
        {
            viewPorts = new Viewport[localCount];

            viewPorts[0] = graphicsDevice.Viewport;
            switch (localCount)
            {
                case 1:
                    break;
                case 2:
                    viewPorts[1] = graphicsDevice.Viewport;
                    viewPorts[0].Width /= 2;
                    viewPorts[1].Width /= 2;
                    viewPorts[1].X = viewPorts[1].Width;
                    break;
                case 3:
                    viewPorts[1] = graphicsDevice.Viewport;
                    viewPorts[2] = graphicsDevice.Viewport;
                    //Set Height
                    viewPorts[0].Height /= 2;
                    viewPorts[1].Height /= 2;
                    viewPorts[2].Height /= 2;
                    //Set Bottom Screens Offset
                    viewPorts[1].Y = viewPorts[0].Height;
                    viewPorts[2].Y = viewPorts[0].Height;
                    //Set Bottom Screens Width
                    viewPorts[1].Width /= 2;
                    viewPorts[2].Width /= 2;
                    //Set 3rd Players X Offset
                    viewPorts[1].X = viewPorts[1].Width;

                    break;
                case 4:
                    viewPorts[1] = graphicsDevice.Viewport;
                    viewPorts[2] = graphicsDevice.Viewport;
                    viewPorts[3] = graphicsDevice.Viewport;
                    
                    viewPorts[0].Height /= 2;
                    viewPorts[1].Height /= 2;
                    viewPorts[2].Height /= 2;
                    viewPorts[3].Height /= 2;
                    
                    viewPorts[0].Width /= 2;
                    viewPorts[1].Width /= 2;
                    viewPorts[2].Width /= 2;
                    viewPorts[3].Width /= 2;

                    viewPorts[1].X = viewPorts[0].Width;
                    viewPorts[3].X = viewPorts[2].Width;
                    viewPorts[2].Y = viewPorts[0].Height;
                    viewPorts[3].Y = viewPorts[1].Height;
                    break;
            }
        }
        private void updateView()
        {
            cView = new Matrix[cLocation.Length];

            for (int i = 0; i < cView.Length; i++)
            {
                cView[i] = 
                    Matrix.CreateLookAt(cLocation[i], 
                                        cDirection[i], 
                                        Vector3.Up);
            }
        }
        private void updateProjection()
        {
            float ratio;

            cProjection = new Matrix[cLocation.Length];
            for (int i = 0; i < cProjection.Length; i++)
            {
                ratio = graphicsDevice.Viewport.AspectRatio;
                if (cProjection.Length > 3 || cProjection.Length < 2)
                {
                    cProjection[i] = Matrix.CreatePerspectiveFieldOfView(
                                 MathHelper.PiOver4, ratio, 1.0f, 10000f);     
                }
                else if (cProjection.Length == 2)
                {
                    cProjection[i] = Matrix.CreatePerspectiveFieldOfView(
                                 MathHelper.PiOver4, ratio / 2, 1.0f, 10000f);   
                }
                else if (cProjection.Length == 3)
                {
                    if (i == 1)
                    {
                        cProjection[i] = Matrix.CreatePerspectiveFieldOfView(
                                MathHelper.PiOver4, ratio / 2, 1.0f, 10000f);
                    }
                    else
                    {
                        cProjection[i] = Matrix.CreatePerspectiveFieldOfView(
                                MathHelper.PiOver4, ratio, 1.0f, 10000f);
                    }
                }
            }
        }

        #region Accessors / Mutators

        public Matrix[] cameraView { get { return cView; } }
        public Matrix[] cameraProjection { get{ return cProjection; }  }
        public Vector3[] locations
        {
            get { return cLocation; }
            set { cLocation = value; }
        }
        public Vector3[] Direction
        {
            get { return cDirection; }
            set { cDirection = value; }
        }
        public int LocalplayerCount
        {
            get { return LocalNetworkGamer.SignedInGamers.Count; }
        }
        public Viewport defaultViewPort
        {
            get { return defViewPort; }
        }
        public void setLocation(int index, Vector3 location)
        {
            if (cLocation == null || index >= cLocation.Length)
                throw new IndexOutOfRangeException("Player Index Out of Range");
            cLocation[index] = location;
        }
        public void setDirection(int index, Vector3 direction)
        {
            if (cDirection == null || index >= cDirection.Length)
                throw new IndexOutOfRangeException("Player Index Out of Range");
            cDirection[index] = direction;
        }
        public Vector3 getLocation(int index)
        {
            if (cLocation == null || index >= cLocation.Length)
                throw new IndexOutOfRangeException("Player Index Out of Range");
            return cLocation[index];
        }
        public Vector3 getDirection(int index)
        {
            if (cDirection == null || index >= cDirection.Length)
                throw new IndexOutOfRangeException("Player Index Out of Range");
            return cDirection[index];
        }
        public Matrix getView(int index)
        {
            if (cView == null || index >= cView.Length)
                throw new IndexOutOfRangeException("Player Index Out of Range");
            return cView[index];
        }
        public Matrix getProjection(int index)
        {
            if (cProjection == null || index >= cProjection.Length)
                throw new IndexOutOfRangeException("Player Index Out of Range");
            return cProjection[index];
        }
        public Viewport getViewPort(int index)
        {
            if (viewPorts == null || index >= viewPorts.Length)
                throw new IndexOutOfRangeException("Player Index Out of Range");
            return viewPorts[index];
        }

        #endregion
    }
}
