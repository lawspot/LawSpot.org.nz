using System.Configuration;

namespace Lawspot.Backend
{
    /// <summary>
    /// Represents the data context for the Lawspot database.
    /// </summary>
    public class LawspotDataContext : DataClassesDataContext
    {
        public LawspotDataContext()
            : base(GetConnectionString())
        {
        }

        private static string GetConnectionString()
        {
            // Check for AppHarbor-inserted connection string.
            if (ConfigurationManager.AppSettings["SQLSERVER_CONNECTION_STRING"] != null)
            {
                return ConfigurationManager.AppSettings["SQLSERVER_CONNECTION_STRING"];
            }
            return ConfigurationManager.ConnectionStrings["DataConnectionString"].ConnectionString;
        }
    }
}