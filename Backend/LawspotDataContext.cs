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
        public LawspotDataContext()
            : base(ConfigurationManager.ConnectionStrings["LawspotConnectionString"].ConnectionString)
        {
        }

        /// <summary>
        /// Execute database migrations, in order.
        /// </summary>
        public static void ExecuteMigrations()
        {
            using (var dataContext = new LawspotDataContext())
            {
                // Get the last migration that was executed.
                var version = 0;
                if (dataContext.Migrations.Any())
                    version = dataContext.Migrations.Max(m => m.Version);

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
                        // Split the batch file on "GO".
                        var splits = System.Text.RegularExpressions.Regex.Split(sql, @"^\s*go\s*[;]?\s*$",
                            System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        foreach (var splitSql in splits)
                        {
                            // Execute the migration.
                            dataContext.ExecuteCommand(splitSql);
                        }

                        // Add a new migration row.
                        var migration = new Migration();
                        migration.Version = version;
                        migration.RunAt = DateTime.Now;
                        dataContext.Migrations.InsertOnSubmit(migration);
                        dataContext.SubmitChanges();

                        scope.Complete();
                    }
                }
            }
        }
    }
}