/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clio.Utilities;
using DeepHoh.Helpers;
using DeepHoh.Providers;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Pathing;

namespace DeepHoh.TaskManager.Actions
{
    internal class POTDNavigation : ITask
    {
        private int level;

        private List<Vector3> SafeSpots;

        private int PortalPercent => (int) Math.Ceiling(DeepDungeonManager.PortalStatus / 11d * 100f);

        private Poi Target => Poi.Current;
        public string Name => "HoHNavigator";

        public async Task<bool> Run()
        {
            if (!Constants.InDeepDungeon) return false;

            if (Target == null) return false;
            
            if (!Core.Me.InCombat && Target.Type == PoiType.Quest)
                Poi.Clear("QUEST_POI");

            if (Navigator.InPosition(Core.Me.Location, Target.Location, 3f) &&
                Target.Type == (PoiType) PoiTypes.ExplorePOI)
            {
                Poi.Clear("We have reached our destination");
                return true;
            }

            string status = string.Format("Current Level {0}. Level Status: {1}% \"Done\": {2}",
                DeepDungeonManager.Level,
                PortalPercent, DDTargetingProvider.Instance.LevelComplete);
            TreeRoot.StatusText = status;

            if (ActionManager.IsSprintReady && Target.Location.Distance2D(Core.Me.Location) > 5 &&
                MovementManager.IsMoving)
            {
                ActionManager.Sprint();
                return true;
            }

            bool res = await CommonTasks.MoveAndStop(
                new MoveToParameters(Target.Location, $"Moving toward HoH Objective: {Target.Name}"), 1.5f);

            //            if (Target.Unit != null)
            //                Logger.Verbose(
            //                    $"[PotdNavigator] Move Results: {res} Moving To: \"{Target.Unit.Name}\" LOS: {Target.Unit.InLineOfSight()}");
            //            else
            //                Logger.Verbose($"[PotdNavigator] Move Results: {res} Moving To: \"{Target.Name}\" ");


            return res;
        }


        public void Tick()
        {
            if (!Constants.InDeepDungeon || CommonBehaviors.IsLoading || QuestLogManager.InCutscene) return;

            if (level != DeepDungeonManager.Level)
            {
                level = DeepDungeonManager.Level;
                SafeSpots = new List<Vector3>();
                SafeSpots.AddRange(GameObjectManager.GameObjects.Where(i => i.Location != Vector3.Zero)
                    .Select(i => i.Location));
            }

            if (!SafeSpots.Any(i => i.Distance2D(Core.Me.Location) < 5)) SafeSpots.Add(Core.Me.Location);

            if (Poi.Current == null || Poi.Current.Type == PoiType.None)
                Poi.Current = new Poi(SafeSpots.OrderByDescending(i => i.Distance2D(Core.Me.Location)).First(),
                    (PoiType) PoiTypes.ExplorePOI);
        }
    }
}