using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Robocode;

namespace Robot24.Config
{
    [XmlRoot("Configuration")]
    [Serializable]
    public class Configuration
    {
        [XmlArray("Strategies")]
        [XmlArrayItem("Strategy")]
        public List<Strategy> Strategies { get; set; }
    }

    [Serializable]
    public class Strategy
    {
        public Requirements Requirements { get; set; }
        public MoveType MoveType { get; set; }
        public FireType FireType { get; set; }

        public bool PassedRequirements(ScannedRobotEvent lastRobotInfo, Robot ourRobot)
        {
            var enemyDirectionRelativeToRobot = lastRobotInfo.Heading - ourRobot.Heading;
            if (enemyDirectionRelativeToRobot > 180)
                enemyDirectionRelativeToRobot -= 360;
            if (enemyDirectionRelativeToRobot < -180)
                enemyDirectionRelativeToRobot += 360;
            var enemyDirectionRelativeToGun = lastRobotInfo.Heading - ourRobot.GunHeading;
            if (enemyDirectionRelativeToGun > 180)
                enemyDirectionRelativeToGun -= 360;
            if (enemyDirectionRelativeToGun < -180)
                enemyDirectionRelativeToGun += 360;
            if (Requirements.EnemyEnergy != null && !(lastRobotInfo.Energy >= Requirements.EnemyEnergy.Min && lastRobotInfo.Energy <= Requirements.EnemyEnergy.Max))
                return false;
            if (Requirements.Energy != null && !(ourRobot.Energy >= Requirements.Energy.Min && ourRobot.Energy <= Requirements.Energy.Max))
                return false;
            if (Requirements.EnemyVelocity != null && !(lastRobotInfo.Velocity >= Requirements.EnemyVelocity.Min && lastRobotInfo.Velocity <= Requirements.EnemyVelocity.Max))
                return false;
            if (Requirements.NumberOfPlayers != null && !(ourRobot.Others >= Requirements.NumberOfPlayers.Min && ourRobot.Others <= Requirements.NumberOfPlayers.Max))
                return false;
            if (Requirements.EnemyDirectionRelativeToRobot != null && !(enemyDirectionRelativeToRobot >= Requirements.EnemyDirectionRelativeToRobot.Min && enemyDirectionRelativeToRobot <= Requirements.EnemyDirectionRelativeToRobot.Max))
                return false;
            if (Requirements.EnemyDirectionRelativeToGun != null && !(enemyDirectionRelativeToGun >= Requirements.EnemyDirectionRelativeToGun.Min && enemyDirectionRelativeToGun <= Requirements.EnemyDirectionRelativeToGun.Max))
                return false;
            return true;
        }
    }

    [Serializable]
    public class Requirements
    {
        public Parameter NumberOfPlayers { get; set; }
        public Parameter Energy { get; set; }
        public Parameter EnemyEnergy { get; set; }
        public Parameter EnemyVelocity { get; set; }
        public Parameter EnemyDirectionRelativeToGun { get; set; }
        public Parameter EnemyDirectionRelativeToRobot { get; set; }
    }

}
