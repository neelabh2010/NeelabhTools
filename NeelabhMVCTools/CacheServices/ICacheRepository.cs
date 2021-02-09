namespace NeelabhMVCTools.CacheServices
{
    public interface ICacheRepository
    {
        void CleanCache();
        int GetVersion();
        void UpdateVersion();
        bool ErrorInCache();
    }
}
