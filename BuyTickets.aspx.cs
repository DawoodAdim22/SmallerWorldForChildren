using Newtonsoft.Json.Linq;
using SmallerWorldForChildren.Classes;
using SmallProject;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SmallerWorldForChildren
{
    public partial class BuyTickets : System.Web.UI.Page
    {
        private static readonly object LogFileLock = new object();
        string callbackURL = System.Configuration.ConfigurationManager.AppSettings["callbackURL"];

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                WriteLog("Page_Load", "BuyTickets page loaded.");
                if (!IsPostBack)
                {
                    lblPricePerticket.Text = "0";
                    lblBalancedTicket.Text = "0";
                    pnlEnterDetails.Visible = false;
                    BindDrpEventName();
                    BindNationality();
                    BindPaymentGateways();
                }
            }
            catch (Exception ex)
            {
                WriteLog("Page Load_Error", "Error while loading event details.", ex);
                ShowAlert("Something went wrong!!", "error");
            }
        }

        protected void drpEventName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                WriteLog("Event_Selected", "Event dropdown changed. Selected value: " + drpEventName.SelectedValue);
                if (drpEventName.SelectedValue != "0" && !string.IsNullOrEmpty(drpEventName.SelectedValue))
                {
                    string eventId = drpEventName.SelectedValue;
                    string qry = @"SELECT EventName, EventVenue, EventLocation, EventDate, EventTime, 
                                BalancedTicket, PricePerTicket, Id 
                                FROM tbl_EventMaster 
                                WHERE Id = @EventId";

                    using (SqlConnection conn = new SqlConnection(CommonFunction.connectionString))
                    {
                        SqlCommand cmd = new SqlCommand(qry, conn);
                        cmd.Parameters.AddWithValue("@EventId", eventId);
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            Session["EventId"] = reader["Id"].ToString();
                            Session["EventName"] = reader["EventName"].ToString();
                            Session["PricePerticket"] = Convert.ToInt32(reader["PricePerTicket"]);
                            Session["BalancedTicket"] = Convert.ToInt32(reader["BalancedTicket"]);

                            lblPricePerticket.Text = "₦" + Convert.ToInt32(reader["PricePerTicket"]).ToString("N0");
                            lblBalancedTicket.Text = reader["BalancedTicket"].ToString();

                            pnlEnterDetails.Visible = true;
                        }
                        reader.Close();
                        conn.Close();
                    }
                }
                else
                {
                    lblPricePerticket.Text = "0";
                    lblBalancedTicket.Text = "0";
                    pnlEnterDetails.Visible = false;
                    Session["EventId"] = null;
                    Session["EventName"] = null;
                    Session["PricePerticket"] = null;
                    Session["BalancedTicket"] = null;
                }
            }
            catch (Exception ex)
            {
                WriteLog("Event_Selected_Error", "Error while loading event details.", ex);
                ShowAlert("Error loading event details:", "error");
            }
        } 
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                WriteLog("Submit_Click", "Ticket booking submit initiated.");
                if (string.IsNullOrWhiteSpace(txtName.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtPhoneNo.Text))
                {
                    ShowAlert("Please Enter Complete Details", "warning");
                    return;
                }

                if (drpEventName.SelectedValue == "0" || string.IsNullOrEmpty(drpEventName.SelectedValue))
                {
                    ShowAlert("Please select an event", "warning");
                    return;
                }

                if (Session["PricePerticket"] == null || Session["BalancedTicket"] == null || Session["EventId"] == null)
                {
                    ShowAlert("Please select an event first", "warning");
                    return;
                }

                int tickets = Convert.ToInt32(txtNoOfTickets.Text);
                int balance = Convert.ToInt32(lblBalancedTicket.Text);

                if (tickets > balance)
                {
                    ShowAlert($"{tickets} tickets are not available for this event", "error");
                    return;
                }

                Session["TicketIdPrefix"] = drpEventName.SelectedItem.Text.Substring(0, 2);
                Session["Name"] = txtName.Text;
                Session["Email"] = txtEmail.Text;
                Session["Phone"] = txtPhoneNo.Text;
                Session["Nationality"] = ddlNationality.SelectedValue;
                Session["phonecode"] = ddlCountryCode.SelectedValue;
                Session["NoOfTickets"] = tickets;

                int price = Convert.ToInt32(Session["PricePerticket"]);
                int totalAmount = tickets * price;
                string requestID = generateRefNo(txtPhoneNo.Text);

                InsertDetails(requestID);
                WriteLog("Booking_Inserted", "Booking inserted. Request ID: " + requestID);

                string paymentURL = GetPaymentURL(
                    ddlPgName.SelectedValue,
                    txtPhoneNo.Text,
                    totalAmount,
                    requestID
                );

                if (!string.IsNullOrEmpty(paymentURL))
                {
                    WriteLog("Redirect_Payment", "Redirecting to payment URL. Gateway: " + ddlPgName.SelectedValue + ", Request ID: " + requestID);
                    Response.Redirect(paymentURL, false);
                }
            }
            catch (Exception ex)
            {
                WriteLog("Submit_Error", "Error while submitting ticket booking.", ex);
                ShowAlert("Something went wrong.", "error");
            }
        }

        string GetPaymentURL(string pg, string mobile, int amount, string requestID)
        {
            switch (pg)
            {
                case "OPAY":
                    WriteLog("Payment_URL", "Generating OPAY payment URL.");
                    return opay.opayPaymentLink(mobile, amount.ToString(), "WEB", callbackURL, requestID, callbackURL);

                case "CORALPAY":
                    WriteLog("Payment_URL", "Generating CORALPAY payment URL.");
                    return coralpaypayment.paymentLink(mobile, amount.ToString(), callbackURL, requestID, "WEB");

                case "MONNIFY":
                    WriteLog("Payment_URL", "Generating MONNIFY payment URL.");
                    return monnify.GetPaymentURL(mobile + "@sw.com", amount.ToString(), mobile, "WEB", callbackURL, requestID);

                case "PROVIDUS":
                    WriteLog("Payment_URL", "Generating PROVIDUS account details.");
                    var response = JObject.Parse(
                        Providus.PiPCreateDynamicAccountNumber(Session["Name"].ToString(), mobile, amount.ToString())
                    );

                    if (response["Status"].ToString() == "0")
                    {
                        Response.Redirect($"ProvidusAccountDetails.aspx?accName={Server.UrlEncode(response["AccountName"].ToString())}&accNo={Server.UrlEncode(response["AccountNumber"].ToString())}&ref={Server.UrlEncode(response["InitiationTranRef"].ToString())}");
                    }
                    else
                        lblmsg.Text = "Error: " + response["Message"];

                    break;
            }
            return null;
        }

        private void WriteLog(string action, string message, Exception ex = null)
        {
            try
            {
                string logDirectory = Server.MapPath("~/App_Data/Logs/");
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                string logPath = Path.Combine(logDirectory, "BuyTickets-" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
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

        void BindDrpEventName()
        {
            string qry = @"SELECT EventName + ', ' + EventVenue + ', ' + EventLocation + 
                        ' (' + CONVERT(varchar, EventDate,106) + '-' + CONVERT(varchar, EventTime,108) + ')' AS EName,* 
                        FROM tbl_EventMaster
                        WHERE EventDate >= CONVERT(date,GETDATE())
                        AND BalancedTicket > 0 AND PricePerTicket > 0";

            DataTable dt = CommonFunction.fetchdata(qry);

            if (dt.Rows.Count > 0)
            {
                drpEventName.DataSource = dt;
                drpEventName.DataTextField = "EName";
                drpEventName.DataValueField = "Id";
                drpEventName.DataBind();
                drpEventName.Items.Insert(0, new ListItem("--Select Event--", "0"));
            }
        }

        public string InsertDetails(string transactionId)
        {
            using (SqlConnection conn = new SqlConnection(CommonFunction.connectionString))
            {
                SqlCommand cmd = new SqlCommand("usp_TicketBooking1", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Name", Session["Name"]);
                cmd.Parameters.AddWithValue("@PhoneNo", Session["Phone"]);
                cmd.Parameters.AddWithValue("@Email", Session["Email"]);
                cmd.Parameters.AddWithValue("@Nationality", Session["Nationality"]);
                cmd.Parameters.AddWithValue("@TicketAmount", Session["PricePerticket"]);
                cmd.Parameters.AddWithValue("@BalancedTicket", Session["BalancedTicket"]);
                cmd.Parameters.AddWithValue("@NoOfTicketsBooked", Session["NoOfTickets"]);
                cmd.Parameters.AddWithValue("@EventName", Session["EventName"]);
                cmd.Parameters.AddWithValue("@PG_TransactionId", transactionId);
                cmd.Parameters.AddWithValue("@EventId", Session["EventId"]);
                cmd.Parameters.AddWithValue("@phonecode", Session["phonecode"]);
                cmd.Parameters.AddWithValue("@OrganisationName", DBNull.Value);

                cmd.Parameters.Add("@SuccessId", SqlDbType.Int).Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            return Guid.NewGuid().ToString();
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

        public static string generateRefNo(string mobile)
        {
            return mobile + DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        void BindNationality()
        {
            string qry = "";

            qry = "SELECT * from tbl_CountryMaster order by Nationality asc";

            DataTable dt = new DataTable();
            dt = CommonFunction.fetchdata(qry);

            if (dt.Rows.Count > 0)
            {
                ddlNationality.DataSource = dt;
                ddlNationality.DataTextField = "Nationality";
                ddlNationality.DataValueField = "Nationality";
                ddlNationality.DataBind();
                ddlNationality.Items.Insert(0, new ListItem("--Select Nationality--", "0"));
            }
            string qry1 = "";

            qry1 = "SELECT *, CONCAT(iso, '(+', phonecode, ')') AS iso_phonecode FROM [tbl_CountryCode] order by iso";

            DataTable dt1 = new DataTable();
            dt1 = CommonFunction.fetchdata(qry1);

            if (dt1.Rows.Count > 0)
            {
                ddlCountryCode.DataSource = dt1;
                ddlCountryCode.DataTextField = "iso_phonecode";
                ddlCountryCode.DataValueField = "phonecode";
                ddlCountryCode.DataBind();
                ddlCountryCode.SelectedIndex = 156;
            }
        }

        void BindPaymentGateways()
        {
            ddlPgName.Items.Clear();
            ddlPgName.Items.Add(new ListItem("OPAY", "OPAY"));
            //ddlPgName.Items.Add(new ListItem("CORALPAY", "CORALPAY"));
            //ddlPgName.Items.Add(new ListItem("MONNIFY", "MONNIFY"));
            //ddlPgName.Items.Add(new ListItem("PROVIDUS", "PROVIDUS"));
            ddlPgName.Items.Insert(0, new ListItem("--Select Payment Gateway--", "0"));
        }
    }
}