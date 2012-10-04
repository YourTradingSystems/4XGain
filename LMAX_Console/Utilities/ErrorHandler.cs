using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using DataTypess;
using Database;
using Sender;

namespace Utilites
{
    public class ErrorHandler
    {
        private String eLogin;
        private String ePassword;
        private MailAddress fromAddress;
        private const string fromPassword = "YTS_admin password";
        private const string subject = "Account notify";
        private SmtpClient smtp;
        private Object sendMessLock = new Object();

        public ErrorHandler(String emailLogin, String emailPasswd)
        {
            fromAddress = new MailAddress("ytsnotify@gmail.com");

            smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
        }

        public void sendMessage(String messReciver, Order order, int messType)
        {
            lock (sendMessLock)
            {
                MailAddress toAddress = new MailAddress(messReciver);
                MailMessage message = new MailMessage(fromAddress, toAddress);
                message.Subject = subject;
                // Create message body
                switch (messType)
                {
                    case Presets.ORDER_NOT_FILLED:
                    case Presets.ORDER_REJECTED:
                        message.Body = "An order not filled : " + (order==null?"<no order data becouse of internal server error>":order.ToString()) + "\n";
                        message.Body += "Reason : " + (order==null?"<no reason description becouse of system error>":order.Comment) + "\n";
                        message.Body += "It means that there are some problems with your account, like not enought money. ";
                        message.Body += "Please, try to solve the problem. For more detail please go to our website or use www.googl.com";
                        break;
                }
                try
                {
                    smtp.Send(message);
                }
                catch (Exception e)
                {
                    Program.WriteError("Exception in Email sender module : ");
                    Program.WriteError(e.Message);
                }
            }
        }


    }
}
