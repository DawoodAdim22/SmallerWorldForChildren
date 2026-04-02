using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using SelectPdf;
using RestSharp;

namespace SmallerWorldForChildren.Classes
{
    public class CommonFunction
    {

        public static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["constring"].ToString();

        public static string MailApiKey = "";
        //public static string pathAPI = System.Configuration.ConfigurationManager.AppSettings["APILink"].ToString();

        // ================= FETCH DATA =================
        public static DataTable fetchdata1(string query, SqlParameter[] param = null)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                if (param != null)
                    cmd.Parameters.AddRange(param);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        // ================= EXECUTE SCALAR =================
        public static object ExecuteScalar(string query, SqlParameter[] param = null)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                if (param != null)
                    cmd.Parameters.AddRange(param);

                con.Open();
                return cmd.ExecuteScalar();
            }
        }
        public static DataTable fetchdata(string query)
        {
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand(query, con))


            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandTimeout = 0;
                adapter.Fill(dt);
            }
            // da.Fill(dt);
            con.Close();
            con.Dispose();
            return dt;
            /* Unmerged change from project '2_App_Code'
            Before:
                }
                public static int insertupdateordelete(string query)
            After:
                }
                public static int insertupdateordelete(string query)
            */
        }
        public static string ToUpperEveryWord(string s)
        {
            // Check for empty string.  
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            var words = s.Split(' ');

            var t = "";
            foreach (var word in words)
            {
                if (word != "")
                    t += char.ToUpper(word[0]) + word.Substring(1) + ' ';
            }
            return t.Trim();
        }

        public static string generateRefNo(string mobile)
        {
            DateTime baseDate = new DateTime(1970, 1, 1);
            TimeSpan diff = DateTime.Now - baseDate;
            string milliseconds = diff.TotalMilliseconds.ToString().Split('.')[0].ToString();
            return mobile + milliseconds;
        }

        public static string MAilchimpSendMail(string email, string subject, string body)
        {
            string senderUserName = ConfigurationManager.AppSettings["userName"].ToString();
            string senderPassword = ConfigurationManager.AppSettings["password"].ToString();
            string senderHost = ConfigurationManager.AppSettings["host"].ToString();
            int senderPort = Convert.ToInt16(ConfigurationManager.AppSettings["port"]);
            Boolean isSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["enableSsl"]);

            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(senderUserName, "International Women's Organization for charity");
                // mailMessage.From(emailId);
                mailMessage.To.Add(email);
                mailMessage.To.Add("azadgourav@gmail.com");
                //mailMessage.Bcc.Add("saurabhbansal2004@gmail.com,dadim@contecglobal.com,gazad@contecglobal.com");
                mailMessage.Bcc.Add("dadim@contecglobal.com");
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;


                SmtpClient smtpClient = new SmtpClient
                {
                    Host = senderHost,
                    Port = senderPort,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderUserName, senderPassword),
                    EnableSsl = true // Enable SSL for secure communication
                };

                // Send email
                smtpClient.Send(mailMessage);

                Console.WriteLine("Email sent successfully!");
                return "1";
            }
            catch (Exception ex)
            {
                CommonFunction.insertLogs(
            "MAILCHIMP_SMTP_ERROR",
            $"Host={senderHost}, User={senderUserName}, Msg={ex.Message}, Inner={ex.InnerException?.Message}",
            "MAIL"
        );
                Console.WriteLine("Error sending email: " + ex.Message);
                return "0";
            }

        }

        public static string SWSendMail(string emailId, string subject, string Data)
        {


            string senderUserName = ConfigurationManager.AppSettings["userName"].ToString();
            string senderPassword = ConfigurationManager.AppSettings["password"].ToString();
            string senderHost = ConfigurationManager.AppSettings["host"].ToString();
            int senderPort = Convert.ToInt16(ConfigurationManager.AppSettings["port"]);
            Boolean isSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["enableSsl"]);

            //MailMessage mailMessage = new MailMessage();
            //mailMessage.From = new MailAddress(senderUserName, emailId);
            //mailMessage.To.Add(emailId);
            //mailMessage.To.Add("ticketing@smallworldnigeria.org");
            //mailMessage.Bcc.Add("rchugh@contecglobal.com");
            //mailMessage.Subject = subject;
            //mailMessage.Body = Data;
            //mailMessage.IsBodyHtml = true;
            //SmtpClient smtpClient = new SmtpClient();
            //smtpClient.Host = senderHost;
            //smtpClient.Port = senderPort;
            //smtpClient.UseDefaultCredentials = false;
            //smtpClient.Credentials = new NetworkCredential(senderUserName, senderPassword);
            //smtpClient.EnableSsl = true;
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(senderUserName, "International Women's Organization for charity");
            mailMessage.To.Add(emailId);
            mailMessage.Bcc.Add("dadim@contecglobal.com");
            mailMessage.Subject = subject;
            mailMessage.Body = Data;
            mailMessage.IsBodyHtml = true;
            //mailMessage.Attachments.Add(new Attachment(new MemoryStream(bytes), "GridViewPDF.pdf"));
            // mailMessage.Attachments.Add(new Attachment("E:/Tickets.pdf"));
            //mailMessage.Attachments.Add(new Attachment(FilePath));
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = senderHost;
            smtpClient.Port = senderPort;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(senderUserName, senderPassword);
            smtpClient.EnableSsl = true;

            try
            {
                smtpClient.Send(mailMessage);
                return "1";
            }
            catch (Exception ex1)
            {
                string msg = ex1.Message;
                return "0";
            }

        }
        //public static string SWK_YourNotifyMail(string emailId, string subject, string Data, string FilePath)
        //{
        //    var options = new RestClientOptions("https://api.yournotify.com")
        //    {
        //        MaxTimeout = -1,
        //    };

        //    var client = new RestClient(options);
        //    var request = new RestRequest("/campaigns/test", Method.Post);

        //    string token = ConfigurationManager.AppSettings["YourNotifyToken"];
        //    request.AddHeader("Authorization", "Bearer " + token);

        //    var safeHtml = Data
        //        .Replace("\\", "\\\\")
        //        .Replace("\"", "\\\"")
        //        .Replace("\r", "")
        //        .Replace("\n", "");

        //                var body = $@"{{
        //        ""channel"": ""email"",
        //        ""email"": ""{emailId}"",
        //        ""subject"": ""{subject}"",
        //        ""from"": ""ticketing@iwocnigeria.org"",
        //        ""html"": ""{safeHtml}"",
        //        ""text"": ""Sandbox delivery check"",
        //        ""media"": [
        //            93,
        //            {{
        //                ""filename"": ""dotnet.pdf"",
        //                ""url"": ""{FilePath}""
        //            }}
        //        ],
        //        ""sandbox"": true
        //    }}";
        //    request.AddJsonBody(body);
        //    Response.Write("<pre>" + body + "</pre>");
        //    Response.End();
        //    //RestResponse response = client.Execute(request);
        //    return response.Content;
        //}
        public static string SWK_YourNotifyMail(string emailId, string subject, string Data, string FileName, string FilePath)
        {
            try
            {
                var options = new RestClientOptions("https://api.yournotify.com")
                {
                    MaxTimeout = -1,
                };

                var client = new RestClient(options);
                var request = new RestRequest("/campaigns/test", Method.Post);

                string token = ConfigurationManager.AppSettings["YourNotifyToken"];
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddHeader("Accept", "application/json");

                // ✅ Escape HTML
                var safeHtml = Data
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\r", "")
                    .Replace("\n", "");

                // ✅ Proper JSON string
                var body = $@"{{
                    ""channel"": ""email"",
                    ""email"": ""{emailId}"",
                    ""subject"": ""{subject}"",
                    ""from"": ""ticketing@iwocnigeria.org"",
                    ""html"": ""{safeHtml}"",
                    ""text"": ""Sandbox delivery check"",
                    ""media"": [
                        93,
                        {{
                            ""filename"": ""{FileName}.pdf"",
                            ""url"": ""{FilePath}""
                        }}
                    ],
                    ""sandbox"": true
                }}";

                // ✅ IMPORTANT: use AddParameter instead of AddJsonBody
                request.AddParameter("application/json", body, ParameterType.RequestBody);

                // ✅ Execute request
                RestResponse response = client.Execute(request);

                // ✅ Return full debug info
                return $"Status: {response.StatusCode}\nResponse: {response.Content}\n\nRequest:\n{body}";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
        public static string SWK_BrevoMail(string emailId, string subject, string Data, string FilePath)
        {
            try
            {
                var options = new RestClientOptions("https://api.brevo.com")
                {
                    MaxTimeout = -1,
                };

                var client = new RestClient(options);
                var request = new RestRequest("/v3/smtp/email", Method.Post);

                request.AddHeader("Accept", "application/json");
                request.AddHeader("api-key", MailApiKey);
                request.AddHeader("Content-Type", "application/json");

                // 🔹 Convert PDF to Base64
                string filePath = FilePath; // change path
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                string base64File = Convert.ToBase64String(fileBytes);

                var body = new
                {
                    sender = new
                    {
                        name = "SmallWorld, Nigeria",
                        email = "DawoodAdim2612@gmail.com"
                    },
                    to = new[]
                    {
                        new
                        {
                            email = emailId,
                            name = "Daud"
                        }
                    },

                    // ✅ BCC added
                    bcc = new[]
                    {
                        new
                        {
                            email = "dawoodadim184@gmail.com",
                            name = "Person One"
                        },
                        new
                        {
                            email = "NishatAnjum482@gmail.com",
                            name = "Person Two"
                        }
                    },

                    subject = subject,

                    htmlContent = Data,

                    attachment = new[]
                    {
                        new
                        {
                            //content = base64File,
                            url = filePath,
                            name = "SWK_Ticket.pdf"
                        }
                    }   
                };

                request.AddJsonBody(body);

                RestResponse response = client.Execute(request);

                return response.Content;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static string SWSendMailTicket(string emailId, string subject, string Data, string dataPDF, string FilePath)
        {

            string htmlString = dataPDF;
            string baseUrl = "";

            string pdf_page_size = "A4";
            PdfPageSize pageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize),
                pdf_page_size, true);

            string pdf_orientation = "Portrait";
            PdfPageOrientation pdfOrientation =
                (PdfPageOrientation)Enum.Parse(typeof(PdfPageOrientation),
                pdf_orientation, true);

            int webPageWidth = 1024;
            try
            {
                webPageWidth = Convert.ToInt32(1024);
            }
            catch { }

            int webPageHeight = 0;
            try
            {
                webPageHeight = Convert.ToInt32(0);
            }
            catch { }

            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();

            // set converter options
            converter.Options.PdfPageSize = pageSize;
            converter.Options.PdfPageOrientation = pdfOrientation;
            converter.Options.WebPageWidth = webPageWidth;
            converter.Options.WebPageHeight = webPageHeight;

            // create a new pdf document converting an url
            SelectPdf.PdfDocument doc = converter.ConvertHtmlString(htmlString, baseUrl);


            {
                {


                    using (var memoryStream = new MemoryStream())
                    {
                        memoryStream.Flush();
                        memoryStream.Position = 0;
                        //PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                        //pdfDoc.Open();
                        //htmlparser.Parse(sr);
                        //pdfDoc.Close();
                        byte[] bytes = memoryStream.ToArray();
                        memoryStream.Close();


                        Data = "Dear customer,<br/><br/>We hope this email finds you well. Thank you for choosing our platform for your upcoming event. We are delighted to confirm your ticket has been booked successfully.Please find attached a copy of your ticket(s) for your reference. Ensure you have the ticket(s) either printed or available on your mobile device for a smooth entry to the event.<br/><br/>Best regards,<br/>International Women's Organization for charity";

                        Data = "DECLARE @LOGO_LEFT NVARCHAR(MAX) = 'https://i.ibb.co/cKNZgNT2/logo2.png';DECLARE @LOGO_RIGHT NVARCHAR(MAX) = 'https://ci3.googleusercontent.com/meips/ADKq_NYA2CjOD3S1q5FrtLcccWkksb9tW_GOBSg2S32Q3cR-yLTEeP8FZ5zaL7dpw8usXz_cs29q=s0-d-e1-ft#https://lis.gov.lr/img/logo.png';SET @body =N'<!DOCTYPE html><html><head><meta charset=UTF-8><title>Resident Permit Application Notification</title><style>  body { font-family: Arial, sans-serif; background-color: #f5f7fa; margin: 0; padding: 0; }  .container { max-width: 700px; margin: 40px auto; background: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); padding: 32px; }  .header { text-align: center; border-bottom: 2px solid #e5e5e5; padding-bottom: 12px; margin-bottom: 20px; }  .header-table { width: 100%; border-collapse: collapse; }  .header-table td { vertical-align: middle; }  .header-title { color: #205078; font-size: 22px; font-weight: 700; text-align: center; }  .logo-left { text-align: left; width: 80px; }  .logo-right { text-align: right; width: 80px; }  .details-box { background: #f8fafd; border: 1px solid #d9e4f2; border-radius: 5px; padding: 14px 18px; margin: 15px 0; }  .details-table { width: 100%; border-collapse: collapse; }  .details-table td { padding: 6px 0; font-size: 15px; }  .details-table .label { font-weight: bold; color: #205078; width: 140px; } .rejection-box {    background: #fdeaea;    border-left: 4px solid #d32f2f;    border-radius: 5px;    padding: 16px 20px;    margin: 24px 0;    color: #000000;  }  .rejection-title {    color: #d32f2f;    font-weight: bold;    font-size: 16px;    margin-bottom: 8px;  }  .footer { color: #888; font-size: 0.9em; border-top: 1px solid #ddd; margin-top: 25px; padding-top: 12px; text-align: center; }</style></head><body><div class=container>  <div class='header'><table class=header-table>      <tr>        <td class=logo-left><img src=' + @LOGO_LEFT + ' alt=Logo width=70></td>        <td class=header-title>SMALLER WORLD</td>        <td class=logo-right><img src=' + @LOGO_RIGHT + ' alt=Logo width=70></td>      </tr>    </table>  </div>  <p><strong>Dear ' + @Applicant + ',</strong></p>  <p style=color:#444;>    We hope this email finds you well. Thank you for choosing our platform for your upcoming event. We are delighted to confirm your ticket has been booked successfully.Please find attached a copy of your ticket(s) for your reference. Ensure you have the ticket(s) either printed or available on your mobile device for a smooth entry to the event.<br><br>    Please review the details below.  </p>  <div class=details-box>    <table class=details-table style=width:100%>      <tr><td class=label colspan=2>Customer Details<hr/></td></tr><tr><td class=label>Name:</td>        <td>' + @AppID + '</td></tr><tr><td class=label>Mobile Number:</td><td>' + @AppID + '</td></tr><tr><td class=label>Email ID:</td><td>' + @AppID + '</td></tr><tr><td class=label>Event Details:</td>        <td>Smaller World, 26th April 2026, at Lagos Oriental Hotel, 3PM.</td></tr></table></div><div class=footer> &copy; 2026 SmallerWorld. All rights reserved.  </div></div></body></html>";


                        string senderUserName = ConfigurationManager.AppSettings["userName"].ToString();
                        string senderPassword = ConfigurationManager.AppSettings["password"].ToString();
                        string senderHost = ConfigurationManager.AppSettings["host"].ToString();
                        int senderPort = Convert.ToInt16(ConfigurationManager.AppSettings["port"]);
                        Boolean isSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["enableSsl"]);

                        MailMessage mailMessage = new MailMessage();
                        mailMessage.From = new MailAddress(senderUserName, "International Women's Organization for charity");
                        mailMessage.To.Add(emailId);
                        mailMessage.Bcc.Add("dadim@contecglobal.com");
                        mailMessage.Subject = subject;
                        mailMessage.Body = Data;
                        mailMessage.IsBodyHtml = true;
                        //mailMessage.Attachments.Add(new Attachment(new MemoryStream(bytes), "GridViewPDF.pdf"));
                        // mailMessage.Attachments.Add(new Attachment("E:/Tickets.pdf"));
                        mailMessage.Attachments.Add(new Attachment(FilePath));
                        SmtpClient smtpClient = new SmtpClient();
                        smtpClient.Host = senderHost;
                        smtpClient.Port = senderPort;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential(senderUserName, senderPassword);
                        smtpClient.EnableSsl = true;

                        try
                        {
                            smtpClient.Send(mailMessage);
                            return "1";
                        }
                        catch (Exception ex1)
                        {
                            string msg = ex1.Message;
                            return "0";
                        }
                    }
                }
            }

            //smtpClient.Send(mailMessage);
            //return "1";
        }



        public static string RenderTicketHtml(TicketData ticket)
        {
            string eventDate = ticket.EventDate.ToString("dddd, dd MMMM yyyy");
            string gateOpens = CalculateGateOpens(ticket.EventTime); // Optional helper for time calc
            string showStarts = CalculateShowStarts(ticket.EventTime);
            string gateCloses = CalculateGateCloses(ticket.EventTime);

            // You can further refine layout and styles as needed
            return $@"
<div style='display: flex; flex-direction: column; align-items: center; width: 800px; margin: 0 auto; color: white; padding: 20px; background-color: #222;'>
    <img src='{ticket.BarcodeUrl}' alt='Barcode' style='width:60px; margin-bottom:10px;'/>
    <h2 style='margin: 0; font-family: Georgia;'>{ticket.EventName}</h2>
    <div style='font-size: 1em; margin-bottom: 8px;'>{eventDate} ({ticket.EventTime})</div>
    <div style='margin-bottom:8px;'>{ticket.EventVenue} - {ticket.EventLocation}</div>
    <div>Ticket ID: <b>{ticket.TicketId}</b></div>
    <div>Amount: ₦{ticket.TicketAmount:N0}</div>
    <div style='margin-top:12px; font-size:0.9em;'>Gate opens: {gateOpens} | Show starts: {showStarts} | Gate closes: {gateCloses}</div>
</div>
";
        }

        // Optionally, create helpers to calculate gate times
        private static string CalculateGateOpens(string eventTime) => eventTime; // Dummy example, add real logic
        private static string CalculateShowStarts(string eventTime) => eventTime;
        private static string CalculateGateCloses(string eventTime) => eventTime;


        public class TicketData
        {
            public string TicketId { get; set; }
            public string EventName { get; set; }
            public DateTime EventDate { get; set; }
            public string EventVenue { get; set; }
            public string EventLocation { get; set; }
            public string EventTime { get; set; }
            public decimal TicketAmount { get; set; }
            public string BarcodeUrl { get; set; } // Path or URL to generated QR/barcode image
                                                   // Add other properties as needed
        }






        public static void insertLogs(string mobileno, string logdesc, string logName)
        {
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("USP_INSERT_LOG", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@logDesc", logdesc);
            cmd.Parameters.AddWithValue("@mobileno", mobileno);
            cmd.Parameters.AddWithValue("@logName", logName);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

        }

        public static void insertNOWNOWPayment(string mobileno, string amount, string servicecharge, string plateformtype, string systxnid, string apibody, string pgtxnid, string pgURLResp, string email, string url)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            try
            {
                cmd.Connection = conn;
                cmd.CommandText = "USP_INSERT_NOWNOW_PAYMENTS";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@mobileno", mobileno);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@servicecharge", servicecharge);
                cmd.Parameters.AddWithValue("@platformType", plateformtype);
                cmd.Parameters.AddWithValue("@systemTxnId", systxnid);
                cmd.Parameters.AddWithValue("@pgURLReq", apibody);
                cmd.Parameters.AddWithValue("@pgTxnId", pgtxnid);
                cmd.Parameters.AddWithValue("@pgURLResp", pgURLResp);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@url", url);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                cmd.Connection.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}