public interface INetworkInitializable
{
    /// <summary>
    /// 숫자가 낮을수록 먼저 실행.
    /// </summary>
    int InitializationPriority { get; }
    public void NetworkInitialize();
}
