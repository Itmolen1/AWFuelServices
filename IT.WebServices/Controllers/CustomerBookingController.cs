﻿using IT.Core.ViewModels;
using IT.Repository;
using IT.WebServices.MISC;
using IT.WebServices.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace IT.WebServices.Controllers
{
    public class CustomerBookingController : ApiController
    {
        UnitOfWork unitOfWork = new UnitOfWork();
        ServiceResponseModel userRepsonse = new ServiceResponseModel();

        readonly string contentType = "application/json";

        [HttpPost]
        public HttpResponseMessage All(PagingParameterModel pagingparametermodel)
        {
            try
            {
                var BookingList = unitOfWork.GetRepositoryInstance<CustomerBookingViewModel>().ReadStoredProcedure("CustomerBookingAll @CompanyId",
                    new SqlParameter("CompanyId", System.Data.SqlDbType.Int) { Value = pagingparametermodel.CompanyId }
                    ).ToList();

                int count = BookingList.Count();
                int TotalCount = count;

                if (pagingparametermodel.SerachKey != null && pagingparametermodel.SerachKey != "")
                {
                    BookingList = BookingList.Where(x => x.CompanyName.ToLower().Contains(pagingparametermodel.SerachKey.ToLower())).ToList();
                }

                // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                int CurrentPage = pagingparametermodel.pageNumber;

                // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                int PageSize = pagingparametermodel.pageSize;

                // Display TotalCount to Records to User  

                var items = new List<CustomerBookingViewModel>();
                // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                int TotalPages = (int)Math.Ceiling(count / (double)PageSize);
                if (pagingparametermodel.sortColumnDir == "asc")
                {
                    items = BookingList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).OrderBy(x => pagingparametermodel.sortColumn).ToList();
                }
                else
                {
                    // Returns List of Customer after applying Paging   
                     items = BookingList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).OrderByDescending(x => x.CompanyName).ToList();
                }
                // if CurrentPage is greater than 1 means it has previousPage  
                var previousPage = CurrentPage > 1 ? "Yes" : "No";

                // if TotalPages is greater than CurrentPage means it has nextPage  
                var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

                if(items.Count > 0)
                {
                    items[0].TotalRows = TotalCount;
                }
                // Object which we are going to send in header   
                var paginationMetadata = new
                {
                    totalCount = TotalCount,
                    pageSize = PageSize,
                    currentPage = CurrentPage,
                    totalPages = TotalPages,
                    previousPage,
                    nextPage,
                };

                HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));

                userRepsonse.Success((new JavaScriptSerializer()).Serialize(items));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }

        [HttpPost]
        public HttpResponseMessage CustomerBookingAdd([FromBody] CustomerBookingViewModel customerBookingViewModel)
        {
            try
            {
                string Number = BookingNumber();

                var BookingAdd = unitOfWork.GetRepositoryInstance<CustomerBookingViewModel>().ReadStoredProcedure("CustomerBookingAdd @CompanyId, @BookQuantity, @UnitPrice, @VAT, @TotalAmount, @Description,@CreatedBy,@Productid,@UnitId,@OrderNo",
                 new SqlParameter("CompanyId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.CompanyId }
               , new SqlParameter("BookQuantity", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.BookQuantity }
               , new SqlParameter("UnitPrice", System.Data.SqlDbType.Money) { Value = customerBookingViewModel.UnitPrice }
               , new SqlParameter("VAT", System.Data.SqlDbType.Money) { Value = customerBookingViewModel.VAT }
               , new SqlParameter("TotalAmount", System.Data.SqlDbType.Money) { Value = customerBookingViewModel.TotalAmount }
               , new SqlParameter("Description", System.Data.SqlDbType.NVarChar) { Value = customerBookingViewModel.Description ?? (object)DBNull.Value }
               , new SqlParameter("CreatedBy", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.CreatedBy }
               , new SqlParameter("Productid", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.Productid }
               , new SqlParameter("UnitId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.UnitId }
               , new SqlParameter("OrderNo", System.Data.SqlDbType.NVarChar) { Value = Number ?? (object)DBNull.Value }
                 ).FirstOrDefault();

                if (BookingAdd.Id > 0)
                {
                    var customerOrderListViewModel = new CustomerOrderListViewModel();
                    CustomerOrderController customerOrderController = new CustomerOrderController();

                    //if (customerOrderListViewModel.IsSend == true)
                    //{
                        //Send Notification
                        customerOrderListViewModel.Title = "Booking Created";
                        customerOrderListViewModel.NotificationCode = "ADM-009";
                        customerOrderListViewModel.Message = "new Booking Created.";

                        int Res = customerOrderController.AdminNotificaton(customerOrderListViewModel);
                   // }
                }
                
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(BookingAdd));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }

        [HttpPost]
        public HttpResponseMessage CustomerBookingUpdate([FromBody] CustomerBookingViewModel customerBookingViewModel)
        {
            UpdateReason updateReason = new UpdateReason(); 
            try
            {
                var BookingUpdate = unitOfWork.GetRepositoryInstance<CustomerBookingViewModel>().ReadStoredProcedure("CustomerBookingUpdate @Id, @BookQuantity, @UnitPrice, @VAT, @TotalAmount, @Description,@UpdatedBy,@Productid,@UnitId",
                     new SqlParameter("Id", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.Id }
                   , new SqlParameter("BookQuantity", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.BookQuantity }
                   , new SqlParameter("UnitPrice", System.Data.SqlDbType.Money) { Value = customerBookingViewModel.UnitPrice }
                   , new SqlParameter("VAT", System.Data.SqlDbType.Money) { Value = customerBookingViewModel.VAT }
                   , new SqlParameter("TotalAmount", System.Data.SqlDbType.Money) { Value = customerBookingViewModel.TotalAmount }
                   , new SqlParameter("Description", System.Data.SqlDbType.NVarChar) { Value = customerBookingViewModel.Description ?? (object)DBNull.Value }
                   , new SqlParameter("UpdatedBy", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.UpdatedBy }
                   , new SqlParameter("Productid", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.Productid }
                   , new SqlParameter("UnitId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.UnitId }
                     ).FirstOrDefault();

                if (customerBookingViewModel.UpdateReasonDescriptionViewModel != null)
                {
                    if (BookingUpdate.Id > 0)
                    {
                        var result = updateReason.Add(customerBookingViewModel.UpdateReasonDescriptionViewModel);
                    }
                }

                userRepsonse.Success((new JavaScriptSerializer()).Serialize(BookingUpdate));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }

        [HttpPost]
        public HttpResponseMessage CustomerBookingSetDueDate(CustomerBookingViewModel customerBookingViewModel)
        {
            try
            {
                var BookingUpdateDueDate = unitOfWork.GetRepositoryInstance<CustomerBookingViewModel>().ReadStoredProcedure("CustomerBookingSetDueDate @Id, @DueDate,@UpdatedBy",
                new SqlParameter("Id", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.Id }
              , new SqlParameter("DueDate", System.Data.SqlDbType.DateTime) { Value = customerBookingViewModel.DueDate }           
              , new SqlParameter("UpdatedBy", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.UpdatedBy }            
                ).FirstOrDefault();
                
                var BookingUpdateReasons = unitOfWork.GetRepositoryInstance<BookingUpdateReason>().ReadStoredProcedure("BookingUpdateReasonAdd @BookingId, @UpdateReason,@CreatedBy",
                new SqlParameter("BookingId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.Id }
              , new SqlParameter("UpdateReason", System.Data.SqlDbType.NVarChar) { Value = customerBookingViewModel.UpdateReason }
              , new SqlParameter("CreatedBy", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.UpdatedBy }
                ).FirstOrDefault();

                BookingUpdateDueDate.bookingUpdateReason = BookingUpdateReasons;

                userRepsonse.Success((new JavaScriptSerializer()).Serialize(BookingUpdateDueDate));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }

        [HttpPost]
        public HttpResponseMessage CustomerBookingGetById(CustomerBookingViewModel customerBookingViewModel)
        {
            try
            {
                var BookingById = unitOfWork.GetRepositoryInstance<CustomerBookingViewModel>().ReadStoredProcedure("CustomerBookingGetById @Id",
                    new SqlParameter("Id", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.Id }
                    ).FirstOrDefault();

                var Documents = unitOfWork.GetRepositoryInstance<UploadDocumentsViewModel>().ReadStoredProcedure("UploadDocumentsGetByRespectiveId @Id,@Flag"
               , new SqlParameter("Id", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.Id }
               , new SqlParameter("Flag", System.Data.SqlDbType.NVarChar) { Value = "Booking" }
               ).ToList();

                var updatereason = unitOfWork.GetRepositoryInstance<UpdateReasonDescriptionViewModel>().ReadStoredProcedure("UpdateReasonDescriptionGet @Id,@Flag"
               , new SqlParameter("Id", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.Id }
               , new SqlParameter("Flag", System.Data.SqlDbType.NVarChar) { Value = "Booking" }
               ).ToList();

                BookingById.uploadDocumentsViewModels = Documents;
                BookingById.updateReasonDescriptionViewModels = updatereason;

                userRepsonse.Success((new JavaScriptSerializer()).Serialize(BookingById));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }

        [HttpPost]
        public HttpResponseMessage CustomerBookingReserved(CustomerBookingViewModel customerBookingViewModel)
        {
            try
            {
                CustomerBookingReservedRemaining customerBookingReservedRemaining = new CustomerBookingReservedRemaining();
                
                if (customerBookingViewModel.CompanyId > 0)
                {
                     var BookingReserved = unitOfWork.GetRepositoryInstance<CustomerBookingViewModel>().ReadStoredProcedure("CustomerBookingReserved @CompanyId",
                        new SqlParameter("CompanyId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.CompanyId }
                        ).ToList();

                    var BookingLefted = unitOfWork.GetRepositoryInstance<CustomerBookingViewModel>().ReadStoredProcedure("CustomerBookingLefted @CompanyId",
                        new SqlParameter("CompanyId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.CompanyId }
                        ).ToList();

                    var BookingDeliverd = unitOfWork.GetRepositoryInstance<CustomerBookingViewModel>().ReadStoredProcedure("CustomerBookingDeliverd @CompanyId",
                       new SqlParameter("CompanyId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.CompanyId }
                       ).ToList();

                    var BookingPending = unitOfWork.GetRepositoryInstance<CustomerBookingViewModel>().ReadStoredProcedure("CustomerBookingPending @CompanyId",
                       new SqlParameter("CompanyId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.CompanyId }
                       ).ToList();

                    var BookingMostRecent = unitOfWork.GetRepositoryInstance<CustomerBookingViewModel>().ReadStoredProcedure("CustomerBookingRecentBooking @CompanyId",
                       new SqlParameter("CompanyId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.CompanyId }
                       ).FirstOrDefault();

                    var bookLeftRemaining = unitOfWork.GetRepositoryInstance<BookLeftRemainingViewModel>().ReadStoredProcedure("CustomerBookingLeftedRemainingQtyByCompanyId @CompanyId",
                      new SqlParameter("CompanyId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.CompanyId }
                      ).FirstOrDefault();

                    
                    customerBookingReservedRemaining.Reserved = BookingReserved;
                    customerBookingReservedRemaining.Lefted = BookingLefted;
                    customerBookingReservedRemaining.Remaining = BookingDeliverd;
                    customerBookingReservedRemaining.Pending = BookingPending;
                    customerBookingReservedRemaining.MostRecentBooking = BookingMostRecent;
                    customerBookingReservedRemaining.bookLeftRemainingViewModel = bookLeftRemaining;

                    userRepsonse.Success((new JavaScriptSerializer()).Serialize(customerBookingReservedRemaining));
                }
                else
                {
                    userRepsonse.Success((new JavaScriptSerializer()).Serialize("Please send CompanyId"));
                }
               
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }

        [HttpPost]
        public HttpResponseMessage CustomerBookingAcceptReject(CustomerBookingViewModel customerBookingViewModel)
        {
            try
            {
                var BookingAcceptReject = unitOfWork.GetRepositoryInstance<CustomerBookingViewModel>().ReadStoredProcedure("CustomerBookingAcceptReject @Id,@IsAccepted,@IsActive,@Description,@CreatedBy",
                    new SqlParameter("Id", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.Id },
                    new SqlParameter("IsAccepted", System.Data.SqlDbType.Bit) { Value = customerBookingViewModel.IsAccepted },
                    new SqlParameter("IsActive", System.Data.SqlDbType.Bit) { Value = customerBookingViewModel.IsActive },
                    new SqlParameter("Description", System.Data.SqlDbType.NVarChar) { Value = customerBookingViewModel.Description ?? (object)DBNull.Value },
                    new SqlParameter("CreatedBy", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.CreatedBy }
                    ).FirstOrDefault();

                CustomerOrderListViewModel customerOrderListViewModel = new CustomerOrderListViewModel();
                CustomerOrderController customerOrderController = new CustomerOrderController();

                if (BookingAcceptReject.IsAccepted == true)
                {

                     var BookingConfirmation = unitOfWork.GetRepositoryInstance<BookingUpdateReason>().ReadStoredProcedure("BookingConfirmationAdd @BookingId,@CreatedBy,@CompanyId",
                       new SqlParameter("BookingId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.Id },
                       new SqlParameter("CreatedBy", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.CreatedBy },
                       new SqlParameter("CompanyId", System.Data.SqlDbType.Int) { Value = BookingAcceptReject.CompanyId }
                         ).FirstOrDefault();

                    customerOrderListViewModel.NotificationCode = "CUS-006";
                    customerOrderListViewModel.Title = "Booking Confirmed";
                    customerOrderListViewModel.Message = "Your booking is accepted and order Confirmation is generated successfully, Please send LPO or call +971 555 771 4620";
                    customerOrderListViewModel.RequestedQuantity = 0;
                    customerOrderListViewModel.CustomerId = BookingAcceptReject.CompanyId;

                    //Send Notification
                    customerOrderController.CustomerNotification(customerOrderListViewModel);
                }
                else
                {
                    customerOrderListViewModel.NotificationCode = "CUS-007";
                    customerOrderListViewModel.Title = "Unfortunately! Booking Rejected";
                    customerOrderListViewModel.Message = "Plase contact Customer Care center +971 555 771 4620";
                    customerOrderListViewModel.RequestedQuantity = 0;
                    customerOrderListViewModel.CustomerId = BookingAcceptReject.CompanyId;

                    //Send Notification
                    customerOrderController.CustomerNotification(customerOrderListViewModel);
                }

                userRepsonse.Success((new JavaScriptSerializer()).Serialize(BookingAcceptReject));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }
        
        [HttpPost]
        public HttpResponseMessage BookingUpdateReasonAllByBookingId(CustomerBookingViewModel customerBookingViewModel)
        {
            try
            {
                var BookingAcceptReject = unitOfWork.GetRepositoryInstance<BookingUpdateReason>().ReadStoredProcedure("BookingUpdateReasonAllByBookingId @BookingId",
                    new SqlParameter("BookingId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.Id }
                      ).ToList();
                               
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(BookingAcceptReject));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }
        
        [HttpPost]
        public HttpResponseMessage BookingConfirmationAdd(CustomerBookingViewModel customerBookingViewModel)
        {
            try
            {
                var BookingConfirmation = unitOfWork.GetRepositoryInstance<BookingUpdateReason>().ReadStoredProcedure("BookingConfirmationAdd @BookingId,@CreatedBy,@CompanyId",
                    new SqlParameter("BookingId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.Id },
                    new SqlParameter("CreatedBy", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.CreatedBy },
                    new SqlParameter("CompanyId", System.Data.SqlDbType.Int) { Value = customerBookingViewModel.CompanyId }
                      ).FirstOrDefault();

                userRepsonse.Success((new JavaScriptSerializer()).Serialize(BookingConfirmation));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }
              
        [HttpPost]
        public HttpResponseMessage BookingConfirmationByCompany(PagingParameterModel pagingparametermodel)
        {
            try
            {
                var BookingList = unitOfWork.GetRepositoryInstance<CustomerBookingViewModel>().ReadStoredProcedure("BookingConfirmationByCompany @CompanyId",
                    new SqlParameter("CompanyId", System.Data.SqlDbType.Int) { Value = pagingparametermodel.CompanyId }
                    ).ToList();

                int count = BookingList.Count();

                // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                int CurrentPage = pagingparametermodel.pageNumber;

                // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                int PageSize = pagingparametermodel.pageSize;

                // Display TotalCount to Records to User  
                int TotalCount = count;

                // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                // Returns List of Customer after applying Paging   
                var items = BookingList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).OrderByDescending(x => x.Id).ToList();

                // if CurrentPage is greater than 1 means it has previousPage  
                var previousPage = CurrentPage > 1 ? "Yes" : "No";

                // if TotalPages is greater than CurrentPage means it has nextPage  
                var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

                // Object which we are going to send in header   
                var paginationMetadata = new
                {
                    totalCount = TotalCount,
                    pageSize = PageSize,
                    currentPage = CurrentPage,
                    totalPages = TotalPages,
                    previousPage,
                    nextPage
                };

                HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));


                userRepsonse.Success((new JavaScriptSerializer()).Serialize(items));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }

        [HttpPost]
        public HttpResponseMessage BookingConfirmationById(SearchViewModel searchViewModel)
        {
            try
            {
                var ConfirmationData = unitOfWork.GetRepositoryInstance<BookingConfirmationViewModel>().ReadStoredProcedure("BookingConfirmationById @Id",
                    new SqlParameter("Id", System.Data.SqlDbType.Int) { Value = searchViewModel.Id }
                    ).FirstOrDefault();
                             
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ConfirmationData));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }

        [HttpPost]
        public HttpResponseMessage BookingAllDetails()
        {
            try
            {
                var bookingDetails = unitOfWork.GetRepositoryInstance<BookingDetailsViewModel>().ReadStoredProcedure("CustomerBookingDetails"
                                     ).ToList();

                userRepsonse.Success((new JavaScriptSerializer()).Serialize(bookingDetails));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }

        [NonAction]
        public string BookingNumber()
        {
            try
            {
                string No = "Order-1000";
                var OrderNo = unitOfWork.GetRepositoryInstance<SingleStringValueResult>().ReadStoredProcedure("CustomerBookingGetLAstOrderNo"
                              ).FirstOrDefault();
                if(OrderNo != null)
                {
                    if(OrderNo.Result != null)
                    {
                        if(OrderNo.Result != No)
                        {
                            int newNo = Convert.ToInt32(OrderNo.Result.Split('-').Last())+1;
                            No = "Order-"+ newNo;
                        }
                    }
                }
                return No;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}




