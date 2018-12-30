﻿using Phantasma.Blockchain;
using Phantasma.Core.Log;
using Phantasma.Spook.GUI;
using System;

namespace Phantasma.Spook.Plugins
{
    public class TPSPlugin : IChainPlugin, IPlugin
    {
        public string Channel => "tps";

        private int periodInSeconds;
        private DateTime lastTime = DateTime.UtcNow;
        private int txCount;
        private Logger logger;
        private Graph graph;
        private ConsoleGUI gui;

        public TPSPlugin(Logger logger, int periodInSeconds)
        {
            this.logger = logger;
            this.periodInSeconds = periodInSeconds;
            this.gui = logger as ConsoleGUI;

            if (gui != null)
            {
                this.graph = new Graph();
                gui.SetChannelGraph(Channel, graph);
            }
        }

        public override void OnTransaction(Chain chain, Block block, Transaction transaction)
        {
            lock (this)
            {
                txCount++;
            }
        }

        public void Update()
        {
            var currentTime = DateTime.UtcNow;
            var diff = (currentTime - lastTime).TotalSeconds;

            if (diff >= periodInSeconds)
            {
                lastTime = currentTime;
                var tps = txCount / (float)periodInSeconds;

                var str = $"{tps.ToString("0.##")} TPS";
                if (gui != null)
                {
                    graph.Add(tps);
                    gui.WriteToChannel(Channel, LogEntryKind.Message, str);
                }
                else
                {
                    logger.Message(str);
                }

                lock (this)
                {
                    txCount = 0;
                }
            }
        }
    }
}
