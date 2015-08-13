namespace SocialPoint.Hardware
{
    public interface IMemoryInfo
    {
        ulong TotalMemory { get; }

        ulong FreeMemory { get; }

        ulong UsedMemory { get; }

        ulong ActiveMemory { get; }
    }
}