// Services/RemoteDataService.cs
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TrackingSheet.Models.VSATdata;

namespace TrackingSheet.Services
{
    public class RemoteDataService
    {
        private readonly string _connectionString;

        public RemoteDataService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("RemoteDatabase");
        }

        public async Task<List<VsatInfo>> GetVsatInfoAsync()
        {
            var vsatInfoList = new List<VsatInfo>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT t1.VsatField, t2.VsatWell, t3.VsatRun
                    FROM Tab1 t1
                    JOIN Tab2 t2 ON t1.ID = t2.ID
                    JOIN Tab3 t3 ON t1.ID = t3.ID
                    WHERE <условия фильтрации>;
                "; // Вставить когда узнаю что и где

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var vsatInfo = new VsatInfo
                            {
                                VsatField = reader.GetString(0),
                                VsatWell = reader.GetString(1),
                                VsatRun = reader.GetInt32(2),
                                VsatDC = reader.GetString(3),
                                VsatDate = reader.GetFieldValue<DateOnly>(4),
                                VsatBit = reader.GetInt64(5),
                                VsatNumber = reader.GetString(6),
                                VsatTool = reader.GetString(7),
                                VsatToolSN = reader.GetInt32(8),
                                VsatOD = reader.GetInt64(9),
                                VsatOffset = reader.GetInt64(10)
                            };

                            vsatInfoList.Add(vsatInfo);
                        }
                    }
                }
            }

            return vsatInfoList;
        }
    }
}
