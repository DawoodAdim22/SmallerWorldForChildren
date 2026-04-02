using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using RestSharp;
using System.Net;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net.Http;
using SmallerWorldForChildren.Classes;

namespace SmallProject
{
    public class opay
    {
        public static string OPayBaseURL = System.Configuration.ConfigurationManager.AppSettings["OPayBaseURL"].ToString();
        public static string OPayMerchantId = System.Configuration.ConfigurationManager.AppSettings["OPayMerchantId"].ToString();
        public static string OPayPublicKey = System.Configuration.ConfigurationManager.AppSettings["OPayPublicKey"].ToString();
        public static string OPaySecretKey = System.Configuration.ConfigurationManager.AppSettings["OPaySecretKey"].ToString();

                public string callbackURL = System.Configuration.ConfigurationManager.AppSettings["callbackURL"].ToString();

        private static string ComputeHmacSha3_512(string data, string key)
        {
            var hmac = new HMac(new Sha3Digest(512));
            var keyBytes = Encoding.UTF8.GetBytes(key);
            hmac.Init(new KeyParameter(keyBytes));

            var dataBytes = Encoding.UTF8.GetBytes(data);
            var result = new byte[hmac.GetMacSize()];
            hmac.BlockUpdate(dataBytes, 0, dataBytes.Length);
            hmac.DoFinal(result, 0);

            return BitConverter.ToString(result).Replace("-", "").ToLower();
        }

        private static JToken SortJToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return new JObject(
                        token.Children<JProperty>()
                             .OrderBy(prop => prop.Name)
                             .Select(prop => new JProperty(prop.Name, SortJToken(prop.Value)))
                    );
                case JTokenType.Array:
                    return new JArray(token.Select(SortJToken));
                default:
                    return token;
            }
        }

        private static void insertOPAYPayment(string mobileNo, string amount, string vat, string appType, string systemTxnId, string pgTxnId, string orderNo
            , string pgURLReq, string pgURLRes)
        {
            SqlConnection conn = new SqlConnection(CommonFunction.connectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "Insert_OPAY_Payment_Detail";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@mobileno", mobileNo);
            cmd.Parameters.AddWithValue("@amount", amount);
            cmd.Parameters.AddWithValue("@vat", vat);
            cmd.Parameters.AddWithValue("@platformType", appType);
            cmd.Parameters.AddWithValue("@systemTxnId", systemTxnId);
            cmd.Parameters.AddWithValue("@pgTxnId", pgTxnId);
            cmd.Parameters.AddWithValue("@orderNo", orderNo);
            cmd.Parameters.AddWithValue("@pgURLReq", pgURLReq);
            cmd.Parameters.AddWithValue("@pgURLResp", pgURLRes);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            cmd.Connection.Dispose();
        }

        private static void updateOPAYPayment(string body, string TransactionRef, string walletTxnid, string fees)
        {
            SqlConnection conn = new SqlConnection(CommonFunction.connectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "Update_OPAY_Payment_Detail_verify";
            cmd.CommandType = CommandType.StoredProcedure;
            // cmd.Parameters.AddWithValue("@PGWebhookResponse", body);   //no webhook for this
            //cmd.Parameters.AddWithValue("@systemTxnId", TransactionRef);
            //cmd.Parameters.AddWithValue("@WalletTxnId", walletTxnid);
            //cmd.Parameters.AddWithValue("@fees", fees);
            cmd.Parameters.AddWithValue("@verifyTxnReq", body);
            cmd.Parameters.AddWithValue("@verifyTxnResp", TransactionRef);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            cmd.Connection.Dispose();
        }

        public static string opayTransactionStatus(string paymentReference, string mobileNo)
        {
            JObject response = null;
            try
            {
                var body = "";
                body = "{\n     \"country\": \"NG\"," +
                        "\n     \"reference\": \"" + paymentReference + "\"" +
                        "\n}";

                string sign = gethashedkey(OPaySecretKey, body.ToString());

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string url = OPayBaseURL + "/cashier/status";
                var client = new RestClient();
                //client.Timeout = -1;
                var request = new RestRequest(url, Method.Post);
                // var request = new RestRequest(url,Method.POST);
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + sign);
                request.AddHeader("MerchantId", OPayMerchantId);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                //IRestResponse opayresponse = client.Execute(request);
                RestResponse opayresponse = client.Execute(request);
                JObject responseContent = JObject.Parse(opayresponse.Content);

                CommonFunction.insertLogs(mobileNo, "[URL : " + url + "][BODY : " + body + "][RESPONSE : " + opayresponse.Content.ToString() + "]", "OPAY_API_PAYMENT_STATUS");

                if (responseContent["message"] != null)
                {
                    if (responseContent["message"].ToString().ToUpper() == "SUCCESSFUL")
                    {
                        if (responseContent["data"]["status"] != null)
                        {
                            if (responseContent["data"]["status"].ToString().ToUpper() == "SUCCESS")
                            {
                                response = JObject.Parse("{\"Status\":\"0\",\"Message\":\"" + responseContent["data"]["status"] + "\"\n}");
                                updateOPAYPayment(body.ToString(),paymentReference,"","");
                            }
                            else
                            {
                                response = JObject.Parse("{\"Status\":\"1\",\"Message\":\"" + responseContent["data"]["status"] + "\"\n}");
                            }
                        }
                        else
                        {
                            response = JObject.Parse("{\"Status\":\"1\",\"Message\":\"" + responseContent["message"].ToString() + "\"\n}");
                        }
                    }
                    else
                    {
                        response = JObject.Parse("{\"Status\":\"1\",\"Message\":\"" + responseContent["message"].ToString() + "\"\n}");
                    }
                }
                else
                {
                    response = JObject.Parse("{\"Status\":\"1\",\"Message\":\"Failed\"\n}");
                }
            }
            catch (Exception ex)
            {
                CommonFunction.insertLogs(mobileNo, ex.Message.ToString(), "OPAY_API_PAYMENT_STATUS_ERROR");
                response = JObject.Parse("{\"Status\":\"-1\",\"Message\":\"" + ex.Message.ToString() + "\"\n}");
            }
            string txnStatus = response["Message"].ToString();

            //string txnStatus = JObject.Parse(responseContent["data"]["status"]);

            return txnStatus;
        }

        public static string gethashedkey(string secretKey, string payload)
        {
            payload = Regex.Replace(payload, @"\s+", string.Empty);
            byte[] key = Encoding.UTF8.GetBytes(secretKey);
            byte[] message1 = Encoding.UTF8.GetBytes(payload);
            var hash = new HMACSHA512(key);
            byte[] hash1 = hash.ComputeHash(message1);
            string hash2 = BitConverter.ToString(hash1).Replace("-", String.Empty);
            return hash2.ToLower();
        }

        public static string payoutSign(string payload, string secretKey)
        {
            string signature = "";
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);

            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                // Compute the HMAC-SHA256 signature
                byte[] signatureBytes = hmac.ComputeHash(payloadBytes);

                // Convert signature to Base64
                signature = Convert.ToBase64String(signatureBytes);
            }

            return signature;
        }

        
        public static string opayPaymentLink(string mobileNo, string amount, string appType, string returnURL, string requestID,string callbackurl)
        {
            JObject response = null;
            try
            {
                var body = "";
                string systemTxnId = "OPAY" + requestID;
                returnURL = returnURL + "?paymentReference=" + systemTxnId;
                //string reference = paymentReference; //aamFunctions.generateRefNo(mobileNo);
                amount = (Double.Parse(amount) * 100).ToString();
                double amountNew = Double.Parse(amount) / 100;
                body = "{\n     \"country\": \"NG\"," +
                        "\n     \"reference\": \"" + systemTxnId + "\"," +
                        "\n     \"amount\": {" +
                        "\n         \"total\": " + Double.Parse(amount) + "," +
                        "\n         \"currency\": \"NGN\"" +
                                "}," +
                        "\n     \"callbackUrl\": \"" + callbackurl + "\"," +
                        "\n     \"returnUrl\": \"" + returnURL + "\"," +
                        "\n     \"displayName\": \"International Women's Organization for charity\"," +
                        "\n     \"product\": {" +
                        "\n         \"description\": \"Payment Link for - " + mobileNo + "\"," +
                        "\n         \"name\": \"User - " + mobileNo + "\"" +
                                "}" +
                        "\n}";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string url = OPayBaseURL + "/cashier/create";
                var client = new RestClient();
                //client.Timeout = -1;

                var request = new RestRequest(url, Method.Post);
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + OPayPublicKey);
                request.AddHeader("MerchantId", OPayMerchantId);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                RestResponse opayresponse = client.Execute(request);
                JObject responseContent = JObject.Parse(opayresponse.Content);

                CommonFunction.insertLogs(mobileNo, "[URL : " + url + "][BODY : " + body + "][RESPONSE : " + opayresponse.Content.ToString() + "]", "OPAY_PAYMENT_LINK_API");

                if (responseContent["message"] != null)
                {
                    if (responseContent["message"].ToString().ToUpper() == "SUCCESSFUL")
                    {
                        string pgTxnId = responseContent["data"]["reference"].ToString();
                        string orderNo = responseContent["data"]["orderNo"].ToString();
                        string checkout_url = responseContent["data"]["cashierUrl"].ToString();
                        string vat = responseContent["data"]["vat"]["total"].ToString();

                        insertOPAYPayment(mobileNo, amountNew.ToString(), vat, appType, systemTxnId, pgTxnId, orderNo, body.ToString(), responseContent.ToString());
                        response = JObject.Parse("{\"Status\":\"0\",\"Message\":\"Success\",\n\"Data\":[\n{\n\"MobileNo\":\"" + mobileNo + "\",\n\"Amount\":\"" + amountNew.ToString() + "\",\n\"Currency\":\"NGN\",\n\"Url\":\"" + checkout_url + "\",\n\"paymentReference\":\"" + pgTxnId + "\"}\n]\n}");
                        return checkout_url;
                    }

                    else
                    {
                        response = JObject.Parse("{\"Status\":\"1\",\"Message\":\"" + responseContent["message"].ToString() + "\",\n\"Data\":[]\n}");
                    }
                }
                else
                {
                    response = JObject.Parse("{\"Status\":\"1\",\"Message\":\"Failed\",\n\"Data\":[]\n}");
                }
            }
            catch (Exception ex)
            {
                CommonFunction.insertLogs(mobileNo, ex.Message.ToString(), "OPAY_PAYMENT_LINK_API_ERROR");
                response = JObject.Parse("{\"Status\":\"-1\",\"Message\":\"" + ex.Message.ToString() + "\",\n\"Data\":[]\n}");
            }

            return null;
            //string link = response["data"]["Url"].ToString();

            ////string txnStatus = JObject.Parse(responseContent["data"]["status"]);

            //return link;
        }
    }
}