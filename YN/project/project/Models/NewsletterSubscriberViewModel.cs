using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public class NewsletterSubscriberViewModel
    {
        public int Id { get; set; }                   // 資料庫 Id
        public string Email { get; set; }             // 訂閱 Email
        public DateTime SubscribeTime { get; set; }   // 訂閱時間
    }
}

