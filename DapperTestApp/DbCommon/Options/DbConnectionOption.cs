using System;

namespace DapperTestApp.DbCommon.Options
{
    public class DbConnectionOption
    {
        public const string DbConnectionStrings = nameof(DbConnectionStrings);

        public string RetailDb { get; set; }

        public void Validate()
        {
            foreach (var propertyInfo in this.GetType().GetProperties())
            {
                var value = propertyInfo.GetValue(obj: this, null);
                if (value is string && string.IsNullOrEmpty(value.ToString()))
                {
                    throw new ArgumentException($"{propertyInfo.Name} is not configured");
                }
            }
        }
    }
}
