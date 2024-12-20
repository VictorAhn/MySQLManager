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
            { 
                ConnectionStringEnum.Default, 
                $"Server={Environment.GetEnvironmentVariable("Service_DB_IP")};" +
                $"Port={Environment.GetEnvironmentVariable("Service_DB_Port")};" +
                $"Database={Environment.GetEnvironmentVariable("Service_DB_Name")};" +
                $"Uid={Environment.GetEnvironmentVariable("Service_DB_Uid")};" +
                $"Pwd={Environment.GetEnvironmentVariable("Service_DB_Pwd")};" +
                $"Allow User Variables=True" }
        };
    }
}
