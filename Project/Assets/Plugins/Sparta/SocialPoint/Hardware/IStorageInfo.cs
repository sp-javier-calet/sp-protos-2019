namespace SocialPoint.Hardware
{
    public interface IStorageInfo
    {
        ulong TotalStorage { get; }

        ulong FreeStorage { get; }

        ulong UsedStorage { get; }
    }
}