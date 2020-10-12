﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using allpax_service_record.Models;
using allpax_service_record.Models.View_Models;
using System.Data.SqlClient;
using System.Configuration;
using System.Dynamic;
using Westwind.Web.Mvc;
using Westwind.Utilities;
using System.Net.Mail;

namespace allpax_service_record.Controllers
{
    public class dailyReportByReportIDPrintController : Controller
    {
        private allpaxServiceRecordEntities db = new allpaxServiceRecordEntities();

            public ActionResult Index(int reportID)
        {
            ViewBag.reportID = reportID;           

            List<vm_dailyReportByReportID> dailyReportByID = new List<vm_dailyReportByReportID>();
            List<string> recipientList = new List<string>();

            string mainconn = ConfigurationManager.ConnectionStrings["allpaxServiceRecordEntities"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(mainconn);

            sqlconn.Open();

            string sqlquery1 =
                "SELECT tbl_dailyReport.dailyReportID, tbl_dailyReport.jobID, tbl_subJobTypes.description, tbl_dailyReport.date, " +
                "tbl_Jobs.customerContact,tbl_customers.customerName, tbl_customers.address, tbl_dailyReport.equipment, " +
                "tbl_dailyReport.startTime, tbl_dailyReport.endTime, tbl_dailyReport.lunchHours, tbl_customers.customerCode " +

                "FROM tbl_dailyReport " +

                "INNER JOIN " +
                "tbl_Jobs ON tbl_Jobs.jobID = tbl_dailyReport.jobID " +
                "INNER JOIN " +
                "tbl_customers ON tbl_customers.customerCode = tbl_Jobs.customerCode " +
                "INNER JOIN " +
                "tbl_jobSubJobs ON tbl_jobSubJobs.jobID = tbl_Jobs.jobID " +
                "INNER JOIN " +
                "tbl_subJobTypes ON tbl_subJobTypes.subJobID = tbl_jobSubJobs.subJobID " +

                "WHERE " +

                "tbl_dailyReport.subJobID = tbl_subJobTypes.subJobID " +
                "AND " +
                "tbl_dailyReport.dailyReportID LIKE @reportID";

            SqlCommand sqlcomm1 = new SqlCommand(sqlquery1, sqlconn);
            sqlcomm1.Parameters.AddWithValue("@reportID", reportID);
            SqlDataAdapter sda1 = new SqlDataAdapter(sqlcomm1);
            DataTable dt1 = new DataTable();
            sda1.Fill(dt1);
            foreach (DataRow dr1 in dt1.Rows)
            {
                vm_dailyReportByReportID vm_dailyReportByReportID = new vm_dailyReportByReportID();

                vm_dailyReportByReportID.dailyReportID = (int)dr1[0];
                vm_dailyReportByReportID.jobID = dr1[1].ToString();
                vm_dailyReportByReportID.description = dr1[2].ToString();
                vm_dailyReportByReportID.date = String.Format("{0:yyyy-MM-dd}", dr1[3]);
                vm_dailyReportByReportID.customerContact = dr1[4].ToString();
                vm_dailyReportByReportID.customerName = dr1[5].ToString();
                vm_dailyReportByReportID.address = dr1[6].ToString();
                vm_dailyReportByReportID.equipment = dr1[7].ToString();
                vm_dailyReportByReportID.startTime = dr1[8].ToString();
                vm_dailyReportByReportID.endTime = dr1[9].ToString();
                vm_dailyReportByReportID.lunchHours = (int)dr1[10];
                vm_dailyReportByReportID.customerCode = dr1[11].ToString();
                vm_dailyReportByReportID.names = namesByTimeEntryID(vm_dailyReportByReportID.dailyReportID);
                vm_dailyReportByReportID.shortNames = shortNamesByTimeEntryID(vm_dailyReportByReportID.dailyReportID);

                vm_dailyReportByReportID.jobCorrespondentName = jobCrspdtNameByJobID(vm_dailyReportByReportID.jobID);                
                vm_dailyReportByReportID.jobCorrespondentEmail = jobCorrespondentEmailByTimeEntryID(vm_dailyReportByReportID.jobID);

                recipientList = vm_dailyReportByReportID.jobCorrespondentEmail;

                dailyReportByID.Add(vm_dailyReportByReportID);
            }

            sqlconn.Close();
            
            string recipientListStr = string.Join(", ", recipientList);

            string html = ViewRenderer.RenderView("~/views/dailyReportByReportIDPrint/index.cshtml", dailyReportByID);

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("allpaxtesting@gmail.com", "Allpax_1234");

            string body = html;

            using (var message = new MailMessage("allpaxtesting@gmail.com", recipientListStr))
            {
                message.Subject = "Test";
                message.Body = body;
                message.IsBodyHtml = true;
                smtp.Send(message);
                }

                return View(dailyReportByID);
        }
        public List<string> namesByTimeEntryID(int dailyReportByID)
        {
            List<string> Names = new List<string>();

            string mainconn = ConfigurationManager.ConnectionStrings["allpaxServiceRecordEntities"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(mainconn);

            string sqlquery1 = "SELECT tbl_Users.name " +

            "FROM " +
            "tbl_Users " +

            "INNER JOIN " +
            "tbl_dailyReportUsers ON " +
            "tbl_Users.userName = tbl_dailyReportUsers.userName " +

            "WHERE " +
            "tbl_dailyReportUsers.dailyReportID = @dailyReportID";

            SqlCommand sqlcomm1 = new SqlCommand(sqlquery1, sqlconn);
            sqlcomm1.Parameters.Add(new SqlParameter("dailyReportID", dailyReportByID));
            SqlDataAdapter sda1 = new SqlDataAdapter(sqlcomm1);
            DataTable dt1 = new DataTable();
            sda1.Fill(dt1);
            foreach (DataRow dr1 in dt1.Rows)
            {
                Names.Add(dr1[0].ToString());
            }
            return Names;
        }
        public List<string> shortNamesByTimeEntryID(int dailyReportByID)
        {
            List<string> shortNames = new List<string>();

            string mainconn = ConfigurationManager.ConnectionStrings["allpaxServiceRecordEntities"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(mainconn);

            string sqlquery1 = "SELECT tbl_Users.shortName " +

            "FROM " +
            "tbl_Users " +

            "INNER JOIN " +
            "tbl_dailyReportUsers ON " +
            "tbl_Users.userName = tbl_dailyReportUsers.userName " +

            "WHERE " +
            "tbl_dailyReportUsers.dailyReportID = @dailyReportID";

            SqlCommand sqlcomm1 = new SqlCommand(sqlquery1, sqlconn);
            sqlcomm1.Parameters.Add(new SqlParameter("dailyReportID", dailyReportByID));
            SqlDataAdapter sda1 = new SqlDataAdapter(sqlcomm1);
            DataTable dt1 = new DataTable();
            sda1.Fill(dt1);
            foreach (DataRow dr1 in dt1.Rows)
            {
                shortNames.Add(dr1[0].ToString());
            }
            return shortNames;
        }
        public List<string> jobCorrespondentEmailByTimeEntryID(string jobID)
        {
            List<string> crspndtEmails = new List<string>();

            string mainconn = ConfigurationManager.ConnectionStrings["allpaxServiceRecordEntities"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(mainconn);

            string sqlquery1 = "SELECT tbl_jobCorrespondents.email " +

            "FROM " +
            "tbl_jobCorrespondents " +

            "INNER JOIN " +
            "tbl_Jobs ON " +
            "tbl_Jobs.jobID = tbl_jobCorrespondents.jobID " +

            "WHERE " +
            "tbl_jobCorrespondents.jobID = @jobID";

            SqlCommand sqlcomm1 = new SqlCommand(sqlquery1, sqlconn);
            sqlcomm1.Parameters.Add(new SqlParameter("jobID", jobID));
            SqlDataAdapter sda1 = new SqlDataAdapter(sqlcomm1);
            DataTable dt1 = new DataTable();
            sda1.Fill(dt1);
            foreach (DataRow dr1 in dt1.Rows)
            {
                crspndtEmails.Add(dr1[0].ToString());
            }
            return crspndtEmails;
        }

        public List<string> jobCrspdtNameByJobID(string jobID)
        {
            List<string> jobCrspdtNames = new List<string>();

            string mainconn = ConfigurationManager.ConnectionStrings["allpaxServiceRecordEntities"].ConnectionString;
            SqlConnection sqlconn = new SqlConnection(mainconn);

            string sqlquery1 = "SELECT tbl_jobCorrespondents.name " +

            "FROM " +
            "tbl_jobCorrespondents " +
            "WHERE " +
            "tbl_jobCorrespondents.jobID = @jobID";

            SqlCommand sqlcomm1 = new SqlCommand(sqlquery1, sqlconn);
            sqlcomm1.Parameters.Add(new SqlParameter("jobID", jobID));
            SqlDataAdapter sda3 = new SqlDataAdapter(sqlcomm1);
            DataTable dt1 = new DataTable();
            sda3.Fill(dt1);
            foreach (DataRow dr1 in dt1.Rows)
            {
                jobCrspdtNames.Add(dr1[0].ToString());
            }
            return jobCrspdtNames;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
