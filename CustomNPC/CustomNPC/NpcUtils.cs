using Microsoft.Xna.Framework;
using OTA;
using OTA.Logging;
using System;
using System.Linq;

namespace CustomNPCS
{
    public static class NpcUtils
    {
        public static int ActivePlayerCount
        {
            get
            {
				return Terraria.Main.player.Count(x => x != null && x.active);
            }
        }

        public static void LogError(string fmt, params object[] args)
        {
			ProgramLog.Error.Log(fmt, args);
        }

        /// <summary>
        /// TShock.Log.Error

        public static void LogConsole(string fmt, params object[] args)
        {
			ProgramLog.Log(fmt, args);
        }

        public static void MessageAllPlayers(string message, Color color)
        {
			Tools.NotifyAllPlayers(message, color);
        }

        public static void InfoMessageAllPlayers(string message, params object[] args)
        {
			Tools.NotifyAllPlayers(String.Format(message, args), Color.Orange/*TODO*/);
        }

        public static Terraria.NPC GetNPCById(int id)
        {
            Terraria.NPC npc = new Terraria.NPC();
            npc.netDefaults(id);
            return npc;
        }
    }
}