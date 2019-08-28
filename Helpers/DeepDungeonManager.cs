﻿/*
DeepDungeon HOH Party is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using ff14bot;
using ff14bot.Directors;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.RemoteAgents;

namespace DeepHoh.Helpers
{
    /// <summary>
    /// helper class for wrapping director calls
    /// </summary>
    public static class DeepDungeonManager
    {
        public static InstanceContentDirector Director => DirectorManager.ActiveDirector as InstanceContentDirector;

        public static bool BossFloor => Director != null ? (Director.DeepDungeonLevel % 10 == 0) : false;

        public static DDInventoryItem GetInventoryItem(Pomander pom)
        {
            return Director.DeepDungeonInventory[(byte)Constants.PomanderInventorySlot(pom)];
        }

        public static DDInventoryItem[] GetInventoryItems()
        {
            return Director.DeepDungeonInventory;
        }

        public static bool IsCasting => Core.Me.IsCasting;

        public static void UsePomander(Pomander pom)
        {
            AgentModule.GetAgentInterfaceByType<AgentDeepDungeonInformation>().UsePomander(pom);
            Navigator.NavigationProvider.ClearStuckInfo(); // don't trigger antistuck
        }

        public static int PortalStatus => Director.DeepDungeonPortalStatus;
        public static int Level => Director.DeepDungeonLevel;
        public static bool PortalActive => Director.DeepDungeonPortalStatus == 11;
        public static bool ReturnActive => Director.DeepDungeonReturnStatus == 11;
    }
}