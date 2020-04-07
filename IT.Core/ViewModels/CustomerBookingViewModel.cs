﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.Core.ViewModels
{
    public class CustomerBookingViewModel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BookQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VAT { get; set; }
        public decimal TotalAmount { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsOpen { get; set; }
        public int Productid { get; set; }
        public int UnitId { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public string UserName { get; set; }        
        public string CompanyName { get; set; }
        public DateTime DueDate { get; set; }
        public string DueDates { get; set; }
        public string CreatedDates { get; set; }
        public bool IsAccepted { get; set; }
        public string UpdateReason { get; set; }

        public BookingUpdateReason bookingUpdateReason { get; set; }
        public List<UploadDocumentsViewModel> uploadDocumentsViewModels { get; set; }
    }
}


