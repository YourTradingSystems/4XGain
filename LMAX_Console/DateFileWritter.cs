using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sender;

namespace Database
{
    public class DateFileWritter
    {
        private FileStream timeFile;
        private String FileLocation = "time.txt";
        private ASCIIEncoding strCoder;

        public DateFileWritter()
        {
            FileMode fMode = File.Exists(FileLocation)?FileMode.Open:FileMode.Create;
            try
            {
                timeFile = new FileStream(FileLocation, fMode);
                strCoder = new ASCIIEncoding();
            }
            catch (Exception e)
            {
                timeFile = null;
                Program.log.Error("Cannot create file stream");
                Program.log.Debug(e.Message);
                Program.log.Debug(e.StackTrace);
            }
        }

        public DateTime ReadTime()
        {
            DateTime result = new DateTime(1979, 01, 01, 00, 00, 00);
            if (timeFile == null) return result;

            byte[] readedData = new byte[23];
            try
            {
                timeFile.Seek(0,SeekOrigin.Begin);
                if (timeFile.Read(readedData, 0, 23) != 0)
                {
                    string readedTime = strCoder.GetString(readedData);
                    result = DateTime.Parse(readedTime);
                }
            }
            catch(Exception e)
            {
                Program.log.Error("Error trying to red file");
                Program.log.Debug(e.Message);
                Program.log.Debug(e.StackTrace);
            }
            return result;
        }

        public void WriteTime(DateTime time)
        {
            if (timeFile == null) return;
            try
            {
                timeFile.Seek(0, SeekOrigin.Begin);
                byte[] writedData = strCoder.GetBytes(time.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                timeFile.Write(writedData, 0, writedData.Length);
                timeFile.Flush();
            }
            catch (Exception e)
            {
                Program.log.Error("Error trying to convert time and write it to file");
                Program.log.Debug(e.Message);
                Program.log.Debug(e.StackTrace);
            }
        }

        ~DateFileWritter()
        {
            if (timeFile != null)
            {
                timeFile.Close();
            }
        }
    }
}
