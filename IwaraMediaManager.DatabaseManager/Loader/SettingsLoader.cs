using IwaraMediaManager.DatabaseManager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IwaraMediaManager.Models;

namespace IwaraMediaManager.DatabaseManager.Loader
{
    public class SettingsLoader
    {
        public async Task<Setting> GetSettingAsync(string key)
        {
            Setting setting;
            using (var dbContext = new DataBaseContext())
            {
                setting = await dbContext.Settings.FindAsync(key);
            }

            return setting;
        }

        public async Task SetSetting(Setting setting)
        {
            using (var dbContext = new DataBaseContext())
            {
                var existingSetting = await dbContext.Settings.FindAsync(setting.Key);
                
                if (existingSetting != null)
                    existingSetting.Value = setting.Value;
                else
                    await dbContext.Settings.AddAsync(setting);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
