using System;
using System.ComponentModel.DataAnnotations;

namespace HafezTelegram.Models
{
    public class TomorrowMeltedGold
    {
        public int Id { get; set; }

        public long MessageId { get; set; }

        [DataType(DataType.DateTime)] public DateTime Date { get; set; }

        public int Price { get; set; }

        public string MessageText { get; set; }

        public string Remark { get; set; }
    }
}