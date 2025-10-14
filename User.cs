using System;
using LiteNetLib;

public class User
{
    public string GUID { get; set; }
    public string Username { get; set; }
    public NetPeer Peer { get; set; }

    public bool ReadyUp { get; set; }
    public long Score { get; set; }
    public byte PlayerID;

}