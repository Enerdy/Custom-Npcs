using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomNPCS
{
        internal static class Extensions
        {
            public static CustomSpawning.BiomeTypes GetCurrentBiomes(this Terraria.Player player)
            {
                CustomSpawning.BiomeTypes biome = CustomSpawning.BiomeTypes.None;

                var _player = player;

                if (_player.ZoneCorrupt)
                    biome |= CustomSpawning.BiomeTypes.Corruption;

                if (_player.ZoneCrimson)
                    biome |= CustomSpawning.BiomeTypes.Crimsion;

                if (_player.ZoneDesert)
                    biome |= CustomSpawning.BiomeTypes.Desert;

                if (_player.ZoneDungeon)
                    biome |= CustomSpawning.BiomeTypes.Dungeon;

                if (_player.ZoneGlowshroom)
                    biome |= CustomSpawning.BiomeTypes.Glowshroom;

                if (_player.ZoneHoly)
                    biome |= CustomSpawning.BiomeTypes.Holy;

                if (_player.ZoneJungle)
                    biome |= CustomSpawning.BiomeTypes.Jungle;

                if (_player.ZoneMeteor)
                    biome |= CustomSpawning.BiomeTypes.Meteor;

                if (_player.ZonePeaceCandle)
                    biome |= CustomSpawning.BiomeTypes.PeaceCandle;

                if (_player.ZoneSnow)
                    biome |= CustomSpawning.BiomeTypes.Snow;

                if (_player.ZoneTowerNebula)
                    biome |= CustomSpawning.BiomeTypes.TowerNebula;

                if (_player.ZoneTowerSolar)
                    biome |= CustomSpawning.BiomeTypes.TowerSolar;

                if (_player.ZoneTowerStardust)
                    biome |= CustomSpawning.BiomeTypes.TowerStardust;

                if (_player.ZoneTowerVortex)
                    biome |= CustomSpawning.BiomeTypes.TowerVortex;

                if (_player.ZoneUndergroundDesert)
                    biome |= CustomSpawning.BiomeTypes.UndergroundDesert;

                if (_player.ZoneWaterCandle)
                    biome |= CustomSpawning.BiomeTypes.WaterCandle;

                if (biome == CustomSpawning.BiomeTypes.None)
                {
                    biome = CustomSpawning.BiomeTypes.Grass;
                }

                return biome;
            }

            public static Rectangle ToPixels(this Rectangle rectangle)
            {
                return new Rectangle(rectangle.X * 16, rectangle.Y * 16, rectangle.Width * 16, rectangle.Height * 16);
            }

            public static Rectangle ToTiles(this Rectangle rectangle)
            {
                return new Rectangle(rectangle.X / 16, rectangle.Y / 16, rectangle.Width / 16, rectangle.Height / 16);
            }
        }
    }
