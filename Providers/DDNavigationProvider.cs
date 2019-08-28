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
using DeepHoh.Memory;
using DeepHoh.Properties;
using ff14bot;
using ff14bot.Directors;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Overlay3D;
using ff14bot.Pathing;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.ServiceClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace DeepHoh.Providers
{
    internal class DDNavigationProvider : WrappingNavigationProvider
    {
        private uint _detourLevel = 0;

        private static Dictionary<uint, List<Vector3>> _walls;
        private static Dictionary<uint, bool> _hit;
        internal static Dictionary<uint, bool> Walls => _hit;

        private List<uint> _traps;

        internal static List<Vector3> Traps { get; private set; }

        private static List<Vector3> _map;
        private const float TrapSize = 2.4f;

        private int _floorId;
        private HashSet<uint> activeWalls;

        private void SetupDetour()
        {
            //if we are not on the lobby & we have already reloaded detour for this floor return
            if (_floorId == DeepDungeonManager.Level)
            {
                return;
            }

            _floorId = DeepDungeonManager.Level;

            uint map = Constants.Maps[WorldManager.RawZoneId];
            if (_detourLevel != map)
            {
                _detourLevel = map;
                _walls = LoadWalls(map);
            }

            //load the map
            _hit = new Dictionary<uint, bool>();
            _traps = new List<uint>();
            Traps = new List<Vector3>();
            _map = new List<Vector3>();

            Logger.Verbose("Updating navigation {0}", map);
            wallList.Clear();
            trapList.Clear();
            activeWalls = FindWalls();

            WallCheck();

            Logger.Debug("Game objects: unit \t NpcID \t ObjID");
            IEnumerable<GameObject> units = GameObjectManager.GameObjects;
            foreach (GameObject unit in units)
            {
                Logger.Debug("Game object: {0,-25} - {1,20} - {2, 15}", unit.Name, unit.NpcId, unit.ObjectId);
            }

            foreach (uint unit in activeWalls)
            {
                Logger.Debug("WallHash: {0,-25}", unit);
            }

            Logger.Debug("Auras");
            foreach (var a in DeepDungeonManager.GetInventoryItems())
            {
                Logger.Debug("Items ID: {0} Count: {1}", a.Id, a.Count);
            }
        }

        private static Dictionary<uint, List<Vector3>> LoadWalls(uint map)
        {
            string text;
            if (map == 70)
            {
                return new Dictionary<uint, List<Vector3>>();
            }

            switch (map)
            {
                case 1:
                    text = Resources._1;
                    break;

                case 2:
                    text = Resources._2;
                    break;

                case 3:
                    text = Resources._3;
                    break;

                case 4:
                    text = Resources._4;
                    break;

                case 5:
                    text = Resources._5;
                    break;

                case 6:
                    text = Resources._6;
                    break;

                case 7:
                    text = Resources._7;
                    break;

                case 8:
                    text = Resources._8;
                    break;

                case 9:
                    text = Resources._9;
                    break;

                default:
                    text = "";
                    break;
            }

            return JsonConvert.DeserializeObject<Dictionary<uint, List<Vector3>>>(text);
        }

        private static HashSet<BoundingBox3> wallList = new HashSet<BoundingBox3>();
        private static HashSet<BoundingCircle> trapList = new HashSet<BoundingCircle>();

        public DDNavigationProvider(NavigationProvider original) : base(original)
        {
        }

        public override MoveResult MoveTo(MoveToParameters location)
        {
            //if we aren't in POTD default to the original mover right away.
            if (!Constants.Maps.ContainsKey(WorldManager.RawZoneId))
            {
                return Original.MoveTo(location);
            }

            SetupDetour();
            AddBlackspots();
            WallCheck();

            location.WorldState = new WorldState() { MapId = WorldManager.ZoneId, Walls = wallList, Avoids = trapList };
            return Original.MoveTo(location);
        }

        public bool WallCheck()
        {
            bool updated = false;
            Vector3 me = Core.Me.Location;
            wallList.Clear();
            if (_walls != null)
            {
                foreach (KeyValuePair<uint, List<Vector3>> id in _walls.Where(i => i.Value[0].Distance2D(Core.Me.Location) < 5 && !_hit.ContainsKey(i.Key) && !activeWalls.Contains(i.Key)))
                {
                    Vector3 wall1 = id.Value[1];
                    wall1.Y -= 5;

                    Vector3 wall2 = id.Value[2];
                    wall2.Y -= 10;

                    wallList.Add(new BoundingBox3() { Min = wall1, Max = wall2 });
                    _hit.Add(id.Key, true);
                    updated = true;
                }
            }

            Logger.Debug($"[walls] {string.Join(", ", _hit.Keys)}");

            return updated;
        }

        //private int floorCache;
        //private List<uint> _wallcache;

        private HashSet<uint> FindWalls()
        {
            IntPtr director = DirectorManager.ActiveDirector.Pointer;

            if (director == IntPtr.Zero)
            {
                return new HashSet<uint>();
            }

            byte v187A = Core.Memory.Read<byte>(director + Offsets.DDMapGroup);

            IntPtr v3 = director + Offsets.Map5xStart + (v187A * Offsets.Map5xSize);
            ushort v332 = Core.Memory.Read<ushort>(v3 + Offsets.WallStartingPoint);

            IntPtr v29 = v3 + 0x10;
            IntPtr v7_location = v29;

            short[] v7 = Core.Memory.ReadArray<short>(v7_location, 5);
            HashSet<uint> wallset = new HashSet<uint>();

            int v5 = 0;

            uint[] types = new uint[] { 1, 2, 4, 8 }; //taken from the client

            for (int v30 = 5; v30 > 1; v30--)
            {
                for (int v8 = 0; v8 < 5; v8++)
                {
                    if (v7[v8] != 0)
                    {
                        IntPtr v9 = v3 + 0x14 * (v7[v8] - v332);

                        // var wall = Core.Memory.Read<uint>(v9 + Offsets.UNK_StartingCircle);
                        //wallset.Add(wall);

                        byte @byte = Core.Memory.Read<byte>(director + v5 + Offsets.WallGroupEnabled);
                        uint[] walls = Core.Memory.ReadArray<uint>(v9 + Offsets.Starting, 4);
                        for (int v16 = 0; v16 < 4; v16++)
                        {
                            if (walls[v16] < 2)
                            {
                                continue;
                            }

                            if ((@byte & types[v16]) != 0) //==0 is closed != 0 is "open"
                            {
                                wallset.Add(walls[v16]);
                            }
                        } //for3
                    }

                    v5++;
                }

                v7_location = v29 + 0xc;
                v7 = Core.Memory.ReadArray<short>(v7_location, 5);
                v29 = v29 + 0xc;
            }

            //_wallcache = wallset;
            return wallset;
        }

        internal static void Render(object sender, DrawingEventArgs e)
        {
            if (!Settings.Instance.DebugRender)
            {
                return;
            }

            if (!Constants.InDeepDungeon)
            {
                return;
            }

            try
            {
                I3DDrawer drawer = e.Drawer;

                //if (_path != null)
                //{
                //    var start = (Vector3)_path.First();
                //    foreach (var x in _path)
                //    {
                //        drawer.DrawLine(start, (Vector3)x, Color.Black);
                //        start = (Vector3)x;
                //    }
                //}

                if (_hit == null)
                {
                    return;
                }

                //List<uint> active = new List<uint>();
                //active.AddRange(_hit.Keys);

                //foreach (var x in active)
                //{
                //    var extents = Bound(_walls[x][2], _walls[x][1]);
                //    drawer.DrawBox(_walls[x][0], extents, Color.FromArgb(150, Color.Goldenrod));
                //}

                List<BoundingBox3> service = new List<BoundingBox3>();
                service.AddRange(wallList);

                foreach (BoundingBox3 x in service)
                {
                    Vector3 extents = Bound(x.Min, x.Max);
                    drawer.DrawBox(Vector3.Lerp(x.Min, x.Max, 0.5f), extents, Color.FromArgb(100, Color.Turquoise));
                }

                List<BoundingCircle> tarp = new List<BoundingCircle>();
                tarp.AddRange(trapList);
                foreach (BoundingCircle t in tarp)
                {
                    drawer.DrawCircleOutline(t.Center, t.Radius, Color.FromArgb(100, Color.Red));
                }
            }
            catch (Exception) { }
        }

        public static Vector3 Bound(Vector3 a, Vector3 b)
        {
            float minX = Math.Min(a.X, b.X);
            float minY = Math.Min(a.Y, b.Y);
            float minZ = Math.Min(a.Z, b.Z);

            float maxX = Math.Max(a.X, b.X);
            float maxY = Math.Max(a.Y, b.Y);
            float maxZ = Math.Max(a.Z, b.Z);

            return new Vector3((maxX - minX), (maxY - minY), (maxZ - minZ)) / 2;
        }

        private void AddBlackspots()
        {
            //if we have added blackspots already OR there aren't any traps
            if (!GameObjectManager.GameObjects.Any(
                i => i.Location != Vector3.Zero && Constants.TrapIds.Contains(i.NpcId) && !_traps.Contains(i.ObjectId)))
            {
                return;
            }

            {
                Logger.Verbose("Adding Black spots {0}", GameObjectManager.GameObjects.Count(i => i.Location != Vector3.Zero && Constants.TrapIds.Contains(i.NpcId)));
                foreach (GameObject i in GameObjectManager.GameObjects.Where(i => i.Location != Vector3.Zero && Constants.TrapIds.Contains(i.NpcId) && !_traps.Contains(i.ObjectId) && i.IsVisible))
                {
                    Logger.Verbose($"[{i.NpcId}] {i.ObjectId} - {i.Location}");
                    //_detour.AddBlackspot(i.Location, TrapSize);
                    trapList.Add(new BoundingCircle() { Center = i.Location, Radius = TrapSize });
                    _traps.Add(i.ObjectId);
                    Traps.Add(i.Location);
                }
            }

            IEnumerable<GameObject> units = GameObjectManager.GameObjects.Where(j => j.Location != Vector3.Zero);
            foreach (GameObject unit in units)
            {
                if (unit == null)
                {
                    Logger.Debug("TRAP NULL unit: {0}", unit.NpcId);
                }

                if (unit.Name == "" && !_traps.Contains(unit.ObjectId) && unit.NpcId != 2002872)
                {
                    Logger.Debug("TRAP empty string name unit: {0}", unit.NpcId);
                }

                if (unit.Name == "" && !Constants.TrapIds.Contains(unit.NpcId) && unit.NpcId != 2002872)
                {
                    Logger.Debug("TRAP? {0}-{1}-{2}-{3} NOT IN LIST", unit, unit.NpcId, unit.ObjectId, unit.IsVisible);
                }
            }
        }
    }

    internal static class StraightPathHelper
    {
        static StraightPathHelper()
        {
            Method = typeof(NavigationProvider).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(i => i.ReturnType == typeof(List<Vector3>));
        }

        private static MethodInfo Method;

        /// <summary>
        /// invoke the get straight path information.
        /// </summary>
        /// <returns></returns>
        internal static List<Vector3> GetStraightPath()
        {
            if (Method == null)
            {
                Logger.Warn($"GSP is null?");
                return null;
            }

            return RealStraightPath();
        }

        internal static List<Vector3> RealStraightPath()
        {
            return (List<Vector3>)Method.Invoke((Navigator.NavigationProvider as WrappingNavigationProvider).Original, new object[] { });
        }
    }
}