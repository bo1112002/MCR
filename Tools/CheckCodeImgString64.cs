using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Tools
{
    /// <summary>
    ///校验码功能类
    /// </summary>
    public class CheckCodeImgString64
    {
        private CheckCodeImgString64()
        {
        }

        //public readonly static string SessionName = "009A94CD-4C9C-4c84-A2CA-82B186B13F27";
        //static readonly char[] CODES = "123456789#@ABCDEFGHIJKLMNOPQRSTUVWXYZ+-?*".ToCharArray();
        /// <summary>
        /// 生成随机校验码字符串
        /// </summary>
        /// <returns>生成的随机校验码字符串</returns>
        public static string GenerateCheckCode(char[] CODES  , int num = 4)
        {
            //char[] CODES = AppSettings.CheckCodes;
            int number;
            string strCode = string.Empty;

            //随机数种子
            Random random = new Random();

            for (int i = 0; i < num; i++) //校验码长度为
            {
                //随机的整数
                number = random.Next(CODES.Length);
                char c = CODES[number];
                strCode += c.ToString();
            }
            return strCode;
        }



        /// <summary>根据校验码输出图片</summary>
        /// <param name="checkCode">产生的随机校验码</param>
        public static byte[] CreateCheckCodeImage(string checkCode)
        {
            //根据校验码的长度确定输出图片的长度
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(75, 25);//(int)Math.Ceiling(Convert.ToDouble(checkCode.Length * 15))
            //创建Graphics对象
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成随机数种子
                Random random = new Random();
                //清空图片背景色
                g.Clear(Color.White);
                //画图片的背景噪音线 10条
                //---------------------------------------------------
                for (int i = 0; i < 10; i++)
                {
                    //噪音线起点坐标(x1,y1),终点坐标(x2,y2)
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);

                    //用银色画出噪音线
                    g.DrawLine(new Pen(Color.Silver, 3f), x1, y1, x2, y2);
                }
                //---------------------------------------------------
                //Brush b = Brushes.Silver;
                //g.FillRectangle(b, 0, 0, image.Width, image.Height);
                //---------------------以上两种任选其一------------------------------
                //输出图片中校验码的字体: 12号Arial,粗斜体
                Font font = new Font("Arial", 16, (FontStyle.Bold | FontStyle.Italic));

                //线性渐变画刷
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.Purple, 1.2f, true);
                g.DrawString(checkCode, font, brush, 2, 2);

                //画图片的前景噪音点 50个
                for (int i = 0; i < 300; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    if (i % 2 == 0)
                    {
                        image.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        image.SetPixel(x, y, Color.Black);
                    }

                }

                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Peru), 0, 0, image.Width - 1, image.Height - 1);

                //创建内存流用于输出图片
                using (MemoryStream ms = new MemoryStream())
                {
                    //图片格式指定为png
                    image.Save(ms, ImageFormat.Jpeg);

                    return ms.ToArray();
                }
            }
            finally
            {
                //释放Bitmap对象和Graphics对象
                g.Dispose();
                image.Dispose();
            }
        }
    }
}
