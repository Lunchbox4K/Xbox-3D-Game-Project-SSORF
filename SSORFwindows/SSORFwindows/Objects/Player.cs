using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//This class needs an array of scooters and the ability to add scooters to the players
//list of purchased scooters or buy upgrades if they have enough money.
namespace SSORF.Objects
{
    
    class Player
    {

        //Needs to be an array of all the vehicles owned
        private Vehicle scooter = new Vehicle();

        //private int money = 0;

        //private bool[] missionsCompleted;

        //Accessor for player scooter so we can pass it along to the mission
        public Vehicle Scooter { get {return scooter; } }

    }
}
