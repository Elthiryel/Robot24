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
        public Strategy CurrentStrategy;
        public ScannedRobotEvent LastRobotInfo;
        public double Movin = 50;

        public override void Run()
        {
            InitializeConfiguration();
            _lastRight = true;
            Movin = Math.Max(BattleFieldWidth, BattleFieldHeight);
            //TurnGunRight(90);
            TurnRight(90);
            while (true)
            {
                Ahead(Movin);
                TurnRight(90);
                DetermineStrategy();
            }
        }

        private void DetermineStrategy()
        {
            foreach (var strategy in CentralConfiguration.Strategies)
            {
                if (strategy.PassedRequirements(LastRobotInfo, this))
                    CurrentStrategy = strategy;
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
            LastRobotInfo = e;
            var bearing = e.Bearing;
            _lastRight = bearing >= 0;
            var distance = e.Distance;
            var velocity = e.Velocity;
            Console.WriteLine("SCANNED, bearing = {0}, distance = {1}, velocity = {2}", bearing, distance, velocity);
            Stop();
            DoFire();
            DoOpeningMove();
            Scan();
            DoEndingMove();
        }

        private void DoEndingMove()
        {
            throw new NotImplementedException();
        }

        private void DoOpeningMove()
        {
            throw new NotImplementedException();
        }

        private void DoFire()
        {
            if (CurrentStrategy == null)
                return;
            switch (CurrentStrategy.FireType)
            {
                case FireType.HeavyFire:
                    HeavyFire();
                    break;
                case FireType.LightFire:
                    LightFire();
                    break;
                case FireType.NoFire:
                    break;
                case FireType.SmartFire:
                    SmartFire();
                    break;
            }
        }

        private void HeavyFire()
        {
            Fire(3);
        }

        private void LightFire()
        {
            Fire(1);
        }

        private void SmartFire()
        {
            var distance = LastRobotInfo.Distance;
            var velocity = LastRobotInfo.Velocity;
            var isHeadingOk = IsHeadingOk(Heading, LastRobotInfo.Heading, 15);
            if (distance < 100 && velocity < 2 && isHeadingOk)
                Fire(5);
            else if (distance < 100 && velocity < 2)
                Fire(3);
            else if (distance < 200 && isHeadingOk)
                Fire(3);
            else if (distance < 200 && velocity < 2)
                Fire(2);
            else if (distance < 300 && isHeadingOk)
                Fire(2);
            else if (distance < 400 || velocity < 1)
                Fire(1);
            else if (distance < 500 && isHeadingOk)
                Fire(1);
        }

        private static bool IsHeadingOk(double my, double his, double limit)
        {
            var diff = Math.Abs(my - his);
            if (diff <= limit)
                return true;
            if (diff >= (180 - limit) && diff <= (180 + limit))
                return true;
            if (diff >= (360 - limit))
                return true;
            return false;
        }
    }
}
