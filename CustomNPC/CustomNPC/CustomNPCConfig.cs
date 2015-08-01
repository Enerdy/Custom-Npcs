using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace CustomNPC
{
    public class CustomNpcConfig
    {
        public Dictionary<string, WaveSet> WaveSets { get; set; }
        /// <summary>
        /// Reads a configuration file from a given path
        /// </summary>
        /// <param name="path">string path</param>
        /// <returns>ConfigFile object</returns>
        public static CustomNpcConfig Read(string path)
        {
            if (!File.Exists(path))
                return new CustomNpcConfig();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs);
            }
        }

        /// <summary>
        /// Reads the configuration file from a stream
        /// </summary>
        /// <param name="stream">stream</param>
        /// <returns>ConfigFile object</returns>
        public static CustomNpcConfig Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var cf = JsonConvert.DeserializeObject<CustomNpcConfig>(sr.ReadToEnd());

                if (ConfigRead != null)
                    ConfigRead(cf);
                return cf;
            }
        }

        /// <summary>
        /// Writes the configuration to a given path
        /// </summary>
        /// <param name="path">string path - Location to put the config file</param>
        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Write(fs);
            }
        }

        /// <summary>
        /// Writes the configuration to a stream
        /// </summary>
        /// <param name="stream">stream</param>
        public void Write(Stream stream)
        {
            //minion spawns
            
            List<SpawnMinion> SpawnMinions1 = new List<SpawnMinion>();        
            SpawnMinion SpawnMinion1;
            SpawnMinion1 = new SpawnMinion("customnpc id", 100, BiomeTypes.None, SpawnConditions.None, false, true);
            SpawnMinion SpawnMinion2 = new SpawnMinion("customnpc id2", 100, BiomeTypes.None, spawnconditions: SpawnConditions.None, isboss: false, unittocheckkillamount: true);
            SpawnMinions1.Add(SpawnMinion1);
            SpawnMinions1.Add(SpawnMinion2);

            //SpawnGroups
            SpawnsGroups SpawnGroup1 = new SpawnsGroups(true, true, SpawnMinions1, 100);
            

            //Waves
            List<Waves> Waves1 = new List<Waves>();
            Waves Wave1 = new Waves("Insert Wavename", SpawnGroup1);
            Waves1.Add(Wave1);

            //WaveSets
            WaveSets = new Dictionary<string, WaveSet>();
            WaveSet WaveSets1 = new WaveSet("WaveName", Waves1);
            WaveSets.Add("WaveSet Name", WaveSets1);

            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(str);
            }
        }

        /// <summary>
        /// On config read hook
        /// </summary>
        public static Action<CustomNpcConfig> ConfigRead;
    }
}
