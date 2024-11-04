using MySQLManager;

namespace TestProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            bool insert = await Insert("UID", "UNM");
            string userName = await Select("UID");
            bool update = await Update("UID", "UNM");
        }
        public static async Task<bool> Insert(string userId, string userName)
        {
            try
            {
                string xmlPath = Path.Combine("Test.xml");

                var query = await MySqlDapperManager.GetQueryFirstFromXmlAsync<bool>(ConnectionStringEnum.Default, xmlPath, "Insert", new { userId = userId, userName = userName });
                return true;
            }
            catch (Exception exception) { }
            return false;
        }
        public static async Task<string> Select(string userId)
        {
            try
            {
                string xmlPath = Path.Combine("Test.xml");

                return await MySqlDapperManager.GetQueryFirstFromXmlAsync<string>(ConnectionStringEnum.Default, xmlPath, "Select", new { userId = userId });
            }
            catch (Exception exception) { }
            return null;
        }
        public static async Task<bool> Update(string userId, string userName)
        {
            try
            {
                string xmlPath = Path.Combine("Test.xml");

                var query = await MySqlDapperManager.GetQueryFirstFromXmlAsync<bool>(ConnectionStringEnum.Default, xmlPath, "Update", new { userId = userId, userName = userName });
                return true;
            }
            catch (Exception exception) { }
            return false;
        }
    }
}