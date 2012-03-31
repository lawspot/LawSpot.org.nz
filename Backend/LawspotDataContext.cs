using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Lawspot.Backend
{
    /// <summary>
    /// Represents the data context for the Lawspot database.
    /// </summary>
    public class LawspotDataContext : DataClassesDataContext
    {
        private static bool migrationsExecuted;

        public LawspotDataContext()
            : base(GetConnectionString())
        {
            if (migrationsExecuted == true)
                return;

            // Get the last migration that was executed.
            var version = 0;
            if (this.Migrations.Any())
                version = this.Migrations.Max(m => m.Version);

            // Get any migrations that are higher than that version.
            while (true)
            {
                version++;
                var path = HttpContext.Current.Server.MapPath(string.Format("/Backend/Migrations/{0}.sql", version));
                if (File.Exists(path) == false)
                    break;
                var sql = File.ReadAllText(path);

                // Execute the migration in a transaction.
                using (var scope = new System.Transactions.TransactionScope())
                {
                    // Execute the migration.
                    this.ExecuteCommand(sql);

                    // Add a new migration row.
                    var migration = new Migration();
                    migration.Version = version;
                    migration.RunAt = DateTime.Now;
                    this.Migrations.InsertOnSubmit(migration);
                    this.SubmitChanges();

                    scope.Complete();
                }
            }

            // Don't try to execute any more migrations.
            migrationsExecuted = true;
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