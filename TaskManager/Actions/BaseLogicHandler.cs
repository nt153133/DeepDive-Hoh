﻿/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System.Threading.Tasks;
using Buddy.Coroutines;
using DeepHoh.Logging;

namespace DeepHoh.TaskManager.Actions
{
    internal class BaseLogicHandler : ITask
    {
        public string Name => "BaseLogicHandler";

        public async Task<bool> Run()
        {
            Logger.Warn("We have reached the Base Logic Handler. This means the bot didn't know what to do.");
            await Coroutine.Sleep(100);
            return true;
        }

        public void Tick()
        {
        }
    }
}