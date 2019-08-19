/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Clio.Utilities;
using DeepHoh.Memory;
using DeepHoh.Properties;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.RemoteAgents;
using Newtonsoft.Json;

namespace DeepHoh
{
    public static class PoiTypes
    {
        public const int ExplorePOI = 9;
        public const int UseBeaconOfReturn = 10;
    }

    /// <summary>
    ///     Notable mobs in Deep Dungeon
    /// </summary>
    internal static class Mobs
    {
        internal static uint PalaceHornet = 4981;
        internal static uint PalaceSlime = 4990;
        internal static uint HeavenlyShark = 7272;
    }

    /// <summary>
    ///     Various entity Ids present in Deep Dungeon
    /// </summary>
    internal static class EntityNames
    {
        internal static uint TrapCoffer = 2005808;
        
        internal static uint GoldCoffer = 2007358;
        internal static uint SilverCoffer = 2007357;

        //internal static uint BandedCoffer = 2007543;

        internal static uint[] MimicCoffer = {2006020, 2006022};

        internal static uint Hidden = 2007542;
        internal static uint BandedCoffer = 2007543;

        internal static uint FloorExit = 2009507;
        internal static uint BossExit = 2005809;

        internal static uint LobbyExit = 2009523;
        internal static uint LobbyEntrance = 2009524;

        internal static uint BeaconofReturn = 2009506;

        #region Pets

        internal static uint RubyCarby = 5478;

        internal static uint Garuda = 1404;
        internal static uint TopazCarby = 1400;
        internal static uint EmeraldCarby = 1401;
        internal static uint Titan = 1403;
        internal static uint Ifrit = 1402;

        internal static uint Eos = 1398;
        internal static uint Selene = 1399;

        internal static uint Rook = 3666;
        internal static uint Bishop = 3667;

        #endregion
    }

    internal static class Items
    {
        internal static int Antidote = 4564;
        internal static int EchoDrops = 4566;
        internal static int SustainingPotion = 20309;
    }

    internal static class Auras
    {
        internal static uint Frog = 1101;
        internal static uint Toad = 439;
        internal static uint Toad2 = 441;
        internal static uint Chicken = 1102;
        internal static uint Imp = 1103;
        internal static uint Odder = 1546;

        internal static uint Lust = 565;
        internal static uint Rage = 565;

        internal static uint Steel = 1100;
        internal static uint Strength = 687;

        internal static uint Sustain = 184;

        public static uint Enervation = 546;
        public static uint Pacification = 620;
        public static uint Silence = 7;


        public static uint[] Poisons =
        {
            18, 275, 559, 560, 686, 801
        };

        #region Floor Debuffs

        internal static uint Pox = 1087;
        internal static uint Blind = 1088;
        internal static uint HpDown = 1089;
        internal static uint DamageDown = 1090;
        internal static uint Amnesia = 1092;

        internal static uint ItemPenalty = 1094;
        internal static uint SprintPenalty = 1095;

        internal static uint KnockbackPenalty = 1096;
        internal static uint NoAutoHeal = 1097;

        #endregion
    }

    internal static class Spells
    {
        internal static uint LustSpell = 6274;
        internal static uint RageSpell = 6273;
        internal static uint ResolutionSpell = 6871;
    }

    internal static class WindowNames
    {
        internal static string DDmenu = "DeepDungeonMenu";
        internal static string DDsave = "DeepDungeonSaveData";
        internal static string DDmap = "DeepDungeonMap";
        internal static string DDStatus = "DeepDungeonStatus";
    }

    internal class Potion
    {
        [JsonProperty("Id")] public uint Id;

        [JsonProperty("Level")] public uint Level;

        [JsonProperty("Max")] public uint[] Max;

        [JsonProperty("Rate")] public float[] Rate;

        public float RecoverMax => Core.Me.MaxHealth * Rate[1];
        public uint Recovery => (uint) Math.Min(RecoverMax, Max[1]);

        public float LevelScore => Max[1] / RecoverMax;
    }


    internal static class Constants
    {
        internal static Vector3 KyuseiNpcPosition = new Vector3(-2.02948f, 3.005981f, -611.3528f);
        internal static uint KyuseiNpcId = 1025846;

        internal static uint RubySeaZoneID = 613; //Adapted to The Ruby Sea (Aetheryte ID is 106)

        //570 is staging.
        //561 - 565 are 1-50
        //593 - 607 are 51-200
        internal static uint[] DeepDungeonRawIds;

        internal static uint[] Exits = {EntityNames.FloorExit, EntityNames.BossExit, EntityNames.LobbyExit};

        //2002872 = some random thing that the bot tries to target in boss rooms. actual purpose unknown
        //7395 Trap ID
        internal static uint[] IgnoreEntity =
        {
            7395, 5402, EntityNames.FloorExit, EntityNames.BeaconofReturn, EntityNames.LobbyEntrance, 2002872,
            EntityNames.RubyCarby, EntityNames.EmeraldCarby, EntityNames.TopazCarby, EntityNames.Garuda,
            EntityNames.Titan, EntityNames.Ifrit, EntityNames.Eos, EntityNames.Selene, EntityNames.Rook,
            EntityNames.Bishop
        };

        internal static uint MapVersion = 4;

        internal static Language Lang;

        internal static int LobbyMapID = 780;

        static Constants()
        {
            Maps = new Dictionary<uint, uint>
            {
                //mapid - wall file
                {770, 0},
                {771, 0},
                {772, 0},
                {773, 0},
                {774, 0},
                {775, 0},
                {776, 0},
                {777, 0},
                {778, 0},
                {779, 0} //,
                
                //{599, 8},
                //{600, 8},
                //{601, 9},
                //{602, 9},
                //{603, 7},
                //{604, 7},
                //{605, 7},
                //{606, 7},
                //{607, 7}
            };

            DeepDungeonRawIds = Maps.Keys.ToArray();
        }

        /// <summary>
        ///     returns true if we are in any of the Deep Dungeon areas.
        /// </summary>
        internal static bool InDeepDungeon => DeepDungeonRawIds.Contains(WorldManager.ZoneId);

        /// <summary>
        ///     Pull range (minimum of 8)
        /// </summary>
        internal static float ModifiedCombatReach
        {
            get
            {
                if (!PartyManager.IsInParty)
                    return 17;
                return Math.Max(8, RoutineManager.Current.PullRange + Settings.Instance.PullRange);
            }
        }

        //cn = 3
        //64 = 2
        //32 = 1
        internal static AgentDeepDungeonSaveData GetSaveInterface()
        {
            return AgentModule.GetAgentInterfaceByType<AgentDeepDungeonSaveData>();
        }

        public static void INIT()
        {
            var field = (Language) typeof(DataManager).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .First(i => i.FieldType == typeof(Language)).GetValue(null);

            Lang = field;

            OffsetManager.Init();
        }

        #region DataAsResource

        internal static Dictionary<uint, uint> Maps;

        internal static uint[] TrapIds =
        {
            2007182,
            2007183,
            2007184,
            2007185,
            2007186,
            2009504
        };

        private static Potion[] _pots;
        internal static Potion[] Pots => _pots ?? (_pots = loadResource<Potion[]>(Resources.pots));

        public static bool InExitLevel => WorldManager.ZoneId == Constants.LobbyMapID; //Maybe the Exit world?

        /// <summary>
        ///     loads a json resource file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file"></param>
        /// <returns></returns>
        private static T loadResource<T>(string text)
        {
            //string text;
            //using (var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file))
            //{
            //    using (var streamReader = new StreamReader(manifestResourceStream))
            //    {
            //        text = streamReader.ReadToEnd();
            //    }
            //}
            return JsonConvert.DeserializeObject<T>(text);
        }

        #endregion
    }
}