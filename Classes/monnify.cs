using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using SmallerWorldForChildren.Classes;

namespace SmallProject
{
    public class InitTransactionRequestBody
    {
        public string amount { get; set; }
        public string customerName { get; set; }
        public string customerEmail { get; set; }
        public string paymentReference { get; set; }
        public string paymentDescription { get; set; }
        public string currencyCode { get; set; }
        public string contractCode { get; set; }
        public string redirectUrl { get; set; }
        public string[] paymentMethods { get; set; }
    }

    public static class monnify
    {
        public static string monifypaymentapiurl = System.Configuration.ConfigurationManager.AppSettings["monifyURL"].ToString();
        public static string monifyBaseURL = System.Configuration.ConfigurationManager.AppSettings["monifyBaseUrl"].ToString();
        public static string monifyApikey = System.Configuration.ConfigurationManager.AppSettings["monifyKey"].ToString();
        public static string monifySecretkey = System.Configuration.ConfigurationManager.AppSettings["monifySecretKey"].ToString();
        public static string monifyContractCode = System.Configuration.ConfigurationManager.AppSettings["monifyContractCode"].ToString();
        public static string monifypaymentcurrency = "NGN";

        public static double gettransactoincharge(string amount)
        {
            double servicecharge = double.Parse(amount) * 0.01;
            servicecharge = servicecharge > 1000 ? 1000 : servicecharge;
            return servicecharge;

        }
        public static string generateRefNo(string mobile)
        {
            DateTime baseDate = new DateTime(1970, 1, 1);
            TimeSpan diff = DateTime.Now - baseDate;
            string milliseconds = diff.TotalMilliseconds.ToString().Split('.')[0].ToString();
            return mobile + milliseconds;
        }
        public static string GetPaymentURL(string email, string amount, string mobileno, string platformtype, string callbackurl, string requestID)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            RestResponse response = null;
            //IRestResponse response = null;
            string responseBody = "";

            double servicecharge = gettransactoincharge(amount);
            double finalamount = double.Parse(amount) + servicecharge;
            finalamount = Math.Ceiling(finalamount);
            string systemTxnId = "MNFY" + requestID;
            string url = monnify.monifyBaseURL + "api/v1/merchant/transactions/";
            var authToken = GetAuthToken();
            var client = new RestClient(url);
            // client.Timeout = -1;
            var request = new RestRequest("init-transaction", Method.Post);
            request.AddHeader("Authorization", "Bearer " + authToken);
            InitTransactionRequestBody initTransactionRequestBody = new InitTransactionRequestBody
            {
                amount = finalamount.ToString(),
                customerEmail = email,
                customerName = "sw_USER",
                paymentReference = systemTxnId,
                paymentDescription = "Payment to sw wallet",
                currencyCode = "NGN",
                contractCode = monifyContractCode,
                redirectUrl = callbackurl,
                paymentMethods = new string[] { "CARD", "ACCOUNT_TRANSFER" }
            };
            string body = JsonConvert.SerializeObject(initTransactionRequestBody);

            request.AddParameter("application/json", body, ParameterType.RequestBody);
            try
            {
                response = client.Execute(request);
                responseBody = response.Content.ToString();
                //  aamFunctions.insertLogs(mobileno, responseBody.ToString(), "MONNIFY_TRANSACTION_RESPONSE");
                string responseSuccessful = JObject.Parse(responseBody)["requestSuccessful"].ToString().ToLower();
                if (responseSuccessful == "true")
                {
                    string pgtxnid = JObject.Parse(responseBody)["responseBody"]["transactionReference"].ToString();
                    string checkouturl = JObject.Parse(responseBody)["responseBody"]["checkoutUrl"].ToString();
                    insertMonifyPayment(mobileno, amount, servicecharge.ToString(), platformtype, systemTxnId, body, pgtxnid, responseBody);

                    return checkouturl;
                }
            }
            catch (Exception ex)
            {
                //aamFunctions.insertLogs(mobileno, ex.Message, "MONNIFY_TRANSACTION_RESPONSE_ERROR");
            }
            return null;
        }
        public static string GetPaymentData(string email, string amount, string mobileno, string plateformtype)
        {
            double servicecharge = gettransactoincharge(amount);
            double finalamount = double.Parse(amount) + servicecharge;
            finalamount = Math.Ceiling(finalamount);
            string systemTxnId = CommonFunction.generateRefNo(mobileno);
            string paymentData = "";
            try
            {
                paymentData = "{\"mobile\":\"" + mobileno + "\",\n\"amount\":\"" + finalamount + "\",\n\"txnRef\":\"" + systemTxnId + "\", \n \"apiKey\":\"" + monifyApikey + "\", \n \"contractCode\":\"" + monifyContractCode + "\"}";

                insertMonifyPayment(mobileno, amount, servicecharge.ToString(), plateformtype, systemTxnId, "NO PG REQUEST", "", paymentData);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return paymentData;
        }

        private static string monnifytransyrl = "http://99.81.124.219:8123/SWINTERFACE/nownow/interface/callapi";
        private static string monnifyPaybaseurl = "https://api.monnify.com/";
        public static string checkTxnStatusfromMonnify(string paymentReference)
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var authToken = GetAuthToken();
            string url = monnify.monifyBaseURL + "api/v2/transactions/" + paymentReference;
            var client = new RestClient();
            //  client.Timeout = -1;
            // var request = new RestRequest("Method.Get");
            var request = new RestRequest(monnifytransyrl, Method.Post);
            //  request.AddHeader("Authorization", "Bearer " + authToken);
            var body = "{\r\n    \"apiurl\":\"" + url + "\",\r\n    \"reqbody\":\"\",\r\n    \"reqheader\":\"Bearer " + authToken + "\",\r\n    \"reqmethod\":\"GET\"\r\n}";

            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            //   aamFunctions.insertLogs("MONNIFY_LOGS_checktxnstatus", response.Content.ToString(), "MONNIFY_LOGS_checktxnstatus");
            string txnStatus = JObject.Parse(response.Content)["responseBody"]["paymentStatus"].ToString();
            // return response.Content.ToString();
            return txnStatus;
        }
        public static string GetAuthToken()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string authTokenURL = monnify.monifyBaseURL + "api/v1/auth/";


            var options = new RestClientOptions(authTokenURL)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            // client.Timeout = -1;
            var request = new RestRequest("login", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Basic " + GetBase64ApiKey());
            var body = "";
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            string[] token = new string[2];
            RestResponse response = client.Execute(request);
            token[0] = JObject.Parse(response.Content.ToString())["responseBody"].ToString();
            //aamFunctions.insertLogs("MONNIFY_LOGS_authtoken", response.Content.ToString(), "MONNIFY_LOGS_authtoken");
            string accessToken = JObject.Parse(response.Content.ToString())["responseBody"]["accessToken"].ToString();
            //["responseBody"]["accessToken"].ToString();
            return accessToken;

        }
        public static string GetBase64ApiKey()
        {
            string apikey = monnify.monifyApikey;
            string apiSecret = monnify.monifySecretkey;

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(apikey + ":" + apiSecret));
        }
        private static void insertMonifyPayment(string mobileno, string amount, string servicecharge, string plateformtype, string systemTxnId, string pgURLReq, string pgtxnid, string pgURLResp)
        {
            SqlConnection conn = new SqlConnection(CommonFunction.connectionString);
            SqlCommand cmd = new SqlCommand();

            try
            {
                cmd.Connection = conn;
                cmd.CommandText = "USP_Insert_Monify_Payments";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@mobileno", mobileno);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@servicecharge", servicecharge);
                cmd.Parameters.AddWithValue("@platformType", plateformtype);
                cmd.Parameters.AddWithValue("@systemTxnId", systemTxnId);
                cmd.Parameters.AddWithValue("@pgURLReq", pgURLReq);
                cmd.Parameters.AddWithValue("@pgTxnId", pgtxnid);
                cmd.Parameters.AddWithValue("@pgURLResp", pgURLResp);
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