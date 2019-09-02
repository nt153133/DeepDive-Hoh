﻿/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.Utilities;
using DeepHoh.Helpers;
using DeepHoh.Logging;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing;
using ff14bot.RemoteWindows;

namespace DeepHoh.TaskManager.Actions
{
    internal class BeaconOfReturn : ITask
    {
        private int Level;
        private Vector3 location = Vector3.Zero;

        private Poi Target => Poi.Current;
        public string Name => "BeaconOfReturn";


        public async Task<bool> Run()
        {
            if (Target.Type != (PoiType) PoiTypes.UseBeaconOfReturn) return false;

            //let the navigation task handle moving toward the object if we are too far away.
            if (Target.Location.Distance2D(Core.Me.Location) > 3) return false;

            GameObject unit = GameObjectManager.GetObjectByNPCId(EntityNames.BeaconOfReturn);
            if (unit == null)
            {
                Logger.Warn("Beacon of return could not be found at this location");
                location = Vector3.Zero;
                Poi.Clear("Beacon of Return could not be found");
                return true;
            }

            if (unit.Distance2D(Core.Me.Location) >= 3)
            {
                await CommonTasks.MoveAndStop(new MoveToParameters(unit.Location, "Beacon of Return"), 2.5f, true);
                return true;
            }

            //if we are a frog / lust we can't open a chest
            if (Constants.AuraTransformed)
            {
                Logger.Warn("Unable to open chest. Waiting for aura to end...");
                await CommonTasks.StopMoving("Waiting on aura to end");

                await Coroutine.Wait(TimeSpan.FromSeconds(30),
                    () => !(Constants.AuraTransformed) || Core.Me.InCombat || DeepDungeonHoH.StopPlz);

                //incase we entered combat
                return true;
            }

            await Coroutine.Yield();

            if (Core.Me.HasAura(Auras.Lust)) await Tasks.Coroutines.Common.CancelAura(Auras.Lust);
            Logger.Verbose("Attempting to interact with: {0}", unit.Name);
            unit.Target();
            unit.Interact();

            await Coroutine.Wait(1000, () => SelectYesno.IsOpen);
            if (SelectYesno.IsOpen)
            {
                SelectYesno.ClickYes();
                await Coroutine.Wait(1000, () => Core.Me.IsCasting);
            }

            await Coroutine.Wait(10000, () => !Core.Me.IsCasting);

            await Coroutine.Sleep(500);

            Poi.Clear("Used Beacon Of Return");
            return true;
        }


        public void Tick()
        {
            if (!Constants.InDeepDungeon || CommonBehaviors.IsLoading || QuestLogManager.InCutscene) return;

            if (location == Vector3.Zero || Level != DeepDungeonManager.Level)
            {
                GameObject ret = GameObjectManager.GetObjectByNPCId(EntityNames.BeaconOfReturn);
                if (ret != null)
                {
                    Level = DeepDungeonManager.Level;
                    location = ret.Location;
                }
            }

            //if we are in combat don't move toward the Beacon of return
            if (Poi.Current != null && (Poi.Current.Type == PoiType.Kill ||
                                        Poi.Current.Type == (PoiType) PoiTypes.UseBeaconOfReturn)) return;

            //party member is dead & we have the location of the cor
            if (PartyManager.AllMembers.Any(member => member.CurrentHealth == 0) && location != Vector3.Zero &&
                Level == DeepDungeonManager.Level)
                Poi.Current = new Poi(location, (PoiType) PoiTypes.UseBeaconOfReturn);
        }
    }
}