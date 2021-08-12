using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CJG.Infrastructure.EF.Helpers
{
	/// <summary>
	/// <typeparamref name="DbMigrationExtensions"/> static class, provides extension methods for <typeparamref name="DbMigration"/> objects.
	/// </summary>
	public static class DbMigrationExtensions
    {
        /// <summary>
        /// Parse the SQL statement and split any GO statements.
        /// </summary>
        /// <param name="migration"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static string[] ParseSql(this DbMigration migration, string sql)
        {
            var parts = Regex.Split(sql, @"^go\n", RegexOptions.IgnoreCase);
            return parts
	            .Where(p => !string.IsNullOrEmpty(p.Trim()))
	            .ToArray();
        }

        /// <summary>
        /// Get all the '*.sql' files at the specified path location.
        /// Read them and parse them into separate SQL statements.
        /// Returns an array of <typeparamref name="KeyValuePair"/> objects, with the Key as the filename and the Value as the SQL.
        /// </summary>
        /// <param name="migration"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string>[] GetSeedScripts(this DbMigration migration, string path)
        {
            var scripts = new List<KeyValuePair<string, string>>();

            if (!Directory.Exists(path))
	            return scripts.ToArray();

            var seedFiles = Directory
	            .GetFiles(path, "*.sql")
	            .OrderBy(n => n);

            foreach (var filename in seedFiles)
            {
	            var sql = File.ReadAllText(filename).Trim();
	            if (string.IsNullOrEmpty(sql))
		            continue;

	            var parts = migration
		            .ParseSql(sql)
		            .Select(s => new KeyValuePair<string, string>(filename, s))
		            .ToArray();

	            scripts.AddRange(parts);
            }

            return scripts.ToArray();
        }
    }
}
