﻿using IT.Core.ViewModels;
using IT.Core.ViewModels.Common;
using IT.Repository;
using IT.WebServices.MISC;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace IT.WebServices.Controllers
{
    public class VenderController : ApiController
    {
        UnitOfWork unitOfWork = new UnitOfWork();
        ServiceResponseModel userRepsonse = new ServiceResponseModel();

        string contentType = "application/json";
        
        [HttpPost]
        public HttpResponseMessage Add(VenderViewModel venderViewModel)
        {
            try
            {
                var Res = unitOfWork.GetRepositoryInstance<SingleIntegerValueResult>().ReadStoredProcedure("VenderAdd @Name,@Address,@City,@LandLine,@Mobile,@Email,@Website,@Remarks,@CreatedBy,@TRN,@Representative,@Title",

                  new SqlParameter("Name", System.Data.SqlDbType.VarChar) { Value = venderViewModel.Name == null ? (object)DBNull.Value : venderViewModel.Name }
                , new SqlParameter("Address", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.Address == null ? (object)DBNull.Value : venderViewModel.Address }
                , new SqlParameter("City", System.Data.SqlDbType.Int) { Value = 1 }
                , new SqlParameter("LandLine", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.LandLine == null ? (object)DBNull.Value : venderViewModel.LandLine }
                , new SqlParameter("Mobile", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.Mobile == null ? (object)DBNull.Value : venderViewModel.Mobile }
                , new SqlParameter("Email", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.Email == null ? (object)DBNull.Value : venderViewModel.Email }
                , new SqlParameter("Website", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.Website == null ? (object)DBNull.Value : venderViewModel.Website }
                , new SqlParameter("Remarks", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.Remarks == null ? (object)DBNull.Value : venderViewModel.Remarks }
                , new SqlParameter("CreatedBy", System.Data.SqlDbType.Int) { Value = venderViewModel.CreatedBy }
                , new SqlParameter("TRN", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.TRN == null ? (object)DBNull.Value : venderViewModel.TRN }
                , new SqlParameter("Representative", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.Representative == null ? (object)DBNull.Value : venderViewModel.Representative }
                , new SqlParameter("Title", System.Data.SqlDbType.VarChar) { Value = venderViewModel.Title == null ? (object)DBNull.Value : venderViewModel.Title }

                ).FirstOrDefault();

                userRepsonse.Success(new JavaScriptSerializer().Serialize(Res.Result));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Exception((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }

        }

        [HttpPost]
        public HttpResponseMessage All()
        {
            try
            {
                var VenderList = unitOfWork.GetRepositoryInstance<VenderViewModel>().ReadStoredProcedure("VenderAll"
                    ).ToList();

                userRepsonse.Success((new JavaScriptSerializer()).Serialize(VenderList));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Exception((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }

        [HttpPost]
        public HttpResponseMessage Edit(int Id)
        {
            try
            {
                var venderDate = unitOfWork.GetRepositoryInstance<VenderViewModel>().ReadStoredProcedure("VenderById @Id"
                , new SqlParameter("Id", System.Data.SqlDbType.Int) { Value = Id }
                ).FirstOrDefault();

                var updatereason = unitOfWork.GetRepositoryInstance<UpdateReasonDescriptionViewModel>().ReadStoredProcedure("UpdateReasonDescriptionGet @Id,@Flag"
               , new SqlParameter("Id", System.Data.SqlDbType.Int) { Value = Id }
               , new SqlParameter("Flag", System.Data.SqlDbType.NVarChar) { Value = "Vender" }
               ).ToList();

                venderDate.updateReasonDescriptionViewModels = updatereason;

                userRepsonse.Success((new JavaScriptSerializer()).Serialize(venderDate));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Exception((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }

        [HttpPost]
        public HttpResponseMessage Update(VenderViewModel venderViewModel)
        {
            try
            {
                var Res = unitOfWork.GetRepositoryInstance<SingleIntegerValueResult>().ReadStoredProcedure("VenderUpdate @Id, @Name,@Address,@City,@LandLine,@Mobile,@Email,@Website,@Remarks,@UpdatedBy,@TRN,@Representative,@Title",
                  new SqlParameter("Id", System.Data.SqlDbType.Int) { Value = venderViewModel.Id }
                , new SqlParameter("Name", System.Data.SqlDbType.VarChar) { Value = venderViewModel.Name == null ? (object)DBNull.Value : venderViewModel.Name }
                , new SqlParameter("Address", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.Address == null ? (object)DBNull.Value : venderViewModel.Address }
                , new SqlParameter("City", System.Data.SqlDbType.Int) { Value = venderViewModel.City }
                , new SqlParameter("LandLine", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.LandLine == null ? (object)DBNull.Value : venderViewModel.LandLine }
                , new SqlParameter("Mobile", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.Mobile == null ? (object)DBNull.Value : venderViewModel.Mobile }
                , new SqlParameter("Email", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.Email == null ? (object)DBNull.Value : venderViewModel.Email }
                , new SqlParameter("Website", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.Website == null ? (object)DBNull.Value : venderViewModel.Website }
                , new SqlParameter("Remarks", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.Remarks == null ? (object)DBNull.Value : venderViewModel.Remarks }
                , new SqlParameter("UpdatedBy", System.Data.SqlDbType.Int) { Value = venderViewModel.UpdatedBy }
                , new SqlParameter("TRN", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.TRN == null ? (object)DBNull.Value : venderViewModel.TRN }
                , new SqlParameter("Representative", System.Data.SqlDbType.NVarChar) { Value = venderViewModel.Representative == null ? (object)DBNull.Value : venderViewModel.Representative }
                , new SqlParameter("Title", System.Data.SqlDbType.VarChar) { Value = venderViewModel.Title == null ? (object)DBNull.Value : venderViewModel.Title }
                
                ).FirstOrDefault();

                if (venderViewModel.updateReasonDescriptionViewModel != null)
                {
                    UpdateReason updateReason = new UpdateReason();
                    if (venderViewModel.Id > 0)
                    {
                        var result = updateReason.Add(venderViewModel.updateReasonDescriptionViewModel);
                    }
                }

                userRepsonse.Success(new JavaScriptSerializer().Serialize(Res.Result));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Exception((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }

        }
        
        [HttpPost]
        public HttpResponseMessage ChangeStatus(VenderViewModel venderViewModel)
        {
            try
            {
                var siteDate = unitOfWork.GetRepositoryInstance<SingleIntegerValueResult>().ReadStoredProcedure("VenderDisable @Id,@IsActive"
                , new SqlParameter("Id", System.Data.SqlDbType.Int) { Value = venderViewModel.Id }
                , new SqlParameter("IsActive", System.Data.SqlDbType.Bit) { Value = venderViewModel.IsActive }
                ).FirstOrDefault();
                userRepsonse.Success((new JavaScriptSerializer()).Serialize(siteDate));
                return Request.CreateResponse(HttpStatusCode.Accepted, userRepsonse, contentType);
            }
            catch (Exception ex)
            {
                userRepsonse.Exception((new JavaScriptSerializer()).Serialize(ex));
                return Request.CreateResponse(HttpStatusCode.BadRequest, userRepsonse, contentType);
            }
        }

    }
}
