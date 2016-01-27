using OTA.Plugin;
using OTA.Command;
using OTA.Logging;
using Terraria;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using OTA;
using OTA.Mod;
using OTA.Mod.Npc;
using System.Linq;

namespace CustomNPCS
{
    public class NPCFunctions
    {
        public OTANpc mainNPC { get; set; }
        private IList<CustomNPCLoot> loots;
        private static Random rand = new Random();

        private void Transform()
        {
            int oldlife = mainNPC.Npc.life;
            //mainNPC.name = 
        }

        public void SelfHealing(int amount)
        {
            mainNPC.Npc.life = Math.Min(mainNPC.Npc.life, mainNPC.Npc.life + amount);
        }

        private void TeleportNPC(OTANpc mainNPC, int x, int y)
        {
            TeleportNPC(mainNPC, x * 16, y * 16);
        }

        public Vector2 ReturnPos(OTANpc mainNPC)
        {
            return new Vector2(mainNPC.Npc.position.X, mainNPC.Npc.position.Y);
        }

        public void OnDeath(OTANpc mainNPC)
        {
            if (mainNPC.Npc.life == 0)
            {
                if (loots != null)
                {
                    foreach (var loot in loots)
                    {
                        foreach (var pfx in loot.itemPrefix)
                            Terraria.Item.NewItem((int)this.mainNPC.Npc.position.X, (int)this.mainNPC.Npc.position.Y, this.mainNPC.Npc.width, this.mainNPC.Npc.height, loot.itemID, loot.itemStack, false, pfx);
                    }
                }
            }
        }

        public static int AliveCount(string OTANpc)
        {
            int count = 0;
            foreach (var vanillaNpc in Main.npc)
            {
                if (vanillaNpc.Mod != null && vanillaNpc.Mod is OTANpc)
                {
                    var mod = vanillaNpc.Mod as OTANpc;
                    if (mod.Npc == null) continue;

                    if (mod.Npc.name.Equals(OTANpc, StringComparison.InvariantCultureIgnoreCase) && mod.Npc.active)
                    {
                        count++;
                        return count;
                    }
                    else if (mod.Npc.displayName.Equals(OTANpc, StringComparison.InvariantCultureIgnoreCase) && mod.Npc.active)
                    {
                        count++;
                        return count;
                    }
                }
            }
            return count;
        }

        public static bool HealthAbove(int npcid, int health)
        {
            NPC mainNPC = Main.npc[npcid];
            if (mainNPC == null) return false;

            return mainNPC.life >= health;
        }

        /// <summary>
        /// Checks if the NPC's current health is below the passed amount
        /// </summary>
        /// <param name="Health"></param>
        /// <returns></returns>
        public static bool HealthBelow(int npcid, int health)
        {
            NPC mainNPC = Main.npc[npcid];
            if (mainNPC == null) return false;

            return mainNPC.life <= health;
        }

        #region Effect Nearby Players Functions

        public void MessageNearByPlayers(OTANpc mainNPC, int distance, string message, Color color)
        {
            Vector2 temp = ReturnPos(mainNPC);
            int squaredist = distance * distance;

			foreach (var obj in Terraria.Main.player)
			{
				if (obj == null || !obj.active) continue;

				if (Vector2.DistanceSquared (temp, obj.position) <= squaredist)
				{
					obj.SendMessage (message, color);
				}
			}
        }

        public static void DebuffNearbyPlayers(int debuffid, int seconds, int npcindex, int distance)
        {
            NPC mainNPC = Main.npc[npcindex];
            if (mainNPC == null) return;

            foreach (var player in PlayersNearBy(mainNPC.position, distance))
            {
                player.AddBuff(debuffid, seconds *60);
                NetMessage.SendData(55, number: player.whoAmI, number2: debuffid, number3: seconds *60);
            }
        }

        public static List<Player> PlayersNearBy(Vector2 position, int distance)
        {
            int squaredist = (distance * 16) * (distance * 16);

            List<Player> playerlist = new List<Player>();
            foreach (Player player in Terraria.Main.player)
            {
                if (player == null || player.dead || !player.active) continue;

                if (Vector2.DistanceSquared(player.position, position) <= squaredist)
                {
                    playerlist.Add(player);
                }
            }

            return playerlist;
        }

        #endregion

        #region Spawning Functions

        public static int SpawnNPCAtLocation(int x, int y, NPC mainNPC)
        {
            return SpawnCustomNPC(x, y, mainNPC);
        }

        public static int SpawnNPCAtLocation(int x, int y, OTANpc mainNPC)
        {
            return SpawnCustomNPC(x, y, mainNPC);
        }

        public static int SpawnNPCAroundNPC(int npcindex, ShotTile shottile, OTANpc mainNPC)
        {
            NPC npc = Main.npc[npcindex];
            if (npc == null) return -1;

            int x = (int)(npc.position.X + shottile.X);
            int y = (int)(npc.position.Y + shottile.Y);

            return SpawnCustomNPC(x, y, mainNPC);
        }

        public static int SpawnNPCAroundNPC(int npcindex, ShotTile shottile, int npcid)
        {
            NPC npc = Main.npc[npcindex];
            if (npc == null) return -1;

            int x = (int)(npc.position.X + shottile.X);
            int y = (int)(npc.position.Y + shottile.Y);

            return SpawnNPC(x, y, npcid);
        }

        private static int SpawnNPC(int x, int y, int npcid)
        {
            if (npcid == 200)
            {
                //DEBUG
                NpcUtils.LogConsole("DEBUG Spawning FAILED (mobcap) at {0}, {1} for npcID {2}", x, y, npcid);
                //DEBUG
                return -1;
            }

            NPC.NewNPC(x, y, npcid);

            NetMessage.SendData ((int)OTA.Packet.NPC_INFO, -1, -1, "", npcid);

            return npcid;
        }

        public static int SpawnMobAroundPlayer(Terraria.Player player, OTANpc mainNPC)
        {
            const int SpawnSpaceX = 3;
            const int SpawnSpaceY = 3;

            //Search for a location
            int screenTilesX = (int)(NPC.sWidth / 16f);
            int screenTilesY = (int)(NPC.sHeight / 16f);
            int spawnRangeX = (int)(screenTilesX * 0.7);
            int spawnRangeY = (int)(screenTilesY * 0.7);
            int safeRangeX = (int)(screenTilesX * 0.52);
            int safeRangeY = (int)(screenTilesY * 0.52);

			Vector2 position = player.position;

            int playerTileX = (int)(position.X / 16f);
            int playerTileY = (int)(position.Y / 16f);

            int spawnRangeMinX = Math.Max(0, Math.Min(Main.maxTilesX, playerTileX - spawnRangeX));
            int spawnRangeMaxX = Math.Max(0, Math.Min(Main.maxTilesX, playerTileX + spawnRangeX));
            int spawnRangeMinY = Math.Max(0, Math.Min(Main.maxTilesY, playerTileY - spawnRangeY));
            int spawnRangeMaxY = Math.Max(0, Math.Min(Main.maxTilesY, playerTileY + spawnRangeY));

            int safeRangeMinX = Math.Max(0, Math.Min(Main.maxTilesX, playerTileX - safeRangeX));
            int safeRangeMaxX = Math.Max(0, Math.Min(Main.maxTilesX, playerTileX + safeRangeX));
            int safeRangeMinY = Math.Max(0, Math.Min(Main.maxTilesY, playerTileY - safeRangeY));
            int safeRangeMaxY = Math.Max(0, Math.Min(Main.maxTilesY, playerTileY + safeRangeY));

            int spawnX = 0;
            int spawnY = 0;

            bool found = false;
            int attempts = 0;
            while (attempts < 50)
            {
                int testX = rand.Next(spawnRangeMinX, spawnRangeMaxX);
                int testY = rand.Next(spawnRangeMinY, spawnRangeMaxY);

                var testTile = Main.tile[testX, testY];
                if (testTile.nactive() && Main.tileSolid[testTile.type])
                {
                    attempts++;
                    continue;
                }

                if (!Main.wallHouse[testTile.wall])
                {
                    for (int y = testY; y < Main.maxTilesY; y++)
                    {
                        var test = Main.tile[testX, y];
                        if (test.nactive() && Main.tileSolid[test.type])
                        {
                            if (testX < safeRangeMinX || testX > safeRangeMaxX || y < safeRangeMinY || y > safeRangeMaxY)
                            {
                                spawnX = testX;
                                spawnY = y;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        attempts++;
                    }

                    int spaceMinX = spawnX - (SpawnSpaceX / 2);
                    int spaceMaxX = spawnX + (SpawnSpaceX / 2);
                    int spaceMinY = spawnY - SpawnSpaceY;
                    int spaceMaxY = spawnY;
                    if (spaceMinX < 0 || spaceMaxX > Main.maxTilesX)
                    {
                        attempts++;
                        continue;
                    }

                    if (spaceMinY < 0 || spaceMaxY > Main.maxTilesY)
                    {
                        attempts++;
                        continue;
                    }

                    if (found)
                    {
                        for (int x = spaceMinX; x < spaceMaxX; x++)
                        {
                            for (int y = spaceMinY; y < spaceMaxY; y++)
                            {
                                if (Main.tile[x, y].nactive() && Main.tileSolid[Main.tile[x, y].type])
                                {
                                    found = false;
                                    break;
                                }

                                if (Main.tile[x, y].lava())
                                {
                                    found = false;
                                    break;
                                }
                            }
                        }

                        if (!found)
                        {
                            attempts++;
                            continue;
                        }
                    }

                    if (spawnX >= safeRangeMinX && spawnX <= safeRangeMaxX)
                    {
                        if (!found)
                        {
                            attempts++;
                            continue;
                        }
                    }
                }
            }

            if (found)
            {
                return SpawnCustomNPC((spawnX * 16) + 8, spawnY * 16, mainNPC);
            }

            return -1;
        }

        public static int SpawnMobAroundPlayer(Terraria.Player player, NPC mainNPC)
        {
            const int SpawnSpaceX = 3;
            const int SpawnSpaceY = 3;

            //Search for a location
            int screenTilesX = (int)(NPC.sWidth / 16f);
            int screenTilesY = (int)(NPC.sHeight / 16f);
            int spawnRangeX = (int)(screenTilesX * 0.7);
            int spawnRangeY = (int)(screenTilesY * 0.7);
            int safeRangeX = (int)(screenTilesX * 0.52);
            int safeRangeY = (int)(screenTilesY * 0.52);

            Vector2 position = player.position;

            int playerTileX = (int)(position.X / 16f);
            int playerTileY = (int)(position.Y / 16f);

            int spawnRangeMinX = Math.Max(0, Math.Min(Main.maxTilesX, playerTileX - spawnRangeX));
            int spawnRangeMaxX = Math.Max(0, Math.Min(Main.maxTilesX, playerTileX + spawnRangeX));
            int spawnRangeMinY = Math.Max(0, Math.Min(Main.maxTilesY, playerTileY - spawnRangeY));
            int spawnRangeMaxY = Math.Max(0, Math.Min(Main.maxTilesY, playerTileY + spawnRangeY));

            int safeRangeMinX = Math.Max(0, Math.Min(Main.maxTilesX, playerTileX - safeRangeX));
            int safeRangeMaxX = Math.Max(0, Math.Min(Main.maxTilesX, playerTileX + safeRangeX));
            int safeRangeMinY = Math.Max(0, Math.Min(Main.maxTilesY, playerTileY - safeRangeY));
            int safeRangeMaxY = Math.Max(0, Math.Min(Main.maxTilesY, playerTileY + safeRangeY));

            int spawnX = 0;
            int spawnY = 0;

            bool found = false;
            int attempts = 0;
            while (attempts < 50)
            {
                int testX = rand.Next(spawnRangeMinX, spawnRangeMaxX);
                int testY = rand.Next(spawnRangeMinY, spawnRangeMaxY);

                var testTile = Main.tile[testX, testY];
                if (testTile.nactive() && Main.tileSolid[testTile.type])
                {
                    attempts++;
                    continue;
                }

                if (!Main.wallHouse[testTile.wall])
                {
                    for (int y = testY; y < Main.maxTilesY; y++)
                    {
                        var test = Main.tile[testX, y];
                        if (test.nactive() && Main.tileSolid[test.type])
                        {
                            if (testX < safeRangeMinX || testX > safeRangeMaxX || y < safeRangeMinY || y > safeRangeMaxY)
                            {
                                spawnX = testX;
                                spawnY = y;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        attempts++;
                    }

                    int spaceMinX = spawnX - (SpawnSpaceX / 2);
                    int spaceMaxX = spawnX + (SpawnSpaceX / 2);
                    int spaceMinY = spawnY - SpawnSpaceY;
                    int spaceMaxY = spawnY;
                    if (spaceMinX < 0 || spaceMaxX > Main.maxTilesX)
                    {
                        attempts++;
                        continue;
                    }

                    if (spaceMinY < 0 || spaceMaxY > Main.maxTilesY)
                    {
                        attempts++;
                        continue;
                    }

                    if (found)
                    {
                        for (int x = spaceMinX; x < spaceMaxX; x++)
                        {
                            for (int y = spaceMinY; y < spaceMaxY; y++)
                            {
                                if (Main.tile[x, y].nactive() && Main.tileSolid[Main.tile[x, y].type])
                                {
                                    found = false;
                                    break;
                                }

                                if (Main.tile[x, y].lava())
                                {
                                    found = false;
                                    break;
                                }
                            }
                        }

                        if (!found)
                        {
                            attempts++;
                            continue;
                        }
                    }

                    if (spawnX >= safeRangeMinX && spawnX <= safeRangeMaxX)
                    {
                        if (!found)
                        {
                            attempts++;
                            continue;
                        }
                    }
                }
            }

            if (found)
            {
                return SpawnCustomNPC((spawnX * 16) + 8, spawnY * 16, mainNPC);
            }

            return -1;
        }

        public static bool Chance(double percentage)
        {
            return rand.NextDouble() * 100 <= percentage;
        }

        private static int SpawnCustomNPC(int x, int y, NPC mainNPC)
        {
            //DEBUG
            NpcUtils.LogConsole("DEBUG Spawning Custom NPC at {0}, {1} with customID {2}", x, y, mainNPC);
            //DEBUG

            int npcid = NPC.NewNPC((int)(x), (int)(y), EntityRegistrar.Npcs[NpcTest.test]);
            if (npcid == 200)
            {
                //DEBUG
                NpcUtils.LogConsole("DEBUG Spawning FAILED (mobcap) at {0}, {1} for customID {2}", x, y, mainNPC.name);
                //DEBUG
                return -1;
            }

            //Data.ConvertNPCToCustom(npcid, mainNPC.Npc);
            DateTime[] dt = null;
            //if (mainNPC.customProjectiles != null)
            // {
            //     dt = Enumerable.Repeat(DateTime.Now, mainNPC.customProjectiles.Count).ToArray();
            // }

            NetMessage.SendData((int)OTA.Packet.NPC_INFO, -1, -1, "", npcid);

            return npcid;
        }

        private static int SpawnCustomNPC(int x, int y, OTANpc mainNPC)
        {
            //DEBUG
            NpcUtils.LogConsole("DEBUG Spawning Custom NPC at {0}, {1} with customID {2}", x, y, mainNPC.Npc);
            //DEBUG

            int npcid = NPC.NewNPC((int)(x), (int)(y), EntityRegistrar.Npcs[NpcTest.test]);
            if (npcid == 200)
            {
                //DEBUG
                NpcUtils.LogConsole("DEBUG Spawning FAILED (mobcap) at {0}, {1} for customID {2}", x, y, mainNPC.Npc.name);
                //DEBUG
                return -1;
            }

            //Data.ConvertNPCToCustom(npcid, mainNPC.Npc);
            DateTime[] dt = null;
            //if (mainNPC.customProjectiles != null)
           // {
           //     dt = Enumerable.Repeat(DateTime.Now, mainNPC.customProjectiles.Count).ToArray();
           // }

            NetMessage.SendData ((int)OTA.Packet.NPC_INFO, -1, -1, "", npcid);

            return npcid;
        }

        public static void SendPrivateMessageNearbyPlayers(string message, Color color, int npcindex, int distance)
        {
            NPC npc = Main.npc[npcindex];
            if (npc == null) return;

            foreach (Player player in PlayersNearBy(npc.position, distance))
            {
                player.SendMessage(message, color);
            }
        }



        #endregion

    }
}

