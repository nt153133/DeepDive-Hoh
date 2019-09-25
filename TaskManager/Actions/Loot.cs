/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Original work done by zzi, contributions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using DeepHoh.Helpers;
using DeepHoh.Logging;
using DeepHoh.Providers;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing;
using ff14bot.RemoteWindows;

namespace DeepHoh.TaskManager.Actions
{
    internal class Loot : ITask
    {
        private Poi Target => Poi.Current;
        public string Name => "Loot";

        public async Task<bool> Run()
        {
            if (Target.Type != PoiType.Collect) return false;

            //let the navigation task handle moving toward the object if we are too far away.
            if (Target.Location.Distance2D(Core.Me.Location) > 3) return false;

            if (Target.Unit == null || !Target.Unit.IsValid)
            {
                Poi.Clear("Target not found at location");
                return true;
            }

            //let the user know we are trying to run a treasure task
            TreeRoot.StatusText = "Treasure";
            if (Target.Unit.IsValid)
            {
                if (Target.Unit?.NpcId == EntityNames.Hidden) return await HandleCacheOfTheHoard();
            }
            else
            {
                return true;
            }

            //treasure... or an "exit"...
            return await TreasureOrExit();
        }

        public void Tick()
        {
            if (!Constants.InDeepDungeon || CommonBehaviors.IsLoading || QuestLogManager.InCutscene) return;

            GameObject t = DDTargetingProvider.Instance.FirstEntity;

            if (t == null || t.Type == GameObjectType.BattleNpc) return;

            //only change if we don't have a poi or are currently performing a collect action.
            if (Poi.Current != null && Poi.Current.Type != PoiType.None && Poi.Current.Type != PoiType.Collect &&
                Poi.Current.Type != (PoiType) PoiTypes.ExplorePOI) return;

            if (Poi.Current != null && Poi.Current.Unit != null && Poi.Current.Unit.Pointer == t.Pointer) return;

            Poi.Current = new Poi(t, PoiType.Collect);
        }

        /// <summary>
        ///     Handles opening treasure coffers or opening an exit portal
        /// </summary>
        /// <returns></returns>
        internal async Task<bool> TreasureOrExit()
        {
            int tries = 0;

/*            if (Target.Unit.IsValid && Target.Unit.NpcId == EntityNames.BossExit)
            {
                
            }*/

            if (Target.Location.Distance2D(Core.Me.Location) >= 3)
            {
                await CommonTasks.MoveAndStop(new MoveToParameters(Target.Location, Target.Name), 2.5f, true);
                return true;
            }

            while (!DeepDungeonHoH.StopPlz && tries < 3 && Target.Unit.IsValid)
            {
                //if we are a frog / lust we can't open a chest
                if (Constants.AuraTransformed)
                {
                    Logger.Warn("Unable to open chest. Waiting for aura to end...");
                    await CommonTasks.StopMoving("Waiting on aura to end");
                    await Coroutine.Wait(TimeSpan.FromSeconds(30),
                        () => !Constants.AuraTransformed || Core.Me.InCombat || DeepDungeonHoH.StopPlz);
                    return true;
                }

                await Coroutine.Yield();

                Logger.Verbose("Attempting to interact with: {0} ({1} / 3)", Target.Name, tries + 1);

                Target.Unit.Target();
                //if (!Settings.Instance.NotLeader || (Settings.Instance.NotLeader && Target.Unit != null && (Target.Unit.NpcId == EntityNames.FloorExit || Target.Unit.NpcId == EntityNames.LobbyExit || Target.Unit.NpcId == EntityNames.BossExit)))
                if (!PartyManager.IsInParty || PartyManager.IsPartyLeader ||
                    PartyManager.IsInParty && Constants.IsExitObject(Target.Unit))
                {
                    if (Constants.IsExitObject(Target.Unit))
                    {
                        if (RaptureAtkUnitManager.GetWindowByName("DeepDungeonResult") != null)
                        {
                            await Coroutine.Sleep(5000);
                            RaptureAtkUnitManager.GetWindowByName("DeepDungeonResult").SendAction(1, 3, uint.MaxValue);
                        }
                    }

                    Target.Unit.Interact();
                }
                else
                    await CommonTasks.StopMoving("Waiting for leader to use chest");

                await Coroutine.Sleep(500);

                tries++;

                if (!Target.Unit.IsValid)
                    break;

                if (!Target.Unit.IsTargetable) break;

                if (SelectYesno.IsOpen) break;
            }

            await Coroutine.Wait(500, () => SelectYesno.IsOpen);

            //if this is an exit
            if (SelectYesno.IsOpen)
            {
                SelectYesno.ClickYes();
                await Coroutine.Wait(TimeSpan.MaxValue,
                    () => DeepDungeonHoH.StopPlz || QuestLogManager.InCutscene || NowLoading.IsVisible);
                return true;
            }

            Blacklist.Add(Target.Unit.ObjectId, TimeSpan.FromMinutes(1),
                $"Tried to Interact with the Target {tries} times");
            Poi.Clear($"Tried to Interact with the Target {tries} times");

            return false;
        }

        /// <summary>
        ///     Handles Cache of the Hoard
        /// </summary>
        /// <returns></returns>
        private async Task<bool> HandleCacheOfTheHoard()
        {
            TreeRoot.StatusText = "Banded Coffer";

            if (
                GameObjectManager.GameObjects.Any(
                    i => Constants.TrapIds.Contains(i.NpcId) && i.Distance2D(Target.Location) < 2))
            {
                Blacklist.Add(Target.Unit, BlacklistFlags.All, TimeSpan.FromMinutes(3),
                    "A trap is close to the Hoard Spawn location. Skipping.");
                Poi.Clear("A trap is close to the Hoard Spawn location. Skipping.");
                await Coroutine.Sleep(250);
                return true;
            }

            if (Target.Location.Distance2D(Core.Me.Location) >= 2)
            {
                Logger.Info("Banded Coffer is >= 3");
                await CommonTasks.MoveAndStop(new MoveToParameters(Target.Location, "Banded Coffer"), 0.5f, true);
                return true;
            }

            await CommonTasks.StopMoving("Spawning Coffer");

            Logger.Info("Found a Cache of the Horde. Waiting for it to spawn... (Giving it a few seconds to spawn)");

            //target will change after the banded coffer is spawned
            GameObject org = Target.Unit;

            //wait for the chest or for us to get into combat.
            await Coroutine.Wait(TimeSpan.FromSeconds(5),
                () =>
                    Core.Me.InCombat || GameObjectManager.NumberOfAttackers > 0 || DeepDungeonHoH.StopPlz ||
                    GameObjectManager.GetObjectsOfType<EventObject>().Any(i => i.NpcId == EntityNames.BandedCoffer)
            );

            if (Core.Me.InCombat || GameObjectManager.NumberOfAttackers > 0)
            {
                Logger.Info("Entered Combat waiting on coffer to spawn.");
                return true;
            }

            Blacklist.Add(org, BlacklistFlags.All | (BlacklistFlags) DeepDungeonManager.Level, TimeSpan.FromMinutes(3),
                "Spawned the Coffer or used all of our time...");
            Poi.Clear("Hidden added to blacklist");
            return true;
        }
    }
}