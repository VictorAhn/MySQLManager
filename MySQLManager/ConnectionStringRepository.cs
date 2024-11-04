using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQLManager
{
    public enum ConnectionStringEnum
    {
        Default
    }
    public class ConnectionStringRepository
    {
        public static readonly Dictionary<ConnectionStringEnum, string> MySqlStorage = new()
        {
            { ConnectionStringEnum.Default, $"Server=192.168.0.8;Port=3306;Database=User;Uid=admin;Pwd=passwd;Allow User Variables=True" }
        };
    }
}
