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
        /// Read a setting value from the Settings table.
        /// </summary>
        /// <typeparam name="T"> The type of value to return. </typeparam>
        /// <param name="key"> The setting name. </param>
        /// <param name="defaultValue"> The value that is returned if the setting doesn't exist. </param>
        /// <returns> The setting value. </returns>
        public T ReadSetting<T>(string key, T defaultValue = default(T))
        {
            var setting = this.Settings.SingleOrDefault(s => s.Key == key);
            if (setting == null)
                return defaultValue;
            return (T)Convert.ChangeType(setting.Value, typeof(T));
        }

        /// <summary>
        /// Write a setting value to the Settings table.
        /// </summary>
        /// <typeparam name="T"> The type of value to set. </typeparam>
        /// <param name="key"> The setting name. </param>
        /// <param name="value"> The new value of the setting. </param>
        public void WriteSetting<T>(string key, T value)
        {
            var setting = this.Settings.SingleOrDefault(s => s.Key == key);
            if (setting == null)
            {
                setting = new Setting();
                setting.Key = key;
                setting.Value = (string)Convert.ChangeType(value, typeof(string));
                this.Settings.InsertOnSubmit(setting);
            }
            else
            {
                setting.Value = (string)Convert.ChangeType(value, typeof(string));
            }
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