using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Linq;

public class ServerManager
{

    public static ServerManager Instance;
    private static Random rand = new Random();
    private NetManager net;
    private EventBasedNetListener listener;
    private const int ServerPort = 7778;

    // Game Rooms
    private List<User> users;
    private Dictionary<NetPeer, User> peerLookup;
    private Dictionary<string, User> usernameLookup;

    // Services
    private Dictionary<ModuleType, ServerModule> modules;
    private bool isRunning = true;

    public static void Main(string[] args)
    {
        new ServerManager();
    }

    public ServerManager()
    {

        Instance = this;

        users = new List<User>();
        peerLookup = new Dictionary<NetPeer, User>();
        usernameLookup = new Dictionary<string, User>();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Prevent immediate termination
            Console.WriteLine("\nStopping server...");
            isRunning = false;
        };

        modules = new Dictionary<ModuleType, ServerModule>
        {
            { ModuleType.GAMEROOM, new GameRoomModule() }
        };

        listener = new EventBasedNetListener();

        listener.ConnectionRequestEvent += (request) => {
            OnConnectionRequest(request);
        };
        
        listener.PeerDisconnectedEvent += (peer, info) => {
            OnPeerDisconnected(peer, info);
        };

        listener.NetworkReceiveEvent += (peer, reader, channel, method) => {
            OnMessageReceive(peer, reader, channel, method);
        };

        net = new NetManager(listener);
        net.Start(ServerPort);

        Console.WriteLine("Server started.");

        while(isRunning)
        {
            net.PollEvents();
            foreach (ServerModule service in modules.Values)
            {
                service.Tick();
            }
            Thread.Sleep(20);
        }

        net.Stop();
        Console.WriteLine("Server stopped.");

    }

    public void OnConnectionRequest(ConnectionRequest req)
    {
        string connectionData = req.Data.GetString();
        string[] connectionDataSections = connectionData.Split(":");

        // Accept peer
        NetPeer peer = req.Accept();
        string GUID = connectionDataSections[0];
        string username = connectionDataSections[1];

        Console.WriteLine("Player connected: " + GUID + " | " + username);

        User user = new User();
        user.GUID = GUID;
        user.Username = username;
        user.Peer = peer;
        users.Add(user);

        peerLookup.Add(peer, user);
        usernameLookup.Add(username, user);

        foreach (ServerModule module in modules.Values)
            module.UserJoined(user);

        SendMessage(user, PacketBuilder.ConfirmConnection(), DeliveryMethod.ReliableUnordered);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
    {

        User user = peerLookup[peer];
        
        foreach(ServerModule module in modules.Values)
        {
            module.DisconnectUser(user);
        }

        users.Remove(user);
        peerLookup.Remove(peer);
        usernameLookup.Remove(user.Username);
        Console.WriteLine("Client disconnected from game server.");
        
    }

    public void OnMessageReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod method)
    {

        byte[] byteData = new byte[reader.AvailableBytes];
        reader.GetBytes(byteData, reader.AvailableBytes);

        NetPacket packet = new NetPacket(byteData);

        ModuleType moduleType = (ModuleType) packet.ReadByte();
        ServiceType serviceType = (ServiceType) packet.ReadByte();
        CommandType commandType = (CommandType) packet.ReadByte();
    
        if(modules.ContainsKey(moduleType))
        {
            modules[moduleType].ReceiveData(peerLookup[peer], packet, moduleType, serviceType, commandType, method);
        }

    }

    public User PeerLookup(NetPeer peer)
    {
        return peerLookup[peer];
    }

    public User UsernameLookup(string username)
    {
        return usernameLookup[username];
    }

    public void SendMessage(User user, NetPacket packet, DeliveryMethod method){
        user.Peer.Send(packet.ByteArray, method);
    }

    public string[] CreatePlayerList()
    {
        return users.Select(u => u.Username).ToArray();
    }

    public int GetRandomID()
    {
        return rand.Next(111_111_111, 1_000_000_000);
    }

}