using System;
using DeepHoh.Logging;
using ff14bot.Managers;
using ff14bot.RemoteWindows;

namespace DeepHoh.Helpers
{
    internal static class DeepTracker
    {
        private static int _startingLevel;
        private static int _currentLevel;
        private static DateTime _currentRunStarTime;
        private static DateTime _starTime;
        private static TimeSpan _lastRunTime;
        private static bool _isMeasuring;
        private static int _deaths;
        private static int _successfulRuns;
        private static int _failedRuns;
        private static float _xpPerHour;
        private static float _deathsPerHour;
        private static uint _startingXP;
        private static uint _runEndXP;
        private static uint _toLevelXP;
        private static uint _totalXPGain;
        private static TimeSpan _elapsedSpan;
        private static DateTime _currentRunEndTime;
        private static uint _xpNeeded;

        private static TimeSpan CurrentRunTime()
        {
            return DateTime.Now.Subtract(_currentRunStarTime);
        }

        public static void InitializeTracker(int currentLevel)
        {
            _startingLevel = currentLevel;
            _starTime = DateTime.Now;
            _startingXP = Experience.CurrentExperience;
            _toLevelXP = Experience.ExperienceRequired;
        }

        public static void Reset()
        {
            _isMeasuring = false;
//            GameStatsManager.uint_2 = Experience.CurrentExperience;
//            GameStatsManager.uint_1 = Experience.ExperienceRequired;

            _deaths = 0;
            _xpPerHour = 0.0f;
            _deathsPerHour = 0.0f;
        }

        private static void StartMeasuring()
        {
            _currentRunStarTime = DateTime.Now;
            _isMeasuring = true;
        }

        private static void StopMeasuring()
        {
            _currentRunEndTime = DateTime.Now;
            _isMeasuring = false;
        }

        public static void StartRun(int currentLevel)
        {
            StartMeasuring();

            if (_startingLevel == 0)
                _startingLevel = currentLevel;

            _currentLevel = currentLevel;

             ChatManager.SendChat($"/echo Run Started: Current level - {currentLevel}");
        }

        public static void EndRun(bool failed)
        {
            if (_isMeasuring == false)
                return;

            StopMeasuring();

            if (failed)
                _failedRuns++;
            else
                _successfulRuns++;

            _lastRunTime = _currentRunEndTime.Subtract(_currentRunStarTime);
            _runEndXP = Experience.CurrentExperience;

            string status = (failed ? "Failed" : "Complete");

            ChatManager.SendChat($"/echo Run Ended - {status}");

            RunReport();
        }

        public static void Died()
        {
            ++_deaths;
        }

        private static void UpdateXP(int realLevel)
        {
            if (realLevel > _currentLevel) _totalXPGain += _xpNeeded;
        }

        public static void RunReport()
        {
            _elapsedSpan = DateTime.Now.Subtract(_starTime);
            Logger.Info(@"

================Status   Report==============
=======================================
Starting Level   : {0}
Current Level    : {1}
Deaths             : {2}
Failed Runs       : {3}
Successful Runs  : {4}
Pre-Run XP       : {9}
Post-Run XP       : {10}
Total XP Gain       : {11}
Last Run Time     : {5} Min  {6} Sec
Total Run Time     : {7} Hours  {8} Min
=======================================

", _startingLevel, _currentLevel, _deaths, _failedRuns, _successfulRuns, _lastRunTime.Minutes, _lastRunTime.Seconds,
                _elapsedSpan.Hours, _elapsedSpan.Minutes, 0, 0, 0);
        }
    }
}