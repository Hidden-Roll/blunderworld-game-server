using LiteNetLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;

public class GameRoom : ServerService
{

    public User HostUser { get; private set; }
    public ServerGameData GameData { get; private set; }

    private List<User> users = new List<User>();
    private Dictionary<ServiceType, ServerService> services = new Dictionary<ServiceType, ServerService>();
    private Dictionary<User, byte> userDebuffs = new Dictionary<User, byte>();

    private int roomID;
    private byte playerIDCount = 1;
    private bool gameStarted;
    private bool roundStarting;

    public GameRoom()
    {
        roomID = ServerManager.Instance.GetRandomID();

        // services.Add(ServiceType.GAME, new GameObject().AddComponent<GameServerService>());

        foreach(ServerService service in services.Values)
            service.Init(this);

        GameData = new ServerGameData(this);

        GameLoop();
    }

    public async void GameLoop()
    {
        while (!gameStarted)
            await Task.Delay(200);

        Console.WriteLine("Starting game for room: " + GetRoomID());
        SendToRoom(PacketBuilder.StartGame(), DeliveryMethod.ReliableUnordered);

        await Task.Delay(5000);

        for (int i = 0; i < 14; i++)
        {
            users.ForEach(u => u.ReadyUp = false);
            while (!users.All(u => u.ReadyUp))
            {
                await Task.Delay(1000);
            }

            roundStarting = true;

            // All readied up, go through and apply all the debuffs
            // TODO: APPLY DEBUFFS
            foreach (User user in users)
            {
                if (userDebuffs.ContainsKey(user))
                {
                    // Apply debuffs to every other user
                    SendToRoom(PacketBuilder.SubmitDebuff(userDebuffs[user]), DeliveryMethod.ReliableUnordered, user);
                }
            }

            await Task.Delay(2000);
            userDebuffs.Clear();

            // NEXT ROUND
            SendToRoom(PacketBuilder.NextRound(), DeliveryMethod.ReliableUnordered);
            await Task.Delay(500);
            users.ForEach(u => u.ReadyUp = false);
            SendToRoom(GetReadyState(), DeliveryMethod.ReliableUnordered);
            await Task.Delay(3000);
            roundStarting = false;
        }

        while (!users.All(u => u.ReadyUp))
        {
            await Task.Delay(1000);
        }

        List<User> placement = users.OrderBy(u => u.Score).ToList();
        placement.Reverse();
        SendToRoom(PacketBuilder.SendPlacement(placement.Select(u => u.GUID).ToArray(), placement.Select(u => u.Score).ToArray()), DeliveryMethod.ReliableUnordered);

        await Task.Delay(15000);

        SendToRoom(PacketBuilder.EndGame(), DeliveryMethod.ReliableUnordered);

    }

    public override void ReceiveData(User user, NetPacket packet, ModuleType moduleType, ServiceType serviceType, CommandType commandType, DeliveryMethod receivedMethod)
    {
        if(services.ContainsKey(serviceType))
        {
            services[serviceType].ReceiveData(user, packet, moduleType, serviceType, commandType, receivedMethod);
        }
    }

    public override void Tick()
    {
        foreach (ServerService service in services.Values)
            service.Tick();
    }

    public override void DisconnectUser(User user)
    {
        if(users.Contains(user))
        {
            users.Remove(user);
        }
    }

    public void SubmitDebuff(User owner, byte debuffID)
    {
        userDebuffs[owner] = debuffID;
    }

    public void ReadyUpUser(User user)
    {
        if (!gameStarted)
        {
            user.ReadyUp = !user.ReadyUp;
            SendToRoom(GetReadyState(), DeliveryMethod.ReliableUnordered);

            if (users.All(u => u.ReadyUp) && users.Count > 1)
            {
                users.ForEach(u => u.ReadyUp = false);
                gameStarted = true;
                StartGame();
            }
        }
        else
        {
            if (!roundStarting)
            {
                user.ReadyUp = !user.ReadyUp;
                SendToRoom(GetReadyState(), DeliveryMethod.ReliableUnordered);
            }
        }
    }

    public void SubmitScore(User user, long score)
    {
        user.Score += score;
    }
    
    public NetPacket GetReadyState()
    {
        // Create checked in list
        List<string> guids = new List<string>();
        List<bool> ready = new List<bool>();
        foreach (User u in users)
        {
            guids.Add(u.GUID);
            ready.Add(u.ReadyUp);
        }
        return PacketBuilder.ReadyState(guids.ToArray(), ready.ToArray());
    }

    public int GetRoomID()
    {
        return roomID;
    }

    public void AddUser(User user)
    {
        users.Add(user);
        user.PlayerID = playerIDCount;
        user.ReadyUp = false;
        user.Score = 0;
        playerIDCount++;
        GameRoomModule.Instance.DocumentUser(this, user);
        SendToRoom(GetReadyState(), DeliveryMethod.ReliableUnordered);
    }

    public void RemoveUser(User user)
    {
        users.Remove(user);
        GameRoomModule.Instance.UndocumentUser(user);

        SendToRoom(PacketBuilder.PlayerDisconnected(user.GUID), DeliveryMethod.ReliableUnordered);

        if (users.Count == 0)
        {
            GameRoomModule.Instance.CloseGameRoom(this);
        }
        SendToRoom(GetReadyState(), DeliveryMethod.ReliableUnordered);
    }

    public void SetLobbyHost(User user)
    {
        if(users.Contains(user))
        {
            this.HostUser = user;
        }
    }

    public int GetLobbyHostIndex()
    {
        return users.IndexOf(HostUser);
    }

    public List<User> GetUsers()
    {
        return users;
    }

    public void SendToRoom(NetPacket packet, DeliveryMethod method, User exception = null)
    {
        foreach(User user in users)
        {
            if(user != exception)
                ServerManager.Instance.SendMessage(user, packet, method);
        }
    }

    public void SendToUser(User user, NetPacket packet, DeliveryMethod method)
    {
        ServerManager.Instance.SendMessage(user, packet, method);
    }

    public override void UserJoined(User user)
    {
        foreach(ServerService service in services.Values)
        {
            service.UserJoined(user);
        }
    }

    public void StartGame()
    {
        SendToRoom(PacketBuilder.LoadGame(), DeliveryMethod.ReliableUnordered);
    }

    public void Log(string message)
    {
        Console.WriteLine("<color=orange><b>GAME ROOM</b></color>: " + message);
    }

}