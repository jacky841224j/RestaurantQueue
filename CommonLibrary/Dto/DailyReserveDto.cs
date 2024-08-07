﻿using CommonLibrary.Enum;

namespace CommonLibrary.Dto
{
    public class DailyReserveDto
    {
        /// <summary>
        /// 每日記錄
        /// </summary>
        public List<DailyReserve> DailyReserves { get; set; } = new List<DailyReserve>();

        /// <summary>
        /// 平均等待時間
        /// </summary>
        public string AvgWaitTime { get; set; }

        /// <summary>
        /// 餐期平均等待時間
        /// </summary>
        public List<MealPeriodTime> MealPeriod { get; set; } = new List<MealPeriodTime>();


        public class DailyReserve
        {
            /// <summary>
            /// 取號號碼
            /// </summary>
            public int number { get; set; }

            /// <summary>
            /// 取號時間
            /// </summary>
            public DateTime ticketTime { get; set; }

            /// <summary>
            /// 入座時間
            /// </summary>
            public DateTime seatTime { get; set; }

            /// <summary>
            /// 等待時間
            /// </summary>
            public TimeSpan waitTime { get; set; }

            /// <summary>
            /// 取號方式
            /// </summary>
            public TakeWayEnum takeWay { get; set; }

            /// <summary>
            /// 電話
            /// </summary>
            public int phone { get; set; }

            /// <summary>
            /// 人數
            /// </summary>
            public int people { get; set; }

            /// <summary>
            /// 目前順位
            /// </summary>
            public int order { get; set; }

            /// <summary>
            /// 桌子大小
            /// </summary>
            public TableSizeEnum tableSize { get; set; }
        }

        public class MealPeriodTime
        {
            public int Count { get; set; }
            public string TimeRange { get; set; }
            public string AvgWaitTime { get; set; }
        }
    }
}
