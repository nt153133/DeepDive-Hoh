/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using DeepHoh.Helpers;
using ff14bot;
using ff14bot.Directors;
using ff14bot.Managers;
using System;
using System.Linq;
using System.Threading.Tasks;
using static DeepHoh.Tasks.Coroutines.Common;

namespace DeepHoh.TaskManager.Actions
{
    internal class Pomanders : ITask
    {
        public string Name => "Pomanders";

        private int PortalPercent => (int)Math.Ceiling((DeepDungeonManager.PortalStatus / 11) * 100f);

        public async Task<bool> Run()
        {
            if (!Constants.InDeepDungeon)
            {
                return false;
            }

            if (PortalPercent < 10)
            {
                if (await BuffCurrentFloor())
                {
                    return true;
                }
            }

            if (PortalPercent > 5 && (Core.Me.HasAura(Auras.NoAutoHeal) || Core.Me.HasAura(Auras.Amnesia)))
            {
                return await UsePomander(Pomander.Petrification);
            }

            if (await BuffBoss())
            {
                return true;
            }

            return false;
        }

        public void Tick()
        {
        }

        private bool _runbuf = false;

        /// <summary>
        /// buff the stuff
        /// </summary>
        /// <returns></returns>
        private async Task<bool> BuffBoss()
        {
            if (DeepDungeonManager.BossFloor && !Core.Me.InCombat)
            {
                _runbuf = true;
                if (Core.Me.HasAura(Auras.Enervation) || Core.Me.HasAura(Auras.Silence))
                {
                    return true;
                }

                if (!DeepDungeonManager.PortalActive)
                {
                    await UsePomander(Pomander.Strength, Auras.Strength);
                    await UsePomander(Pomander.Steel, Auras.Steel);
                    await UsePomander(Pomander.Frailty);
                }
            }
            else
            {
                _runbuf = false;
                return await BuffMe();
            }

            return false;
        }

        /// <summary>
        /// Player pomander buffs
        /// </summary>
        /// <returns></returns>
        private static async Task<bool> BuffMe()
        {
            if (Core.Me.HasAura(Auras.ItemPenalty))
            {
                return false;
            }

            if (CombatTargeting.Instance.LastEntities.Count() > 4)
            {
                return await UsePomander(Pomander.Petrification);
            }

            if (DeepDungeonManager.GetInventoryItem(Pomander.Petrification).Count == 3)
            {
                return await UsePomander(Pomander.Petrification);
            }

            if (await UsePomander(Pomander.Raising))
            {
                return true;
            }

            if (await UsePomander(Pomander.Intuition))
            {
                return true;
            }

            if (!Settings.Instance.SaveSteel || DeepDungeonManager.GetInventoryItem(Pomander.Steel).Count > 1)
            {
                if (await UsePomander(Pomander.Steel, Auras.Steel))
                {
                    return true;
                }
            }

            if (!Settings.Instance.SaveStr || DeepDungeonManager.GetInventoryItem(Pomander.Strength).Count > 1)
            {
                if (await UsePomander(Pomander.Strength, Auras.Strength))
                {
                    return true;
                }
            }

            if (Core.Me.HasAura(Auras.Pox))
            {
                if (await UsePomander(Pomander.Purity))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// buffs the start of the level
        /// </summary>
        /// <returns></returns>
        private async Task<bool> BuffCurrentFloor()
        {
            //do not buff the floor if there is a boss...
            if (DeepDungeonManager.BossFloor)
            {
                return false;
            }

            if (PartyManager.IsPartyLeader && (Core.Me.HasAura(Auras.Amnesia) || Core.Me.HasAura(Auras.ItemPenalty) || Core.Me.HasAura(Auras.NoAutoHeal)))
            {
                await UsePomander(Pomander.Serenity);
            }

            if (await Traps())
            {
                return true;
            }

            if (await UsePomander(Pomander.Fortune))
            {
                return true;
            }
            //            if (await UsePomander(Pomander.Rage))
            //                return true;
            if (await UsePomander(Pomander.Intuition))
            {
                return true;
            }

            //SaveFrailty

            if (!Settings.Instance.SaveFrailty || DeepDungeonManager.GetInventoryItem(Pomander.Frailty).Count > 1)
            {
                if (await UsePomander(Pomander.Frailty))
                {
                    return true;
                }
            }

            if (await BuffNextFloor())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// stores the floor # for the level we last removed traps from
        /// </summary>
        private int _trapPomanderUsageCheck = 0;

        /// <summary>
        /// remove traps
        /// only uses one of the two trap removers per level
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Traps()
        {
            if (Core.Me.HasAura(Auras.ItemPenalty))
            {
                return false;
            }

            if (DeepDungeonManager.PortalActive || _trapPomanderUsageCheck == DeepDungeonManager.Level)
            {
                return false;
            }

            if (await UsePomander(Pomander.Safety))
            {
                _trapPomanderUsageCheck = DeepDungeonManager.Level;
                return true;
            }

            if (await UsePomander(Pomander.Sight))
            {
                _trapPomanderUsageCheck = DeepDungeonManager.Level;
                return true;
            }
            return false;
        }

        /// <summary>
        /// buffs for the next floor
        /// </summary>
        /// <returns></returns>
        private async Task<bool> BuffNextFloor()
        {
            if (Core.Me.HasAura(Auras.ItemPenalty) || DeepDungeonManager.BossFloor)
            {
                return false;
            }

            if (await UsePomander(Pomander.Flight))
            {
                return true;
            }

            if (Settings.Instance.UsePomAlt)
            {
                if (await UsePomander(Pomander.Alteration))
                {
                    return true;
                }
            }

            return await UsePomander(Pomander.Affluence);
        }
    }
}