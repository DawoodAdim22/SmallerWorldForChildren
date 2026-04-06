using SelectPdf;
using SmallerWorldForChildren.Classes;
using SmallProject;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZXing;
using ZXing.QrCode;
using System.Web.Util;
using System.Configuration;

namespace SmallerWorldForChildren
{
    public partial class PaymentStatus : System.Web.UI.Page
    {
        private static readonly object LogFileLock = new object();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    WriteLog("Page_Load", "PaymentStatus page loaded.");
                    string requestId = Request.QueryString["paymentReference"]
                                    ?? Request.QueryString["id"]
                                    ?? Request.QueryString["requestId"];

                    if (string.IsNullOrEmpty(requestId))
                    {
                        WriteLog("Missing_RequestId", "No payment reference found in query string.");
                        return;
                    }
                    WriteLog("Request_Received", "Payment reference received: " + requestId);

                    string qry = @"Select MobileNo,systemTxnId as pgTxnId from tblCoralPayPayments where systemTxnId ='" + requestId + @"' union select MobileNo,pgTxnId from tblOPAYPaymentDetails where systemTxnId='" + requestId + "'";

                    DataTable dt = CommonFunction.fetchdata(qry);

                    if (dt.Rows.Count == 0)
                    {
                        WriteLog("Request_NotFound", "No transaction mapping found for request ID: " + requestId);
                        return;
                    }

                    string transId = dt.Rows[0]["pgTxnId"].ToString();
                    string phone = dt.Rows[0]["MobileNo"].ToString();

                    string result = "";
                    string gateway = "";

                    if (requestId.Contains("CPAY"))
                    {
                        result = coralpaypayment.checkTxnStatusfromcoralpay(transId);
                        gateway = "CPAY";
                    }
                    else if (requestId.Contains("OPAY"))
                    {
                        result = opay.opayTransactionStatus(transId, phone);
                        gateway = "OPAY";
                    }
                    else if (requestId.Contains("MNFY"))
                    {
                        result = monnify.checkTxnStatusfromMonnify(transId);
                        gateway = "MNFY";
                    }

                    if (result == "SUCCESS" || result == "00" || result == "PAID")
                    {
                        WriteLog("Payment_Success", "Payment success from " + gateway + " for request ID: " + requestId);
                        PrintTicket(requestId);
                        UpdatePgStatus(requestId, "SUCCESS", gateway);
                    }
                    else
                    {
                        WriteLog("Payment_Failed", "Payment failed from " + gateway + " for request ID: " + requestId + ", response: " + result);
                        lblmsg.Visible = true;
                        lblmsg.Text = "Your Payment was unsuccessful.";
                        lblmsg.ForeColor = Color.Red;
                        UpdatePgStatus(requestId, "FAILED", gateway);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("Page_Load_Error", "Unhandled error in PaymentStatus page load.", ex);
                    ShowAlert("Something went wrong.", "error");
                }
            }
        }

        public int UpdatePgStatus(string requestId, string status, string gateway)
        {
            try
            {
                WriteLog("Update_PG_Status", "Updating PG status. Request ID: " + requestId + ", Status: " + status + ", Gateway: " + gateway);
                int pgStatus = status == "SUCCESS" ? 1 : -1;

                using (SqlConnection conn = new SqlConnection(CommonFunction.connectionString))
                {
                    SqlCommand cmd = new SqlCommand("usp_UpdateTicketBooking", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@pgStatus", pgStatus);
                    cmd.Parameters.AddWithValue("@transactionid", requestId);
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@gateway", gateway);

                    cmd.Parameters.Add("@SuccessId", SqlDbType.Int).Direction = ParameterDirection.Output;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    return Convert.ToInt32(cmd.Parameters["@SuccessId"].Value);
                }
            }
            catch (Exception ex)
            {
                WriteLog("Update_PG_Status_Error", "Error while updating payment status.", ex);
                ShowAlert("Something went wrong.", "error");
                return 0;
            }
        }
        void ShowAlert(string msg, string type)
        {
            string safeMsg = HttpUtility.JavaScriptStringEncode(msg ?? string.Empty);
            string safeType = HttpUtility.JavaScriptStringEncode(type ?? "error");
            string script = $"swal('', '{safeMsg}', '{safeType}', {{button:'Ok',closeOnClickOutside:false}});";

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", script, true);
            }
            else
            {
                ClientScript.RegisterStartupScript(GetType(), "alert", script, true);
            }
        }
        public void PrintTicket(string transactionId)
        {
            try
            {
                WriteLog("Print_Ticket", "Print ticket initiated for transaction: " + transactionId);

                string qry = @"Select TicketId From tbl_TicketSaleMaster 
                            Where PG_TransactionId IN 
                            (select SUBSTRING(pgtxnid,5,LEN(pgtxnid)) from tblOPAYPaymentDetails Where pgTxnId='" + transactionId + @"')
                            union 
                            Select TicketId From tbl_TicketSaleMaster 
                            Where PG_TransactionId IN 
                            (select SUBSTRING(systemTxnId,5,LEN(systemTxnId)) from tblCoralPayPayments Where systemTxnId='" + transactionId + "')";

                DataTable dt = CommonFunction.fetchdata(qry);

                if (dt.Rows.Count == 0)
                {
                    WriteLog("Print_Ticket_NotFound", "No ticket found for transaction: " + transactionId);
                    return;
                }

                string ticketId = dt.Rows[0]["TicketId"].ToString();
                BindFields(transactionId);
            }
            catch (Exception ex)
            {
                WriteLog("Print_Ticket_Error", "Error while printing ticket.", ex);
                ShowAlert("Something went wrong.", "error");
            }
            
        }

        public void BindFields(string txnId)
        {
            try
            {
                WriteLog("Bind_Fields", "Binding ticket fields for transaction: " + txnId);

                string id = txnId.Substring(4);

                StringBuilder html = new StringBuilder();
                StringBuilder pdfHtml = new StringBuilder();

                string qry = @"select TSM.TicketCount,EM.EventName,EM.EventDate,
                        TSM.TicketId,EM.EventTime,EM.EventVenue,
                        EM.EventLocation,TSM.TicketAmount
                        from tbl_EventMaster EM
                        INNER JOIN tbl_TicketSaleMaster TSM on em.ID=TSM.EventId
                        where TSM.PG_transactionId='" + id + "'";

                DataTable dt = CommonFunction.fetchdata(qry);

                if (dt.Rows.Count == 0)
                {
                    WriteLog("Bind_Fields_NoData", "No ticket rows found for transaction: " + txnId);
                    return;
                }

                string firstTicketId = dt.Rows[0]["TicketId"].ToString();
                ViewState["firstTicketId"] = firstTicketId;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string currentTicketId = dt.Rows[i]["TicketId"].ToString();

                    GenerateCode(currentTicketId);

                    html.Append($@"
                        <div id='Ticket_{i}' style='text-align:center;margin:20px auto 10px;'>
                            <div style='position:relative;display:inline-block;width:558px;line-height:0;'>
                                <img src='{ResolveUrl("~/Images/SmallerWorldPicFront.png")}' style='width:558px;display:block'/>
                                <img src='{ResolveUrl("~/Images/QRImage/" + currentTicketId + ".jpg")}' style='position:absolute;right:44px;bottom:58px;width:125px;height:125px;z-index:2;'/>
                                <div style='position:absolute;right:36px;bottom:18px;width:141px;text-align:center;color:#000;font-size:22px;font-weight:700;font-family:Arial, Helvetica, sans-serif;letter-spacing:0.4px;line-height:1;white-space:nowrap;z-index:2;'>
                                    {currentTicketId}
                                </div>
                            </div>
                        </div>
                        <div style='display:flex;justify-content:center;gap:24px;margin-bottom:35px;'>
                            <input type='button'
                                style='background-color:#48790e;border-width:0px;padding:7px 45px;color:#ffffff;cursor:pointer;font-family:Poppins, sans-serif;'
                                value='Download'
                                onclick='downloadImageusingd2i(""Ticket_{i}"")' />
                            <input type='button'
                                value='Email Voucher'
                                data-toggle='modal'
                                data-target='#myModal'
                                style='background-color:#ed5404;border-width:0px;padding:7px 45px;color:#ffffff;cursor:pointer;font-family:Poppins, sans-serif;' />
                        </div>
                        <br/>
                    ");
                    // for QRImage style = 'position:relative;top:-112px;right:11px;width:56px;'
                    // for ticketId style='position:relative;top:-37px;right:82px;font-size:16px;font-weight:bold;Color:white;'
                    // for button div style='display:flex;justify-content:center;margin-top:-10px;margin-bottom:35px;'
                    pdfHtml.Append($@"
                        <div style='page-break-inside:avoid;text-align:center;margin-bottom:16px;'>
                            <div style='position:relative;display:inline-block;width:558px;line-height:0;'>
                                <img src='{Server.MapPath("~/Images/SmallerWorldPicFront.png")}' style='width:558px;display:block;'/>
                                <img src='{Server.MapPath("~/Images/QRImage/" + currentTicketId + ".jpg")}' style='position:absolute;right:44px;bottom:58px;width:125px;height:125px;z-index:2;'/>
                                <div style='position:absolute;right:36px;bottom:18px;width:141px;text-align:center;color:#000;font-size:22px;font-weight:700;font-family:Arial, Helvetica, sans-serif;letter-spacing:0.4px;line-height:1;white-space:nowrap;z-index:2;'>
                                    {currentTicketId}
                                </div>
                            </div>
                        </div>
                    ");
                    //for ticketId style='font-size:16px;font-weight:bold'
                }

                Div_card.InnerHtml = html.ToString();

                PnlTicket.Visible = true;

                CreatePDF(pdfHtml.ToString(), firstTicketId);


                Session["dataPDF"] = pdfHtml.ToString();
                string EmailBody = "Dear customer,<br/><br/>We hope this email finds you well. Thank you for choosing our platform for your upcoming event. We are delighted to confirm your ticket has been booked successfully.Please find attached a copy of your ticket(s) for your reference. Ensure you have the ticket(s) either printed or available on your mobile device for a smooth entry to the event.<br/><br/>Best regards,<br/>International Women's Organization for charity";
                //CommonFunction.SWSendMailTicket("dadim@contecglobal.com","Booking Confirmation","",                 pdfHtml.ToString(),Server.MapPath("~/Images/Tickets_" + firstTicketId + ".pdf"));
                string baseUrl = ConfigurationManager.AppSettings["BaseFileUrl"];
                string fileUrl = baseUrl + "Tickets_" + firstTicketId + ".pdf";

                CommonFunction.SWK_YourNotifyMail(Session["Email"].ToString(), "Booking Confirmation", EmailBody, firstTicketId, fileUrl );
                ShowAlert("Your event tickets are booked. Transaction Id is " + id, "success");
                WriteLog("Bind_Fields_Success", "Ticket rendering and email completed for transaction: " + txnId);
            }
            catch (Exception ex)
            {
                WriteLog("Bind_Fields_Error", "Error while binding ticket fields.", ex);
                ShowAlert("Something went wrong!", "error");
            }
        }

        public void CreatePDF(string html, string ticketId)
        {
            HtmlToPdf converter = new HtmlToPdf();
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            string path = Server.MapPath("~/Images/Tickets_" + ticketId + ".pdf");

            PdfDocument doc = converter.ConvertHtmlString(html);
            doc.Save(path);
            doc.Close();

            Session["FilePath"] = path;
        }

        private void GenerateCode(string ticketId)
        {
            string folder = Server.MapPath("~/Images/QRImage/");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string path = folder + ticketId + ".jpg";

            BarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Width = 250,
                    Height = 250,
                    Margin = 1
                }
            };

            Bitmap bmp = new Bitmap(writer.Write(ticketId));
            bmp.Save(path, ImageFormat.Jpeg);
        }

        protected void btnsend_Click(object sender, EventArgs e)
        {
            try
            {
                WriteLog("Manual_Send_Click", "Manual voucher email requested.");
                //CommonFunction.SWSendMailTicket(
                //    txtEmailId.Text.Trim(),
                //    "Booking Confirmation",
                //    "",
                //    Session["dataPDF"].ToString(),
                //    Session["FilePath"].ToString());

                string EmailBody = "Dear customer,<br/><br/>We hope this email finds you well. Thank you for choosing our platform for your upcoming event. We are delighted to confirm your ticket has been booked successfully.Please find attached a copy of your ticket(s) for your reference. Ensure you have the ticket(s) either printed or available on your mobile device for a smooth entry to the event.<br/><br/>Best regards,<br/>International Women's Organization for charity";

                string baseUrl = ConfigurationManager.AppSettings["BaseFileUrl"];
                string fileUrl = baseUrl + "Tickets_" + ViewState["firstTicketId"].ToString() + ".pdf";

                CommonFunction.SWK_YourNotifyMail(txtEmailId.Text.Trim(), "Smallworld Vouchers", EmailBody, ViewState["firstTicketId"].ToString(), fileUrl);

                ScriptManager.RegisterStartupScript(this, GetType(), "ok",
                "swal('', 'Your voucher has been sent to your email.', 'success');", true);
                WriteLog("Manual_Send_Success", "Manual voucher sent to: " + txtEmailId.Text.Trim());
            }
            catch (Exception ex)
            {
                WriteLog("Manual_Send_Error", "Error while sending manual voucher.", ex);
                ScriptManager.RegisterStartupScript(this, GetType(), "err",
                "swal('', 'Something went wrong.', 'error');", true);
            }
        }

        private void WriteLog(string action, string message, Exception ex = null)
        {
            try
            {
                string logDirectory = Server.MapPath("~/App_Data/Logs/");
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                string logPath = Path.Combine(logDirectory, "PaymentStatus-" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
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