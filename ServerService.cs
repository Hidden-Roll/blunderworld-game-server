using LiteNetLib;

public abstract class ServerService
{

    protected GameRoom room;

    public virtual void Init(GameRoom room)
    {
        this.room = room;
    }
    public abstract void ReceiveData(User user, NetPacket packet, ModuleType moduleType, ServiceType serviceType, CommandType commandType, DeliveryMethod receivedMethod);
    public abstract void Tick();
    public abstract void UserJoined(User user);
    public abstract void DisconnectUser(User user);
}