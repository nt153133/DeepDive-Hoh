﻿/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System.Threading.Tasks;
using DeepHoh.Helpers;
using DeepHoh.Logging;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Navigation;

namespace DeepHoh.TaskManager.Actions
{
    /// <summary>
    ///     does an hp check for traps
    /// </summary>
    /// <returns></returns>
    internal class TrapHandler : ITask
    {
        private bool HasTrapAura => Core.Me.HasAnyAura(Auras.Pacification, Auras.Silence, Auras.Toad, Auras.Frog,
            Auras.Toad2, Auras.Odder);


        public string Name => "TrapHandler";

        public async Task<bool> Run()
        {
            if (!HasTrapAura) return false;

            if (CombatTargeting.Instance.FirstEntity == null) return false;

            if (Core.Me.InRealCombat()) return false;
            TreeRoot.StatusText = "Waiting on Trap Auras";
            Logger.Info("Trap auras detected");

            if (Core.Me.HasAura(Auras.Silence) && Settings.Instance.UseEchoDrops)
                if (await Tasks.Coroutines.Common.UseItemById(Items.EchoDrops))
                    return true;
            Navigator.Clear();

            return true;
        }


        public void Tick()
        {
        }
    }
}