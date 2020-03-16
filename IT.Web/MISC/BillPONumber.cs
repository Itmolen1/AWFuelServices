﻿using IT.Core.ViewModels;
using IT.Repository.WebServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace IT.Web.MISC
{
    public class BillPONumber
    {
        WebServices webServices = new WebServices();

        public string PnNumber()
        {
            string AlreadyNumber = "";
            string SerailNO = "";
            var LPONoResult = webServices.Post(new SingleStringValueResult(), "Bill/BillNumber");
            if (LPONoResult.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                if (LPONoResult.Data != "[]")
                {
                    string LPNo = (new JavaScriptSerializer()).Deserialize<string>(LPONoResult.Data);


                    SerailNO = LPNo.Substring(4, 8);

                    SerailNO = SerailNO.ToString().Substring(0, 6);


                    string TotdayNumber = POClass.PONumber().Substring(0, 6);
                    int Counts = 0;
                    if (SerailNO == TotdayNumber)
                    {
                        Counts = Convert.ToInt32(LPNo.Substring(10, 2)) + 1;

                        if (Counts.ToString().Length == 1)
                        {
                            SerailNO = "BIL-" + TotdayNumber + "0" + Counts;
                        }
                        else
                        {
                            SerailNO = "BIL-" + TotdayNumber + Counts.ToString();
                        }
                    }
                    else
                    {
                        AlreadyNumber = POClass.PONumber();

                        SerailNO = "BIL-" + AlreadyNumber;
                    }
                }
                else
                {
                    AlreadyNumber = POClass.PONumber();

                    SerailNO = "BIL-" + AlreadyNumber;
                }

            }
            else
            {
                AlreadyNumber = POClass.PONumber();

                SerailNO = "BIL-" + AlreadyNumber;
            }

            return SerailNO;
        }
    }
}