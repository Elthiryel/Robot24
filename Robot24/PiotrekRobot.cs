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
        public double evadingMove = 30;
        public double MaxMovingDistance = 50;
        private bool _onHitLaunched = false;

        public override void Run()
        {
            InitializeConfiguration();
            MaxMovingDistance = Math.Max(BattleFieldWidth, BattleFieldHeight);
            TurnRight(90);
            Random r= new Random();
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
                
                /*Ahead(MaxMovingDistance);
                TurnRight(90);*/
                RandomizeMovement(r.Next(2),r.Next(2),r.Next(360),r.Next((int)BattleFieldHeight));
                DetermineStrategy();
            }
        }

        private void RandomizeMovement(int direction, int turnDirection, int angle, int distance)
        {
            if(turnDirection == 0)
                TurnLeft(angle);
            else
                TurnRight(angle);

            if (direction == 0)
                Ahead(distance);
            else
                Back(distance);
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

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            if (_onHitLaunched)
                return;

            _onHitLaunched = true;
            
            if (CurrentStrategy != null && CurrentStrategy.MoveType == MoveType.Evade)
            {
                var angle = Heading - GunHeading + evnt.Bearing;
                if (angle > 180)
                    angle -= 360;
                if (angle < -180)
                    angle += 360;
                TurnRight(angle+90);
                SynchronizeGunWithHeading();
                var direction = CalculateEvadeDirectionFromHeading();
                TurnGunLeft(90);
                if (direction)
                    Ahead(evadingMove);
                else
                    Back(evadingMove);
            }
            _onHitLaunched = false;
        }

        private bool CalculateEvadeDirectionFromHeading()
        {
            bool isYDriven = Math.Cos(this.Heading) > 0;
            //false - back, true - ahead
            if (isYDriven)
                if (this.Y > this.BattleFieldHeight/2)
                    return false;
                else
                    return true;
            else if (this.X > this.BattleFieldWidth/2)
                return false;
            else
                return true;
        }

        public override void OnHitWall(HitWallEvent evnt)
        {
            return;
            _moveDirection *=-1;
            TurnRight(Heading - GunHeading + evnt.Bearing);
            SynchronizeGunWithHeading();
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
                    TurnRight(GetTurnAngle());
                    Fire(5);
                    break;
                default:
                    Fire(1);
                    break;
            }
            Scan();
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
            TurnRight(GetTurnAngle());
            SynchronizeGunWithHeading();
            Ahead(Math.Max(LastRobotInfo.Distance / 10, 10));
        }

        private void DoEvadeOpeningMove()
        {
            TurnRight(LastRobotInfo.Bearing + 90);
            SynchronizeGunWithHeading();
            TurnGunLeft(90);
            
            if (CalculateEvadeDirectionFromHeading())
                Ahead(LastRobotInfo.Distance /5);
            else
                Back(LastRobotInfo.Distance / 5);
        }
        
        private void DoCircleOpeningMove()
        {
            if (LastRobotInfo.Velocity == 0)
                _moveDirection *= -1;
            TurnRight(GetTurnAngle() + 90);
            SynchronizeGunWithHeading();
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
            var relativeHeading = GetRelativeHeading(Heading, LastRobotInfo.Heading);
            var velocity = LastRobotInfo.Velocity;

            var isMovingRight = relativeHeading > 0;
            if (velocity < 0)
                isMovingRight = !isMovingRight;

            if (Math.Abs(relativeHeading) < 25 || Math.Abs(velocity) < 3)
            {
                PerformScanning(isMovingRight);
                return;
            }

            for (var j = 10; j <= 20; j += 5)
            {
                for (var i = 0; i < 4; ++i)
                {
                    if (isMovingRight)
                        TurnRight(j);
                    else
                        TurnLeft(j);
                    Scan();
                }
            }
        }

        private void PerformScanning(bool isMovingRight)
        {
            for (var i = 0; i < 5; ++i)
            {
                if (isMovingRight)
                    TurnRight(10);
                else
                    TurnLeft(10);
                Scan();
            }
            for (var i = 0; i < 5; ++i)
            {
                if (isMovingRight)
                    TurnLeft(20);
                else
                    TurnRight(20);
                Scan();
            }
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
            var angle = Heading - GunHeading + LastRobotInfo.Bearing;
            if (angle > 180)
                angle -= 360;
            if (angle < -180)
                angle += 360;
            return angle;
        }

        private double GetRelativeHeading(double my, double his)
        {
            var diff = his - my;
            if (diff > 180)
                diff -= 360;
            if (diff < -180)
                diff += 360;
            return diff;
        }
    }
}
