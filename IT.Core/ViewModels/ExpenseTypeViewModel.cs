﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.Core.ViewModels
{
    public class ExpenseTypeViewModel
    {
        public int Id { get; set; }
        public string ExpensType { get; set; }
        public bool IsActive { get; set; }
    }
}
