using LiteNetLib;

public abstract class ServerModule
{
    public abstract void ReceiveData(User user, NetPacket packet, ModuleType moduleType, ServiceType serviceType, CommandType commandType, DeliveryMethod receivedMethod);
    public abstract void Tick();
    public abstract void DisconnectUser(User user);
    public abstract void UserJoined(User user);
}