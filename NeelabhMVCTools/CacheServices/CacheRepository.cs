using System;
using System.Collections.Generic;
using System.Data;

namespace NeelabhMVCTools.CacheServices
{
    public abstract class CacheRepository<T>
        : ICacheRepository, ICacheGenericeRepository<T>
    {
        ///<summary>Keep your data.</summary>
        private T DataCache { get; set; }

        ///<summary>To know whether Datacache loaded successfully or not </summary>
        private bool HasErrorInCache { get; set; } = false;    // true when Loading success From DB 

        ///<summary>Get current version fo DataCache.</summary>
        private int DataVersion { get; set; } = 0;      // use datetime stamp instead -----------

        ///<summary>Make it true if you again want to load DataCache from databse server.</summary>
        private bool IsCacheClean { get; set; } = true;

        ///<summary>To check whether DataCache is loading.</summary>
        private bool IsCacheLoading { get; set; } = false;

        ///<summary>Use it to set the DataCache.</summary>
        public void SetDataCache(T DataCache)
        {
            this.DataCache = DataCache;
            IsCacheClean = false;
            IsCacheLoading = false;
        }

        ///<summary>Use it to retrive the current DataCache reference.</summary>
        public T GetDataCache(bool LocalCacheOnly = false)
        {
            if (!LocalCacheOnly && (DataCache == null || IsCacheClean))
            {
                IsCacheLoading = true;
                LoadRepository();
                IsCacheLoading = false;

                // If _datalist is still null (due to internet is disconnected)
                if (DataCache == null) throw new Exception("Unable to load data from server.");
            }

            return DataCache;
        }

        ///<summary>Reload DataCache from database and return DataCache.</summary>
        public T FreshDataCache()
        {
            CleanCache();
            return GetDataCache(false);
        }

        public T GetCloneCache(bool LocalCacheOnly = false)
        {
            return ExtraTools.Clone<T>(GetDataCache(LocalCacheOnly));
        }

        ///<summary>Clean your exising cache in DataCache.</summary>
        public void CleanCache()
        {
            DataCache = default;
            IsCacheClean = true;
            IsCacheLoading = false;
        }

        ///<summary>Get current version of chache (DataCache).</summary>
        public int GetVersion()
        {
            return DataVersion;
        }

        ///<summary>Assign/ Increase version of chache (DataCache).</summary>
        public ResultInfo UpdateVersion(ResultInfo resultInfo)
        {
            if (!resultInfo.HasError) UpdateVersion();
            return resultInfo;
        }

        public void UpdateVersion()
        {
            DataVersion++;
            IsCacheClean = true;
        }

        ///<summary>Method needs implementaiton to load data from database in DataCache.</summary>
        protected abstract bool LoadRepository();

        ///<summary>Convert DataTable to List and save it to DataCache.</summary>
        protected bool TableToList<TModel>(ResultInfo resultInfo, Func<DataRow, TModel> DataRowToModelDelegate)
        {
            var data = new List<TModel>();

            if (!resultInfo.HasError)
            {
                foreach (DataRow dataRow in resultInfo.Table.Rows)
                    data.Add(DataRowToModelDelegate(dataRow));

                SetDataCache(ConvertToT(data));
                HasErrorInCache = false;
                return true;
            }

            SetErrorInCache();
            return false;
        }

        ///<summary>Just convert DataTable to List and not save it to DataCache</summary>
        protected List<TModel> TableToList<TModel>(DataTable dataTable, Func<DataRow, TModel> DataRowToModelDelegate)
        {
            if (dataTable == null) return null;
            else if (DataRowToModelDelegate == null) return null;

            var list = new List<TModel>();
            foreach (DataRow dataRow in dataTable.Rows)
                list.Add(DataRowToModelDelegate(dataRow));

            return list;
        }

        ///<summary>Type cast template class TModle to template class T.</summary>
        private T ConvertToT(object TModel)
        {
            return (T)TModel;
        }

        ///<summary>Retrun true is any error occurrs while loading the data from db</summary>
        public bool ErrorInCache()
        {
            return HasErrorInCache;
        }

        ///<summary>Retrun true is any error occurrs while loading the data from db</summary>
        public void SetErrorInCache()
        {
            HasErrorInCache = true;
            CleanCache();
        }
    }
}
