/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using DeepHoh.Helpers;
using DeepHoh.Logging;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.RemoteWindows;

namespace DeepHoh.TaskManager.Actions
{
    internal class LobbyHandler : ITask
    {
        private GameObject _target;

        public string Name => "Lobby";

        public async Task<bool> Run()
        {
            if (!Constants.InExitLevel) return false;

            await Coroutine.Sleep(5000);

            //_target = GameObjectManager.GetObjectByNPCId(EntityNames.LobbyExit);
            _target = GameObjectManager.GameObjects.Where(r => r.NpcId == EntityNames.LobbyExit)
                .OrderBy(r => r.Distance()).FirstOrDefault();
            //Vector3 loc = new Vector3(-10.02527f, 0.01519775f, -150.0115f);

            Navigator.Stop();
            Navigator.Clear();

            TreeRoot.StatusText = "Lobby Room";

            if (_target == null || !_target.IsValid)
            {
                Logger.Warn("Unable to find Lobby Target");
                return false;
            }


            // move closer plz
            if (_target.Location.Distance2D(Core.Me.Location) >= 4.4)
            {
                // await CommonTasks.MoveAndStop(new MoveToParameters(_target.Location, "Lobby Exit"), 1f, true);
                Logger.Verbose("target range" + _target.Location.Distance2D(Core.Me.Location));

                Navigator.Stop();
                //Vector3 exit = GameObjectManager.GetObjectByNPCId(EntityNames.LobbyExit).Location;

                Navigator.PlayerMover.MoveTowards(_target.Location);
                while (_target.Location.Distance2D(Core.Me.Location) >= 4.4)
                {
                    Navigator.PlayerMover.MoveTowards(_target.Location);
                    await Coroutine.Sleep(100);
                }

                //await Buddy.Coroutines.Coroutine.Sleep(1500); // (again, probably better to just wait until distance to destination is < 2.0f or something)
                Navigator.PlayerMover.MoveStop();
                //return false;
            }


            // await Coroutine.Sleep(5000);

            //await Coroutine.Wait(-1, () => Core.Me.InCombat || !Constants.InExitLevel || CommonBehaviors.IsLoading || QuestLogManager.InCutscene);
            //Poi.Clear("Floor has changed or we have entered combat");
            //Navigator.Clear();
            //GameObjectManager.GetObjectByNPCId(EntityNames.LobbyExit)).Location

            /*            if (Core.Me.Location.Distance2D(loc) >= 2)
                        {
                            Navigator.Clear();
                            await CommonTasks.StopMoving();

                            Navigator.Stop();
                            Vector3 exit = GameObjectManager.GetObjectByNPCId(EntityNames.LobbyExit).Location;
                            Navigator.PlayerMover.MoveTowards(exit);
                            await Buddy.Coroutines.Coroutine.Sleep(1500); // (again, probably better to just wait until distance to destination is < 2.0f or something)
                            Navigator.PlayerMover.MoveStop();

                            //GameObjectManager.GetObjectByNPCId(EntityNames.LobbyExit).Location
                            //            if (!Navigator.InPosition(_target.Location, Core.Me.Location, 10))
                            //            {
                            //                if (!await CommonTasks.MoveAndStop(new MoveToParameters(_target.Location, "Moving to Lobby Exit"), 3))
                            //                {
                            //                    Logger.Warn("Failed to move toward the exit?");
                            //                }
                            //                return true;
                            //            }
                            Logger.Verbose("target2 range" + _target.Location.Distance2D(Core.Me.Location));
                        }*/

            _target.Interact();
            await Coroutine.Wait(500, () => SelectYesno.IsOpen);
            await Coroutine.Sleep(1000);
            SelectYesno.ClickYes();
            DeepTracker.EndRun(false);
            return true;
        }

        public void Tick()
        {
            if (_target != null && !_target.IsValid) _target = null;
            if (!Constants.InExitLevel) return;

            _target = GameObjectManager.GameObjects.Where(i => i.NpcId == EntityNames.LobbyExit)
                .OrderBy(i => i.Distance2D(Core.Me.Location)).FirstOrDefault();
        }
    }
}