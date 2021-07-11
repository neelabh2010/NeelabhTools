using System.Collections.Generic;

namespace NeelabhMVCTools.Utilities
{
    public enum DataReloadType
    {
        None, Reload, HardReload
    }

    public class ItemGroup
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class ModelError
    {
        public string Name { get; set; }
        public string ErrorKey { get; set; }
        public string Message { get; set; }
    }


    public class ModelErrorItems
    {
        public List<ModelError> Errors = null;
    }

    public interface IModelErrorIntegration
    {
        public ModelErrorItems ModelErrors();
    }

    public class VMBase
    {
        public bool IsPostBack { get; set; }

        public string EncId { get; set; }

        public string UserKey { get; set; }

        public string ModelErrorMsg { get; set; }
    }

}
