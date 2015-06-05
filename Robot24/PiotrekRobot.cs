using System;
using Robocode;

namespace Robot24
{
    public class PiotrekRobot : Robot
    {
        private bool _lastRight;

        public override void Run()
        {
            _lastRight = true;
            while (true)
            {
                if (_lastRight)
                    TurnRight(10);
                else
                    TurnLeft(10);
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
