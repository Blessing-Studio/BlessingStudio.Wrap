using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Client
{
    public class PeerManager
    {
        public List<Connection> connections = new();
        public PeerManager() { }
        public void AddPeer(Connection connection)
        {
            connections.Add(connection);
            connection.AddHandler((ChannelCreatedEvent e) =>
            {
                Console.WriteLine("gg!");
            });
        }
    }
}
