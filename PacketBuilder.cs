
using System;
using System.Collections.Generic;
using System.Linq;

public enum ModuleType
{
    NONE, MAIN, LOGIN, GAMEROOM, DB
}

public enum ServiceType
{
    NONE, LOBBY, GAME,
}

public enum CommandType
{
    /* MAIN */ EXIT,
    /* GAMEROOM */ CREATE_LOBBY, LOBBY_INVITE, REQUEST_INVITE, ACCEPT_INVITE, LOBBY_KICK, START_GAME, READY_UP,
    /* LOBBY */ PLAYER_LIST, READY_STATE, LOAD_GAME, CHECK_IN_STATES, CONFIRM_CONNECTION, CONFIRM_INVITE, NEXT_ROUND, SUBMIT_DEBUFF, SUBMIT_SCORE, SEND_PLACEMENT, END_GAME, PLAYER_DISCONNECTED,
    /* GAME */ PLAYER_SCORE, PLAYER_GOLD, PLAYER_MESSAGE, PLAYER_STREAM, PLAYER_EMOTE, PLAYER_PACTS,
}

public class PacketBuilder
{

    public static Guid Guid = Guid.Empty;

    /* GAME ROOM */

    public static NetPacket StartGame()
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.GAMEROOM);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.START_GAME);
        return packet;
    }

    public static NetPacket CreateLobby()
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.GAMEROOM);
        packet.Write((byte)ServiceType.NONE);
        packet.Write((byte)CommandType.CREATE_LOBBY);
        // packet.Write(NetworkManager.Instance.Username);
        return packet;
    }

    public static NetPacket CheckInStates(int[] playerIDs, string[] usernames)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.NONE);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.CHECK_IN_STATES);
        packet.Write(playerIDs);
        packet.Write(usernames);
        return packet;
    }

    public static NetPacket RequestInvite()
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.GAMEROOM);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.REQUEST_INVITE);
        return packet;
    }

    public static NetPacket AcceptInvite(string lobbyID)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.GAMEROOM);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.ACCEPT_INVITE);
        packet.Write(lobbyID);
        return packet;
    }

    public static NetPacket KickPlayer(string player)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.GAMEROOM);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.LOBBY_KICK);
        packet.Write(player);
        return packet;
    }

    public static NetPacket ReadyUp()
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.GAMEROOM);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.READY_UP);
        return packet;
    }

    /* LOBBY */
    public static NetPacket PlayerList(string[] usernames)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.NONE);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.PLAYER_LIST);
        packet.Write(usernames);
        return packet;
    }

    public static NetPacket ReadyState(string[] guids, bool[] ready)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.NONE);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.READY_STATE);
        packet.Write(guids);
        packet.Write(ready);
        return packet;
    }

    public static NetPacket ConfirmInvite(bool createLobby, int roomID = -1)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.NONE);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.CONFIRM_INVITE);
        packet.Write(createLobby);
        if (roomID != -1)
            packet.Write(roomID);
        return packet;
    }

    public static NetPacket InviteToLobby(string toInvite)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.NONE);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.LOBBY_INVITE);
        packet.Write(toInvite);
        return packet;
    }

    public static NetPacket LoadGame()
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.NONE);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.LOAD_GAME);
        return packet;
    }

    public static NetPacket ConfirmConnection()
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.NONE);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.CONFIRM_CONNECTION);
        return packet;
    }

    public static NetPacket NextRound()
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.NONE);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.NEXT_ROUND);
        return packet;
    }

    public static NetPacket SubmitDebuff(byte debuffID)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.GAMEROOM);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.SUBMIT_DEBUFF);
        packet.Write(debuffID);
        return packet;
    }

    public static NetPacket SubmitScore(long score)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.GAMEROOM);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.SUBMIT_SCORE);
        packet.Write(score);
        return packet;
    }

    public static NetPacket SendPlacement(string[] guids, long[] scores)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.GAMEROOM);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.SEND_PLACEMENT);
        packet.Write(guids);
        packet.Write(scores);
        return packet;
    }

    public static NetPacket EndGame()
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.GAMEROOM);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.END_GAME);
        return packet;
    }

    public static NetPacket PlayerDisconnected(string guid)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ModuleType.NONE);
        packet.Write((byte)ServiceType.LOBBY);
        packet.Write((byte)CommandType.PLAYER_DISCONNECTED);
        packet.Write(guid);
        return packet;
    }

    /* GAME */
    public static NetPacket PlayerScore(string guid, string score)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ServiceType.GAME);
        packet.Write((byte)CommandType.PLAYER_SCORE);
        packet.Write(guid);
        packet.Write(score);
        return packet;
    }

    public static NetPacket PlayerGold(string guid, int gold)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ServiceType.GAME);
        packet.Write((byte)CommandType.PLAYER_GOLD);
        packet.Write(guid);
        packet.Write(gold);
        return packet;
    }

    public static NetPacket ChatMessage(string name, string message)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ServiceType.GAME);
        packet.Write((byte)CommandType.PLAYER_MESSAGE);
        packet.Write(name);
        packet.Write(message);
        return packet;
    }

    public static NetPacket ChatEmote(string guid, string emote)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ServiceType.GAME);
        packet.Write((byte)CommandType.PLAYER_EMOTE);
        packet.Write(guid);
        packet.Write(emote);
        return packet;
    }

    public static NetPacket PlayerStream(string guid, byte[] streamData)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ServiceType.GAME);
        packet.Write((byte)CommandType.PLAYER_STREAM);
        packet.Write(guid);
        packet.Write(streamData);
        return packet;
    }

    public static NetPacket PlayerPacts(string guid, string[] pactIdentifiers)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)ServiceType.GAME);
        packet.Write((byte)CommandType.PLAYER_PACTS);
        packet.Write(guid);
        packet.Write(pactIdentifiers);
        return packet;
    }

}