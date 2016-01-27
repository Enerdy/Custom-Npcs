using Microsoft.Xna.Framework;
using OTA.Mod.Npc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace CustomNPCS
{
    public class CustomSpawning
    {
        public static BiomeTypes[] availableBiomes = Enum.GetValues(typeof(BiomeTypes)).Cast<BiomeTypes>().Where(x => x != BiomeTypes.None).ToArray();
        public static CustomNPCData Data = new CustomNPCData();
        public BiomeTypes spawnBiome { get; set; }
        public string spawnRegion { get; set; }
        public int spawnRate { get; set; }
        public double spawnChance { get; set; }
        public SpawnConditions spawnConditions { get; set; }
        public bool useTerrariaSpawn { get; set; }
        public CustomSpawning(int spawnrate, SpawnConditions spawnconditions, bool useterrariaspawn = true, BiomeTypes spawnbiome = BiomeTypes.None, string spawnregion = "", double spawnchance = 100.0)
        {
            spawnBiome = spawnbiome;
            spawnRegion = spawnregion;
            spawnRate = spawnrate;
            spawnChance = spawnchance;
            spawnConditions = spawnconditions;
            useTerrariaSpawn = useterrariaspawn;
        }

    [Flags]
    public enum BiomeTypes
        {
            /// <summary>
            /// Doesn't Spawn in Biomes
            /// </summary>
            None = 0,

            /// <summary>
            /// Player is in noZone
            /// </summary>
            Grass = 1 << 0,

            /// <summary>
            /// Player is in Corruption
            /// </summary>
            Corruption = 1 << 1,

            /// <summary>
            /// Player is in Crimsion
            /// </summary>
            Crimsion = 1 << 2,

            /// <summary>
            /// Player is in Crimsion
            /// </summary>
            Desert = 1 << 3,

            /// <summary>
            /// Player is in zoneDungeon
            /// </summary>
            Dungeon = 1 << 4,

            /// <summary>
            /// Player is in Glowshroom
            /// </summary>
            Glowshroom = 1 << 5,

            /// <summary>
            /// Player is in zoneMeteor
            /// </summary>
            Meteor = 1 << 6,

            /// <summary>
            /// Player is in zoneHoly
            /// </summary>
            Holy = 1 << 7,

            /// <summary>
            /// Player is in zoneJungle
            /// </summary>
            Jungle = 1 << 8,

            /// <summary>
            /// Player is in Peace Candle
            /// </summary>
            PeaceCandle = 1 << 9,

            /// <summary>
            /// Player is in zoneSnow
            /// </summary>
            Snow = 1 << 10,

            /// <summary>
            /// Player is in TowerNebula
            /// </summary>
            TowerNebula = 1 << 11,

            /// <summary>
            /// Player is in TowerSolar
            /// </summary>
            TowerSolar = 1 << 12,

            /// <summary>
            /// Player is in TowerStardust
            /// </summary>
            TowerStardust = 1 << 13,

            /// <summary>
            /// Player is in TowerVortex
            /// </summary>
            TowerVortex = 1 << 14,

            /// <summary>
            /// Player is in UndergroundDesert
            /// </summary>
            UndergroundDesert = 1 << 15,

            /// <summary>
            /// Player is in zoneWaterCandle
            /// </summary>
            WaterCandle = 1 << 16
        }

        [Flags]
        public enum SpawnConditions
        {
            /// <summary>
            /// Spawn anytime
            /// </summary>
            None = 0,

            /// <summary>
            /// Allowed in Day time (Main.dayTime == true)
            /// </summary>
            DayTime = 1 << 0,

            /// <summary>
            /// Allowed in Night time (Main.dayTime == true)
            /// </summary>
            NightTime = 1 << 1,

            /// <summary>
            /// Allowed in Eclipse
            /// </summary>
            Eclipse = 1 << 2,

            /// <summary>
            /// Allowed in Bloodmoon
            /// </summary>
            BloodMoon = 1 << 3,

            /// <summary>
            /// Allowed in SnowMoon
            /// </summary>
            SnowMoon = 1 << 10,

            /// <summary>
            /// Allowed when Raining
            /// </summary>
            Raining = 1 << 4,

            /// <summary>
            /// Allowed when Raining
            /// </summary>
            SlimeRaining = 1 << 9,

            /// <summary>
            /// Allowed during day
            /// </summary>
            /// <remarks>
            /// <code>(Main.dayTime == true && Main.time => 150.0 && Main.time <= 26999.0)</code>
            /// </remarks>
            Day = 1 << 5,

            /// <summary>
            /// Allowed during night
            /// </summary>
            /// <remarks>
            /// <code>(Main.dayTime == false && Main.time => 0.0 && Main.16200)</code>
            /// </remarks>
            Night = 1 << 6,

            /// <summary>
            /// Allowed during noon
            /// </summary>
            /// <remarks>
            /// <code>(Main.dayTime == true && Main.time => 27000.0 && Main.time <= 54000)</code>
            /// </remarks>
            Noon = 1 << 7,

            /// <summary>
            /// Allowed during midnight
            /// </summary>
            /// <remarks>
            /// <code>(Main.dayTime == false && Main.time => 16200 && Main.time <= 32400)</code>
            /// </remarks>
            Midnight = 1 << 8
        }

        //todo:  FIX This for OTA, Not working
        // OTA Api needs to be exposed.
        internal static void SpawnMobsInBiomeAndRegion()
        {
            //loop through all players

			foreach (Player player in Terraria.Main.player)
            {
                //Check if player exist and is connected

				if (player == null || !player.active) continue;


                //Log.ConsoleInfo("{0} - Checking spawn for player", player.Name);

                //Check all biome spawns
                BiomeTypes biomes = player.GetCurrentBiomes();
                foreach (BiomeTypes biome in availableBiomes.Where(x => biomes.HasFlag(x)))
                {
                    //Log.ConsoleInfo("{0} - Checking biome for player", biome.ToString());

                    //Get list of mobs that can be spawned in that biome
                    List<Tuple<string, CustomSpawning>> biomeSpawns;
                    if (!Data.BiomeSpawns.TryGetValue(biome, out biomeSpawns)) continue;

                    foreach (Tuple<string, CustomSpawning> obj in biomeSpawns)
                    {
                        //Log.ConsoleInfo("{0} - Checking mob spawn", obj.Item1);
                        //Check spawn conditions
                        if (!CheckSpawnConditions(obj.Item2.spawnConditions))
                        {
                            //Log.ConsoleInfo("False Conditions");
                            continue;
                        }

                        //NPC mainNPC = Data.GetNPCbyID(obj.Item1);
                        NPC mainNPC = CustomNPCS.NpcUtils.GetNPCById(99);

                        //Make sure not spawning more then maxSpawns
                        //if (mainNPC.Npc.maxspawns != -1 && mainNPC.currSpawnsVar >= mainNPC.Npc.maxSpawns)
                        //{
                        //    continue;
                        //}

                        //Get the last spawn attempt
                        DateTime lastSpawnAttempt;
                        if (!Data.LastSpawnAttempt.TryGetValue(mainNPC.name, out lastSpawnAttempt))
                        {
                            lastSpawnAttempt = default(DateTime);
                            Data.LastSpawnAttempt[mainNPC.name] = lastSpawnAttempt;
                        }

                        //If not enough time has passed, we skip and go to the next NPC.
                        if ((DateTime.Now - lastSpawnAttempt).TotalSeconds < obj.Item2.spawnRate) continue;

                        //Check spawn chance
                        if (!NPCFunctions.Chance(obj.Item2.spawnChance)) continue;

                        //Check spawn method
                        if (obj.Item2.useTerrariaSpawn)
                        {
                            //All checks completed --> spawn mob
                            int npcid = NPCFunctions.SpawnMobAroundPlayer(player, mainNPC);
                            if (npcid != -1)
                            {

                                //OTANpc.[npcid].target = player.whoAmI;

                                //Data.LastSpawnAttempt[customnpc.customID] = DateTime.Now;

                              //  mainNPC.currSpawnsVar++;
                            }
                        }
                        else
                        {
                            //All checks completed --> spawn mob

                            var spawn = GetRandomClearTile ((int)(player.position.X / 16f), (int)(player.position.Y / 16f));

                            int npcid = NPCFunctions.SpawnNPCAtLocation((int)(spawn.X * 16f) + 8, (int)(spawn.Y * 16f), mainNPC);
                            if (npcid == -1) continue;

                            Data.LastSpawnAttempt[mainNPC.name] = DateTime.Now;


                            Main.npc [npcid].target = player.whoAmI;

                            //customnpc.currSpawnsVar++;
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


        public static bool IsTileValid(int x, int y)
        {
            return (x >= 0 && x <= Main.maxTilesX && y >= 0 && y <= Main.maxTilesY);
        }

        public static bool IsTileClear(int x, int y)
        {
#if MemTile
			return Main.tile[x, y] == OTA.Memory.MemTile.Empty || !Main.tile[x, y].active() || Main.tile[x, y].inActive();
#else
            return Main.tile[x, y] == null || !Main.tile[x, y].active() || Main.tile[x, y].inActive();
#endif
        }

        public static Vector2 GetRandomClearTile(float x, float y, int attempts = 1, int rangeY = 50, int rangeX = 50)
        {
            return GetRandomClearTile((int)x, (int)y, attempts, rangeY, rangeX);
        }

        public static Vector2 GetRandomClearTile(int x, int y, int attempts = 1, int rangeX = 50, int rangeY = 50)
        {
            Vector2 tileLocation = new Vector2(0, 0);
            try
            {
                if (Main.rand == null)
                    Main.rand = new Random();

                //                if (!forceRange)
                //                {
                //                    rangeX = (Main.tile.GetLength(0)) - x;
                //                    rangeY = (Main.tile.GetLength(1)) - y;
                //                }

                for (int i = 0; i < attempts; i++)
                {
                    tileLocation.X = x + ((Main.rand.Next(rangeX * -1, rangeX)) / 2);
                    tileLocation.Y = y + ((Main.rand.Next(rangeY * -1, rangeY)) / 2);
                    if ((IsTileValid((int)tileLocation.X, (int)tileLocation.Y) &&
                        IsTileClear((int)tileLocation.X, (int)tileLocation.Y)))
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }

            if (tileLocation.X == 0 && tileLocation.Y == 0)
                return new Vector2(x, y);

            return tileLocation;
        }


        internal static bool CheckSpawnConditions(SpawnConditions conditions)
        {
            if (conditions == SpawnConditions.None)
            {
                //Log.ConsoleInfo("Failed on None");
                return false;
            }
            if (conditions.HasFlag(SpawnConditions.BloodMoon) && !Main.bloodMoon)
            {
                //Log.ConsoleInfo("Failed on BloodMoon");
                return false;
            }
            if (conditions.HasFlag(SpawnConditions.Eclipse) && !Main.eclipse)
            {
                //Log.ConsoleInfo("Failed on Eclipse");
                return false;
            }
            if (conditions.HasFlag(SpawnConditions.SnowMoon) && !Main.snowMoon)
            {
                //Log.ConsoleInfo("Failed on SnowMoon");
                return false;
            }
            if (conditions.HasFlag(SpawnConditions.DayTime) && !Main.dayTime)
            {
                //Log.ConsoleInfo("Failed on DayTime");
                return false;
            }
            if (conditions.HasFlag(SpawnConditions.NightTime) && Main.dayTime)
            {
                //Log.ConsoleInfo("Failed on NightTime");
                return false;
            }
            if (conditions.HasFlag(SpawnConditions.Day) && (!Main.dayTime || (Main.dayTime && Main.time <= 150.0 && Main.time >= 26999.0)))
            {
                //Log.ConsoleInfo("Failed on Day");
                return false;
            }
            if (conditions.HasFlag(SpawnConditions.Noon) && (!Main.dayTime || (Main.dayTime && Main.time <= 16200.0 && Main.time >= 32400.0)))
            {
                //Log.ConsoleInfo("Failed on Noon");
                return false;
            }
            if (conditions.HasFlag(SpawnConditions.Night) && (Main.dayTime || (!Main.dayTime && Main.time <= 27000.0 && Main.time >= 54000.0)))
            {
                //Log.ConsoleInfo("Failed on Night");
                return false;
            }
            if (conditions.HasFlag(SpawnConditions.Midnight) && (Main.dayTime || (!Main.dayTime && Main.time <= 16200.0 && Main.time >= 32400.0)))
            {
                //Log.ConsoleInfo("Failed on Midnight");
                return false;
            }
            if (conditions.HasFlag(SpawnConditions.Raining) && !Main.raining)
            {
                //Log.ConsoleInfo("Failed on Raining");
                return false;
            }
            if (conditions.HasFlag(SpawnConditions.SlimeRaining) && !Main.slimeRain)
            {
                //Log.ConsoleInfo("Failed on Slime Raining");
                return false;
            }
            return true;
        }

        public class CustomNPCData
        {
            internal Dictionary<string, CustomNPCDefinition> CustomNPCs = new Dictionary<string, CustomNPCDefinition>();
            internal Dictionary<BiomeTypes, List<Tuple<string, CustomSpawning>>> BiomeSpawns = new Dictionary<BiomeTypes, List<Tuple<string, CustomSpawning>>>();
            internal Dictionary<string, List<Tuple<string, CustomSpawning>>> RegionSpawns = new Dictionary<string, List<Tuple<string, CustomSpawning>>>();
            internal Dictionary<string, DateTime> LastSpawnAttempt = new Dictionary<string, DateTime>();

            public CustomNPCDefinition GetNPCbyID(string id)
            {
                CustomNPCDefinition npcdef;
                this.CustomNPCs.TryGetValue(id, out npcdef);
                return npcdef;
            }

            private void AddCustomNPCToBiome(BiomeTypes biome, string id, CustomSpawning spawning)
            {
                List<Tuple<string, CustomSpawning>> spawns;
                if (!BiomeSpawns.TryGetValue(biome, out spawns))
                {
                    spawns = new List<Tuple<string, CustomSpawning>>();
                    BiomeSpawns[biome] = spawns;
                }

                var pair = Tuple.Create(id, spawning);
                if (!spawns.Contains(pair))
                {
                    spawns.Add(pair);
                }
            }

            private void AddCustomNPCToRegion(string regionName, string id, CustomSpawning spawning)
            {
                List<Tuple<string, CustomSpawning>> spawns;
                if (!RegionSpawns.TryGetValue(regionName, out spawns))
                {
                    spawns = new List<Tuple<string, CustomSpawning>>();
                    RegionSpawns[regionName] = spawns;
                }

                var pair = Tuple.Create(id, spawning);
                if (!spawns.Contains(pair))
                {
                    spawns.Add(pair);
                }
            }

            /// <summary>
            /// This is called once the CustomNPCDefinitions have been loaded into the DefinitionManager.
            /// </summary>
            /// <param name="definitions"></param>
            internal void LoadFrom(DefinitionManager definitions)
            {
                foreach (var pair in definitions.Definitions)
                {
                    string id = pair.Key;
                    CustomNPCDefinition definition = pair.Value;

                    CustomNPCs.Add(id, definition);
                    foreach (var spawning in definition.customSpawning)
                    {
                        if (spawning.spawnBiome != BiomeTypes.None)
                        {
                            AddCustomNPCToBiome(spawning.spawnBiome, id, spawning);
                        }

                        if (!string.IsNullOrEmpty(spawning.spawnRegion))
                        {
                            AddCustomNPCToRegion(spawning.spawnRegion, id, spawning);
                        }
                    }
                }
            }
        }
    }
    }
