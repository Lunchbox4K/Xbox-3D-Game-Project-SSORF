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

namespace SSORFlibrary
{
    /// <summary>
    /// Returned Struct from LocationMap Pipeline
    /// Contains a 2D array of Colors held in 
    /// Vector3 Types. Along with a scale for
    /// the X,Z coords.
    /// </summary>
    /// 
    public struct LocationMap
    {
        // X - Model ID ??
        // Y - 
        // Z - 
        public Vector3[][] Color;
        public float scale;
        /// <summary>
        /// Shallow/Deep Copy Constructor
        /// </summary>
        /// <param name="deep">True = Deep Copy</param>
        /// <returns>Shallow returns this, and deep returns New LocationMap</returns>
        public LocationMap Copy(bool deep)
        {
            if (deep)
            {
                LocationMap tmp = new LocationMap();
                tmp.scale = scale;
                tmp.Color = new Vector3[Color.Length][];
                for (int i = 0; i < Color.Length; i++)
                {
                    tmp.Color[i] = new Vector3[Color[i].Length];
                    for (int j = 0; j < Color[i].Length; i++)
                    {
                        tmp.Color[i][j] = Color[i][j];
                    }
                }
                return tmp;
            }
            return this;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct LocationMapAsset
    {
        public string asset_location;
        public float asset_colorID;

        public LocationMapAsset Copy(bool clone)
        {
            if (clone)
            {
                LocationMapAsset tmp = new LocationMapAsset();
                tmp.asset_colorID = asset_colorID;
                tmp.asset_location = asset_location;
                return tmp;
            }
            return this;
        }
    }
}

