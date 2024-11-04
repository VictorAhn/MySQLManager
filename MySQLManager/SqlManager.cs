using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MySQLManager
{
    public class SqlManager
    {
        public static string RootPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        private readonly string filePath;
        private readonly Dictionary<string, string> sqlDictionary = new ();
        private DateTime fileWriteTime;

        public SqlManager(string xmlPath)
        {
            filePath = Path.Combine(RootPath, "Sql", xmlPath);
            LoadSql();
        }

        public void LoadSql()
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"SQL XML 파일을 찾을 수 없습니다: {filePath}");

            fileWriteTime = File.GetLastWriteTime(filePath);
            var xmlContent = File.ReadAllText(filePath);

            var xml = new XmlDocument();
            xml.LoadXml(xmlContent);

            sqlDictionary.Clear();
            var mapperNode = xml["mapper"];
            if (mapperNode == null) throw new InvalidOperationException("XML 파일에 'mapper' 루트 노드가 없습니다.");

            foreach (XmlNode node in mapperNode.ChildNodes)
            {
                var idAttribute = node.Attributes?["id"];
                if (idAttribute == null) continue;

                sqlDictionary[idAttribute.Value.ToLower()] = node.InnerText.Trim();
            }
        }
        public string GetSql(string sqlId)
        {
            var normalizedSqlId = sqlId.ToLower();
            if (!sqlDictionary.ContainsKey(normalizedSqlId)) throw new KeyNotFoundException($"XML에 요청한 Sql Id가 존재하지 않습니다: {normalizedSqlId}");

            if (fileWriteTime < File.GetLastWriteTime(filePath)) LoadSql();

            return sqlDictionary[normalizedSqlId];
        }
    }
}
