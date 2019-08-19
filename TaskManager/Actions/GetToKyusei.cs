/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Pathing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepHoh.Logging;
using ff14bot.Enums;

namespace DeepHoh.TaskManager.Actions
{
    class GetToKyusei : ITask
    {
        public string Name => "GetToRyusei";

        public async Task<bool> Run()
        {
            //we are inside POTD
            if (Constants.InDeepDungeon || Constants.InExitLevel) return false;

            if (WorldManager.ZoneId != Constants.RubySeaZoneID ||
               GameObjectManager.GetObjectByNPCId(Constants.KyuseiNpcId) == null ||
               GameObjectManager.GetObjectByNPCId(Constants.KyuseiNpcId).Distance2D(Core.Me.Location) > 300)
            {
                if (Core.Me.IsCasting)
                {
                    await Coroutine.Sleep(500);
                    return true;
                }

                if (!WorldManager.TeleportById(106))
                {
                    Logger.Error("We can't get to The Ruby Sea. something is very wrong...");
                    TreeRoot.Stop();
                    return false;
                }
                await Coroutine.Sleep(1000);
                return true;

            }
            if (GameObjectManager.GetObjectByNPCId(Constants.KyuseiNpcId) == null || GameObjectManager.GetObjectByNPCId(Constants.KyuseiNpcId).Distance2D(Core.Me.Location) > 5f)
            {
                var moving = MoveResult.GeneratingPath;
                while (!(moving == MoveResult.Done ||
                         moving == MoveResult.ReachedDestination ||
                         moving == MoveResult.Failed ||
                         moving == MoveResult.Failure ||
                         moving == MoveResult.PathGenerationFailed))
                {
                    moving = Flightor.MoveTo(new FlyToParameters(Constants.KyuseiNpcPosition));

                    await Coroutine.Yield();
                }


                //return await CommonTasks.MoveAndStop(new MoveToParameters(Constants.KyuseiNpcPosition, "Moving toward NPC"), 5f, true);
            }
            return false;
        }

        public void Tick()
        {
            
        }
    }
}
