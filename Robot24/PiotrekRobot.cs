using System;
using System.IO;
using System.Xml.Serialization;
using Robocode;
using Robot24.Config;

namespace Robot24
{
    public class PiotrekRobot : Robot
    {
        private int _moveDirection = 1;

        public Configuration CentralConfiguration;
        public Strategy CurrentStrategy;
        public ScannedRobotEvent LastRobotInfo;
        public double MaxMovingDistance = 50;

        public override void Run()
        {
            InitializeConfiguration();
            MaxMovingDistance = Math.Max(BattleFieldWidth, BattleFieldHeight);
            TurnRight(90);
            while (true)
            {
                if (CurrentStrategy != null && CurrentStrategy.MoveType == MoveType.Circle)
                {
                    while (true)
                    {
                        Ahead(30*_moveDirection);
                        TurnRight(20);
                    }
                }
                
                Ahead(MaxMovingDistance);
                TurnRight(90);
                DetermineStrategy();
            }
        }

        private void InitializeConfiguration()
        {
            GetResourceTextFile("configuration.xml");
        }

        private void GetResourceTextFile(string filename)
        {
            var serializer = new XmlSerializer(typeof(Configuration));

            using (var stream = GetType().Assembly.GetManifestResourceStream("Robot24." + filename))
            {
                if (stream == null)
                    return;
                using (var sr = new StreamReader(stream))
                {
                    CentralConfiguration = (Configuration) serializer.Deserialize(sr);
                }
            }
        }

        private void DetermineStrategy()
        {
            CurrentStrategy = null;
            foreach (var strategy in CentralConfiguration.Strategies)
            {
                if (strategy.PassedRequirements(LastRobotInfo, this))
                    CurrentStrategy = strategy;
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            LastRobotInfo = e;
            DetermineStrategy();
            Stop();
            DoFire();
            DoOpeningMove();
            Scan();
            DoEndingMove();
        }

        public override void OnHitWall(HitWallEvent evnt)
        {
            _moveDirection *=-1;
            TurnRight(evnt.Bearing);
            if (Math.Abs(GunHeading - Heading) < 10)
                TurnGunRight(90);
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            if (CurrentStrategy == null)
                return;
            switch (CurrentStrategy.MoveType)
            {
                case MoveType.Straight:
                    TurnRight(e.Bearing);
                    Fire(5);
                    break;
                default:
                    Fire(1);
                    break;
            }
        }

        private void DoOpeningMove()
        {
            if (CurrentStrategy == null)
                return;
            switch (CurrentStrategy.MoveType)
            {
                case MoveType.Straight:
                    DoStraightOpeningMove();
                    break;
                case MoveType.Evade:
                    DoEvadeOpeningMove();
                    break;
                case MoveType.Circle:
                    DoCircleOpeningMove();
                    break;
            }
        }

        private void DoStraightOpeningMove()
        {
            TurnRight(LastRobotInfo.Bearing);
            SynchronizeGunWithHeading();
            Ahead(LastRobotInfo.Distance / 10);
        }

        private void DoEvadeOpeningMove()
        {
            TurnRight(LastRobotInfo.Bearing + 90);
            Ahead(LastRobotInfo.Distance);
        }
        
        private void DoCircleOpeningMove()
        {
            if (LastRobotInfo.Velocity == 0)
                _moveDirection *= -1;
            TurnRight(LastRobotInfo.Bearing + 90);
            TurnGunLeft(90);
            Ahead(10 * _moveDirection);
        }

        private void DoEndingMove()
        {
            if (CurrentStrategy == null)
                return;
            switch (CurrentStrategy.MoveType)
            {
                case MoveType.Straight:
                    DoStraightEndingMove();
                    break;
                case MoveType.Evade:
                    DoEvadeEndingMove();
                    break;
                case MoveType.Circle:
                    DoCircleEndingMove();
                    break;
            }
        }

        private void DoStraightEndingMove()
        {
            
        }

        private void DoEvadeEndingMove()
        {
            
        }

        private void DoCircleEndingMove()
        {
            
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

        private void SynchronizeGunWithHeading()
        {
            var diff = Heading - GunHeading;
            if (diff > 180)
                diff -= 360;
            if (diff < -180)
                diff += 360;
            TurnGunRight(diff);
        }

        private double GetTurnAngle()
        {
            var angle = LastRobotInfo.Bearing - (GunHeading - Heading);
            return 0; // TODO
        }
    }
}
