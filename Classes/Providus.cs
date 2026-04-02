using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using SmallerWorldForChildren.Classes;

namespace SmallProject
{


    public class Providus
    {
       // public static string xAUTH = "BE09BEE831CF262226B426E39BD109f2AF84DC63076D4174FAC78A2261F9A3D6E59744983B8326B69CDF2963FE314DFC89635CFA37A40596508DD6EAAB09402C7";
        public static string clientId = System.Configuration.ConfigurationManager.AppSettings["ProvidusClientID"].ToString();
        public static string BaseURL = System.Configuration.ConfigurationManager.AppSettings["ProvidusURL"].ToString();
        public static string SecretKey = System.Configuration.ConfigurationManager.AppSettings["ProvidusSecretKet"].ToString();



        private static void InsertProvidusPayment(string mobileNo, string amount, string pgTxnId,string pgURLReq, string pgURLRes,string accName, string accNo)
        {
            SqlConnection conn = new SqlConnection(CommonFunction.connectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "USP_InsertProvidusDynamicAccount";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MobileNo", mobileNo);
            cmd.Parameters.AddWithValue("@TotalAmount", amount);
            cmd.Parameters.AddWithValue("@InitiationTranRef", pgTxnId);
            cmd.Parameters.AddWithValue("@pgURLReq", pgURLReq);
            cmd.Parameters.AddWithValue("@pgURLResp", pgURLRes);
            cmd.Parameters.AddWithValue("@AccountNumber", accName);
            cmd.Parameters.AddWithValue("@AccountName", accNo);
            
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            cmd.Connection.Dispose();
        }

        private static string ComputeSHA512(string input)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha512.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static string PiPCreateDynamicAccountNumber(string account_name,string mobile,string amount)
        {
            JObject response = null;
            string rawString = $"{clientId}:{SecretKey}";

            // 2. Compute SHA512 hash
            string xAUTH = ComputeSHA512(rawString);

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //RestResponse response = null;
            //string responseBody = "";
            try
            {
                var body = "{\n \"account_name\": \"" + account_name + "\" \n}";


                //string sign = gethashedkey(OPaySecretKey, body.ToString());


                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string url = BaseURL + "PiPCreateDynamicAccountNumber";
                var client = new RestClient();
                //client.Timeout = -1;
                var request = new RestRequest(url, Method.Post);
                // var request = new RestRequest(url,Method.POST);
                request.AddHeader("accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Auth-Signature", xAUTH);
                request.AddHeader("Client-Id", clientId);
                //request.AddParameter("application/json", body, ParameterType.RequestBody);
                request.AddStringBody(body, DataFormat.Json);
                //IRestResponse opayresponse = client.Execute(request);
                RestResponse providusresponse = client.Execute(request);
                JObject responseContent = JObject.Parse(providusresponse.Content);

                CommonFunction.insertLogs(account_name, "[URL : " + url + "][BODY : " + body + "][RESPONSE : " + providusresponse.Content.ToString() + "]", "providusresponse_API_PAYMENT_STATUS");
                if (responseContent["requestSuccessful"] != null)
                {
                    string msg = responseContent["responseMessage"]?.ToString();

                    if (responseContent["responseMessage"].ToString().ToUpper() == "OPERATION SUCCESSFUL")
                    {
                       
                            
                        response = JObject.Parse("{\"Status\":\"0\",\"Message\":\"" + responseContent["responseMessage"] + "\"\n}");
                        string accNo = responseContent["account_number"]?.ToString();
                        string accName = responseContent["account_name"]?.ToString();
                        string initiationTranRef = responseContent["initiationTranRef"]?.ToString();

                        InsertProvidusPayment(mobile, amount, initiationTranRef, request.ToString(), responseContent.ToString(), accName, accNo);
                        response["Status"] = "0";
                        response["Message"] = msg;
                        response["AccountNumber"] = accNo;
                        response["AccountName"] = accName;
                        response["InitiationTranRef"] = initiationTranRef;
                        return response.ToString();
                    }
                    else
                    {
                        // response = JObject.Parse("{\"Status\":\"1\",\"Message\":\"" + responseContent["responseMessage"].ToString() + "\"\n}");
                        response["Status"] = "1";
                        response["Message"] = msg ?? "Unknown error";
                        return response.ToString();
                    }
                }
                else
                {
                    // response = JObject.Parse("{\"Status\":\"1\",\"Message\":\"Failed\"\n}");
                    response["Status"] = "1";
                    response["Message"] = "Failed";
                    return response.ToString();
                }
            }
            catch (Exception ex)
            {
                response["Status"] = "1";
                response["Message"] = "Error: " + ex.Message;
                return response.ToString();
            }

            //return null;
        }

    }

}