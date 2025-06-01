using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace project.Models
{
    public enum OrderStatus
    {
        Pending = 0,           // 學生下單、等待老師確認
        Accepted = 1,          // 老師已接受、等待學生確認
        Confirmed = 2,         // 雙方確認，進行中
        TeacherCompleted = 3,  // 老師按了完成，學生還沒完成
        StudentCompleted = 4,  // 學生按了完成，老師還沒完成
        Finished = 5,          // 雙方都完成
        StudentCancelled = 6,  // 學生取消
        TeacherRejected = 7    // 老師拒絕
    }



}
