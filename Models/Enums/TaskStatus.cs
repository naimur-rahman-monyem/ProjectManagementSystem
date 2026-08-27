using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectManagementSystem.Models.Enums
{
    public enum TaskStatus
    {
        Pending = 1,
        InProgress = 2,
        Blocked = 3,
        Completed = 4,
        Cancelled = 5
    }
}