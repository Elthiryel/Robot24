using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
