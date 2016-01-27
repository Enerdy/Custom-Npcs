using OTA.Plugin;
using OTA.Command;
using OTA.Logging;
using Terraria;
using System.IO;
using OTA;
using OTA.Mod;
using OTA.Mod.Npc;
using System;
using System.Timers;
using Microsoft.Xna.Framework;
using System.Linq;

namespace CustomNPCS
{
    [OTAVersion(1, 0)]
    public class CUSTOMNPCS : BasePlugin
    {
        public CUSTOMNPCS()
        {
            this.Version = "1";
            this.Author = "Pychnight";
            this.Name = "Custom NPCS";
            this.Description = "Simple Custom NPCS";
        }

        private Timer mainLoop = new Timer(1000 / 60.0);

        protected override void Initialized(object state)
        {
            ProgramLog.Plugin.Log("Your plugin is initialising");
        }

        void mainLoop_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Don't run this when there is no players
            if (NpcUtils.ActivePlayerCount == 0)
            {
                return;
            }

            //check if NPC has been deactivated (could mean NPC despawned)
            CheckActiveNPCs();
            //Spawn mobs into regions and specific biomes
            SpawnMobsInBiomeAndRegion();

            //Commented out, since this should happen on the main thread
            //Update All NPCs with custom AI (that are alive)
            CustomNPCUpdate(true, false);

        }

        private void SpawnMobsInBiomeAndRegion()
        {
            CustomSpawning.SpawnMobsInBiomeAndRegion();
        }

        private void CheckActiveNPCs()
        {

            foreach (var vanillaNpc in Main.npc)
            {
                if (vanillaNpc.Mod != null && vanillaNpc.Mod is OTANpc)
                {
                    var mod = vanillaNpc.Mod as OTANpc;

                    if (mod == null) continue;

                    if ((mod.Npc.active = false && !mod.Npc.dontCountMe) || mod == null || mod.Npc.life <= 0 || mod.Npc.type == 0)
                    {
                        mod.Npc.checkDead();
                    }
                    else if (mod.Npc.active == true)
                    {
                        mod.Npc.whoAmI = mod.Npc.whoAmI;
                    }
                }
            }
        }


        [Hook(HookOrder.NORMAL)]
        void killevent(ref HookContext ctx, ref HookArgs.NpcKilled args)
        {
            //Your implementation
        }

        [Hook]
        void customloot(ref HookContext ctx, ref HookArgs.NpcDropLoot args)
        {
        }

        [Hook]
        void customlootBOSS(ref HookContext ctx, ref HookArgs.NpcDropBossBag args)
        {
            //Your implementation
        }


        [Hook]
        void OnPlayerEnter(ref HookContext ctx, ref HookArgs.PlayerEnteredGame args)
        {
            OTA.Logging.ProgramLog.Log($"Spawning {NpcTest.test}");
            NPC.NewNPC((int)(ctx.Player.position.X), (int)(ctx.Player.position.Y), EntityRegistrar.Npcs[NpcTest.test]);
        }

        [Hook]
        void onupdate(ref HookContext ctx, ref HookArgs.ServerUpdate args)
        {
            CustomNPCUpdate(true, false);
        }

        //
        /// <summary>
        /// Fires a projectile given the target starting position and projectile class
        /// </summary>
        /// <param name="target"></param>
        /// <param name="origin"></param>
        /// <param name="projectile"></param>
        private void FireProjectile(Player target, OTANpc mainNPC, CustomProjectiles projectile)
        {
            //Loop through all ShotTiles
            foreach (ShotTile obj in projectile.projectileShotTiles)
            {
                //Make sure target actually exists - at this point it should always exist
                if (target == null) continue;

                //Calculate starting position
                Vector2 start = GetStartPosition(mainNPC, obj);
                //Calculate speed of projectile
                Vector2 speed = CalculateSpeed(start, target);
                //Get the projectile AI
                Tuple<float, float> ai = Tuple.Create(projectile.projectileAIParams1, projectile.projectileAIParams2);

                // Create new projectile VIA Terraria's method, with all customizations
                int projectileIndex = Projectile.NewProjectile(
                                                      start.X,
                                                      start.Y,
                                                      speed.X,
                                                      speed.Y,
                                                      projectile.projectileID,
                                                      projectile.projectileDamage,
                                                      0,
                                                      255,
                                                      ai.Item1,
                                                      ai.Item2);

                // customize AI for projectile again, as it gets overwritten
                var proj = Main.projectile[projectileIndex];
                proj.ai[0] = ai.Item1;
                proj.ai[1] = ai.Item2;

                // send projectile as a packet
                NetMessage.SendData(27, -1, -1, string.Empty, projectileIndex, 0f, 0f, 0f, 0, 0, 0);
            }
        }

        // Returns start position of projectile with shottile offset
        private Vector2 GetStartPosition(OTANpc mainNPC, ShotTile shottile)
        {
            Vector2 offset = new Vector2(shottile.X, shottile.Y);
            return mainNPC.Npc.Center + offset;
        }

        //calculates the x y speed required angle

        private Vector2 CalculateSpeed(Vector2 start, Player target)

        {

            Vector2 targetCenter = target.Center;

            float dirX = targetCenter.X - start.X;
            float dirY = targetCenter.Y - start.Y;
            float factor = 10f / (float)Math.Sqrt((dirX * dirX) + (dirY * dirY));
            float speedX = dirX * factor;
            float speedY = dirY * factor;

            return new Vector2(speedX, speedY);
        }

        /// <summary>
        /// Checks if any player is within targetable range of the npc, without obstacle and within firing cooldown timer.
        /// </summary>
        private void ProjectileCheck()
        {
            //Loop through all custom npcs currently spawned
            foreach (var vanillaNpc in Main.npc)
            {
                if (vanillaNpc.Mod != null && vanillaNpc.Mod is OTANpc) // CustomCore.CNpc
                {
                    var mod = vanillaNpc.Mod as OTANpc;

                    //Check if they exists and are active
                    if (mod.Npc == null || !mod.Npc.active) continue;

                    //We only want the npcs with custom projectiles
                    if (mod.Npc == null) continue;

                    //customProjectiles

                    //Save the current time
                    DateTime savedNow = DateTime.Now;

                    int k = 0;
                    //Loop through all npc projectiles they can fire
                    /*
                    foreach (CustomProjectiles projectile in mod.customprojectile )
                    {

                    //customProjectiles
                    //Check if projectile last fire time is greater then equal to its next allowed fire time
                    if ((savedNow - mod.lastAttemptedProjectile[k]).TotalMilliseconds >= projectile.projectileFireRate)
                        {
                            //public DateTime[] lastAttemptedProjectile { get; set; }
                            //Make sure chance is checked too, don't bother checking if its 100
                            if (projectile.projectileFireChance == 100 || NPCFunctions.Chance(projectile.projectileFireChance))
                            {
                                Terraria.Player target = null;

                                if (projectile.projectileLookForTarget)
                                {
                                    //Find a target for it to shoot that isn't dead or disconnected

                                    foreach (Player player in Terraria.Main.player.Where(x => x != null && !x.dead && x.active))
                                    {
                                        //Check if that target can be shot ie/ no obstacles, or if it if projectile goes through walls ignore this check
                                        if (!projectile.projectileCheckCollision || CanHit(player.position, (int)player.bodyFrame.Width, (int)player.bodyFrame.Height, mod.Npc.position, (int)mod.Npc.frame.Width, (int)mod.Npc.frame.Height))
                                        {
                                            //Make sure distance isn't further then what tshock allows
                                            float currDistance = Vector2.DistanceSquared(player.position, mod.Npc.Center);

                                            //Distance^2 < 4194304 is the same as Distance < 2048, but faster
                                            if (currDistance < 4194304)
                                            {
                                                //Set the target player
                                                target = player;
                                                //Set npcs target to the player its shooting at
                                                mod.Npc.target = player.whoAmI;
                                                //Break since no need to find another target
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {

                                    target = Terraria.Main.player[mod.Npc.target];
                                }

                                //Check if we found a valid target
                                if (target != null)
                                {
                                    //All checks completed. Fire projectile
                                    FireProjectile(target, mod, projectile);
                                    //Set last attempted projectile to now
                                    // mod.Npc.lastAttemptedProjectile[k] = savedNow;
                                }
                            }
                            else
                            {
                                // mod.Npc.lastAttemptedProjectile[k] = savedNow;
                            }
                            */
                }

                //Increment Index
                // k++;
            }
        }

        public static bool CanHit(Vector2 Position1, int Width1, int Height1, Vector2 Position2, int Width2, int Height2)
        {
            int num = (int)((Position1.X + (float)(Width1 / 2)) / 16);
            int num2 = (int)((Position1.Y + (float)(Height1 / 2)) / 16);
            int num3 = (int)((Position2.X + (float)(Width2 / 2)) / 16);
            int num4 = (int)((Position2.Y + (float)(Height2 / 2)) / 16);
            if (num <= 1)
            {
                num = 1;
            }
            if (num >= Main.maxTilesX)
            {
                num = Main.maxTilesX - 1;
            }
            if (num3 <= 1)
            {
                num3 = 1;
            }
            if (num3 >= Main.maxTilesX)
            {
                num3 = Main.maxTilesX - 1;
            }
            if (num2 <= 1)
            {
                num2 = 1;
            }
            if (num2 >= Main.maxTilesY)
            {
                num2 = Main.maxTilesY - 1;
            }
            if (num4 <= 1)
            {
                num4 = 1;
            }
            if (!(num4 < Main.maxTilesY))
            {
                num4 = Main.maxTilesY - 1;
            }
            bool flag;
            try
            {
                while (true)
                {
                    int num5 = Math.Abs(num - num3);
                    int num6 = Math.Abs(num2 - num4);
                    if (num == num3 && num2 == num4)
                    {
                        flag = true;
                        return flag;
                    }
                    if (num5 <= num6)
                    {
                        num2 = ((num2 < num4) ? (num2 + 1) : (num2 - 1));
                        if (Main.tile[num - 1, num2] == null)
                        {
                            break;
                        }
                        if (Main.tile[num + 1, num2] == null)
                        {
                            flag = false;
                            return flag;
                        }
                        if (!Main.tile[num - 1, num2].inActive() && Main.tile[num - 1, num2].active() && Main.tileSolid[(int)Main.tile[num - 1, num2].type] && !Main.tileSolidTop[(int)Main.tile[num - 1, num2].type] && Main.tile[num - 1, num2].slope() == 0 && !Main.tile[num - 1, num2].halfBrick() && !Main.tile[num + 1, num2].inActive() && Main.tile[num + 1, num2].active() && Main.tileSolid[(int)Main.tile[num + 1, num2].type] && !Main.tileSolidTop[(int)Main.tile[num + 1, num2].type] && Main.tile[num + 1, num2].slope() == 0 && !Main.tile[num + 1, num2].halfBrick())
                        {
                            flag = false;
                            return flag;
                        }
                    }
                    else
                    {
                        num = ((num < num3) ? (num + 1) : (num - 1));
                        if (Main.tile[num, num2 - 1] == null)
                        {
                            flag = false;
                            return flag;
                        }
                        if (Main.tile[num, num2 + 1] == null)
                        {
                            flag = false;
                            return flag;
                        }
                        if (!Main.tile[num, num2 - 1].inActive() && Main.tile[num, num2 - 1].active() && Main.tileSolid[(int)Main.tile[num, num2 - 1].type] && !Main.tileSolidTop[(int)Main.tile[num, num2 - 1].type] && Main.tile[num, num2 - 1].slope() == 0 && !Main.tile[num, num2 - 1].halfBrick() && !Main.tile[num, num2 + 1].inActive() && Main.tile[num, num2 + 1].active() && Main.tileSolid[(int)Main.tile[num, num2 + 1].type] && !Main.tileSolidTop[(int)Main.tile[num, num2 + 1].type] && Main.tile[num, num2 + 1].slope() == 0 && !Main.tile[num, num2 + 1].halfBrick())
                        {
                            flag = false;
                            return flag;
                        }
                    }
                    if (Main.tile[num, num2] == null)
                    {
                        flag = false;
                        return flag;
                    }
                    if (!Main.tile[num, num2].inActive() && Main.tile[num, num2].active() && Main.tileSolid[(int)Main.tile[num, num2].type] && !Main.tileSolidTop[(int)Main.tile[num, num2].type])
                    {
                        flag = false;
                    }
                }
                flag = false;
                return flag;
            }
            catch
            {
                flag = false;
            }
            return flag;
        }


        private void CustomNPCUpdate(bool onlyCustom = true, bool updateDead = false)
        {
            foreach (var vanillaNpc in Main.npc)
            {
                if (vanillaNpc.Mod != null && vanillaNpc.Mod is OTANpc)
                {
                    var mod = vanillaNpc.Mod as OTANpc;

                    if (mod == null || mod.Npc.life <= 0 || mod.Npc.type == 0)
                    {
                        if (updateDead)
                            continue;
                    }

                    if (!onlyCustom)
                    {
                        if (Main.netMode == 2)
                        {
                            //Schedule updates to occur every server tick (once every 1/60th second)
                            if (mod.Npc.netSpam > 0) mod.Npc.netSpam = 0;
                            mod.Npc.netUpdate = true;
                            mod.Npc.netUpdate2 = true;
                        }
                        else
                        {
                            NetMessage.SendData(23, -1, -1, "", mod.Npc.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                        }
                    }
                }
            }
        }
    }
}

