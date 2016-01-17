﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using CustomNPC.EventSystem;
using System.Linq;

#if TShock
using TerrariaApi.Server;
using TShockAPI;
#elif OTAPI
using OTA;
using OTA.Extensions;
#endif

namespace CustomNPC.Plugins
{
    public sealed class PluginManager<TPlugin> : MarshalByRefObject
        where TPlugin : IPlugin
    {
        private IEventRegister _eventRegister;
        private DefinitionManager _definitionManager;
        //private PluginDiscoverer<TPlugin> _discoverer;
        private IList<Assembly> _assemblies;
        private IList<TPlugin> _plugins;

        public PluginManager(IEventRegister register, DefinitionManager definitionManager)
        {
            _eventRegister = register;
            _definitionManager = definitionManager;
            //_discoverer = new PluginDiscoverer<TPlugin>();
            _assemblies = new List<Assembly>();
            _plugins = new List<TPlugin>();
        }

        public static string PluginPath
        {
#if TShock
            get { return Path.Combine(ServerApi.ServerPluginsDirectoryPath, "CustomNPCs"); }
#elif OTAPI
            get { return Path.Combine(Globals.PluginPath, "CustomNPCs"); }
#endif
        }

        public IEventRegister EventRegister
        {
            get { return _eventRegister; }
        }

        public DefinitionManager Definitions
        {
            get { return _definitionManager; }
        }

        public IReadOnlyList<TPlugin> Plugins
        {
            get { return new ReadOnlyCollection<TPlugin>(_plugins); }
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Load(
#if USE_APPDOMAIN
            AppDomain domain
#endif
            )
        {
            if (!Directory.Exists(PluginPath))
            {
                Directory.CreateDirectory(PluginPath);
            }

            foreach (var file in Directory.GetFiles(PluginPath, "*.dll"))
            {
                //                string[] typeNames = _discoverer.GetPluginTypeNames(file);
                //                foreach (string typeName in typeNames)
                //                {
                //                    object[] args =
                //                    {
                //                        EventRegister,
                //                        Definitions
                //                    };

                //#if USE_APPDOMAIN
                //                    var plugin = (TPlugin)domain.CreateInstanceFromAndUnwrap(file, typeName, true, BindingFlags.Default, null, args, null, null);
                //                    _plugins.Add(plugin);
                //#else
                try
                {
                    Assembly asm = Assembly.Load(File.ReadAllBytes(file));
                    _assemblies.Add(asm);

                    //var plugin = (TPlugin)asm.CreateInstance(typeName, true, BindingFlags.Default, null, args, null, null);
                    foreach (var type in asm.GetTypes().Where(x => typeof(TPlugin).IsAssignableFrom(x) && !x.IsAbstract))
                    {
                        var plugin = (TPlugin)Activator.CreateInstance(type, EventRegister, Definitions);
                        _plugins.Add(plugin);
                    }
                }
                catch (Exception e)
                {
                    OTA.Logging.ProgramLog.Log(e, "Failed to load plugin " + file);
                }
                //#endif
                //                }
            }

            foreach (TPlugin plugin in _plugins)
            {
                plugin.Initialize();
            }
        }

        public void Reload()
        {
        }

        public void Unload()
        {
            foreach (TPlugin plugin in _plugins)
            {
                plugin.Dispose();
            }
        }
    }
}
