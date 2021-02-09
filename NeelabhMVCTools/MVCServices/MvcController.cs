using Microsoft.AspNetCore.Mvc;

namespace NeelabhMVCTools.MVCServices
{
    public abstract class MvcController : Controller
    {
        private bool IsHeaderErrorAdded { get; set; } = false;
        protected bool MVC_HasModelError { get; private set; } = false;
        protected static string MSG_RecordMoved { get; } = "The record is either not selected or moved away.";

        protected void ResponseHeader(string key, string value)
        {
            Response.Headers.Add(key, value);
        }

        public JsonResult JsonError(string Message = "", bool IsFormError = false)
        {
            ResponseHeader("IsJSON", true.ToString());
            ResponseHeader("HasError", true.ToString());
            if (IsFormError) ResponseHeader("IsFormError", IsFormError.ToString());

            return Json(new
            {
                status = false,
                message = Message == "" ? "Oops! Something went wrong." : Message
            });
        }

        public JsonResult JsonSuccess(string message = "process completed")
        {
            ResponseHeader("IsJSON", true.ToString());

            return Json(new
            {
                status = true,
                message
            });
        }

        public JsonResult JsonSuccess(JsonResult jsonResult, string message = "process completed")
        {
            ResponseHeader("IsJSON", true.ToString());

            return Json(new
            {
                status = true,
                message,
                data = jsonResult
            });
        }

        public JsonResult JsonSuccess(JsonResult jsonResult)
        {
            ResponseHeader("IsJSON", true.ToString());
            return jsonResult;
        }

        public void HeaderError(bool IsModelError = false, bool IsJson = false)
        {
            if (IsHeaderErrorAdded) return;

            ResponseHeader("HasError", true.ToString());

            if (IsModelError)
            {
                ResponseHeader("IsFormError", IsModelError.ToString());
            }

            ResponseHeader("IsJSON", IsJson.ToString());

            IsHeaderErrorAdded = true;
        }

        public void ModelError(bool IsJson = false)
        {
            if (IsHeaderErrorAdded) return;

            ResponseHeader("HasError", true.ToString());
            ResponseHeader("IsFormError", true.ToString());
            ResponseHeader("IsJSON", IsJson.ToString());

            MVC_HasModelError = true;
            IsHeaderErrorAdded = true;
        }

        public void ModelError(string key, string errorMessage)
        {
            ModelError();
            ModelState.AddModelError(key, errorMessage);
        }
    }
}
