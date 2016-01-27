using OTA.Mod.Npc;
using OTA.Mod;
using Terraria;
using OTA.Plugin;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CustomNPCS
{
    [NativeMod(test)]
    public class NpcTest : OTANpc
    {
        public const string test = "Skeleton King";

        public override void OnSetDefaults()
        {
            base.OnSetDefaults();

            EmulateNPC(Terraria.ID.NPCID.ArmoredSkeleton);

            Npc.IsTownNpc = false;
            Npc.townNPC = false;
            Npc.friendly = false;
            Npc.boss = true;
            Npc.height = 5;
            Npc.aiStyle = 3;
            Npc.damage = 10;
            Npc.defense = 15;
            Npc.life = 30000;
            Npc.lifeMax = 30000;
            Npc.soundHit = 1;
            Npc.soundKilled = 1;
            Npc.knockBackResist = 0.5f;

            SetName(test);

            KillCount = 0;
            FrameCount = 16;
        }

        public override double OnPreSpawn(HookArgs.NpcPreSpawn info)
        {
            if (NPCFunctions.AliveCount("HMDB02") == 0)
            {
                customSpawn.Add(new CustomSpawning(30, CustomSpawning.SpawnConditions.NightTime, true, CustomSpawning.BiomeTypes.None, "hm_dungeon_boss_room1", 12.0));
               // customProjectiles.Add(new CustomProjectiles(180, new List<ShotTile>() { ShotTile.Middle }, 10, 250, true));
                //customNPCLoots.Add(new CustomNPCLoot(808, new List<int> { 0 }, 1, 50));
            }

            return 0.25;
        }

        private Random _random = new Random();
        private DateTime?[] _TimerHMDB01_colide = new DateTime?[Main.maxNPCs];
        private DateTime?[] _TimerHMDB01 = new DateTime?[Main.maxNPCs];
        private DateTime?[] _TimerHMDB01_debuff = new DateTime?[Main.maxNPCs];
        private List<int> HMDB01NUMB = new List<int> { 20, 22, 35, 32 };
        private List<int> HMDB01NUMB2 = new List<int> { 20, 22, 30, 35, 32 };
        private List<int> HMDB02NUMB = new List<int> { 20, 22, 46 };
        private List<int> HMDB02NUMB2 = new List<int> { 21, 22, 24, 31, 46, 47 };
        private List<int> HMDB03NUMB = new List<int> { 20, 22, 46 };
        private List<int> HMDB03NUMB2 = new List<int> { 21, 22, 24, 31, 46, 47 };
        private List<int> HMDB03NUMB3 = new List<int> { 21, 22, 24, 31, 46, 47 };
        private List<int> HMDB03NUMB4 = new List<int> { 21, 22, 24, 31, 46, 47 };
        private List<string> HME001taunts = new List<string> { "Chaos Guardian: You Will Fail!", "Chaos Guardian: The Mad Eye will Defeat you!", "Chaos Guardian: Lord Mad Eye please assist me!", "Chaos Guardian: There will be no escape!" };
        private List<string> HMDB01transform = new List<string> { "Skeleton King: You Truely never had any hope of winning.", "Skeleton King: You have made me angry now!!", "Skeleton King: You will not escape from me!", "Skeleton King: In This room, i will end you!" };
        private List<string> HMDB01taunts = new List<string> { "Skeleton King: You will Lose this fight!", "Skeleton King: This Room was made to Seal me, you broke that Seal, I thank you for your foolish actions.", "Skeleton King: You Dare Challenge me? you are weak!", "Skeleton King: Do you Really think you will leave here alive?!" };
        private List<string> HMDB01DebuffSpeak = new List<string> { "Skeleton King: I Give you this curse, you will not live!", "Skeleton King: This Will take Away any chance you think you will actually defeat me!", "Skeleton King: I will show you my true power!", "Skeleton King: This curse will end you!" };

        public bool HMDB01_Cursemessage = false;
        public bool MoreDebuff = false;
        public bool HMDB01_HP19K = false;
        public bool HMDB01_HP29K = false;

        public override bool OnUpdate()
        {
            checkcollision();
            Events();

            return true;
         }

        public override void OnDeath()
        {
            
        }

        public override bool OnDamage(double damage)
        {
            return true;
        }

        private void Events()
        {
            var npc = Npc.whoAmI;
            if (npc == null)
                return;

            int HMDB01Debuffindex = _random.Next(HMDB01DebuffSpeak.Count);
            var HMDB01Debuff = HMDB01DebuffSpeak[HMDB01Debuffindex];

            int HMDB01index = _random.Next(HMDB01taunts.Count);
            var HMDB01taunt = HMDB01taunts[HMDB01index];

            int HMDB01buff1index = _random.Next(HMDB01NUMB.Count);
            var HMDB01buff1 = HMDB01NUMB[HMDB01buff1index];

            int HMDB01buff2index = _random.Next(HMDB01NUMB2.Count);
            var HMDB01buff2 = HMDB01NUMB2[HMDB01buff2index];
            {
                if (_TimerHMDB01_debuff[Npc.whoAmI] == null)
                {
                    _TimerHMDB01_debuff[Npc.whoAmI] = DateTime.Now;
                }

                if ((HMDB01_Cursemessage != true && NPCFunctions.AliveCount("Skeleton King") >= 1))
                {
                    // checks if alive then checks if Bool is false after that checks if it's alive
                    if (NPCFunctions.HealthAbove(Npc.whoAmI, 100))
                    {
                        // checks health before advancing Do not want it to spam debuff messages if it's too low on Health
                        if (HMDB01_Cursemessage != true && NPCFunctions.AliveCount("Skeleton King") >= 1)
                        {
                            //Check the health and set the value to true if it's below a certain point
                            if ((DateTime.Now - _TimerHMDB01_debuff[Npc.whoAmI]).Value.TotalSeconds <= 30 && NPCFunctions.HealthBelow(Npc.whoAmI, 29600) && HMDB01_HP29K != true)
                            {
                                //chance for debuff amount it could be one or two or none
                                if (MoreDebuff != true && NPCFunctions.Chance(30))
                                {
                                    NPCFunctions.DebuffNearbyPlayers(HMDB01buff1, 12, Npc.whoAmI, 30);
                                    MoreDebuff = true;
                                }

                                else if (MoreDebuff != true && NPCFunctions.Chance(30))
                                {
                                    NPCFunctions.DebuffNearbyPlayers(HMDB01buff1, 12, Npc.whoAmI, 30);
                                    NPCFunctions.DebuffNearbyPlayers(HMDB01buff1, 12, Npc.whoAmI, 30);
                                    MoreDebuff = true;
                                }

                                if (MoreDebuff == true)
                                {
                                    NPCFunctions.SendPrivateMessageNearbyPlayers(HMDB01taunt, Color.DarkRed, Npc.whoAmI, 30);
                                    MoreDebuff = false;
                                }

                                if (NPCFunctions.HealthBelow(Npc.whoAmI, 20000))
                                {
                                    HMDB01_HP29K = true;
                                }
                            }

                            if ((DateTime.Now - _TimerHMDB01_debuff[Npc.whoAmI]).Value.TotalSeconds <= 30 && NPCFunctions.HealthBelow(Npc.whoAmI, 19600) && HMDB01_HP19K != true)
                            {
                                NPCFunctions.DebuffNearbyPlayers(HMDB01buff1, 12, Npc.whoAmI, 30);
                                NPCFunctions.SendPrivateMessageNearbyPlayers(HMDB01taunt, Color.DarkRed, Npc.whoAmI, 30);
                                if (NPCFunctions.HealthBelow(Npc.whoAmI, 11000))
                                {
                                    HMDB01_HP19K = true;
                                }
                            }

                            HMDB01_Cursemessage = true;
                        }
                    }

                    return;
                }

                else if ((DateTime.Now - _TimerHMDB01_debuff[Npc.whoAmI]).Value.TotalSeconds >= 30 && HMDB01_Cursemessage == true && NPCFunctions.AliveCount("Skeleton King") >= 1)
                {
                    //Reset time and exectue this code again
                    _TimerHMDB01_debuff[Npc.whoAmI] = DateTime.Now;
                    HMDB01_Cursemessage = false;
                }
            }
        }

        private void checkcollision()
        {
            foreach (var vanillaNpc in Main.npc)
            {
                if (vanillaNpc.Mod != null && vanillaNpc.Mod is OTANpc)
                {
                    var mod = vanillaNpc.Mod as OTANpc;

                    //check null and if dead
                    if (mod.Npc == null) continue; //mainNPC.isDead) continue;

                    float num05;
                    Npc.FindClosestPlayer(out num05);
                    if(num05 < 1f)
                    {
                        CollisionHit();
                    }
                }
            }
        }

        private void CollisionHit()
        {
            NPCFunctions.DebuffNearbyPlayers(5, 12, Npc.whoAmI, 30);
        }


    }
}