using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Taxi
{
    public class DataBaseApi
    {
        private const string Url = "http://taxiviking.ru";
        private const string ApiKey = "k6uy6ien-v9cj-wi5h-nnd0-skp7m423s8jo";

        private static HttpClient _httpClient = new HttpClient();

        private enum ApiFile
        {
            GetCountByLogin,
            GetCountByPhone,
            CheckPassword,
            TryRegistration,
            AddOrderTaxi,
            GetStatusOrderById,
            ReupdateStatusToSearchByIdOrder,
            GetFioByLogin,
            GetRoleByLogin,
            GetDriverCoordersByIdOrder,
            GetTaxiInfoByIdOrder,
            FastSearch,
            SetStatusToWaitingDriverByIdOrder,
            GetActualPrice
        }

        public static async Task<int> GetCountByLogin(string login)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "login", login }
            };

            JsonObjecktOne resultCount = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.GetCountByLogin, inputData));

            return Convert.ToInt32(resultCount.Value);
        }

        public static async Task<int> GetCountByPhone(string phone)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "phone", phone }
            };

            JsonObjecktOne resultCount = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.GetCountByPhone, inputData));

            return Convert.ToInt32(resultCount.Value);
        }

        public static async Task<bool> CheckPassword(string login, string password)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "login", login },
                { "password", password }
            };
            
            JsonObjecktOne resultCount = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.CheckPassword, inputData));

            return Convert.ToBoolean(Convert.ToInt32(resultCount.Value));
        }

        public static async Task<bool> TryRegistration(string login, string password, string name, string lastName, string phone)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "login", login.Trim() },
                { "password", password.Trim() },
                { "name", name.Trim() },
                { "lastname", lastName.Trim() },
                { "phone", phone.Trim() }
            };

            JsonObjecktOne resultCount = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.TryRegistration, inputData));

            return Convert.ToBoolean(Convert.ToInt32(resultCount.Value));
        }

        public static async Task<int> AddOrderTaxi(string distance, string duration, string durationInTraffic, string startShort, string finishShort, string startLong, string finishLong, string startCoorders, string finishCoorders, string priority, string price, string rate, string paymentType)
        {
            App.Current.Properties.TryGetValue("login", out object loginObj);
            string login = $"{loginObj}";

            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "distance", distance },
                { "duration", duration },
                { "durationInTraffic", durationInTraffic },
                { "startShort", startShort },
                { "finishShort", finishShort },
                { "startLong", startLong },
                { "finishLong", finishLong },
                { "startCoorders", startCoorders },
                { "finishCoorders", finishCoorders },
                { "priority", priority },
                { "price", price },
                { "rate", rate },
                { "paymentType", paymentType },
                { "userLogin", login }
            };

            JsonObjecktOne resultCount = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.AddOrderTaxi, inputData));
            int idOrder = Convert.ToInt32(resultCount.Value);

            return idOrder;
        }

        public static async Task<string> GetStatusOrderById(int id)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{id}" }
            };

            JsonObjecktOne resultCount = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.GetStatusOrderById, inputData));

            return resultCount.Value;
        }

        public static async void ReupdateStatusToSearchByIdOrder(int id)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{id}" }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.ReupdateStatusToSearchByIdOrder), new FormUrlEncodedContent(inputData));
        }

        public static async Task<string> GetFioByLogin(string login)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "login", login }
            };

            JsonObjecktOne resultCount = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.GetFioByLogin, inputData));

            return resultCount.Value;
        }

        public static async Task<string> GetRoleByLogin(string login)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "login", login }
            };

            JsonObjecktOne resultCount = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.GetRoleByLogin, inputData));

            return resultCount.Value;
        }

        public static async Task<string> GetDriverCoordersByIdOrder(int idOrder)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{idOrder}" }
            };

            JsonObjecktOne resultCount = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.GetDriverCoordersByIdOrder, inputData));

            return resultCount.Value;
        }

        public static async Task<JsonTaxiInfo> GetTaxiInfoByIdOrder(int idOrder)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{idOrder}" }
            };

            JsonTaxiInfo result = JsonConvert.DeserializeObject<JsonTaxiInfo>(await RequestApiGetJson(ApiFile.GetTaxiInfoByIdOrder, inputData));

            return result;
        }

        public static async Task<JsonPrice> GetActualPrice()
        {
            JsonPrice price = JsonConvert.DeserializeObject<JsonPrice>(await RequestApiGetJson(ApiFile.GetActualPrice, new Dictionary<string, string>()));

            return price;
        }

        public static async void FastSearch(int idOrder)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{idOrder}" }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.FastSearch), new FormUrlEncodedContent(inputData));
        }

        public static async void SetStatusToWaitingDriverByIdOrder(int idOrder, long timeInSearchMilliseconds)
        {
            TimeSpan timeInSearch = new TimeSpan(0, 0, 0, 0, (int)timeInSearchMilliseconds);

            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{idOrder}" },
                { "time", new DateTimeOffset(2024, 1, 1, timeInSearch.Hours, timeInSearch.Minutes, timeInSearch.Seconds, timeInSearch.Milliseconds, TimeSpan.Zero).ToString("HH:mm:ss") }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.SetStatusToWaitingDriverByIdOrder), new FormUrlEncodedContent(inputData));
        }

        private static string GetUrl(ApiFile name)
        {
            return Url + "/api/" + name + ".php?Api_key=" + ApiKey;
        }

        private static async Task<string> RequestApiGetJson(ApiFile api, Dictionary<string, string> data)
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            HttpResponseMessage requestMessage = await _httpClient.PostAsync(GetUrl(api), new FormUrlEncodedContent(data));

            return await requestMessage.Content.ReadAsStringAsync();
        }
    }
}
