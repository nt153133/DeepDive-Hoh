﻿/*
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
using Buddy.Coroutines;
using Clio.Utilities.Helpers;
using DeepHoh.Memory;
using DeepHoh.Providers;
using DeepHoh.Enums;
using DeepHoh.Helpers;
using DeepHoh.Logging;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Directors;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;

namespace DeepHoh.Tasks.Coroutines
{
    internal class Common
    {
        /// <summary>
        /// Cancel player aura
        /// </summary>
        /// <param name="aura">Aura id to cancel</param>
        /// <returns></returns>
        internal static async Task<bool> CancelAura(uint aura)
        {
            var auraname = '"' + DataManager.GetAuraResultById(aura).CurrentLocaleName + '"';
            while (Core.Me.HasAura(aura))
            {
                Logger.Verbose("Cancel Aura {0}", auraname);
                ChatManager.SendChat("/statusoff " + auraname);
                await Coroutine.Yield();
            }
            return true;
        }

        internal static ItemState PomanderState = ItemState.None;

        internal static async Task<bool> UsePomander(Pomander number, uint auraId = 0)
        {
            if (Core.Me.HasAura(Auras.ItemPenalty) && number != Pomander.Serenity) return false;

            //cannot use pomander while under the auras of rage / lust
            if (Core.Me.HasAnyAura(Auras.Lust, Auras.Rage)) return false;

            var data = DeepDungeonManager.GetInventoryItem(number);
            if (data.Count == 0) return false;

            if (data.HasAura) return false;

            if (Core.Me.HasAura(auraId) &&
                    Core.Me.GetAuraById(auraId).TimespanLeft > TimeSpan.FromMinutes(1)) return false;

            await Coroutine.Wait(5000, () => !DeepDungeonManager.IsCasting);

            var cnt = data.Count;
            await Coroutine.Wait(5000, () => !DeepDungeonManager.IsCasting);
            var wt = new WaitTimer(TimeSpan.FromSeconds(30));
            wt.Reset();
            while (cnt == data.Count && !wt.IsFinished)
            {
                Logger.Verbose("Using Pomander: {0}", number);
                DeepDungeonManager.UsePomander(number);
                await Coroutine.Sleep(150);

                await Coroutine.Wait(5000, () => !DeepDungeonManager.IsCasting);
                data = DeepDungeonManager.GetInventoryItem(number);
            }

            //TODO this is probabbly stored somewhere in the client...
            switch (number)
            {
                case Pomander.Rage:
                    PomanderState = ItemState.Rage;
                    break;

                case Pomander.Lust:
                    PomanderState = ItemState.Lust;
                    break;

                case Pomander.Resolution:
                    PomanderState = ItemState.Resolution;
                    break;
            }

            return true;
        }

        private static List<uint> PotIDs => Constants.Pots.Select(i => i.Id).ToList();

        /// <summary>
        /// can we use a potion
        /// </summary>
        /// <returns></returns>
        internal static bool CanUsePot()
        {
            var pot = InventoryManager.FilledSlots.FirstOrDefault(r => PotIDs.Contains(r.RawItemId));
            return pot != null && pot.CanUse();
        }

        internal static bool CanUseItem(int itemid)
        {
            var pot = InventoryManager.FilledSlots.FirstOrDefault(r => r.RawItemId == itemid);
            return pot != null && pot.CanUse();
        }

        internal static async Task<bool> UseSustain()
        {
            if (!Settings.Instance.UseSustain) return false;
            if (Core.Me.CurrentHealthPercent > 50) return false;
            if (Core.Me.HasAura(Auras.Sustain)) return false;
            var i = InventoryManager.FilledSlots.FirstOrDefault(r => r.RawItemId == Items.SustainingPotion);
            if (i == null) return false;
            return i.CanUse() && await UseItem(i);
        }

        /// <summary>
        /// Recover hp.
        /// </summary>
        /// <param name="force"></param>
        /// <returns></returns>
        internal static async Task<bool> UsePots(bool force = false)
        {
            if (Core.Me.CurrentHealthPercent > 99) return false;

            var tenpcnt = Core.Me.MaxHealth;
            foreach (var pots in Constants.Pots.Select( i=> new
                {
                    pot = InventoryManager.FilledSlots.FirstOrDefault(r => r.RawItemId == i.Id),
                    data = i,
                    sort = i.Recovery / tenpcnt
            } ).Where(i => i.pot != null)
                .Where(i => i.data.Recovery <= Core.Me.MissingHealth())
                .OrderByDescending(i => i.sort)
                )
            {
                var pot = pots.pot;
                if (pot.CanUse())
                {
                    Logger.Info($"Attempting to recover: {pots.data.Recovery} hp");

                    if (await UseItem(pot))
                    {
                        return true;
                    }
                }
                await Coroutine.Yield();
            }
            return false;
        }

        internal static async Task<bool> UseItemById(int id)
        {
            var pot = InventoryManager.FilledSlots.FirstOrDefault(r => r.RawItemId == id);
            if (pot != null && pot.CanUse() && await UseItem(pot))
            {
                return true;
            }
            return false;
        }

        private static async Task<bool> UseItem(BagSlot i)
        {
            Logger.Warn("Attempting to use {0}", i.Item.CurrentLocaleName);
            i.UseItem();
            await Coroutine.Yield();
            return true;
        }


    }
}