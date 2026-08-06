using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools
{
    /// <summary>版本信息类</summary>
    public class AppVersion
    {
        string _Name = string.Empty;
        /// <summary>软件名</summary>
        public string Name
        {
            get { return _Name; }
            private set { _Name = value; }
        }
        string _Version = "1.0";
        /// <summary>版本号</summary>
        public string Version
        {
            get { return _Version; }
            private set { _Version = value; }
        }
        string _Path = string.Empty;
        /// <summary>软件的全路径</summary>
        public string Path
        {
            get { return _Path; }
            set { _Path = value; }
        }


        public AppVersion(string name, string version, string path)
        {
            this.Name = name;
            this.Version = version;
            this.Path = path;
        }

    }
}
