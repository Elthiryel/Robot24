﻿using System;
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
            throw new NotImplementedException();
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
