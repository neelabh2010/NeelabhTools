using NeelabhMVCTools.Utilities;

namespace NeelabhMVCTools.MVCServices
{
    public abstract class MvcController<TViewModel> : MvcController
    {
        protected abstract TViewModel PrepareModelList(TViewModel viewModel);

        protected abstract ResultInfo DecryptModelData(TViewModel viewModel);

        // Validate Result Methods --
        protected ResultInfo ValidateResult(ResultInfo resultInfo,
            TViewModel viewModel = default, bool IsPrepareList = false)
        {
            ModelErrorItems modelErrors = null;
            if (viewModel != null)
            {
                try
                {
                    var mei = (IModelErrorIntegration)viewModel;
                    modelErrors = mei.ModelErrors();
                }
                catch
                {
                    modelErrors = null;
                }
            }

            if (resultInfo.HasError)
            {
                // Error --
                if (modelErrors != null && modelErrors.Errors != null)
                {
                    foreach (var item in modelErrors.Errors)
                    {
                        if (item.ErrorKey == resultInfo.Message)
                        {
                            ModelError(item.Name, item.Message);
                        }
                    }
                }

                if (MVC_HasModelError)
                {
                    // model error--
                    resultInfo.ErrorType = DbExceptions.ModelException;
                }
                else
                {
                    // unknown error--
                    resultInfo.ErrorType = DbExceptions.UnknownException;
                }
            }
            else
            {
                // Success --
                if (IsPrepareList && viewModel != null) PrepareModelList(viewModel);
            }

            return resultInfo;
        }
    }
}
