using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robocode;

namespace Robot24
{
    public class MarcinRobot : Robot
    {
        private bool fuckerFound = false;
        // The main method of your robot containing robot logics
        public override void Run()
        {
            // -- Initialization of the robot --

            // Here we turn the robot to point upwards, and move the gun 90 degrees
            TurnLeft(Heading - 90);
            TurnGunRight(90);

            // Infinite loop making sure this robot runs till the end of the battle round
            while (true)
            {
                // -- Commands that are repeated forever --
                if (!fuckerFound)
                {
                    // Move our robot 5000 pixels ahead
                    Ahead(5000);

                    // Turn the robot 90 degrees
                    TurnRight(90);
                }
                else
                    for (int i = 0; i < 30; i++)
                    {
                        TurnGunRight(i);
                    }

                // Our robot will move along the borders of the battle field
                // by repeating the above two statements.
            }
        }

        // Robot event handler, when the robot sees another robot
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            Fire(3);
            this.Stop();
            this.TurnRight(e.Bearing);
            //this.TurnRadarRight(e.Bearing);
            this.Scan();
            Ahead(e.Distance);
            fuckerFound = true;            

            //(this.Heading + angleToEnemy % 360)

            // Calculate the angle to the scanned robot
            //double angle = Math.toRadians((robotStatus.getHeading() + angleToEnemy % 360);

            // Calculate the coordinates of the robot
            //double enemyX = (robotStatus.getX() + Math.sin(angle) * e.getDistance());
            //double enemyY = (robotStatus.getY() + Math.cos(angle) * e.getDistance());
        }

        public override void OnRobotDeath(RobotDeathEvent evnt)
        {
            fuckerFound = false;
        }
    }
}
