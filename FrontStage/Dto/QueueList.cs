﻿using FrontStage.Enum;

namespace FrontStage.Dto
{
    public class QueueList
    {
        /// <summary>
        /// 取號號碼
        /// </summary>
        public int queueNumber { get; set; }

        /// <summary>
        /// 取號時間
        /// </summary>
        public string ticketTime { get; set; }

        /// <summary>
        /// 入座時間
        /// </summary>
        public string seatTime { get; set; }

        /// <summary>
        /// 取號方式
        /// </summary>
        public TakeWay takeWay { get; set; }

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
}
