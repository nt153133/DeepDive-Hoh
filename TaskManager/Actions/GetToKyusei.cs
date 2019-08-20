/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System.Threading.Tasks;
using Buddy.Coroutines;
using DeepHoh.Logging;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Pathing;

namespace DeepHoh.TaskManager.Actions
{
    internal class GetToKyusei : ITask
    {
        public string Name => "GetToRyusei";

        public async Task<bool> Run()
        {
            //we are inside POTD
            if (Constants.InDeepDungeon || Constants.InExitLevel) return false;

            if (WorldManager.ZoneId != Constants.RubySeaZoneID || Core.Me.Distance2D(Constants.KyuseiNpcPosition) > 110)
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
                Logger.Verbose("Still Teleporting");

                await Coroutine.Sleep(1000);
                return true;
            }


            if (GameObjectManager.GetObjectByNPCId(Constants.KyuseiNpcId) == null || GameObjectManager
                    .GetObjectByNPCId(Constants.KyuseiNpcId).Distance2D(Core.Me.Location) > 4f)
            {
//                var moving = MoveResult.GeneratingPath;
//                while (!(moving == MoveResult.Done ||
//                         moving == MoveResult.ReachedDestination ||
//                         moving == MoveResult.Failed ||
//                         moving == MoveResult.Failure ||
//                         moving == MoveResult.PathGenerationFailed))
//                {
//                    moving = Flightor.MoveTo(new FlyToParameters(GameObjectManager.GetObjectByNPCId(Constants.KyuseiNpcId).Location));
//
//                    await Coroutine.Yield();
//                }
                Logger.Verbose("at Move");
                return await CommonTasks.MoveAndStop(new MoveToParameters(GameObjectManager.GetObjectByNPCId(Constants.KyuseiNpcId).Location, "Moving toward NPC"), 4f, true);
                //return await CommonTasks.MoveAndStop(new MoveToParameters(Constants.KyuseiNpcPosition, "Moving toward NPC"), 5f, true);
            }

            return false;
        }

        public void Tick()
        {
        }
    }
}