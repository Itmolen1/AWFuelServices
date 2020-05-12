﻿using CrystalDecisions.CrystalReports.Engine;
using IT.Core.ViewModels;
using IT.Repository.WebServices;
using IT.Web.MISC;
using IT.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace IT.Web.Areas.Invoice.Controllers
{
    [Autintication]
    public class InvoiceController : Controller
    {
        WebServices webServices = new WebServices();

        List<ProductViewModel> ProductViewModel = new List<ProductViewModel>();
        List<ProductUnitViewModel> productUnitViewModels = new List<ProductUnitViewModel>();
        readonly List<CustomerViewModel> customerViewModels = new List<CustomerViewModel>();
        LPOInvoiceViewModel lPOInvoiceViewModel = new LPOInvoiceViewModel();
        List<LPOInvoiceDetails> lPOInvoiceDetails = new List<LPOInvoiceDetails>();
        List<LPOInvoiceViewModel> lPOInvoiceViewModels = new List<LPOInvoiceViewModel>();
        List<CompanyViewModel> companyViewModels = new List<CompanyViewModel>();

        
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetAll(DataTablesParm parm)
        {
            try
            {
                int pageNo = 1;
                int totalCount = 0;

                if (parm.iDisplayStart >= parm.iDisplayLength)
                {
                    pageNo = (parm.iDisplayStart / parm.iDisplayLength) + 1;
                }

                //int CompanyId = Convert.ToInt32(Session["CompanyId"]);

                if (HttpContext.Cache["InvoiceData"] != null)
                {
                    lPOInvoiceViewModels = HttpContext.Cache["InvoiceData"] as List<LPOInvoiceViewModel>;
                }
                else
                {
                    var result = webServices.Post(new VehicleViewModel(), "Invoice/All");
                    lPOInvoiceViewModels = (new JavaScriptSerializer()).Deserialize<List<LPOInvoiceViewModel>>(result.Data.ToString());

                    HttpContext.Cache["InvoiceData"] = lPOInvoiceViewModels;
                }
                if (parm.sSearch != null)
                {
                    totalCount = lPOInvoiceViewModels.Where(x => x.Name.ToLower().Contains(parm.sSearch.ToLower()) ||
                               x.GrandTotal.ToString().Contains(parm.sSearch) ||
                               x.UserName.ToLower().Contains(parm.sSearch.ToLower()) ||
                               x.PONumber.ToString().Contains(parm.sSearch)).Count();

                    lPOInvoiceViewModels = lPOInvoiceViewModels.ToList()
                        .Where(x => x.Name.ToLower().Contains(parm.sSearch.ToLower()) ||
                               x.GrandTotal.ToString().Contains(parm.sSearch) ||
                               x.UserName.ToLower().Contains(parm.sSearch.ToLower()) ||
                               x.PONumber.ToString().Contains(parm.sSearch))
                               .Skip((pageNo - 1) * parm.iDisplayLength)
                               .Take(parm.iDisplayLength)
                   .Select(x => new LPOInvoiceViewModel
                   {
                       Id = x.Id,
                       Name = x.Name,
                       Total = x.Total,
                       VAT = x.VAT,
                       GrandTotal = x.GrandTotal,
                       UserName = x.UserName,
                       PONumber = x.PONumber,
                       FDate = x.FDate,
                       DDate = x.DDate
                   }).ToList();
                }
                else
                {
                    totalCount = lPOInvoiceViewModels.Count();

                    lPOInvoiceViewModels = lPOInvoiceViewModels
                                                       .Skip((pageNo - 1) * parm.iDisplayLength)
                                                       .Take(parm.iDisplayLength)
                        .Select(x => new LPOInvoiceViewModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Total = x.Total,
                            VAT = x.VAT,
                            GrandTotal = x.GrandTotal,
                            UserName = x.UserName,
                            PONumber = x.PONumber,
                            FDate = x.FDate,
                            DDate = x.DDate

                        }).ToList();
                }
                return Json(
                    new
                    {
                        aaData = lPOInvoiceViewModels,
                        parm.sEcho,
                        iTotalDisplayRecords = totalCount,
                        data = lPOInvoiceViewModels,
                        iTotalRecords = totalCount,
                    }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                throw;
            }

        }

        public ActionResult Create()
        {
            try
            {
                string SerailNO = "";

                InvoicePNNumber invoicePNNumber = new InvoicePNNumber();

                SerailNO = invoicePNNumber.PnNumber();

                var result = webServices.Post(new ProductViewModel(), "Product/All");
                ProductViewModel = (new JavaScriptSerializer()).Deserialize<List<ProductViewModel>>(result.Data.ToString());
                ProductViewModel.Insert(0, new ProductViewModel() { Id = 0, Name = "Select Item" });
                ViewBag.Product = ProductViewModel;

                var results = webServices.Post(new ProductUnitViewModel(), "ProductUnit/All");
                productUnitViewModels = (new JavaScriptSerializer()).Deserialize<List<ProductUnitViewModel>>(results.Data.ToString());
                productUnitViewModels.Insert(0, new ProductUnitViewModel() { Id = 0, Name = "Select Unit" });
                ViewBag.ProductUnit = productUnitViewModels;

                var Res = webServices.Post(new CompanyViewModel(), "Company/CompayAll");
                companyViewModels = (new JavaScriptSerializer()).Deserialize<List<CompanyViewModel>>(Res.Data.ToString());
                companyViewModels.Insert(0, new CompanyViewModel() { Id = 0, Name = "Select Customer Name" });

                ViewBag.Vender = companyViewModels;


                ViewBag.PO = SerailNO;

                ViewBag.titles = "Invoice";

                LPOInvoiceViewModel lPOInvoiceVModel = new LPOInvoiceViewModel
                {
                    FromDate = System.DateTime.Now,
                    DueDate = System.DateTime.Now
                };
                return View(lPOInvoiceVModel);


            }
            catch (Exception ex)
            {
                throw ex;
            }
         
        }

        [HttpPost]
        public ActionResult Create(LPOInvoiceViewModel lPOInvoiceViewModel)
        {
            try
            {
                lPOInvoiceViewModel.FromDate = Convert.ToDateTime(lPOInvoiceViewModel.FromDate);
                lPOInvoiceViewModel.DueDate = Convert.ToDateTime(lPOInvoiceViewModel.DueDate);

                lPOInvoiceViewModel.CreatedBy = Convert.ToInt32(Session["UserId"]);

                var result = webServices.Post(lPOInvoiceViewModel, "Invoice/Add");

                if (result.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    if (result.Data != "[]")
                    {
                        int Res = (new JavaScriptSerializer()).Deserialize<int>(result.Data);

                        HttpContext.Cache.Remove("InvoiceData");
                        TempData["Id"] = Res;

                        // HttpContext.Cache.Remove("InvoiceData");
                        return Json(Res, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Failed", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Failed", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        public ActionResult Details(int? Id)
        {
            try
            {
                var Result = webServices.Post(new LPOInvoiceViewModel(), "Invoice/Edit/" + Id);

                if (Result.Data != "[]")
                {
                    lPOInvoiceViewModel = (new JavaScriptSerializer().Deserialize<LPOInvoiceViewModel>(Result.Data.ToString()));
                    ViewBag.lPOInvoiceViewModel = lPOInvoiceViewModel;
                    lPOInvoiceViewModel.Heading = "Invoice";
                    var Results = webServices.Post(new LPOInvoiceDetails(), "Invoice/EditDetails/" + Id);

                    if (Results.Data != "[]")
                    {
                        lPOInvoiceDetails = (new JavaScriptSerializer().Deserialize<List<LPOInvoiceDetails>>(Results.Data.ToString()));
                        ViewBag.lPOInvoiceDetails = lPOInvoiceDetails;

                        if (TempData["Success"] == null)
                        {
                            if (TempData["Download"] != null)
                            {
                                ViewBag.IsDownload = TempData["Download"].ToString();
                                ViewBag.Id = Id;
                            }
                        }
                        else
                        {
                            ViewBag.Success = TempData["Success"];
                        }

                        return View();
                    }
                    else
                    {
                        return View();
                    }
                }
                else
                {
                    return View();
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public ActionResult Edit(int? Id)
        {
            try
            {
                var Result = webServices.Post(new LPOInvoiceViewModel(), "Invoice/Edit/" + Id);


                var result = webServices.Post(new ProductViewModel(), "Product/All");
                ProductViewModel = (new JavaScriptSerializer()).Deserialize<List<ProductViewModel>>(result.Data.ToString());
                ProductViewModel.Insert(0, new ProductViewModel() { Id = 0, Name = "Select Item" });
                ViewBag.Product = ProductViewModel;

                var results = webServices.Post(new ProductUnitViewModel(), "ProductUnit/All");
                productUnitViewModels = (new JavaScriptSerializer()).Deserialize<List<ProductUnitViewModel>>(results.Data.ToString());
                productUnitViewModels.Insert(0, new ProductUnitViewModel() { Id = 0, Name = "Select Unit" });
                ViewBag.ProductUnit = productUnitViewModels;


                List<VatModel> model = new List<VatModel> {
                    new VatModel() { Id = 0, VAT = 0 },
                    new VatModel() { Id = 5, VAT = 5 }
                };
                ViewBag.VatDrop = model;

                if (Result.Data != "[]")
                {
                    lPOInvoiceViewModel = (new JavaScriptSerializer().Deserialize<LPOInvoiceViewModel>(Result.Data.ToString()));
                    ViewBag.lPOInvoiceViewModel = lPOInvoiceViewModel;

                    var Results = webServices.Post(new LPOInvoiceDetails(), "Invoice/EditDetails/" + Id);

                    if (Results.Data != "[]")
                    {
                        lPOInvoiceDetails = (new JavaScriptSerializer().Deserialize<List<LPOInvoiceDetails>>(Results.Data.ToString()));
                        ViewBag.lPOInvoiceDetails = lPOInvoiceDetails;

                        lPOInvoiceViewModel.Heading = "Invoice";

                        return View();
                    }
                    else
                    {
                        return View();
                    }
                }
                else
                {
                    return View();
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public ActionResult Update(LPOInvoiceViewModel lPOInvoiceViewModel)
        {
            try
            {
                lPOInvoiceViewModel.FromDate = Convert.ToDateTime(lPOInvoiceViewModel.FromDate);
                lPOInvoiceViewModel.DueDate = Convert.ToDateTime(lPOInvoiceViewModel.DueDate);

                var result = webServices.Post(lPOInvoiceViewModel, "Invoice/Update");

                if (result.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    if (result.Data != "[]")
                    {
                        int Res = (new JavaScriptSerializer()).Deserialize<int>(result.Data);

                        HttpContext.Cache.Remove("InvoiceData");

                        if (lPOInvoiceViewModel.IsDownload != null)
                        {
                            TempData["Download"] = "True";
                        }

                        TempData["Id"] = Res;
                        TempData["FileName"] = Res + "-" + lPOInvoiceViewModel.PONumber + ".pdf";
                        int Download = UploadFileToFolder(Res);

                        return Json(Res, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(result.Data.ToString(), JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(result.Data.ToString(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return Json(ex.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public int UploadFileToFolder(int Id)
        {
            string pdfname = "";

            try
            {
                ReportDocument Report = new ReportDocument();
                Report.Load(Server.MapPath("~/Reports/LPOReport.rpt"));

                List<IT.Web.Models.CompnayModel> compnayModels = new List<IT.Web.Models.CompnayModel>();
                List<IT.Web.Models.LPOInvoiceModel> lPOInvoiceModels = new List<IT.Web.Models.LPOInvoiceModel>();
                List<LPOInvoiceDetails> lPOInvoiceDetails = new List<LPOInvoiceDetails>();
                List<VenderModel> venderModels = new List<VenderModel>();

                var LPOInvoice = webServices.Post(new IT.Core.ViewModels.LPOInvoiceModel(), "Invoice/EditReport/" + Id);
                lPOInvoiceModels = (new JavaScriptSerializer()).Deserialize<List<IT.Web.Models.LPOInvoiceModel>>(LPOInvoice.Data.ToString());

                int CompanyId = Convert.ToInt32(Session["CompanyId"]);

                var companyData = webServices.Post(new IT.Core.ViewModels.CompnayModel(), "Company/Edit/" + CompanyId);
                compnayModels = (new JavaScriptSerializer()).Deserialize<List<IT.Web.Models.CompnayModel>>(companyData.Data.ToString());
                               
                var LPOInvoiceDetails = webServices.Post(new LPOInvoiceDetails(), "Invoice/EditDetails/" + Id);
                lPOInvoiceDetails = (new JavaScriptSerializer()).Deserialize<List<LPOInvoiceDetails>>(LPOInvoiceDetails.Data.ToString());

                IT.Web.Models.CompnayModel companyViewModel = new IT.Web.Models.CompnayModel();

                VenderViewModel venderViewModel = new VenderViewModel();
                
                var companyDatsa = webServices.Post(new IT.Core.ViewModels.CompnayModel(), "Company/Edit/" + lPOInvoiceModels[0].VenderId);
                companyViewModel = (new JavaScriptSerializer()).Deserialize<List<IT.Web.Models.CompnayModel>>(companyDatsa.Data.ToString()).FirstOrDefault();
                               
                venderModels.Add(new VenderModel()
                {

                    Name = companyViewModel.Name,
                    Address = companyViewModel.Address,
                    Representative = companyViewModel.OwnerRepresentaive,
                    LandLine = companyViewModel.Phone,
                    Mobile = companyViewModel.Cell,
                    Title = "Mr",
                    TRN = companyViewModel.TRN,
                    UserName = "Customer Info:"
                });
                

                Report.Database.Tables[0].SetDataSource(compnayModels);
                Report.Database.Tables[1].SetDataSource(lPOInvoiceModels);
                Report.Database.Tables[2].SetDataSource(lPOInvoiceDetails);
                Report.Database.Tables[3].SetDataSource(venderModels);
                
                Stream stram = Report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stram.Seek(0, SeekOrigin.Begin);

                string companyName = Id + "-" + lPOInvoiceModels[0].PONumber;

                var root = Server.MapPath("/PDF/");
                pdfname = String.Format("{0}.pdf", companyName);
                var path = Path.Combine(root, pdfname);
                path = Path.GetFullPath(path);

                Report.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, path);

                stram.Close();

                return 1;
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpPost]
        public ActionResult DeleteInvoiceDetailsRow(DeleteRowViewModel deleteRowViewModel)
        {
            try
            {
               
                decimal ResultVAT = CalculateVat(deleteRowViewModel.VAT, deleteRowViewModel.RowTotal);

                lPOInvoiceViewModel.lPOInvoiceDetailsList = new List<LPOInvoiceDetails>();

                var LPOData = webServices.Post(new LPOInvoiceViewModel(), "Invoice/Edit/" + deleteRowViewModel.Id);
                lPOInvoiceViewModel = (new JavaScriptSerializer()).Deserialize<LPOInvoiceViewModel>(LPOData.Data.ToString());

                lPOInvoiceViewModel.Total = lPOInvoiceViewModel.Total - deleteRowViewModel.RowTotal;
                lPOInvoiceViewModel.GrandTotal = lPOInvoiceViewModel.GrandTotal - ResultVAT;
                lPOInvoiceViewModel.GrandTotal = lPOInvoiceViewModel.GrandTotal - deleteRowViewModel.RowTotal;
                lPOInvoiceViewModel.VAT = lPOInvoiceViewModel.VAT - ResultVAT;
                lPOInvoiceViewModel.detailId = deleteRowViewModel.detailId;

                var result = webServices.Post(lPOInvoiceViewModel, "Invoice/DeleteDeatlsRow");

                if (result.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    HttpContext.Cache.Remove("InvoiceData");
                    return Json("Success", JsonRequestBehavior.AllowGet);

                }
                return new JsonResult { Data = new { Status = "Fail" } };
            }
            catch (Exception)
            {
                return new JsonResult { Data = new { Status = "Fail" } };
            }
        }

        public static decimal CalculateVat(decimal vat, decimal Total)
        {
            decimal Result = 0;
            try
            {
                Result = Convert.ToDecimal((Total / 100) * vat);
                return Result;
            }
            catch (Exception)
            {
                return Result;
            }
        }

        [HttpPost]
        public ActionResult SaveDwnload(LPOInvoiceViewModel lPOInvoiceViewModel)
        {
            try
            {

                var result = webServices.Post(lPOInvoiceViewModel, "Invoice/Add");

                if (result.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    if (result.Data != "[]")
                    {
                        int Res = (new JavaScriptSerializer()).Deserialize<int>(result.Data);

                        //  HttpContext.Cache.Remove("InvoiceData");

                        TempData["Download"] = "True";

                        //TempData.Keep();

                        TempData["Id"] = Res;
                        TempData["FileName"] = Res + "-" + lPOInvoiceViewModel.PONumber + ".pdf";

                        int Download = UploadFileToFolder(Res);

                        return Json(Res, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Failed", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Failed", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        public ActionResult PrintInvoice(int Id)
        {
            string pdfname = "";
            try
            {

                ReportDocument Report = new ReportDocument();
                Report.Load(Server.MapPath("~/Reports/LPOReport.rpt"));

                List<IT.Core.ViewModels.CompnayModel> compnayModels = new List<IT.Core.ViewModels.CompnayModel>();
                List<IT.Core.ViewModels.LPOInvoiceModel> lPOInvoiceModels = new List<IT.Core.ViewModels.LPOInvoiceModel>();
                List<LPOInvoiceDetails> lPOInvoiceDetails = new List<LPOInvoiceDetails>();
                List<VenderModel> venderModels = new List<VenderModel>();

                var LPOInvoice = webServices.Post(new IT.Core.ViewModels.LPOInvoiceModel(), "Invoice/EditReport/" + Id);
                if (LPOInvoice.Data != "[]")
                {
                    lPOInvoiceModels = (new JavaScriptSerializer()).Deserialize<List<IT.Core.ViewModels.LPOInvoiceModel>>(LPOInvoice.Data.ToString());
                }
                string companyName;
                if (lPOInvoiceModels.Count > 0)
                {
                     companyName = Id + "-" + lPOInvoiceModels[0].PONumber + ".pdf";
                }
                else
                {
                    companyName = Id + " No Date Found";
                }
                //if (System.IO.File.Exists(Server.MapPath("~/PDF/" + companyName)))
                //{
                //    System.IO.File.Delete(Server.MapPath("~/PDF/" + companyName));
                //}

                int CompanyId = Convert.ToInt32(Session["CompanyId"]);

                var companyData = webServices.Post(new IT.Core.ViewModels.CompnayModel(), "Company/Edit/" + CompanyId);

                if (companyData.Data != "[]")
                {
                    compnayModels = (new JavaScriptSerializer()).Deserialize<List<IT.Core.ViewModels.CompnayModel>>(companyData.Data.ToString());

                }
                var LPOInvoiceDetails = webServices.Post(new LPOInvoiceDetails(), "Invoice/EditDetails/" + Id);
                if (LPOInvoiceDetails.Data != "[]")
                {
                    lPOInvoiceDetails = (new JavaScriptSerializer()).Deserialize<List<LPOInvoiceDetails>>(LPOInvoiceDetails.Data.ToString());
                }
                IT.Web.Models.CompnayModel companyViewModel = new IT.Web.Models.CompnayModel();

                VenderViewModel venderViewModel = new VenderViewModel();

                if (lPOInvoiceModels.Count > 0)
                {
                    var companyDatsa = webServices.Post(new IT.Core.ViewModels.CompnayModel(), "Company/Edit/" + lPOInvoiceModels[0].VenderId);

                    if(companyDatsa.Data != "[]")
                    {
                        companyViewModel = (new JavaScriptSerializer()).Deserialize<List<IT.Web.Models.CompnayModel>>(companyDatsa.Data.ToString()).FirstOrDefault();
                    }
                }    
              
                venderModels.Add(new VenderModel()
                {
                    Name = companyViewModel.Name,
                    Address = companyViewModel.Address,
                    Representative = companyViewModel.OwnerRepresentaive,
                    LandLine = companyViewModel.Phone,
                    Mobile = companyViewModel.Cell,
                    Title = "Mr",
                    TRN = companyViewModel.TRN,
                    UserName = "Customer Info:"
                });


                Report.Database.Tables[0].SetDataSource(compnayModels);
                Report.Database.Tables[1].SetDataSource(lPOInvoiceModels);
                Report.Database.Tables[2].SetDataSource(lPOInvoiceDetails);
                Report.Database.Tables[3].SetDataSource(venderModels);


                Stream stram = Report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stram.Seek(0, SeekOrigin.Begin);

                companyName = Id + "-" + lPOInvoiceModels[0].PONumber;

                var root = Server.MapPath("/PDF/");
                pdfname = String.Format("{0}.pdf", companyName);
                var path = Path.Combine(root, pdfname);
                path = Path.GetFullPath(path);

                Report.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, path);

                stram.Close();


                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                string fileName = companyName + ".PDF";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);

                // Stream stram = Report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                // stram.Seek(0, SeekOrigin.Begin);

                // return new FileStreamResult(stram, "application/pdf");
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost]
        public ActionResult CheckISFileExist(int Id)
        {
            try
            {
                List<IT.Web.Models.LPOInvoiceModel> lPOInvoiceModels = new List<IT.Web.Models.LPOInvoiceModel>();
                var LPOInvoice = webServices.Post(new IT.Core.ViewModels.LPOInvoiceModel(), "Invoice/EditReport/" + Id);
                lPOInvoiceModels = (new JavaScriptSerializer()).Deserialize<List<IT.Web.Models.LPOInvoiceModel>>(LPOInvoice.Data.ToString());

                string companyName = Id + "-" + lPOInvoiceModels[0].PONumber;

                string FileName = companyName + ".pdf";

                if (System.IO.File.Exists(Server.MapPath(FileName)))
                {
                    TempData["Id"] = Id;
                    TempData["FileName"] = FileName;
                    return Json("Exist", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    int Res = UploadFileToFolder(Id);
                    if (Res > 0)
                    {
                        TempData["Id"] = Id;
                        TempData["FileName"] = FileName;
                        return Json("Exist", JsonRequestBehavior.AllowGet);
                    }

                    return Json("Failed", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public ActionResult CreateFromQuotation(LPOInvoiceViewModel lPOInvoiceViewModel)
        {
            try
            {

                lPOInvoiceViewModel.CreatedBy = Convert.ToInt32(Session["UserId"]);
                var result = webServices.Post(lPOInvoiceViewModel, "Invoice/AddFromQuotation");
                int Res = (new JavaScriptSerializer()).Deserialize<int>(result.Data);

                if (Res > 0)
                {
                    HttpContext.Cache.Remove("QuotationData");
                    HttpContext.Cache.Remove("InvoiceData");
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Failed", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                throw;
            }


        }
    }
}