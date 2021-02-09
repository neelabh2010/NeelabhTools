namespace NeelabhMVCTools.CacheServices
{
    public interface ICacheGenericeRepository<T> : ICacheRepository
    {
        T GetDataCache(bool LocalCacheOnly = false);
        void SetDataCache(T DataCache);
    }
}
