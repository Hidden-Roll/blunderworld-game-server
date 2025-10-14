

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiteNetLib;

public class GameRoomModule : ServerModule
{

    public static GameRoomModule Instance;

    private Dictionary<int, GameRoom> gameRooms = new Dictionary<int, GameRoom>();
    private Dictionary<User, GameRoom> userRooms = new Dictionary<User, GameRoom>();

    public GameRoomModule()
    {
        Instance = this;
    }

    public override void DisconnectUser(User user)
    {
        if(userRooms.ContainsKey(user))
        {
            GameRoom room = userRooms[user];
            room.RemoveUser(user);
        }
    }

    public override void ReceiveData(User user, NetPacket packet, ModuleType moduleType, ServiceType serviceType, CommandType commandType, DeliveryMethod receivedMethod)
    {
        if(userRooms.ContainsKey(user))
        {
            userRooms[user].ReceiveData(user, packet, moduleType, serviceType, commandType, receivedMethod);
        }

        switch(commandType)
        {
            case CommandType.REQUEST_INVITE:
            {
                bool createLobby = false;
                int roomID = 0;
                if (!userRooms.ContainsKey(user))
                {
                    // User is not in a room, create a room
                    GameRoom g = RegisterGameRoom();
                    g.AddUser(user);
                    createLobby = true;
                    roomID = g.GetRoomID();
                }
                // Tell them to create the lobby
                ServerManager.Instance.SendMessage(user, PacketBuilder.ConfirmInvite(createLobby, roomID), DeliveryMethod.ReliableUnordered);
                break;
            }

            case CommandType.ACCEPT_INVITE:
            {
                string lobbyID = packet.ReadString();
                GameRoom g = gameRooms[int.Parse(lobbyID)];
                g.AddUser(user);
                Console.WriteLine("User " + user.PlayerID + " joined lobby " + lobbyID);
                break;
            }

            case CommandType.LOBBY_KICK:
            {
                string player = packet.ReadString();
                GameRoom g = userRooms[ServerManager.Instance.UsernameLookup(player)];
                g.RemoveUser(user);
                break;
            }

            case CommandType.START_GAME:
            {
                GameRoom g = userRooms[user];
                g.StartGame();
                break;
            }
        
            case CommandType.READY_UP:
            {
                GameRoom g = userRooms[user];
                g.ReadyUpUser(user);
                break;
            }
            
            case CommandType.SUBMIT_DEBUFF:
            {
                GameRoom g = userRooms[user];
                g.SubmitDebuff(user, packet.ReadByte());
                break;
            }
            
            case CommandType.SUBMIT_SCORE:
            {
                GameRoom g = userRooms[user];
                g.SubmitScore(user, packet.ReadLong());
                break;
            }

        }

    }

    public override void Tick()
    {
        foreach(GameRoom room in gameRooms.Values)
        {
            room.Tick();
        }
    }

    public GameRoom RegisterGameRoom()
    {
        GameRoom room = new GameRoom();
        gameRooms.Add(room.GetRoomID(), room);
        return room;
    }

    public void DocumentUser(GameRoom room, User user)
    {
        userRooms[user] = room;
    }

    public void UndocumentUser(User user)
    {
        userRooms.Remove(user);
    }

    public override void UserJoined(User user)
    {
        
    }

    public void CloseGameRoom(GameRoom g)
    {
        Console.WriteLine("Closing game room: " + g.GetRoomID());
        gameRooms.Remove(g.GetRoomID());
    }

}