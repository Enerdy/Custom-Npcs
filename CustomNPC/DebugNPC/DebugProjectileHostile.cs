using System.Collections.Generic;
using CustomNPC;

namespace DebugNPC
{
    public sealed class DebugProjHostile : CustomNPCDefinition
    {
        public DebugProjHostile()
            : base(3)
        {
            customProjectiles.Add(new CustomNPCProjectiles(2, new List<ShotTile>() { ShotTile.Middle }, 10, 250, true, 100));
        }

        public override string customID
        {
            get { return "DEBUGPROJH"; }
        }

        public override string customName
        {
            get { return "Friendly player projectile turned hostile (test)"; }
        }

        public override bool overrideBaseNPCLoot
        {
            get { return true; }
        }

        public override string customSpawnMessage
        {
            get
            {
                return null;
            }
        }

        public override bool isBoss
        {
            get
            {
                return true;
            }
        }

        public override int customDefense
        {
            get
            {
                return 100;
            }
        }

        public override int customHealth
        {
            get
            {
                return 50;
            }
        }
    }
}