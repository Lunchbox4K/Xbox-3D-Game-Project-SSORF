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
    public class ScooterData
    {

        public short IDnum;
        public string name;
        public string description1;
        public string description2;
        public string description3;
        public string description4;
        public float outputPower;
        public float brakePower;
        public float weight;
        public short cost;
        public float wheelMaxAngle;
        public float wheelRadius;
        public float wheelBaseLength;
        public float gripRating;
        public float coefficientDrag;
        public float frontalArea;
        public float rollingResistance;

        public void Copy(ScooterData source)
        {
            this.IDnum = source.IDnum;
            this.name = source.name;
            this.description1 = source.description1;
            this.description2 = source.description2;
            this.description3 = source.description3;
            this.description4 = source.description4;
            this.outputPower = source.outputPower;
            this.brakePower = source.brakePower;
            this.weight = source.weight;
            this.cost = source.cost;
            this.wheelMaxAngle = source.wheelMaxAngle;
            this.wheelRadius = source.wheelRadius;
            this.wheelBaseLength = source.wheelBaseLength;
            this.gripRating = source.gripRating;
            this.coefficientDrag = source.coefficientDrag;
            this.frontalArea = source.frontalArea;
            this.rollingResistance = source.rollingResistance;
        }
    }
}
