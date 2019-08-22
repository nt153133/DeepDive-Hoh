﻿/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */
using Buddy.Coroutines;
using DeepHoh.Logging;
using ff14bot.Managers;
using System;
using System.Threading.Tasks;


namespace DeepHoh.Windows
{
    internal class DeepDungeonMenu
    {
        internal static bool IsOpen => RaptureAtkUnitManager.GetWindowByName(WindowNames.DDmenu) != null;

        internal static async Task OpenSaveMenu()
        {
            AtkAddonControl menu = RaptureAtkUnitManager.GetWindowByName(WindowNames.DDmenu);
            if (menu == null)
            {
                return;
            }

            try
            {
                menu.SendAction(1, 3, 0);
                await Coroutine.Wait(3000, () => DeepDungeonSaveData.IsOpen);
            }
            catch (Exception ex)
            {
                Logger.Verbose("{0}", ex);
            }

        }

        internal static async Task OpenResetMenu()
        {
            AtkAddonControl wind = RaptureAtkUnitManager.GetWindowByName(WindowNames.DDmenu);
            if (wind == null)
            {
                throw new Exception("Open Reset Menu Failed. POTD Menu is not open. (The bot will attempt to correct this issue)");
            }

            wind.SendAction(1, 3, 1);
            await Coroutine.Wait(3000, () => DeepDungeonSaveData.IsOpen);
        }

    }
}