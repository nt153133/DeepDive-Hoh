/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using Clio.Utilities;
using Clio.Utilities.Helpers;
using DeepHoh.Logging;
using DeepHoh.Providers;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Navigation;
using System;
using System.Threading.Tasks;

namespace DeepHoh.TaskManager.Actions
{
    internal class StuckDetection : ITask
    {
        private const float Distance = 0.25f;

        private readonly WaitTimer _moveTimer = new WaitTimer(TimeSpan.FromSeconds(15));
        private Vector3 _location = Vector3.Zero;
        public string Name => "Stuck Detection";

        public async Task<bool> Run()
        {
            if (_moveTimer.IsFinished && Poi.Current != null && Poi.Current.Type != PoiType.None)
            {
                System.Collections.Generic.List<Vector3> path = StraightPathHelper.RealStraightPath();
                Logger.Info("Dump path:");
                foreach (Vector3 x in path)
                {
                    Logger.Info(x.ToString());
                }

                Logger.Warn("No activity was detected for {0} seconds. Adding target {1} to the blacklist and trying again",
                    _moveTimer.WaitTime.TotalSeconds, Poi.Current);
                if (Poi.Current.Unit != null)
                {
                    DDTargetingProvider.Instance.AddToBlackList(Poi.Current.Unit, TimeSpan.FromSeconds(30),
                        "Navigation Error");
                }

                if (Poi.Current.Type != PoiType.None)
                {
                    Poi.Clear("No activity detected (not none): PoiType: " + Poi.Current.Type);
                }

                if (Poi.Current.Type != PoiType.Wait)
                {
                    Poi.Clear("No activity detected (not wait): PoiType: " + Poi.Current.Type);
                }

                _moveTimer.Reset();
                return true;
            }

            if (_moveTimer.IsFinished)
            {
                Logger.Warn("No activity was detected for {0} seconds. Clearing Navigator?",
                    _moveTimer.WaitTime.TotalSeconds);
                await CommonTasks.StopMoving();
                Navigator.Clear();
                _moveTimer.Reset();
                return true;
            }

            return false;
        }

        public void Tick()
        {
            Vector3 location = Core.Me.Location;
            if (location.DistanceSqr(_location) > Distance)
            {
                _location = location;
                _moveTimer.Reset();
            }
        }
    }
}