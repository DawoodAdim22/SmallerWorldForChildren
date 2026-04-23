using SmallerWorldForChildren.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SmallerWorldForChildren
{
    public partial class Contact : Page
    {
        private static readonly object LogFileLock = new object();
        protected void Page_Load(object sender, EventArgs e)
        {
            WriteLog("Page_Load", "Contact page loaded.");
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            WriteLog("Submit_Click", "Contact form submit initiated. Email: " + txtEmail.Text.Trim());
            string body = "Dear Admin,<br/><br/><table> <tbody> <tr> <td><strong>Customer Name:</strong> </td> <td>" + txtName.Text + "</td> </tr> <tr> <td><strong>Phone Number:</strong></td> <td>" + txtPhoneNo.Text + "</td> </tr> <tr> <td><strong>Email Id:</strong></td> <td>" + txtEmail.Text + "</td> </tr> <tr> <td><strong>Description:</strong></td> <td>" + txtDescription.Text + "</td> </tr> </tbody> </table><br/><br/>Thank You<br/>";
            //CommonFunction.SWSendMailTicket(txtEmail.Text.Trim(), "Contact us", txtDescription.Text , Session["dataPDF"].ToString(), Session["FilePath"].ToString());
            // string response=CommonFunction.MAilchimpSendMail(txtEmail.Text.Trim(), "Contact us", body);
            //string response = CommonFunction.SWSendMail(txtEmail.Text.Trim(), "Contact us", body);
            string response = CommonFunction.SWK_YourNotifyMail(txtEmail.Text.Trim(), "Contact Us", body, "", "");

            if (!string.IsNullOrWhiteSpace(response) &&
                (response.Contains("Status: OK") || response.Contains("\"status\":\"success\"")))
            {
                WriteLog("Submit_Success", "Contact form email sent successfully.");
                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "alertScript",
                    "swal('', 'Successfully sent', 'success', {button: 'Ok', closeOnClickOutside: false})",
                    true
                );
            }
            else
            {
                WriteLog("Submit_Failed", "Contact form email send failed. Response: " + response);
                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "alertScript",
                    "swal('', 'Something Failed', 'error', {button: 'Ok', closeOnClickOutside: false})",
                    true
                );
            }
            txtName.Text = "";
            txtPhoneNo.Text = "";
            txtEmail.Text = "";
            txtDescription.Text = "";
        }

        private void WriteLog(string action, string message, Exception ex = null)
        {
            try
            {
                string logDirectory = Server.MapPath("~/App_Data/Logs/");
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                string logPath = Path.Combine(logDirectory, "Contact-" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
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