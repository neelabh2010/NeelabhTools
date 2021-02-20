using NeelabhCoreTools.Sets;
using System;
using System.IO;
using System.Net;

namespace NeelabhCoreTools.FTPClient
{
    public class FTPTools
    {
        public static FtpWebRequest CreateFTPWebRequest(FTPSet ftpSet)
        {
            string dirPath = ftpSet.Host + ftpSet.Root.IsNotEmpty("/", "") + ftpSet.Root;
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(dirPath);
            ftpRequest.Proxy = null;
            ftpRequest.UsePassive = true;
            ftpRequest.UseBinary = true;
            ftpRequest.KeepAlive = false;
            return ftpRequest;
        }

        public static bool IsDirectoryExists(FTPSet ftpSet)
        {
            try
            {
                var ftpRequest = CreateFTPWebRequest(ftpSet);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                return ftpRequest.GetResponse() != null;
            }
            catch
            {
                return false;
            }
        }

        public static ResultInfo CreateDirectory(FTPSet ftpSet)
        {
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                var ftpRequest = CreateFTPWebRequest(ftpSet);
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

                var response = (FtpWebResponse)ftpRequest.GetResponse();
                resultInfo.Object = response.StatusCode;
                response.Close();

                return resultInfo.SetSuccess("Directory is created successfully");
            }
            catch(Exception ex)
            {
                return resultInfo.SetError(ex.Message);
            }
        }

        public static ResultInfo UploadFile(FTPSet ftpSet, string source)
        {
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                using (var client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(ftpSet.User, ftpSet.Password);
                    client.UploadFile(ftpSet.Root, WebRequestMethods.Ftp.UploadFile, source);
                }

                return resultInfo.SetSuccess("File uploaded successfully");
            }
            catch(Exception ex)
            {
                return resultInfo.SetError(ex.Message);
            }
        }

        public static ResultInfo DownloadFile(FTPSet ftpSet, string destination)
        {
            ResultInfo resultInfo = new ResultInfo();
            
            try
            {
                var ftpRequest = CreateFTPWebRequest(ftpSet);
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                resultInfo.AdditionalItems = new System.Collections.Generic.List<string>();

                using (var response = (FtpWebResponse)ftpRequest.GetResponse())
                using (var stream = response.GetResponseStream())
                
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        resultInfo.AdditionalItems.Add(reader.ReadLine());
                    }
                }

                return resultInfo.SetSuccess("File uploaded successfully");
            }
            catch (Exception ex)
            {
                return resultInfo.SetError(ex.Message);
            }
        }

        public static ResultInfo DeleteDirectory(FTPSet ftpSet)
        {
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                var ftpRequest = CreateFTPWebRequest(ftpSet);
                ftpRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
                ftpRequest.GetResponse().Close();
                return resultInfo.SetSuccess("Directory deleted successfully");
            }
            catch(Exception ex)
            {
                return resultInfo.SetError(ex.Message);
            }
        }

        public static ResultInfo DeleteFile(FTPSet ftpSet)
        {
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                var ftpRequest = CreateFTPWebRequest(ftpSet);
                ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                ftpRequest.GetResponse().Close();
                return resultInfo.SetSuccess("Directory deleted successfully");
            }
            catch (Exception ex)
            {
                return resultInfo.SetError(ex.Message);
            }
        }
    }
}
