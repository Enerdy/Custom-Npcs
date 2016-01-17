﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Terraria;

#if TShock
using TShockAPI;
using TShockAPI.DB;

#elif OTAPI
using Microsoft.Xna.Framework;
#endif

namespace CustomNPC
{
	public static class NPCManager
	{
		public static BiomeTypes[] availableBiomes = Enum.GetValues (typeof(BiomeTypes)).Cast<BiomeTypes> ().Where (x => x != BiomeTypes.None).ToArray ();
		private static Random rand = new Random ();
		internal static CustomNPCVars[] NPCs = new CustomNPCVars[200];
		public static CustomNPCData Data = new CustomNPCData ();

		internal static void LoadFrom (DefinitionManager definitions)
		{
			Data.LoadFrom (definitions);
		}

		internal static void SpawnMobsInBiomeAndRegion ()
		{
			//loop through all players
			#if TShock
            foreach (TSPlayer player in TShock.Players)
			#elif OTAPI
			foreach (Player player in Terraria.Main.player)
			#endif
            {
				//Check if player exist and is connected
				#if TShock
                if (player == null || !player.ConnectionAlive) continue;
				#elif OTAPI
				if (player == null || !player.active) continue;
				#endif
                
				//Log.ConsoleInfo("{0} - Checking spawn for player", player.Name);

				//Check all biome spawns
				BiomeTypes biomes = player.GetCurrentBiomes ();
				foreach (BiomeTypes biome in availableBiomes.Where(x => biomes.HasFlag(x)))
				{
					//Log.ConsoleInfo("{0} - Checking biome for player", biome.ToString());

					//Get list of mobs that can be spawned in that biome
					List<Tuple<string, CustomNPCSpawning>> biomeSpawns;
					if (!Data.BiomeSpawns.TryGetValue (biome, out biomeSpawns)) continue;
                    
					foreach (Tuple<string, CustomNPCSpawning> obj in biomeSpawns)
					{
						//Log.ConsoleInfo("{0} - Checking mob spawn", obj.Item1);
						//Check spawn conditions
						if (!CheckSpawnConditions (obj.Item2.spawnConditions))
						{
							//Log.ConsoleInfo("False Conditions");
							continue;
						}

						CustomNPCDefinition customnpc = Data.GetNPCbyID (obj.Item1);

						//Make sure not spawning more then maxSpawns
						if (customnpc.maxSpawns != -1 && customnpc.currSpawnsVar >= customnpc.maxSpawns)
						{
							continue;
						}

						//Get the last spawn attempt
						DateTime lastSpawnAttempt;
						if (!Data.LastSpawnAttempt.TryGetValue (customnpc.customID, out lastSpawnAttempt))
						{
							lastSpawnAttempt = default(DateTime);
							Data.LastSpawnAttempt [customnpc.customID] = lastSpawnAttempt;
						}

						//If not enough time has passed, we skip and go to the next NPC.
						if ((DateTime.Now - lastSpawnAttempt).TotalSeconds < obj.Item2.spawnRate) continue;

						//Check spawn chance
						if (!NPCManager.Chance (obj.Item2.spawnChance)) continue;
                            
						//Check spawn method
						if (obj.Item2.useTerrariaSpawn)
						{
							//All checks completed --> spawn mob
							int npcid = SpawnMobAroundPlayer (player, customnpc);
							if (npcid != -1)
							{
#if TShock
                                Main.npc [npcid].target = player.Index;
#elif OTAPI
                                Main.npc[npcid].target = player.whoAmI;
#endif
                                Data.LastSpawnAttempt [customnpc.customID] = DateTime.Now;
								customnpc.currSpawnsVar++;
							}                                            
						}
						else
						{
                            //All checks completed --> spawn mob
#if TShock
                            var spawn = GetRandomClearTile((int)(player.TileX / 16f), (int)(player.TileY / 16f));
#elif OTAPI
                            var spawn = GetRandomClearTile ((int)(player.position.X / 16f), (int)(player.position.Y / 16f));
#endif

                            int npcid = SpawnNPCAtLocation ((int)(spawn.X * 16f) + 8, (int)(spawn.Y * 16f), customnpc);
							if (npcid == -1) continue;

							Data.LastSpawnAttempt [customnpc.customID] = DateTime.Now;
#if TShock
                            Main.npc [npcid].target = player.Index;
#elif OTAPI
                            Main.npc [npcid].target = player.whoAmI;
#endif
                            customnpc.currSpawnsVar++;
						}
					}
				}


//                //Then check regions as well
//                Rectangle playerRectangle = new Rectangle(player.TileX, player.TileY, player.TPlayer.width, player.TPlayer.height);
//                foreach (Region obj in Data.RegionSpawns.Keys.Select(name => TShock.Regions.GetRegionByName(name)).Where(region => region != null && region.InArea(playerRectangle)))
//                {
//                    List<Tuple<string, CustomNPCSpawning>> regionSpawns;
//                    if (!Data.RegionSpawns.TryGetValue(obj.Name, out regionSpawns)) continue;
//                    
//                    foreach (Tuple<string, CustomNPCSpawning> obj2 in regionSpawns)
//                    {
//                        //Invalid spawn conditions
//                        if (!CheckSpawnConditions(obj2.Item2.spawnConditions)) continue;
//
//                        CustomNPCDefinition customnpc = Data.GetNPCbyID(obj2.Item1);
//
//                        //Make sure not spawning more then maxSpawns
//                        if (customnpc.maxSpawns != -1 && customnpc.currSpawnsVar >= customnpc.maxSpawns)
//                        {
//                            continue;
//                        }
//
//                        //Get the last spawn attempt
//                        DateTime lastSpawnAttempt;
//                        if (!Data.LastSpawnAttempt.TryGetValue(customnpc.customID, out lastSpawnAttempt))
//                        {
//                            lastSpawnAttempt = default(DateTime);
//                            Data.LastSpawnAttempt[customnpc.customID] = lastSpawnAttempt;
//                        }
//
//                        //If not enough time passed, we skip and go to the next NPC.
//                        if ((DateTime.Now - lastSpawnAttempt).TotalSeconds < obj2.Item2.spawnRate) continue;
//
//                        Data.LastSpawnAttempt[customnpc.customID] = DateTime.Now;
//
//                        if (!NPCManager.Chance(obj2.Item2.spawnChance)) continue;
//                        
//                        int spawnX;
//                        int spawnY;
//                        TShock.Utils.GetRandomClearTileWithInRange(player.TileX, player.TileY, 50, 50, out spawnX, out spawnY);
//                        int npcid = SpawnNPCAtLocation((spawnX * 16) + 8, spawnY * 16, customnpc);
//                        if (npcid == -1) continue;
//
//                        Main.npc[npcid].target = player.Index;
//                        customnpc.currSpawnsVar++;
//                    }
//                }
			}
		}


		public static bool IsTileValid (int x, int y)
		{
			return (x >= 0 && x <= Main.maxTilesX && y >= 0 && y <= Main.maxTilesY);
		}

		public static bool IsTileClear (int x, int y)
		{
#if MemTile
			return Main.tile[x, y] == OTA.Memory.MemTile.Empty || !Main.tile[x, y].active() || Main.tile[x, y].inActive();
#else
			return Main.tile [x, y] == null || !Main.tile [x, y].active () || Main.tile [x, y].inActive ();
#endif
		}

		public static Vector2 GetRandomClearTile (float x, float y, int attempts = 1, int rangeY = 50, int rangeX = 50)
		{
			return GetRandomClearTile ((int)x, (int)y, attempts, rangeY, rangeX);
		}

		public static Vector2 GetRandomClearTile (int x, int y, int attempts = 1, int rangeX = 50, int rangeY = 50)
		{
			Vector2 tileLocation = new Vector2 (0, 0);
			try
			{
				if (Main.rand == null)
					Main.rand = new Random ();

				//                if (!forceRange)
				//                {
				//                    rangeX = (Main.tile.GetLength(0)) - x;
				//                    rangeY = (Main.tile.GetLength(1)) - y;
				//                }

				for (int i = 0; i < attempts; i++)
				{
					tileLocation.X = x + ((Main.rand.Next (rangeX * -1, rangeX)) / 2);
					tileLocation.Y = y + ((Main.rand.Next (rangeY * -1, rangeY)) / 2);
					if ((IsTileValid ((int)tileLocation.X, (int)tileLocation.Y) &&
					    IsTileClear ((int)tileLocation.X, (int)tileLocation.Y)))
					{
						break;
					}
				}
			}
			catch (Exception)
			{
			}

			if (tileLocation.X == 0 && tileLocation.Y == 0)
				return new Vector2 (x, y);

			return tileLocation;
		}


		internal static bool CheckSpawnConditions (SpawnConditions conditions)
		{
			if (conditions == SpawnConditions.None)
			{
				//Log.ConsoleInfo("Failed on None");
				return false;
			}
			if (conditions.HasFlag (SpawnConditions.BloodMoon) && !Main.bloodMoon)
			{
				//Log.ConsoleInfo("Failed on BloodMoon");
				return false;
			}
			if (conditions.HasFlag (SpawnConditions.Eclipse) && !Main.eclipse)
			{
				//Log.ConsoleInfo("Failed on Eclipse");
				return false;
			}
			if (conditions.HasFlag (SpawnConditions.SnowMoon) && !Main.snowMoon)
			{
				//Log.ConsoleInfo("Failed on SnowMoon");
				return false;
			}
			if (conditions.HasFlag (SpawnConditions.DayTime) && !Main.dayTime)
			{
				//Log.ConsoleInfo("Failed on DayTime");
				return false;
			}
			if (conditions.HasFlag (SpawnConditions.NightTime) && Main.dayTime)
			{
				//Log.ConsoleInfo("Failed on NightTime");
				return false;
			}
			if (conditions.HasFlag (SpawnConditions.Day) && (!Main.dayTime || (Main.dayTime && Main.time <= 150.0 && Main.time >= 26999.0)))
			{
				//Log.ConsoleInfo("Failed on Day");
				return false;
			}
			if (conditions.HasFlag (SpawnConditions.Noon) && (!Main.dayTime || (Main.dayTime && Main.time <= 16200.0 && Main.time >= 32400.0)))
			{
				//Log.ConsoleInfo("Failed on Noon");
				return false;
			}
			if (conditions.HasFlag (SpawnConditions.Night) && (Main.dayTime || (!Main.dayTime && Main.time <= 27000.0 && Main.time >= 54000.0)))
			{
				//Log.ConsoleInfo("Failed on Night");
				return false;
			}
			if (conditions.HasFlag (SpawnConditions.Midnight) && (Main.dayTime || (!Main.dayTime && Main.time <= 16200.0 && Main.time >= 32400.0)))
			{
				//Log.ConsoleInfo("Failed on Midnight");
				return false;
			}
			if (conditions.HasFlag (SpawnConditions.Raining) && !Main.raining)
			{
				//Log.ConsoleInfo("Failed on Raining");
				return false;
			}
			if (conditions.HasFlag (SpawnConditions.SlimeRaining) && !Main.slimeRain)
			{
				//Log.ConsoleInfo("Failed on Slime Raining");
				return false;
			}
			return true;
		}

		public static CustomNPCVars GetCustomNPCByIndex (int index)
		{
			//Do a short range check here...
			if (index < 0 || index >= NPCs.Length) return null;

			return NPCs [index];
		}

		/// <summary>
		/// Returns a list of all Custom NPC that are spawned of the given type.
		/// </summary>
		/// <param name="customnpc">The type to look for</param>
		/// <returns></returns>
		public static List<CustomNPCVars> GetAllOfType (CustomNPCDefinition customnpc)
		{
			List<CustomNPCVars> tbr = new List<CustomNPCVars> ();
			foreach (CustomNPCVars v in NPCs)
			{
				if (v == null) continue;

				if (v.customNPC == customnpc) tbr.Add (v);
			}

			return tbr;
		}

		public static int SpawnNPCAtLocation (int x, int y, CustomNPCDefinition customnpc)
		{
			return SpawnCustomNPC (x, y, customnpc);
		}

		public static int SpawnNPCAroundNPC (int npcindex, ShotTile shottile, CustomNPCDefinition customnpc)
		{
			NPC npc = Main.npc [npcindex];
			if (npc == null) return -1;

			int x = (int)(npc.position.X + shottile.X);
			int y = (int)(npc.position.Y + shottile.Y);

			return SpawnCustomNPC (x, y, customnpc);
		}

		public static int SpawnNPCAroundNPC (int npcindex, ShotTile shottile, int npcid)
		{
			NPC npc = Main.npc [npcindex];
			if (npc == null) return -1;

			int x = (int)(npc.position.X + shottile.X);
			int y = (int)(npc.position.Y + shottile.Y);

			return SpawnNPC (x, y, npcid);
		}

		private static int SpawnNPC (int x, int y, int npcid)
		{ 
			if (npcid == 200)
			{
				//DEBUG
				NpcUtils.LogConsole ("DEBUG Spawning FAILED (mobcap) at {0}, {1} for npcID {2}", x, y, npcid);
				//DEBUG
				return -1;
			}

			NPC.NewNPC (x, y, npcid);
#if TShock
            TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npcid);
#elif OTAPI
            NetMessage.SendData ((int)OTA.Packet.NPC_INFO, -1, -1, "", npcid);
#endif

            return npcid;
		}

#if TShock
		public static int SpawnMobAroundPlayer(TSPlayer player, CustomNPCDefinition definition)
		

#elif OTAPI
		public static int SpawnMobAroundPlayer (Terraria.Player player, CustomNPCDefinition definition)
#endif
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

#if TShock
			Vector2 position = player.TPlayer.position;
#elif OTAPI
			Vector2 position = player.position;
#endif
			int playerTileX = (int)(position.X / 16f);
			int playerTileY = (int)(position.Y / 16f);

			int spawnRangeMinX = Math.Max (0, Math.Min (Main.maxTilesX, playerTileX - spawnRangeX));
			int spawnRangeMaxX = Math.Max (0, Math.Min (Main.maxTilesX, playerTileX + spawnRangeX));
			int spawnRangeMinY = Math.Max (0, Math.Min (Main.maxTilesY, playerTileY - spawnRangeY));
			int spawnRangeMaxY = Math.Max (0, Math.Min (Main.maxTilesY, playerTileY + spawnRangeY));

			int safeRangeMinX = Math.Max (0, Math.Min (Main.maxTilesX, playerTileX - safeRangeX));
			int safeRangeMaxX = Math.Max (0, Math.Min (Main.maxTilesX, playerTileX + safeRangeX));
			int safeRangeMinY = Math.Max (0, Math.Min (Main.maxTilesY, playerTileY - safeRangeY));
			int safeRangeMaxY = Math.Max (0, Math.Min (Main.maxTilesY, playerTileY + safeRangeY));

			int spawnX = 0;
			int spawnY = 0;

			bool found = false;
			int attempts = 0;
			while (attempts < 50)
			{
				int testX = rand.Next (spawnRangeMinX, spawnRangeMaxX);
				int testY = rand.Next (spawnRangeMinY, spawnRangeMaxY);

				var testTile = Main.tile [testX, testY];
				if (testTile.nactive () && Main.tileSolid [testTile.type])
				{
					attempts++;
					continue;
				}

				if (!Main.wallHouse [testTile.wall])
				{
					for (int y = testY; y < Main.maxTilesY; y++)
					{
						var test = Main.tile [testX, y];
						if (test.nactive () && Main.tileSolid [test.type])
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
								if (Main.tile [x, y].nactive () && Main.tileSolid [Main.tile [x, y].type])
								{
									found = false;
									break;
								}

								if (Main.tile [x, y].lava ())
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
				return SpawnCustomNPC ((spawnX * 16) + 8, spawnY * 16, definition);
			}

			return -1;
		}

		private static int SpawnCustomNPC (int x, int y, CustomNPCDefinition definition)
		{
			//DEBUG
			NpcUtils.LogConsole ("DEBUG Spawning Custom NPC at {0}, {1} with customID {2}", x, y, definition.customID);
			//DEBUG

			int npcid = NPC.NewNPC (x, y, definition.customBase.type);
			if (npcid == 200)
			{
				//DEBUG
				NpcUtils.LogConsole ("DEBUG Spawning FAILED (mobcap) at {0}, {1} for customID {2}", x, y, definition.customID);
				//DEBUG
				return -1;
			}

			Data.ConvertNPCToCustom (npcid, definition);
			DateTime[] dt = null;
			if (definition.customProjectiles != null)
			{
				dt = Enumerable.Repeat (DateTime.Now, definition.customProjectiles.Count).ToArray ();
			}
			NPCs [npcid] = new CustomNPCVars (definition, dt, Main.npc [npcid]);
#if TShock
            TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npcid);
#elif OTAPI
			NetMessage.SendData ((int)OTA.Packet.NPC_INFO, -1, -1, "", npcid);
#endif
			return npcid;
		}

		/// <summary>
		/// Chance system, returns true based on the percentage passed through
		/// </summary>
		/// <param name="percentage">At most 2 decimal points</param>
		/// <returns></returns>
		public static bool Chance (double percentage)
		{
			return rand.NextDouble () * 100 <= percentage;
		}

		public static void DebuffNearbyPlayers (int debuffid, int seconds, int npcindex, int distance)
		{
#if TShock
            NPC npc = Main.npc[npcindex];
            if (npc == null) return;

            foreach (var player in PlayersNearBy(npc.position, distance))
            {
                player.SetBuff(debuffid, seconds * 60);
			}
#endif
		}

		public static void SendPrivateMessageNearbyPlayers (string message, Color color, int npcindex, int distance)
		{
#if TShock
            NPC npc = Main.npc[npcindex];
            if (npc == null) return;
            
            foreach (var player in PlayersNearBy(npc.position, distance))
            {
                player.SendMessage(message, color);
			}
#endif
		}

#if TShock
        public static List<TSPlayer> PlayersNearBy(Vector2 position, int distance)
        {
            int squaredist = (distance * 16) * (distance * 16);

            List<TSPlayer> playerlist = new List<TSPlayer>();
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null || player.Dead || !player.ConnectionAlive) continue;

                if (Vector2.DistanceSquared(player.TPlayer.position, position) <= squaredist)
                {
                    playerlist.Add(player);
                }
            }

            return playerlist;
        }
		

#elif OTAPI
		public static List<Player> PlayersNearBy (Vector2 position, int distance)
		{
			int squaredist = (distance * 16) * (distance * 16);

			List<Player> playerlist = new List<Player> ();
			foreach (Player player in Terraria.Main.player)
			{
				if (player == null || player.dead || !player.active) continue;

				if (Vector2.DistanceSquared (player.position, position) <= squaredist)
				{
					playerlist.Add (player);
				}
			}

			return playerlist;
		}
#endif

		/// <summary>
		/// Checks if the NPC's current health is above the passed amount
		/// </summary>
		/// <param name="Health"></param>
		/// <returns></returns>
		public static bool HealthAbove (int npcid, int health)
		{
			NPC mainNPC = Main.npc [npcid];
			if (mainNPC == null) return false;
            
			return mainNPC.life >= health;
		}

		/// <summary>
		/// Checks if the NPC's current health is below the passed amount
		/// </summary>
		/// <param name="Health"></param>
		/// <returns></returns>
		public static bool HealthBelow (int npcid, int health)
		{
			NPC mainNPC = Main.npc [npcid];
			if (mainNPC == null) return false;
            
			return mainNPC.life <= health;
		}

		/// <summary>
		/// Checks if the NPC currently has a buff placed on them
		/// </summary>
		/// <param name="buffid"></param>
		/// <returns></returns>
		public static bool HasBuff (int npcid, int buffid)
		{
			NPC mainNPC = Main.npc [npcid];
			if (mainNPC == null) return false;
            
			return mainNPC.buffType.Contains (buffid);
		}

		public static void AddBuff (int npcid, int buffid, int seconds)
		{
			NPC mainNPC = Main.npc [npcid];
			if (mainNPC == null) return;

			mainNPC.AddBuff (buffid, seconds * 60);
		}

		public static void AddBuffToPlayer (int playerindex, int buffid, int seconds)
		{
#if TShock
            TSPlayer player = TShock.Players[playerindex];
            if (player == null) return;

            player.TPlayer.AddBuff(40, seconds * 60);
            player.SetBuff(buffid, seconds * 60);
#endif
		}

		public static int AliveCount (string customid)
		{
			int count = 0;
			foreach (CustomNPCVars obj in NPCs)
			{
				if (obj == null) continue;

				if (obj.customNPC.customID.Equals (customid, StringComparison.InvariantCultureIgnoreCase) && !obj.isDead)
				{
					count++;
				}
			}
			return count;
		}

		public static class CustomNPCInvasion
		{
			public static Dictionary<string, WaveSet> WaveSets { get; set; }

			private static Timer InvasionTimer = new Timer (1000);

			private static WaveSet CurrentInvasion { get; set; }

			private static Waves CurrentWave { get; set; }

			private static int CurrentWaveIndex { get; set; }

			private static int waveSize { get; set; }

			private static bool spawnedBosses = false;
			public static bool invasionStarted = false;

			public static int WaveSize {
				get { return waveSize; }
				set {
					waveSize = value;
					if (value == 0)
					{
						NextWave ();
					}
				}
			}

			public static void NextWave ()
			{
				if (CurrentInvasion == null) return;

				if (CurrentInvasion.Waves.Count - 1 == CurrentWaveIndex)
				{
					NpcUtils.InfoMessageAllPlayers ("{0} has been defeated!", CurrentInvasion.WaveSetName);
					StopInvasion ();
					return;
				}

				//Temporarily stop (pause) the timer while we change the variables.
				InvasionTimer.Enabled = false;

				//Update variables
				CurrentWaveIndex++;
				CurrentWave = CurrentInvasion.Waves [CurrentWaveIndex];
				WaveSize = CurrentWave.SpawnGroup.KillAmount;
				spawnedBosses = false;

				//Send Wave message
				NpcUtils.InfoMessageAllPlayers ("Wave {0}: {1} has begun!", CurrentWaveIndex + 1, CurrentWave.WaveName);

				//Reenable timer (unpause).
				InvasionTimer.Enabled = true;
			}

			public static void StartInvasion (WaveSet waveset)
			{
				//Disallow the starting of more invasions.
				invasionStarted = true;

				//Initialize the variables
				CurrentInvasion = waveset;
				CurrentWave = waveset.Waves [0];
				if (CurrentWave.SpawnGroup.PlayerMultiplier)
					WaveSize = CurrentWave.SpawnGroup.KillAmount * NpcUtils.ActivePlayerCount;
				else
					WaveSize = CurrentWave.SpawnGroup.KillAmount;

				CurrentWaveIndex = 0;
				spawnedBosses = false;

				//Send start message
				NpcUtils.InfoMessageAllPlayers ("Invasion: {0} has started!", waveset.WaveSetName);

				//Start timer
				InvasionTimer.Elapsed += InvasionTimer_Elapsed;
				InvasionTimer.Enabled = true;
			}

			public static void StopInvasion ()
			{
				//Disable the timer.
				InvasionTimer.Elapsed -= InvasionTimer_Elapsed;
				InvasionTimer.Enabled = false;

				//Send invasion end message.
				NpcUtils.InfoMessageAllPlayers ("Invasion: {0} has stopped!", CurrentInvasion.WaveSetName);

				//Clear variables
				CurrentInvasion = null;
				CurrentWave = null;
				WaveSize = 0;
				CurrentWaveIndex = 0;
				spawnedBosses = false;

				//Allow starting new invasions now we are FULLY done.
				invasionStarted = false;
			}

			private static void InvasionTimer_Elapsed (object sender, ElapsedEventArgs e)
			{
				if (NpcUtils.ActivePlayerCount == 0) return;

				bool boss = spawnedBosses;
				spawnedBosses = true;

				int spawnX = Main.spawnTileX - 150;
				int spawnY = Main.spawnTileY - 150;
				Rectangle spawnRegion = new Rectangle (spawnX, spawnY, 300, 300).ToPixels ();

				int attempts;
				foreach (SpawnMinion minion in CurrentWave.SpawnGroup.SpawnMinions)
				{
					if (minion.isBoss)
					{
						//We already spawned bosses this wave.
						if (boss) continue;

						//Boss NPC's get 5 (250) spawn attempts.
						attempts = 5;
					}
					else
					{
						//Normal NPC's get 3 (150) spawn attempts.
						attempts = 3;
					}

					//Get NPC definition
					var npcdef = NPCManager.Data.GetNPCbyID (minion.MobID);
					if (npcdef == null)
					{
						NpcUtils.LogConsole ("[CustomNPC]: Error! The custom mob id \"{0}\" does not exist!", minion.MobID);
						continue;
					}

					//Check spawn conditions
					if (minion.SpawnConditions != SpawnConditions.None && NPCManager.CheckSpawnConditions (minion.SpawnConditions)) continue;

					SpawnMinion (spawnRegion, minion, npcdef, attempts);
				}
			}

			/// <summary>
			/// Attempts to spawn the given minion for every player online, where applicable.
			/// </summary>
			/// <param name="spawnRegion"></param>
			/// <param name="minion"></param>
			/// <param name="npcdef"></param>
			/// <param name="attempts"></param>
			private static void SpawnMinion (Rectangle spawnRegion, SpawnMinion minion, CustomNPCDefinition npcdef, int attempts)
			{
				//Loop through players
#if TShock
				foreach (TSPlayer player in TShock.Players)
				{
				if (player == null || player.Dead || !player.Active || !NPCManager.Chance(minion.Chance)) continue;
#elif OTAPI
				foreach (var player in Terraria.Main.player)
				{
					if (player == null || player.dead || !player.active || !NPCManager.Chance (minion.Chance)) continue;
#endif

					//Check if the minions can spawn anywhere, or if we need to check if players see them.
					if (!CurrentWave.SpawnAnywhere)
					{
#if TShock
						Rectangle playerFrame = new Rectangle((int)player.TPlayer.position.X, (int)player.TPlayer.position.Y, player.TPlayer.width, player.TPlayer.height);
#elif OTAPI
						Rectangle playerFrame = new Rectangle ((int)player.position.X, (int)player.position.Y, player.width, player.height);
#endif
						if (!playerFrame.Intersects (spawnRegion)) continue;
					}

					//Check biomes
					if (minion.BiomeConditions != BiomeTypes.None)
					{
						BiomeTypes biomes = player.GetCurrentBiomes ();

						if ((minion.BiomeConditions & biomes) == 0) continue;
					}

					int mobid = -1;

					if (npcdef.maxSpawns != -1 && npcdef.currSpawnsVar >= npcdef.maxSpawns)
					{
						npcdef.currSpawnsVar = NPCManager.AliveCount (npcdef.customID);
						continue;
					}
					else if (npcdef.maxSpawns == -1)
					{
						for (int i = 0; mobid == -1 && i < attempts; i++)
						{
							mobid = NPCManager.SpawnMobAroundPlayer (player, npcdef);
							npcdef.currSpawnsVar++;
						}
					}
                        
					//Try max attempts times. This gives attempts*50 spawn attempts at random positions.
					for (int i = 0; mobid == -1 && i < attempts; i++)
					{
						mobid = NPCManager.SpawnMobAroundPlayer (player, npcdef);
						npcdef.currSpawnsVar++;
					}

					//Spawning failed :(
					if (mobid == -1) continue;

					NPCManager.GetCustomNPCByIndex (mobid).isInvasion = true;
				}

			}

			public static WaveSet ReturnWaveSetByName (string name)
			{
				WaveSet waveset;
				if (WaveSets.TryGetValue (name, out waveset))
				{
					return waveset;
				}
				else
				{
					return null;
				}
			}
		}

		public static bool DeallocateNpc (int index)
		{
			var npc = NPCs [index];
			if (npc != null)
			{
				npc.Dispose ();
				NPCs [index] = null;
			}

			return false;
		}
	}
}
