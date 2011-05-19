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
    /// 
    /// </summary>
    public class LevelLayout
    {
        //View Tree Properties
        public byte viewTree_refreshRate;
        public BoundingBox viewTree_area; //Y Coord Will Be Maxed and Mined for detection on all Y Values

        //Terrain Properies
        public string level_heightMap;
        public string level_textureMap;
        public string level_textureR;
        public string level_textureG;
        public string level_textureB;
        public string level_effect;

        //Instanced Model Properties
        public List<LocationMapAsset> statics_models;
        public string statics_locationMap;

        public List<LocationMapAsset> instanced_models;
        public string instances_locationMap;


        /// <summary>
        /// Shallow/Deep Copy Function
        /// </summary>
        /// <param name="deep"></param>
        /// <returns></returns>
        public LevelLayout Copy(bool deep)
        {
            if (deep)
            {
                LevelLayout layout = new LevelLayout();

                //View Tree
                layout.viewTree_area.Max = viewTree_area.Max;
                layout.viewTree_area.Min = viewTree_area.Min;
                layout.viewTree_refreshRate = viewTree_refreshRate;

                //Copy Terrain Properties
                layout.level_heightMap = level_heightMap;
                layout.level_textureMap = level_textureMap;
                layout.level_textureR = level_textureR;
                layout.level_textureG = level_textureG;
                layout.level_textureB = level_textureB;
                layout.level_effect = level_effect;
                //layout.level_centerLocation = level_centerLocation;
                if (statics_models != null)
                {
                    //Copy Static Model Properties
                    layout.statics_locationMap = statics_locationMap;
                    layout.statics_models = new List<LocationMapAsset>();
                    for (int i = 0; i < statics_models.Count; i++)
                        layout.statics_models.Add(statics_models[i].Copy(true));
                }
                //Copy Instanced Model Properties
                if (instanced_models != null)
                {
                    layout.instances_locationMap = instances_locationMap;
                    layout.instanced_models = new List<LocationMapAsset>();
                    for (int i = 0; i < instanced_models.Count; i++)
                        layout.instanced_models.Add(instanced_models[i].Copy(true));
                }
                return layout;
            }
            return this;
        }
    }




}
