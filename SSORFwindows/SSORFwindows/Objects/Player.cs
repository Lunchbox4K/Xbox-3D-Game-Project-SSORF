using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//This class needs an array of scooters and the ability to add scooters to the players
//list of purchased scooters or buy upgrades if they have enough money.
namespace SSORF.Objects
{
     public struct upgradeSpecs
        {
            public float power;
            public float weight;
        }

    class Player
    {
        private int money = 15000;

        //indexes of bool array correspond to scooter ID nums
        private bool[] scootersOwned = new bool[10] 
        {true,false,false,false,false,false,false,false,false,false};
        //Player starts with amigo RTX (ID = 0)

        private bool[,] upgradesPurchased = new bool[10,3];

        //An array of stat changes corresponding to 
        private upgradeSpecs[] scooterUpgrades = new upgradeSpecs[10];

        //Id number of the selected scooter
        private short selectedScooter = 0;

        private bool PurchaseItem(short cost)
        {
            if (cost > money)
                return false;
            else
                money -= cost;

            return true;
        }

        public string PurchaseUpgrade(SSORFlibrary.UpgradeData upgrade)
        {
            //if upgrade already purchased for that scooter return message
            if (upgradesPurchased[selectedScooter,upgrade.IDnum] == true)
                return "This upgrade has already been purchased";
            //if player has insufficent funds return message
            if (PurchaseItem(upgrade.cost) == false)
                return "You don't have enough cash for this upgrade";

            //otherwise add upgrade values to total upgrade specs for that scooter
            scooterUpgrades[selectedScooter].power += upgrade.power;
            scooterUpgrades[selectedScooter].weight += upgrade.weight;
            upgradesPurchased[selectedScooter, upgrade.IDnum] = true;
            return "Upgrade purchased";
        }

        public string PurchaseScooter(SSORFlibrary.ScooterData scooter)
        {
            //if scooter is already owned return message
            if (scootersOwned[scooter.IDnum] == true)
                return "You already own this scooter";
            //if player has insufficent funds return message
            if (PurchaseItem(scooter.cost) == false)
                return "You don't have enough cash to buy this scooter";

            scootersOwned[scooter.IDnum] = true;
            return scooter.name + " was delivered to your garage";
        }


        public short SelectedScooter
        {
            get { return selectedScooter; }
            set { selectedScooter = value; }
        }

        public int Money
        {
            get { return money; }
            set { money = value; }
        }

        public bool[] ScootersOwned
        {
            get { return scootersOwned; }
        }

        public upgradeSpecs[] UpgradeTotals
        {
            get { return scooterUpgrades; }
        }
    }
}
