using System;
using System.Collections.Generic;
using Terraria;
using OTA.Mod.Npc;
using Microsoft.Xna.Framework;

namespace CustomNPCS
{
    public abstract class CustomNPCDefinition
    {
        private static readonly Color SummonColor = new Color(175, 75, 255);

        private NPC baseNPC;
        public IList<CustomProjectiles> projectiles;
        public IList<CustomNPCLoot> loots;
        public IList<CustomSpawning> spawns;

        protected CustomNPCDefinition(int id)
        {
            baseNPC = NpcUtils.GetNPCById(id);

            // check to make sure this npc is valid
            if (baseNPC == null || baseNPC.netID == 0)
                throw new ArgumentException("Invalid BaseNPC id specified: " + id, "id");

            projectiles = new List<CustomProjectiles>();
            loots = new List<CustomNPCLoot>();
            spawns = new List<CustomSpawning>();
        }

        public virtual IList<CustomProjectiles> customProjectiles
        {
            get { return projectiles; }
        }

        public virtual IList<CustomNPCLoot> customNPCLoots
        {
            get { return loots; }
        }

        public virtual IList<CustomSpawning> customSpawning
        {
            get { return spawns; }
        }

        public virtual bool overrideBaseNPCLoot
        {
            get { return false; }
        }

        public virtual long SEconReward
        {
            get { return 0L; }
        }

        public abstract string customID { get; }

        public virtual string customSpawnMessage
        {
            get { return null; }
        }

        public virtual Color customSpawnMessageColor
        {
            get { return SummonColor; }
        }

    }

}