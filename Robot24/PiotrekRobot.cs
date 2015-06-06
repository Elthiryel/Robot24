using System;
using System.IO;
using System.Security.Permissions;
using System.Xml.Serialization;
using Robocode;
using Robot24.Config;

namespace Robot24
{
    //[FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
    public class PiotrekRobot : Robot
    {
        private bool _lastRight;
        public Configuration CentralConfiguration;

        public override void Run()
        {
            InitializeConfiguration();
            _lastRight = true;
            while (true)
            {
                if (_lastRight)
                    TurnRight(10);
                else
                    TurnLeft(10);
            }
        }

        public void InitializeConfiguration()
        {
            GetResourceTextFile("configuration.xml");
        }

        public void GetResourceTextFile(string filename)
        {
            var serializer = new XmlSerializer(typeof(Configuration));

            using (Stream stream = this.GetType().Assembly.
                       GetManifestResourceStream("Robot24." + filename))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    CentralConfiguration = (Configuration)serializer.Deserialize(sr);
                }
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            var bearing = e.Bearing;
            _lastRight = bearing >= 0;
            var distance = e.Distance;
            var velocity = e.Velocity;
            Console.WriteLine("SCANNED, bearing = {0}, distance = {1}, velocity = {2}", bearing, distance, velocity);
            Stop();
            var toMove = distance > 50 ? distance / 10 : 5;
            if ((distance < 100 && IsHeadingOk(Heading, e.Heading, 20)) || (distance < 200 && velocity < 1))
                Fire(5);
            else if ((distance < 200 && IsHeadingOk(Heading, e.Heading, 20)) || (distance < 200 && velocity < 3))
                Fire(2);
            else if (distance < 300 || velocity < 1)
                Fire(1);
            else
                Ahead(toMove);
            TurnRight(bearing);
            Scan();
            Ahead(toMove);
        }

        private static bool IsHeadingOk(double my, double his, double limit)
        {
            if (Math.Abs(my - his) < limit)
                return true;
            if (Math.Abs(my + 180 - his) < limit)
                return true;
            if (Math.Abs(my + 360 - his) < limit)
                return true;
            if (Math.Abs(my - 180 - his) < limit)
                return true;
            if (Math.Abs(my - 360 - his) < limit)
                return true;
            return false;
        }
    }
}
