using System.Collections.Generic;
using System.Data;

public class ResultInfo
{
    // Result Summary --
    public string Title { get; set; } = "Success";
    public string Message { get; set; } = "";

    // Result Data --
    public string Result { get; set; } = "";
    public string AdditionalInfo { get; set; } = "";
    public List<string> AdditionalItems { get; set; }
    public DataTable Table { get; set; }
    public object Object { get; set; }
    public dynamic Data { get; set; }

    // Error Related --
    public bool HasError { get; set; }  = false;
    public int ErrorNo { get; set; } = 0;
    public bool IsDBError { get; set; } = false;
    public string SystemMessage { get; set; } = "";
    public DbExceptions ErrorType { get; set; } = DbExceptions.None;

    /// <summary>
    /// Set error message to the ResultInfo object, and HasError = true through it
    /// </summary>
    public string ErrorMessage
    {
        set
        {
            Message = value;
            HasError = true;
            Title = "Error";
            this.ErrorNo = 0;
        }
    }

    public ResultInfo SetSuccess(string message)
    {
        HasError = false;
        Title = "Success";
        SystemMessage = Message;
        this.Message = message;
        ErrorNo = 0;

        return this;
    }

    public ResultInfo SetError(string message, int errorNo = 0, bool handyError = false)
    {
        HasError = true;
        Title = "Error";
        SystemMessage = message;
        this.Message = handyError ? HandyErrorMsg(errorNo, message) : message;
        this.ErrorNo = errorNo;

        return this;
    }

    public string GetSystemMessage()
    {
        return SystemMessage;
    }

    private string HandyErrorMsg(int errorNo = 0, string systemMessage = "")
    {
        if (errorNo != 0) IsDBError = true;

        if (errorNo == 2601 || errorNo == 2627)  // Unique Key Error
        {
            ErrorType = DbExceptions.DuplicateRecordException;
            return "You are entering some duplicate entry.";
        }
        else if (errorNo == 547)  // Foreign Key Error
        {
            ErrorType = DbExceptions.ForeignKeyException;
            return "Record is referred in some other file(s). You can't delete the record.";
        }
        else if (errorNo == 8152) // Length is exceeding
        {
            ErrorType = DbExceptions.FiledLenghtExceedingException;
            return "Some entries lengths are exceeding than the defined size Please review your entries and try again.";
        }
        else if (errorNo == 8114) // Data Type mismatched
        {
            ErrorType = DbExceptions.TypeMismatchedException;
            return "Provided data is not compatible as per our database. Please review your entries and try again.";
        }
        else if (errorNo == 15530) // Database connectivity
        {
            ErrorType = DbExceptions.DBConnectionException;
            return "We are not able to connect the database this time. Please try again later.";
        }
        else if (errorNo == 1205) // Deadlock
        {
            ErrorType = DbExceptions.DeadlockException;
            return "Deadlock position occurred. Please try again later.";
        }
        else
        {
            ErrorType = DbExceptions.UnknownException;

            if (systemMessage.ToUpper().StartsWith("[DBMSG]"))
                return systemMessage.TrimStart("[DBMSG]");
            else
                return "An error occurred during the execution.";
        }
    }

}
