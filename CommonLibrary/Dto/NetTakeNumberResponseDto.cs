﻿namespace CommonLibrary.Dto
{
    public class NetTakeNumberResponseDto
    {
        /// <summary>
        /// 當前號碼
        /// </summary>
        public int number { get; set; }

        /// <summary>
        /// 目前順位
        /// </summary>
        public int order { get; set; }

        /// <summary>
        /// 取號訊息
        /// </summary>
        public string message { get; set; }
    }
}
