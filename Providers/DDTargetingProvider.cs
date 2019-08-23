/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using Clio.Utilities;
using DeepHoh.Helpers;
using DeepHoh.Logging;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeepHoh.Providers
{
    internal class DDTargetingProvider
    {
        private static DDTargetingProvider _instance;

        private int _floor;
        private DateTime _lastPulse = DateTime.MinValue;

        public DDTargetingProvider()
        {
            LastEntities = new ReadOnlyCollection<GameObject>(new List<GameObject>());
        }

        internal static DDTargetingProvider Instance => _instance ?? (_instance = new DDTargetingProvider());


        public ReadOnlyCollection<GameObject> LastEntities { get; set; }

        internal bool LevelComplete
        {
            get
            {
                if (!DeepDungeonManager.PortalActive)
                {
                    return false;
                }

                if (Settings.Instance.GoExit && PartyManager.IsInParty)
                {
                    if (PartyManager.AllMembers.Any(i => i.CurrentHealth == 0))
                    {
                        return false;
                    }

                    if (Settings.Instance.GoForTheHoard)
                    {
                        return !LastEntities.Any(i =>
                            (i.NpcId == EntityNames.Hidden || i.NpcId == EntityNames.BandedCoffer) &&
                            !Blacklist.Contains(i.ObjectId, (BlacklistFlags)DeepDungeonManager.Level));
                    }

                    //Logger.Instance.Verbose("Full Explore : {0} {1}", _levelComplete, !NotMobs().Any());
                    return true;
                }

                return !LastEntities.Any();
            }
        }

        /// <summary>
        ///     decide what we need to do
        /// </summary>
        public GameObject FirstEntity => LastEntities.FirstOrDefault();

        internal void Reset()
        {
            Blacklist.Clear(i => true);
        }

        internal void Pulse()
        {
            if (CommonBehaviors.IsLoading)
            {
                return;
            }

            if (!Constants.InDeepDungeon)
            {
                return;
            }

            if (_floor != DeepDungeonManager.Level)
            {
                Logger.Info("Level has Changed. Clearing Targets");
                _floor = DeepDungeonManager.Level;
                Blacklist.Clear(i => i.Flags == (BlacklistFlags)DeepDungeonManager.Level);
            }

            //if (BeaconofReturn != null && !BeaconofReturn.IsValid)
            //    BeaconofReturn = null;

            //if (Portal != null && !Portal.IsValid)
            //    Portal = null;

            using (new PerformanceLogger("Targeting Pulse"))
            {
                LastEntities = new ReadOnlyCollection<GameObject>(GetObjectsByWeight());

                if (_lastPulse + TimeSpan.FromSeconds(5) < DateTime.Now)
                {
                    Logger.Verbose($"Found {LastEntities.Count} Targets");

                    if (LastEntities.Count == 0)
                    {
                        Reset();
                    }

            //        foreach (GameObject unit in LastEntities)
            //        {
             //           Logger.Verbose("Name:{0}, Type:{3}, ID:{1}, Obj:{2}", unit, unit.NpcId, unit.ObjectId, unit.GetType());
             //       }
                    _lastPulse = DateTime.Now;
                }
            }
        }
        //{
        //    get
        //    {
        //        var badGuys = (CombatTargeting.Instance.Provider as DDCombatTargetingProvider)?.GetObjectsByWeight();

        //        var anyBadGuysAround = badGuys != null && badGuys.Any();

        //        //if (Beta.Target != null && Beta.Target.IsValid && !Blacklist.Contains(Beta.Target.ObjectId, (BlacklistFlags)DeepDungeonManager.Level) && Beta.Target.Type != GameObjectType.GatheringPoint)
        //        //    return null;

        //        // Party member is dead
        //        if (PartyManager.AllMembers.Any(member => member.CurrentHealth == 0))
        //        {
        //            // Select Beacon of Return as highest priority if it is known and can be used.
        //            if (BeaconofReturn != null && DeepDungeonManager.ReturnActive)
        //                return BeaconofReturn;

        //            // If the Beacon of Return is not yet active and there are any mobs around: Kill the mobs.
        //            if (anyBadGuysAround)
        //                return new Poi(badGuys.First(), PoiType.Kill);
        //        }

        //        // Beacon of Passage
        //        if (LevelComplete && Portal != null)
        //            return Portal;

        //        // Bosses or Pomander of Rage / Pomander of Lust
        //        if ((DeepDungeonManager.BossFloor || Core.Me.HasAura(Auras.Lust)) && anyBadGuysAround)
        //            return new Poi(badGuys.First(), PoiType.Kill);

        //        // Chests
        //        if (LastEntities != null && LastEntities.Any())
        //            return LastEntities.First();

        //        // Kill something
        //        if (anyBadGuysAround)
        //            return new Poi(badGuys.First(), PoiType.Kill);

        //        return new Poi(
        //            SafeSpots.OrderByDescending(i => i.Distance2D(Core.Me.Location)).First(),
        //            PoiType.Hotspot
        //        );


        //    }
        //}

        internal void AddToBlackList(GameObject obj, string reason)
        {
            AddToBlackList(obj, TimeSpan.FromMinutes(1), reason);
        }

        internal void AddToBlackList(GameObject obj, TimeSpan time, string reason)
        {
            Blacklist.Add(obj, (BlacklistFlags)_floor, time, reason);
            Poi.Clear(reason);
        }

        private List<GameObject> GetObjectsByWeight()
        {
            return GameObjectManager.GameObjects
                .Where(Filter)
                .OrderByDescending(Sort)
                .ToList();
        }

        private float Sort(GameObject obj)
        {
            float weight = 100f;

            weight -= obj.Distance2D();

            if (obj.Type == GameObjectType.BattleNpc)
            {
                return weight / 2;
            }

            if (obj.NpcId == EntityNames.BandedCoffer)
            {
                weight += 500;
            }

            if (DeepDungeonManager.PortalActive && Settings.Instance.GoForTheHoard && obj.NpcId == EntityNames.Hidden)
            {
                weight += 5;
            }
            else if (DeepDungeonManager.PortalActive && Settings.Instance.GoExit &&
                     obj.NpcId != EntityNames.FloorExit && PartyManager.IsInParty)
            {
                weight -= 10;
            }

            return weight;
        }

        private bool Filter(GameObject obj)
        {
            if (obj.NpcId == 5042) //script object
            {
                return false;
            }

            if (obj.NpcId == 7396) //script object
            {
                return false;
            }

            if (obj.Location == Vector3.Zero)
            {
                return false;
            }

            if (Blacklist.Contains(obj) || Constants.TrapIds.Contains(obj.NpcId) ||
                Constants.IgnoreEntity.Contains(obj.NpcId))
            {
                return false;
            }

            if (obj.Type == GameObjectType.BattleNpc)
            {
                if (DeepDungeonManager.PortalActive)
                {
                    return false;
                }

                BattleCharacter battleCharacter = (BattleCharacter)obj;
                return !battleCharacter.IsDead;
            }

            return obj.Type == GameObjectType.EventObject || obj.Type == GameObjectType.Treasure ||
                   obj.Type == GameObjectType.BattleNpc;
        }
    }
}