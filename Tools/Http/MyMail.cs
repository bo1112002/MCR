using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.IO;


namespace Tools.Http
{
    /// <summary>
    /// 发送邮件的服务器设置接口
    /// </summary>
    public interface IMailServer
    {
        string UserName { get; }
        string Password { get; }
        string Server { get; }
    }

    public class MyMail
    {

        private MyMail() { }

        /// <summary> 
        /// 发送邮件程序 
        /// </summary> 
        /// <param name="from">发送人邮件地址</param> 
        /// <param name="fromname">发送人显示名称</param> 
        /// <param name="to">发送给谁(邮件地址)</param> 
        /// <param name="subject">标题</param> 
        /// <param name="body">内容</param> 
        /// <param name="username">邮件登录名</param> 
        /// <param name="password">邮件密码</param> 
        /// <param name="server">邮件服务器</param> 
        /// <param name="fujian">附件</param> 
        /// <returns>send ok</returns> 
        /// 调用方法 SendMail("abc@126.com", "某某人", "cba@126.com", "你好", "我测试下邮件", "邮箱登录名", "邮箱密码", "smtp.126.com", ""); 
        public static Result Send(string from, string fromname, string to, string subject, string body,   string[] annexFileNames , IMailServer mailSer)
        {
            try
            {
                //邮件发送类 
                MailMessage mail = new MailMessage();
                //是谁发送的邮件 
                mail.From = new MailAddress(from, fromname);
                //发送给谁 
                mail.To.Add(to);
                //标题 
                mail.Subject = subject;
                //内容编码 
                mail.BodyEncoding = Encoding.Default;
                //发送优先级 
                mail.Priority = MailPriority.High;
                //邮件内容 
                mail.Body = body;
                //是否HTML形式发送 
                mail.IsBodyHtml = true;
                //附件 
                if (annexFileNames != null && annexFileNames.Length > 0)
                {
                    foreach (string s in annexFileNames)
                    {
                        if (File.Exists(s))
                        {
                            mail.Attachments.Add(new Attachment(s));
                        }
                    }
                    
                }
                //邮件服务器和端口 
                SmtpClient smtp = new SmtpClient(mailSer.Server, 25);
                smtp.UseDefaultCredentials = true;
                //指定发送方式 
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //指定登录名和密码 
                smtp.Credentials = new System.Net.NetworkCredential(mailSer.UserName, mailSer.Password );
                //超时时间 
                smtp.Timeout = 10000 *  (annexFileNames == null ? 1 : annexFileNames.Length ) + 10000;
                smtp.Send(mail);
                return Result.OK;
            }
            catch (Exception err)
            {
                return new Result(false, err.Message);
            }

        }
    }

}
