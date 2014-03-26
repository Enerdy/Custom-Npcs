﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace CustomNPC
{
    public abstract class CustomNPCDefinition
    {
        private static readonly Color SummonColor = new Color(175, 75, 255);

        private NPC baseNPC;
        private IList<CustomNPCProjectiles> projectiles;
        private IList<CustomNPCLoot> loots;

        protected CustomNPCDefinition(int id)
        {
            baseNPC = TShock.Utils.GetNPCById(id);

            // check to make sure this npc is valid
            if (baseNPC == null || baseNPC.netID == 0)
                throw new ArgumentException("Invalid BaseNPC id specified: " + id, "id");

            projectiles = new List<CustomNPCProjectiles>();
            loots = new List<CustomNPCLoot>();
        }

        /// <summary>
        /// NPC Variable Customizations
        /// </summary>
        public virtual NPC customBase
        {
            get { return baseNPC; }
        }
        public virtual bool isReplacement
        {
            get { return false; }
        }
        public abstract string customID { get; }
        public abstract string customName { get; }
        public virtual int customHealth
        {
            get { return customBase.lifeMax; }
        }
        public virtual int customDefense
        {
            get { return customBase.defense; }
        }
        protected virtual int customSpeed
        {
            get { throw new NotImplementedException(); }
        }
        public virtual int customAI
        {
            get { return customBase.aiStyle; }
        }
        public virtual bool isBoss
        {
            get { return customBase.boss; }
        }
        public virtual bool noGravity
        {
            get { return customBase.noGravity; }
        }
        public virtual bool noTileCollide
        {
            get { return customBase.noTileCollide; }
        }
        public virtual bool lavaImmune
        {
            get { return customBase.lavaImmune; }
        }

        /// <summary>
        /// NPC Restrictions/Conditions
        /// </summary>
        //public virtual List<BiomeTypes> customBiomeSpawn
        //{
        //    get { return new List<BiomeTypes>(); }
        //}
        //public virtual IList<string> customRegionSpawn
        //{
        //    get { return null; }
        //}

        // NPC Projectile Variables

        /// <summary>
        /// Gets a list of projectiles this NPC can fire.
        /// </summary>
        public virtual IList<CustomNPCProjectiles> customProjectiles
        {
            get { return projectiles; }
        }

        // NPC Loot Variables

        /// <summary>
        /// Gets a list of items this NPC can drop.
        /// </summary>
        public virtual IList<CustomNPCLoot> customNPCLoots
        {
            get { return loots; }
        }

        /// <summary>
        /// Gets a boolean that determines whether this NPC should drop its base NPC loot.
        /// </summary>
        public virtual bool overrideBaseNPCLoot
        {
            get { return false; }
        }

        // NPC spawning variables

        /// <summary>
        /// Spawn definition defined here
        /// </summary>
        public virtual IList<CustomNPCSpawning> customNPCSpawning
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the number of seconds between spawn attempts.
        /// </summary>
        //public virtual int customSpawnTimer
        //{
        //    get { return 5; }
        //}

        /// <summary>
        /// Gets the chance (out of 100) to spawn this NPC.
        /// </summary>
        //public virtual double customSpawnChance
        //{
        //    get { return 100.0; }
        //}

        /// <summary>
        /// Gets the message displayed when this NPC is spawned.
        /// </summary>
        public virtual string customSpawnMessage
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the color of the custom message displayed when this NPC is spawned.
        /// </summary>
        public virtual Color customSpawnMessageColor
        {
            get { return SummonColor; }
        }

        /// <summary>
        /// NPC MISC
        /// </summary>
        //internal CustomParticle customParticle { get; set; }; 

        public virtual void OnDeath(CustomNPCVars vars)
        {
        }
    }
}
