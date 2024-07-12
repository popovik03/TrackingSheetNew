// Services/RemoteDataService.cs
using System.Collections.Generic;
using System.Data.SqlClient;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NuGet.Packaging.Signing;
using TrackingSheet.Models.VSATdata;


namespace TrackingSheet.Services
{
    public class RemoteDataService
    {
        //Подключение к базе данных 
        private readonly string _connectionString;

        public RemoteDataService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("RemoteDatabase");
        }

        //Получение информации из базы данных 
        public async Task<VsatInfo> GetLatestVsatInfoAsync()
        {
            VsatInfo vsatInfo = new VsatInfo();
            using (var connection = new SqlConnection(_connectionString))

            {
                await connection.OpenAsync(); //открытие базы данных
                //Получение имени скважины
                string queryWELL_NAME = "SELECT TOP 1 WELL_NAME FROM WELL_TAB ORDER BY WELL_UPDATE_DATE DESC";
                using (var command = new SqlCommand(queryWELL_NAME, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            vsatInfo.WELL_NAME = reader["WELL_NAME"].ToString();
                        }
                    }
                }
                //получение названия месторождения 
                string queryOOIN_NAME = "SELECT TOP 1 OOIN_NAME FROM OBJECT_OF_INTEREST_TAB ORDER BY OOIN_UPDATE_DATE DESC";
                using (var command = new SqlCommand(queryOOIN_NAME, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            vsatInfo.OOIN_NAME = reader["OOIN_NAME"].ToString();
                        }
                    }
                }

                //получение номера куста 
                string queryFCTY_NAME = "SELECT TOP 1 FCTY_NAME FROM FACILITY_TAB ORDER BY FCTY_UPDATE_DATE DESC";
                using (var command = new SqlCommand(queryFCTY_NAME, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            vsatInfo.FCTY_NAME = reader["FCTY_NAME"].ToString();
                        }
                    }
                }

                //получение ID заказчика 
                string queryCPNM_IDENTIFIER = "SELECT TOP 1 CPNM_IDENTIFIER FROM FACILITY_TAB ORDER BY FCTY_UPDATE_DATE DESC";
                using (var command = new SqlCommand(queryCPNM_IDENTIFIER, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            vsatInfo.CPNM_IDENTIFIER = reader["CPNM_IDENTIFIER"].ToString();
                        }
                    }
                }

                //получение наименование заказчика
                string queryCPNM_NAME = "SELECT TOP 1 CPNM_NAME FROM COMPANY_NAME ORDER BY CPNM_UPDATE_NAME DESC";
                using (var command = new SqlCommand(queryCPNM_NAME, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            vsatInfo.FCTY_NAME = reader["CPNM_NAME"].ToString();
                        }
                    }
                }

                //получение номера последнего рейса
                string queryMWRU_NUMBER = "SELECT TOP 1 MWRU_NUMBER FROM MWD_RUN ORDER BY MWRU_NUMBER DESC";
                using (var command = new SqlCommand(queryMWRU_NUMBER, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            vsatInfo.MWRU_NUMBER = Convert.ToInt32(reader["MWRU_NUMBER"]);
                        }
                    }
                }

                //получение диаметра по рейсу
                string queryMWRU_HOLE_DIAMETER = "SELECT TOP 1 MWRU_HOLE_DIAMETER FROM MWD_RUN ORDER BY MWRU_NUMBER DESC";
                using (var command = new SqlCommand(queryMWRU_HOLE_DIAMETER, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            vsatInfo.MWRU_HOLE_DIAMETER = Convert.ToSingle(reader["MWRU_HOLE_DIAMETER"])*1000;
                        }
                    }
                }

                //ID рейса
                string queryMWRU_IDENTIFIER = "SELECT TOP 1 MWRU_IDENTIFIER FROM MWD_RUN ORDER BY MWRU_NUMBER DESC";
                using (var command = new SqlCommand(queryMWRU_IDENTIFIER, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            vsatInfo.MWRU_IDENTIFIER = reader["MWRU_HOLE_DIAMETER"].ToString();
                        }
                    }
                }

                //
                string queryMWCO_IDENTIFIER = @"SELECT MWCO_IDENTIFIER FROM MWD_RUN_TO_COMPONENT WHERE MWRU_IDENTIFIER = @MWRU_IDENTIFIER";
                using (var command = new SqlCommand(queryMWCO_IDENTIFIER, connection)) //@MWRU_IDENTIFIER это переменная которая была определена выше
                {
                    command.Parameters.AddWithValue("@MWRU_IDENTIFIER", vsatInfo.MWRU_IDENTIFIER); //отправляем запрос в базу данных заменяем параметр на реальный из код с помощью .Add

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync()) //каждый цикл ReadAsync возвращает true и сдвигает указатель на следующую строку результат запроса
                        {
                            string mwcoIdentifier = reader["MWCO_IDENTIFIER"].ToString();
                            vsatInfo.MWCO_IDENTIFIER.Add(mwcoIdentifier);
                        }
                    }
                }
            }
            return vsatInfo;
        }

       
        

    }
}
