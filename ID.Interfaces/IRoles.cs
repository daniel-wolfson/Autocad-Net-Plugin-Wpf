﻿using System;

namespace General.Interfaces
{
    public interface IRoles
    {
        string Description { get; set; }
        int RoleId { get; set; }
        string RoleName { get; set; }
        DateTime? UpdateDate { get; set; }
    }
}