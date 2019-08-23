﻿/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */
using Clio.Utilities;
using DeepHoh.Helpers;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace DeepHoh.Providers
{
    // ReSharper disable once InconsistentNaming
    internal class DDCombatTargetingProvider : ITargetingProvider
    {
        private Vector3 _distance;

        internal IEnumerable<BattleCharacter> Targets { get; private set; }
        public List<BattleCharacter> GetObjectsByWeight()
        {
            _distance = Core.Me.Location;

            Targets = GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(target =>
                {
                    if (target.NpcId == 5042)
                    {
                        return false;
                    }

                    if (Constants.InDeepDungeon && (Blacklist.Contains(target) && !target.InCombat))
                    {
                        return false;
                    }

                    if (Constants.TrapIds.Contains(target.NpcId) || Constants.IgnoreEntity.Contains(target.NpcId))
                    {
                        return false;
                    }

                    return !target.IsDead;
                })
                .Where(target =>
                    target.StatusFlags.HasFlag(StatusFlags.Hostile) && (target.InLineOfSight() || target.InCombat)
                );

            return Targets.Where(target =>
            {
                if (DeepDungeonManager.BossFloor)
                {
                    return true;
                }

                return target.Distance2D() < Constants.ModifiedCombatReach;
            })
                .OrderByDescending(Priority)
                .ToList();
        }

        private double Priority(BattleCharacter battleCharacter)
        {
            double weight = 1000.0;
            weight -= battleCharacter.Distance2D(_distance) / 2.25;
            weight += battleCharacter.ClassLevel / 1.25;
            weight += 100 - battleCharacter.CurrentHealthPercent;

            if (battleCharacter.HasTarget && battleCharacter.TargetCharacter == Core.Me)
            {
                weight += 50;
            }

            if (!battleCharacter.InCombat)
            {
                weight -= 5;
            }
            else
            {
                weight += 50;
            }

            if (Core.Target != null && Core.Target.ObjectId == battleCharacter.ObjectId)
            {
                weight += 10;
            }

            if (battleCharacter.InCombat && battleCharacter.Location.Distance2D(Core.Me.Location) < 5)
            {
                weight *= 1.5;
            }

            if (battleCharacter.Distance2D(_distance) > 20)
            {
                weight /= 2;
            }

            if ((battleCharacter.NpcId == Mobs.PalaceHornet || battleCharacter.NpcId == Mobs.PalaceSlime) && battleCharacter.InCombat)
            {
                return weight * 100.0;
            }

            return weight;
        }
    }
}
