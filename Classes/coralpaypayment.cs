using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using RestSharp;
using System.IO;
using SmallerWorldForChildren.Classes;

namespace SmallProject
{
    public class coralpaypayment
    {

         
    //private static string coralPaybaseurl = "https://testdev.coralpay.com/VergeTest/api/v1/";
    //private static string coralPayUsername = "hirenNRIiiPh!^g^P^emDqUVTAZOnbK";
    //private static string coralPayPassword = "ISCYA4VQMU#@G3SX&4MH4CCT!O$P&%5!$D7$^Q9C";

    //private static string coralPaybaseurl = "https://testdev.coralpay.com/VergeTest/api/v1/";

    private static string coralPaybaseurl = "https://cpg.coralpay.com/api/v1/";
        private static string coralPayUsername = "hirenfMYOUkpPiZa!dKXB!QTOqpE&Tc";
        private static string coralPayPassword = "#(%)YR48&G#D352^3M1RA6#9(N6$$5WC*@$!3G#7";

        // private static string coralPayUsername = "GLotto001";
        //private static string coralPayPassword = "J@&20e:iDLHPyT@6";

        private static string coralPayMerchantId = "4001686IN69O501";
        //private static string coralPayMerchantId = "000000GRL01";
        public static string cpayverifyURL = coralPaybaseurl;

        private static string coralPayAuthusingheader()
        {
            var plainauth = System.Text.Encoding.UTF8.GetBytes(coralPayUsername + ":" + coralPayPassword);
            return "Basic " + Convert.ToBase64String(plainauth);
        }

        private static string Timestamp()
        {
            return ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
        }

        private static string traceId()
        {
            return ((int)(DateTime.UtcNow - new DateTime(1960, 1, 1)).TotalSeconds).ToString();
        }


//        private static string getToken()
//        {
//            var options = new RestClientOptions("https://testdev.coralpay.com:5000/")
//            {
//                MaxTimeout = -1,
//            };
//            var client = new RestClient(options);
//            var request = new RestRequest("/GwApi/api/v1/authentication", Method.Post);
//            request.AddHeader("Authentication", "Basic R0xvdHRvMDAxOkpAJjIwZTppRExIUHlUQDY=");
//            request.AddHeader("Content-Type", "application/json");
//            var body = @"{
//" + "\n" +
//            @"""Username"": ""hirenfMYOUkpPiZa!dKXB!QTOqpE&Tc"",
//" + "\n" +
//            @"""Password"":""#(%)YR48&G#D352^3M1RA6#9(N6$$5WC*@$!3G#7""
//" + "\n" +
//            @"}";
//            request.AddStringBody(body, DataFormat.Json);
//            //RestResponse response = await client.ExecuteAsync(request);
//            RestResponse response = client.Execute(request);
//            Console.WriteLine(response.Content);
//            // string[] t = JObject.Parse(response.Content.ToString());
//            string Token = JObject.Parse(response.Content.ToString())["token"].ToString() + "," + JObject.Parse(response.Content.ToString())["key"].ToString();
//            return Token;
//        }
        private static string[] getcoralPayAuthToken()
        {
            //string token1 = getToken();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var options = new RestClientOptions(coralPaybaseurl)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("authentication", Method.Post);
            request.AddHeader("Authorization", coralPayAuthusingheader());
            request.AddHeader("Content-Type", "application/json");
            var body = "{\n\"Username\": \"" + coralPayUsername + "\",\n\"Password\":\"" + coralPayPassword + "\"\n}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            string[] token = new string[2];

            try
            {
                RestResponse response = client.Execute(request);
                //token[0] = JObject.Parse(response.Content.ToString())["Token"].ToString();
                //token[1] = JObject.Parse(response.Content.ToString())["Key"].ToString();
                //token = token1.Split(',');
                JObject jsonResponse = JObject.Parse(response.Content.ToString());

                // Extract Token and Key
                token[0] = jsonResponse["token"]?.ToString(); // Null-safe access
                token[1] = jsonResponse["key"]?.ToString();  // Null-safe access
                //if (!string.IsNullOrEmpty(token[0]))
                //{
                //    token = token[0].Split(',');
                //}

            }
            catch (Exception ex)
            {
                //CommonFunction.insertLogs(mobileno, "[REQ:" + body + "][RESP:" + ex.Message.ToString() + "]", "CORALPAY_AUTH_TOKEN_ERROR");
            }

            return token;
        }

        private static string Signature(string merchantId, string key, string timestamp, string traceid)
        {
            try
            {
                StringBuilder signature = new StringBuilder();
                signature.Append(coralPayMerchantId.Trim()).Append(traceid.Trim())
                    .Append(timestamp.Trim()).Append(key);

                return convertToSHA256(signature.ToString());
            }
            catch (Exception ex)
            {
                //aamFunctions.insertLogs(mobileno, "[Error:" + ex.Message.ToString() + "]", "CPAY-SIGN-" + mobileno);
                return "-1";
            }
        }

        //private static string ComputeHash(string input)
        //{
        //    var data = Encoding.UTF8.GetBytes(input);
        //    Sha1Digest hash = new Sha1Digest();
        //    hash.BlockUpdate(data, 0, data.Length);
        //    byte[] result = new byte[hash.GetDigestSize()];
        //    hash.DoFinal(result, 0);
        //    return Convert.ToBase64String(result);
        //}

        public static string convertToSHA256(string input)
        {
            string hash = String.Empty;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                foreach (byte b in hashValue)
                {
                    hash += $"{b:X2}";
                }
            }

            return hash;
        }

        public static string generateRefNo(string mobile)
        {
            DateTime baseDate = new DateTime(1970, 1, 1);
            TimeSpan diff = DateTime.Now - baseDate;
            string milliseconds = diff.TotalMilliseconds.ToString().Split('.')[0].ToString();
            return mobile + milliseconds;
        }


        public static string paymentLink(string mobileno, string amount, string returnURL, string requestid, string plateformType)

        {
            StringBuilder sb = new StringBuilder();
            //  returnURL = "abc.com";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //var options = new RestClientOptions(coralPaybaseurl + "invokepayment")
            var options = new RestClientOptions(coralPaybaseurl)
            {
                MaxTimeout = -1
            };
            var client = new RestClient(options);
            var request = new RestRequest("invokepayment", Method.Post);
            string[] tokenapi = getcoralPayAuthToken();
            string token = tokenapi[0].ToString().Trim();
            string key = tokenapi[1].ToString().Trim();
            string timestamp = Timestamp().Trim();
            string systxnid = "CPAY"+ requestid;
            string sign = Signature(coralPayMerchantId, key, timestamp, systxnid).ToLower();
            string body = "";
            string payurl = "";
            string status = "06";
            string servicecharge = "0";
            string txnId = "-1";
            double totalamount = double.Parse(amount) + double.Parse(servicecharge);

            try
            {
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddHeader("Content-Type", "application/json");
                body = "{\n\"RequestHeader\": {\n\"MerchantId\": \"" + coralPayMerchantId + "\",\n\"TimeStamp\": \"" + timestamp + "\"," +
                    "\n\"Signature\": \"" + sign + "\"\n},\n" +
                    "\"Customer\":{\n\"Email\": \"rchugh@contecglobal.com\",\n\"Name\": \"Small World\",\n\"Phone\": \"" + mobileno + "\"\n" +
                    "},\n\"Customization\": {\n\"LogoUrl\": \"http://sampleurl.com\",\n\"Title\": \"Small World\"," +
                    "\n\"Description\": \"ADD MONEY\"\n},\n\"MetaData\": {\n\"Data1\": \"" + mobileno + "\"," +
                    "\n\"Data2\": \"" + amount + "\",\n\"Data3\": \"" + servicecharge + "\"\n}," +
                    "\n\"TraceId\": \"" + systxnid + "\",\n\"Amount\": " + totalamount + ",\n\"Currency\": \"NGN\"," +
                    "\n\"FeeBearer\": \"C\",\n\"ReturnUrl\": \"" + returnURL + "\"\n}";
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                RestResponse response = client.Execute(request);
                CommonFunction.insertLogs(mobileno, "[REQ:" + body.ToString() + "][RESP:" + response.Content.ToString() + "]", "CORALPAY_PAYMENT_LINK");
                sb.Append(response.Content);
                //File.AppendAllText( Server.MapPath("~/abc.txt"), sb.ToString());
                //File.AppendAllText(System.Web.HttpContext.Current.Server.MapPath("~/abc.txt"), sb.ToString());

                status = JObject.Parse(response.Content.ToString())["responseHeader"]["responseCode"].ToString();
                txnId = JObject.Parse(response.Content.ToString())["transactionId"].ToString();
                insertCRLPAYPayment(mobileno, amount, servicecharge, plateformType, systxnid, body, txnId, response.Content.ToString());
                if (status == "00")
                {
                    payurl = JObject.Parse(response.Content.ToString())["payPageLink"].ToString();
                }
                else
                {
                    payurl = "NA";
                }

            }
            catch (Exception ex)
            {
                CommonFunction.insertLogs(mobileno, "[REQ:" + body.ToString() + "][RESP:" + ex.Message.ToString() + "]", "CORALPAY_PAYMENT_LINK_ERROR");
                payurl = "-1";
            }

            return payurl;
        }


        private static void insertCRLPAYPayment(string mobileno, string amount, string servicecharge, string plateformtype, string systxnid, string apibody, string pgtxnid, string pgURLResp)
        {
            SqlConnection conn = new SqlConnection(CommonFunction.connectionString);
            SqlCommand cmd = new SqlCommand();

            try
            {
                cmd.Connection = conn;
                cmd.CommandText = "USP_INSERT_CORALPAY_PAYMENTS";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@mobileno", mobileno);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@servicecharge", servicecharge);
                cmd.Parameters.AddWithValue("@platformType", plateformtype);
                cmd.Parameters.AddWithValue("@systemTxnId", systxnid);
                cmd.Parameters.AddWithValue("@pgURLReq", apibody);
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

        public static string checkTxnStatusfromcoralpay(string traceid)
        {
            string url = cpayverifyURL;
            var options = new RestClientOptions(url)
            {
                MaxTimeout = -1
            };
            var client = new RestClient(options);
            string[] tokenapi = getcoralPayAuthToken();
            string token = tokenapi[0].ToString().Trim();
            string key = tokenapi[1].ToString().Trim();
            string timestamp = Timestamp().Trim();
            string sign = Signature(coralPayMerchantId, key, timestamp, traceid).ToLower();
            var request = new RestRequest("transactionquery", Method.Post);
            request.AddHeader("Authorization", "Bearer " + token);
            var body = "{\n\"RequestHeader\": {\n\"MerchantId\": \"" + coralPayMerchantId + "\",\n\"TimeStamp\": \"" + timestamp + "\"," +
                "\n\"Signature\": \"" + sign + "\"\n},\n" +
                    "\n\"TraceId\": \"" + traceid + "\"\n}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            CommonFunction.insertLogs("CORALPAY_VERIFY_LOG", response.Content.ToString(), "CORALPAY_VERIFY_LOG");
            JObject responseContent = JObject.Parse(response.Content);
            string txnStatus;
            if (responseContent["responseMessage"]?.ToString().ToUpper() == "SUCCESSFUL")
            {
                // Extract "responseCode"
                txnStatus = responseContent["responseCode"]?.ToString();
            }
            else
            {
                // Handle the case where the response is not "SUCCESSFUL"
                txnStatus = "FAILED";
            }

            // Log the response
            //CommonFunction.insertLogs("CORALPAY_VERIFY_LOG", response.Content.ToString(), "CORALPAY_VERIFY_LOG");

            // Return the transaction status
            return txnStatus;
        }

    }
}