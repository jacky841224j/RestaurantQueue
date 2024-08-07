﻿using CommonLibrary.Dto;
using CommonLibrary.Enum;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using static CommonLibrary.Dto.DailyReserveDto;

namespace CommonLibrary.Service
{
    /// <summary>
    /// 資料庫服務
    /// </summary>
    public class DbService
    {
        private readonly SqliteConnection _con;
        private IConfiguration _configuration;
        private readonly ILogger<DbService> _log;


        public DbService(SqliteConnection con, ILogger<DbService> log)
        {
            _log = log;
            _con = con;
            InitDailyReserve();
            InitBlackList();
        }

        /// <summary>
        /// 建立DailyReserve資料表
        /// </summary>
        private void InitDailyReserve()
        {
            try
            {
                using (_con)
                {
                    Open();

                    // 建立 DailyReserve 資料表
                    string sql = @"CREATE TABLE IF NOT EXISTS DailyReserve (
                                        [ID] INTEGER PRIMARY KEY,
                                        [TicketTime] TEXT,
                                        [SeatTime] TEXT,
                                        [TakeWay] INT,
                                        [Phone] TEXT,
                                        [People] INT,
                                        [Order] INT,
                                        [TableSize] TEXT,
                                        [QueueNumber] INT,
                                        [Flag] int
                                        )";
                    using (var command = new SqliteCommand(sql, _con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                _con.Dispose();
            }
        }

        /// <summary>
        /// 建立Reserve資料表
        /// </summary>
        private void InitBlackList()
        {
            try
            {
                using (_con)
                {
                    Open();

                    // 建立 Customer 資料表
                    string sql = @"CREATE TABLE IF NOT EXISTS BlackList (
                                        [ID] INTEGER PRIMARY KEY,
                                        [Phone] TEXT,
                                        [Cancel] INT,
                                        [Block] INT
                                        )";
                    using (var command = new SqliteCommand(sql, _con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                _con.Dispose();
            }
        }

        /// <summary>建立資料庫連線</summary>
        private SqliteConnection Open()
        {
            try
            {
                if (_con.State == ConnectionState.Open) _con.Close();
                _con.Open();
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                _con.Dispose();
                throw;
            }

            return _con;
        }

        /// <summary>
        /// 新增預約紀錄
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> AddDailyReserve(AddDailyReserveDto dto)
        {
            int result = 0;

            try
            {
                using (_con)
                {
                    Open();
                    string sql = @"INSERT INTO DailyReserve ([TicketTime],[SeatTime], [TakeWay], [Phone], [People], [QueueNumber], [Order], [TableSize], [Flag])" +
                                 @"VALUES (@TicketTime,@SeatTime, @TakeWay, @Phone, @People, @QueueNumber, @Order, @TableSize, @Flag)";

                    using (var command = new SqliteCommand(sql, _con))
                    {
                        // 設定參數值
                        command.Parameters.AddWithValue("@TicketTime", dto.ticketTime);
                        command.Parameters.AddWithValue("@SeatTime", dto.seatTime ?? string.Empty);
                        command.Parameters.AddWithValue("@TakeWay", (int)dto.takeWay);
                        command.Parameters.AddWithValue("@Phone", dto.phone);
                        command.Parameters.AddWithValue("@People", dto.people);
                        command.Parameters.AddWithValue("@QueueNumber", dto.queueNumber);
                        command.Parameters.AddWithValue("@Order", dto.order);
                        command.Parameters.AddWithValue("@TableSize", dto.tableSize);
                        command.Parameters.AddWithValue("@Flag", (int)dto.flag);
                        result = await command.ExecuteNonQueryAsync();

                        if (result > 0)
                        {
                            _log.LogInformation("Executing SQL: {updateSql} with parameters: TicketTime={TicketTime}, SeatTime={SeatTime}, TakeWay={TakeWay}, Phone={Phone}, People={People}, QueueNumber={QueueNumber}, Order={Order}, TableSize={TableSize} ,Flag={Flag} ",
                                                sql.Trim(), dto.ticketTime, dto.seatTime, dto.takeWay, dto.phone, dto.people, dto.queueNumber, dto.order, dto.tableSize, dto.flag);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                _con.Dispose();
                throw;
            }

            return result == 0 ? false : true;
        }

        /// <summary>
        /// 新增取消紀錄
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task AddCancelReserve(AddCancelReserveDto dto)
        {
            try
            {
                using (_con)
                {
                    Open();

                    //搜尋失約紀錄
                    string selectSql = @"SELECT [Cancel] From BlackList WHERE [Phone] = @Phone";
                    using (var command = new SqliteCommand(selectSql, _con))
                    {
                        // 設定參數值
                        command.Parameters.AddWithValue("@Phone", dto.phone);

                        var cancel = await command.ExecuteScalarAsync();

                        //若沒紀錄則新增
                        if (cancel == null)
                        {
                            string insertSql = @"INSERT INTO BlackList ([Phone],[Cancel],[Block]) VALUES (@Phone,@Cancel,@Block)";
                            using (var com = new SqliteCommand(insertSql, _con))
                            {
                                // 設定參數值
                                com.Parameters.AddWithValue("@Phone", dto.phone);
                                com.Parameters.AddWithValue("@Cancel", 0);
                                com.Parameters.AddWithValue("@Block", 0);
                                var result = await com.ExecuteNonQueryAsync();

                                if (result > 0)
                                {
                                    _log.LogInformation("Executing SQL: {updateSql} with parameters: Phone={phone}, Cancel= 0, Block= 0", insertSql, dto.phone);
                                }
                            }
                        }
                        else
                        {
                            string updateSql = @"UPDATE BlackList SET [Cancel] = @Cancel , [Block] = @Block
                                                 WHERE [Phone] = @Phone ";
                            using (var com = new SqliteCommand(updateSql, _con))
                            {
                                // 設定參數值
                                com.Parameters.AddWithValue("@Phone", dto.phone);
                                com.Parameters.AddWithValue("@Cancel", (long)cancel + 1);
                                com.Parameters.AddWithValue("@Block", (long)cancel + 1 == 3 ? 1 : 0);
                                var result = await com.ExecuteNonQueryAsync();

                                if (result > 0)
                                {
                                    _log.LogInformation("Executing SQL: {updateSql} with parameters: Phone={phone}, Cancel={cancel}, Block={block}",
                                                         updateSql, dto.phone, (long)cancel + 1, (long)cancel + 1 == 3 ? 1 : 0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                _con.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 查詢取消紀錄
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<long> GetCancelRecord(GetCancelRecordDto dto)
        {
            try
            {
                using (_con)
                {
                    Open();

                    //搜尋失約紀錄
                    string selectSql = @"SELECT [Cancel] From BlackList WHERE [Phone] = @Phone";
                    using (var command = new SqliteCommand(selectSql, _con))
                    {
                        // 設定參數值
                        command.Parameters.AddWithValue("@Phone", dto.phone);

                        var count = await command.ExecuteScalarAsync();

                        if (count != null)
                        {
                            return (long)count;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                _con.Dispose();
                throw;
            }

            return 0;
        }
        /// <summary>
        /// 取得預約紀錄
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<DailyReserveDto> GetDailyReserve(DailyReportDto dto)
        {
            DailyReserveDto dailyReserves = new DailyReserveDto();
            List<TimeSpan> waitTimes = new List<TimeSpan>();

            try
            {
                using (_con)
                {
                    Open();

                    string sql = @"SELECT * FROM DailyReserve 
                                   WHERE [TicketTime] >= @StartTime AND [TicketTime] <= @EndTime ";

                    using (var command = new SqliteCommand(sql, _con))
                    {
                        // 設定參數值
                        command.Parameters.AddWithValue("@StartTime", dto.startTime.Value.Date);
                        command.Parameters.AddWithValue("@EndTime", dto.endTime.Value.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                // 讀取並解析日期時間欄位
                                var ticketTimeString = reader.GetString(reader.GetOrdinal("TicketTime"));
                                var seatTimeString = reader.GetString(reader.GetOrdinal("SeatTime"));

                                // 解析日期時間
                                if (DateTime.TryParse(ticketTimeString, out var ticketTime) && DateTime.TryParse(seatTimeString, out var seatTime))
                                {
                                    dailyReserves.DailyReserves.Add(new DailyReserve
                                    {
                                        number = reader.GetInt32(reader.GetOrdinal("QueueNumber")),
                                        ticketTime = ticketTime,
                                        seatTime = seatTime,
                                        waitTime = seatTime - ticketTime,
                                        takeWay = (TakeWayEnum)reader.GetInt32(reader.GetOrdinal("TakeWay")),
                                        phone = reader.GetInt32(reader.GetOrdinal("Phone")),
                                        people = reader.GetInt32(reader.GetOrdinal("People")),
                                        order = reader.GetInt32(reader.GetOrdinal("Order")),
                                        tableSize = (TableSizeEnum)reader.GetInt32(reader.GetOrdinal("TableSize"))
                                    });
                                }
                            }
                        }
                    }
                }
                return dailyReserves;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                _con.Dispose();
                throw;
            }
        }
        /// <summary>
        /// 新增黑名單
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task AddBlackList(AddBlackListDto dto)
        {
            try
            {
                using (_con)
                {
                    Open();

                    string insertSql = @"INSERT INTO BlackList ([Phone],[Cancel],[Block]) 
                                            VALUES (@Phone,@Cancel,@Block)";
                    using (var command = new SqliteCommand(insertSql, _con))
                    {
                        // 設定參數值
                        command.Parameters.AddWithValue("@Phone", dto.phone);
                        command.Parameters.AddWithValue("@Cancel", 0);
                        command.Parameters.AddWithValue("@Block", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                _con.Dispose();
                throw;
            }
        }

        public async Task DeleteBlackList(DeleteBlackListDto dto)
        {
            try
            {
                using (_con)
                {
                    Open();

                    string deleteSql = @"DELETE BlackList WHERE [Phone] = @Phone";

                    using (var command = new SqliteCommand(deleteSql, _con))
                    {
                        // 設定參數值
                        command.Parameters.AddWithValue("@Phone", dto.phone);
                        await command.ExecuteNonQueryAsync();
                    }

                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                _con.Dispose();
                throw;
            }
        }
    }
}
