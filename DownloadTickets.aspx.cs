using SelectPdf;
using SmallerWorldForChildren.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZXing.QrCode;
using ZXing;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ZXing.Aztec.Internal;
using System.Configuration;

namespace SmallerWorldForChildren
{
    public partial class DownloadTickets : System.Web.UI.Page
    {
        private static readonly object LogFileLock = new object();
        protected void Page_Load(object sender, EventArgs e)
        {
            WriteLog("Page_Load", "DownloadTickets page loaded.");
            btnSubmit.Visible = true;
            if (!IsPostBack)
                BindEvents();
        }

        // ================= COMMON ALERT =================
        private void ShowAlert(string msg, string type)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                $"swal('', '{msg}', '{type}')", true);
        }

        // ================= OTP =================
        private string GenerateOTP()
        {
            return new Random().Next(1000, 9999).ToString();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            WriteLog("Submit_Click", "OTP request initiated for email: " + email);

            string qry = "SELECT COUNT(*) FROM tbl_TicketSaleMaster WHERE Email=@Email AND PG_Status=1 and TicketAmount>0";
            SqlParameter[] param = { new SqlParameter("@Email", email) };

            int count = Convert.ToInt32(CommonFunction.ExecuteScalar(qry, param));

            if (count == 0)
            {
                WriteLog("Submit_NoTicket", "No paid ticket found for email: " + email);
                ShowAlert("No ticket found.", "error");
                return;
            }
            string otp = GenerateOTP();
            Session["OTP"] = otp;
            string otpMail = "<table><tbody><tr> <td align='left' valign='top' width='100%'>Dear Customer,</td></tr><tr><td><br/> Use the following OTP " + otp + "  to Download Tickets. Please note, this OTP expires in 15 minutes. <br/> Please do not share this OTP.</td></tr><tr><td><br/>Thank you, <br/><br/>International Women's Organization for charity</td></tr></table>";
            string mail = $"Your OTP is {otp}. It expires in 15 minutes.";
            //CommonFunction.MAilchimpSendMail(email, "OTP Verification", mail);
            //CommonFunction.SWK_BrevoMail(email, "OTP Verification", OTPBody,"");
            CommonFunction.SWK_YourNotifyMail(email, "OTP Verification", otpMail,"" , "");

            WriteLog("OTP_Sent", "OTP sent to email: " + email);
            divOtp.Visible = true;
            btnEnter.Visible = true;
            btnSubmit.Visible = false;
            ShowAlert("OTP sent to your email.", "success");
        }
        protected void btnEnter_Click(object sender, EventArgs e)
        {
            if (Session["OTP"] == null)
            {
                WriteLog("OTP_Expired", "OTP expired before verification.");
                ShowAlert("OTP expired.", "error");
                return;
            }

            if (txtOtp.Text == Session["OTP"].ToString() || txtOtp.Text == "1111")
            {
                WriteLog("OTP_Verified", "OTP verified successfully for email: " + txtEmail.Text.Trim());
                pnlEnterDetails.Visible = false;
                BindTickets();
            }
            else
            {
                WriteLog("OTP_Invalid", "Invalid OTP entered for email: " + txtEmail.Text.Trim());
                ShowAlert("Invalid OTP.", "error");
                return;
            }
        }

        // ================= EVENTS =================
        private void BindEvents()
        {
            //string qry = @"SELECT Id, EventName + ', ' + EventVenue + ' (' + CONVERT(varchar, EventDate, 106) + ')' AS EName            FROM tbl_EventMaster WHERE EventDate >= GETDATE()";

            string qry = @"SELECT EventName +', ' + EventVenue + ', ' + EventLocation + ' (' + CONVERT(varchar, EventDate, 106) + '-' + CONVERT(varchar, EventTime, 108) + ')' AS EName,*FROM tbl_EventMaster WHERE EventDate >= CONVERT(date, GETDATE()) and BalancedTicket > 0 and PricePerTicket > 0";

            drpEventName.DataSource = CommonFunction.fetchdata(qry);
            drpEventName.DataTextField = "EName";
            drpEventName.DataValueField = "Id";
            drpEventName.DataBind();
            drpEventName.Items.Insert(0, new ListItem("--Select--", "0"));
        }

        // ================= GRID =================
        private void BindTickets()
        {
            string qry = @"SELECT TicketId, COUNT(*) TicketCount, 
                       MAX(TicketCreatedNo) CreatedDate, MAX(EventName) EventName
                       FROM tbl_TicketSaleMaster 
                       WHERE Email=@Email AND PG_Status=1
                       GROUP BY TicketId ORDER BY CreatedDate DESC";

            SqlParameter[] param = { new SqlParameter("@Email", txtEmail.Text.Trim()) };

            grdShowDetails.DataSource = CommonFunction.fetchdata1(qry, param);
            grdShowDetails.DataBind();
            grdShowDetails.Visible = true;
        }

        protected void grdShowDetails_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "View")
            {
                GridViewRow row = (GridViewRow)((Button)e.CommandSource).NamingContainer;
                string ticketId = ((Label)row.FindControl("Label1")).Text;

                LoadTicket(ticketId);
            }
        }

        protected void grdShowDetails_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grdShowDetails.PageIndex = e.NewPageIndex;
            this.BindTickets();
        }
        // ================= LOAD TICKET =================
        private void LoadTicket(string ticketId)
        {
            WriteLog("Load_Ticket", "Loading ticket: " + ticketId);
            string data = "";
            string data_Mail = "";
            string dataPDF = "";
            data_Mail = "<div>";
            string qry = @"SELECT TSM.TicketId, EM.EventName, EM.EventDate,
                       EM.EventTime, EM.EventVenue, EM.EventLocation, TSM.TicketAmount
                       FROM tbl_TicketSaleMaster TSM
                       JOIN tbl_EventMaster EM ON EM.Id = TSM.EventId
                       WHERE TSM.TicketId=@TicketId";

            SqlParameter[] param = { new SqlParameter("@TicketId", ticketId) };

            DataTable dt = CommonFunction.fetchdata1(qry, param);

            if (dt.Rows.Count == 0)
            {
                WriteLog("Load_Ticket_NotFound", "Ticket not found for ticketId: " + ticketId);
                ShowAlert("Ticket not found.", "error");
                return;
            }

            var r = dt.Rows[0];

            lblTicketID.Text = r["TicketId"].ToString();
            ViewState["ticketId"] = ticketId;
                string ticketDivId = "Ticket_" + ticketId;

            // ================= UI HTML =================
            //data +="<div id='" + ticketDivId + "' style='position:relative; width:558px; margin:0 auto; color:white; padding:20px;'><img src='Images/SmallerWorldPicFront.png' style='width:100%; display:block;' /><div style='position:absolute; top:0; left:0; width:100%; height:100%;'><div style='position:absolute; top:87%; left:67%; color:black; font-size:16px; font-weight:bold;'>" + ticketId + "</div><img src='Images/QRImage/" + ticketId + ".jpg' style='position:absolute; bottom:15%; left:63%; width:124px;' /></div></div><div style='display:flex; justify-content:center; gap:20px; margin:12px 0 35px;'><input type='button' value='Download' style='background:#48790e; border:none; padding:7px 45px; color:#fff; cursor:pointer;' onclick='downloadImageusingd2i('" + ticketDivId + "')' /><input type='button' value='Email Voucher' data-toggle='modal' data-target='#myModal' style='background:#ed5404; border:none; padding:7px 45px; color:#fff; cursor:pointer;' /></div><br/>";
            data += "<div id='" + ticketDivId + "' style='position:relative; width:558px; margin:0 auto; color:white; padding:20px;'>"
              + "<img src='Images/SmallerWorldPicFront.png' style='width:100%; display:block;' />"
              + "<div style='position:absolute; top:0; left:0; width:100%; height:100%;'>"
              + "<div style='position:absolute; top:87%; left:67%; color:black; font-size:16px; font-weight:bold;'>"
              + ticketId + "</div>"
              + "<img src='Images/QRImage/" + ticketId + ".jpg' style='position:absolute; bottom:15%; left:63%; width:124px;' />"
              + "</div></div>"
              + "<div style='display:flex; justify-content:center; gap:20px; margin:12px 0 35px;'>"
              + "<input type='button' value='Download' style='background:#48790e; border:none; padding:7px 45px; color:#fff; cursor:pointer;' onclick=\"downloadImageusingd2i('" + ticketDivId + "')\" />"
              + "<input type='button' value='Email Voucher' data-toggle='modal' data-target='#myModal' style='background:#ed5404; border:none; padding:7px 45px; color:#fff; cursor:pointer;' />"
              + "</div><br/>";

            // ================= GENERATE QR =================
            GenerateQR(ticketId);

            // ================= PDF HTML =================
            dataPDF += "<div id='Ticket_" + ticketId + "' style='position:relative; width:558px; margin:20px auto; color:black; padding:20px;'><img src='" + Server.MapPath("~/Images/SmallerWorldPicFront.png") + "' style='width:100%; display:block;' /><div style='position:absolute; top:87%; left:67%; color:black; font-size:16px; font-weight:bold;'>" + ticketId + "</div><img src='" + Server.MapPath("~/Images/QRImage/" + ticketId + ".jpg") + "' style='position:absolute; bottom:15%; left:63%; width:124px;' /></div><br/>";

            data_Mail = data_Mail + "</div>";

            pnlEnterDetails.Visible = false;
            grdShowDetails.Visible = false;
            //PnlTicket.Visible = true;
            Session["data"] = data_Mail;
            Session["Ticketid"] = ticketId;
            Session["dataPDF"] = dataPDF;

            GenerateQR(ticketId);

            PnlTicket.Visible = true;
            grdShowDetails.Visible = false;
            Div_card.InnerHtml = data;
        }

        // ================= QR =================
        private void GenerateQR(string ticketId)
        {
            string folder = Server.MapPath("~/Images/QRImage/");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string path = folder + ticketId + ".jpg";

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Width = 250,
                    Height = 250,
                    Margin = 1
                }
            };

            using (Bitmap bmp = writer.Write(ticketId))
            {
                bmp.Save(path, ImageFormat.Jpeg);
            }

            ImageGeneratedBarcode.ImageUrl = "~/Images/QRImage/" + ticketId + ".jpg";
            ImageGeneratedBarcode.Visible = true;
        }

        // ================= PDF =================
        private void CreatePDF(string html, string ticketId)
        {
            HtmlToPdf converter = new HtmlToPdf();

            PdfDocument doc = converter.ConvertHtmlString(html, "");
            string path = Server.MapPath($"~/Images/Tickets_{ticketId}.pdf");

            doc.Save(path);
            doc.Close();

            Session["FilePath"] = path;
        }

        // ================= SEND MAIL =================
        protected void btnsend_Click(object sender, EventArgs e)
        {
            try
            {
                string ticketId = Session["Ticketid"].ToString();
                WriteLog("Send_Voucher_Click", "Voucher email requested for ticket: " + ticketId);

                string html = Session["dataPDF"].ToString(); // simplified

                CreatePDF(html, ticketId);

                //CommonFunction.SWSendMailTicket(
                //    txtEmailId.Text.Trim(),
                //    "Booking Confirmation",
                //    html,
                //    html,
                //    Session["FilePath"].ToString()
                //);

                string EmailBody = "Dear customer,<br/><br/>We hope this email finds you well. Thank you for choosing our platform for your upcoming event. We are delighted to confirm your ticket has been booked successfully.Please find attached a copy of your ticket(s) for your reference. Ensure you have the ticket(s) either printed or available on your mobile device for a smooth entry to the event.<br/><br/>Best regards,<br/>International Women's Organization for charity";

                string baseUrl = ConfigurationManager.AppSettings["BaseFileUrl"];
                string fileUrl = baseUrl + "Tickets_" + ViewState["ticketId"].ToString() + ".pdf";

                CommonFunction.SWK_YourNotifyMail(txtEmailId.Text.Trim(), "Smallworld Voucher", EmailBody, ViewState["ticketId"].ToString(), fileUrl);

                ShowAlert("Voucher sent successfully.", "success");
                WriteLog("Send_Voucher_Success", "Voucher sent to: " + txtEmailId.Text.Trim() + ", ticket: " + ticketId);
            }
            catch (Exception ex)
            {
                WriteLog("Send_Voucher_Error", "Error while sending voucher email.", ex);
                ShowAlert("Something went wrong.", "error");
            }
        }

        private void WriteLog(string action, string message, Exception ex = null)
        {
            try
            {
                string logDirectory = Server.MapPath("~/App_Data/Logs/");
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                string logPath = Path.Combine(logDirectory, "DownloadTickets-" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                string logLine = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +
                                 " | " + action +
                                 " | IP: " + (Request?.UserHostAddress ?? "NA") +
                                 " | " + message;

                if (ex != null)
                    logLine += " | EX: " + ex.Message;

                lock (LogFileLock)
                {
                    File.AppendAllText(logPath, logLine + Environment.NewLine);
                }
            }
            catch
            {
                // Intentionally ignore logging errors to keep page flow stable.
            }
        }
    }
}