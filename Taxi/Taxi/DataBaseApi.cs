using Android.Icu.Text;
using Android.Locations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using static Android.Provider.Telephony.Mms;
using static Android.Resource;

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
            SetStatusToWaitingUserByIdOrder,
            SetStatusToEndByIdOrder,
            SetStatusToDriveByIdOrder,
            GetActualPrice,
            SetRatingOrderById,
            AddDriverRatingByOrderId,
            SetStatusToCanseledByIdOrder,
            GetActiveOrderIdByLoginUser,
            GetRouteInfoByIdOrder,
            GetDriverCarsByDriverLogin,
            SetDriverStatusToSearchById,
            GetDriverCurrentCarByDriverLogin,
            GetOrders,
            GetOrderById,
            SetDriverStatusToSleepByLogin,
            SetDriverCoordersByLogin,
            PickOrderDriver,
            SetDriverStatusToDriveByLogin,
            GetUserPhoneByOrderId,
            GetDriverStatusByLogin,
            GetCurrentDriverOrderIdByDriverLogin,
            GetFavoriteAddressesByLogin,
            RemoveFavoriteAddressById,
            AddFavoriteAddress
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

        public static async void SetStatusToWaitingUserByIdOrder(int idOrder, long timeInWaitingDriverMilliseconds)
        {
            TimeSpan timeInWaitingDriver = new TimeSpan(0, 0, 0, 0, (int)timeInWaitingDriverMilliseconds);

            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{idOrder}" },
                { "time", new DateTimeOffset(2024, 1, 1, timeInWaitingDriver.Hours, timeInWaitingDriver.Minutes, timeInWaitingDriver.Seconds, timeInWaitingDriver.Milliseconds, TimeSpan.Zero).ToString("HH:mm:ss") }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.SetStatusToWaitingUserByIdOrder), new FormUrlEncodedContent(inputData));
        }

        public static async void SetStatusToEndByIdOrder(int idOrder, long timeInDriveMilliseconds, int price)
        {
            TimeSpan timeInWaitingDriver = new TimeSpan(0, 0, 0, 0, (int)timeInDriveMilliseconds);

            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{idOrder}" },
                { "time", new DateTimeOffset(2024, 1, 1, timeInWaitingDriver.Hours, timeInWaitingDriver.Minutes, timeInWaitingDriver.Seconds, timeInWaitingDriver.Milliseconds, TimeSpan.Zero).ToString("HH:mm:ss") },
                { "price", $"{price}"}
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.SetStatusToEndByIdOrder), new FormUrlEncodedContent(inputData));
        }

        public static async void SetStatusToDriveByIdOrder(int idOrder)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{idOrder}" }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.SetStatusToDriveByIdOrder), new FormUrlEncodedContent(inputData));
        }

        public static async void SetRatingOrderById(int idOrder, int rating)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{idOrder}" },
                { "rating", $"{rating}"}
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.SetRatingOrderById), new FormUrlEncodedContent(inputData));
        }

        public static async void SetStatusToCanseledByIdOrder(int idOrder)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{idOrder}" }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.SetStatusToCanseledByIdOrder), new FormUrlEncodedContent(inputData));
        }

        public static async void AddDriverRatingByOrderId(int idOrder, int rating)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{idOrder}" },
                { "rating", $"{rating}"}
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.AddDriverRatingByOrderId), new FormUrlEncodedContent(inputData));
        }

        public static async Task<int> GetActiveOrderIdByLoginUser(string loginUser)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "login", loginUser }
            };

            JsonObjecktOne resultCount = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.GetActiveOrderIdByLoginUser, inputData));

            return Convert.ToInt32(resultCount.Value);
        }

        public static async Task<JsonRouteInfo> GetRouteInfoByIdOrder(int idOrder)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{idOrder}" }
            };

            JsonRouteInfo result = JsonConvert.DeserializeObject<JsonRouteInfo>(await RequestApiGetJson(ApiFile.GetRouteInfoByIdOrder, inputData));

            return result;
        }

        public static async Task<JsonCars> GetDriverCarsByDriverLogin(string loginDriver)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "login", loginDriver }
            };

            JsonCars result = JsonConvert.DeserializeObject<JsonCars>(await RequestApiGetJson(ApiFile.GetDriverCarsByDriverLogin, inputData));

            return result;
        }

        public static async Task<JsonCar> GetDriverCurrentCarByDriverLogin(string loginDriver)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "login", loginDriver }
            };

            JsonCar result = JsonConvert.DeserializeObject<JsonCar>(await RequestApiGetJson(ApiFile.GetDriverCurrentCarByDriverLogin, inputData));

            return result;
        }

        public static async void SetDriverStatusToSearchById(int idDriver, int idCar, string coorders)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "idDriver", $"{idDriver}" },
                { "idCar", $"{idCar}" },
                { "coorders", coorders }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.SetDriverStatusToSearchById), new FormUrlEncodedContent(inputData));
        }

        public static async Task<JsonOrders> GetOrders(string rate, string isChildSeet)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "rate", rate },
                { "isChildSeet", isChildSeet }
            };

            JsonOrders result = JsonConvert.DeserializeObject<JsonOrders>(await RequestApiGetJson(ApiFile.GetOrders, inputData));

            return result;
        }

        public static async Task<JsonOrder> GetOrderById(int idOrder)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{idOrder}" }
            };

            JsonOrder result = JsonConvert.DeserializeObject<JsonOrder>(await RequestApiGetJson(ApiFile.GetOrderById, inputData));

            return result;
        }

        public static async void SetDriverStatusToSleepByLogin(string loginDriver)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "loginDriver", loginDriver }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.SetDriverStatusToSleepByLogin), new FormUrlEncodedContent(inputData));
        }

        public static async void PickOrderDriver(int idOrder, string loginDriver)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "idOrder", $"{idOrder}" },
                { "loginDriver", loginDriver }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.PickOrderDriver), new FormUrlEncodedContent(inputData));
        }

        public static async void SetDriverStatusToDriveByLogin(string loginDriver)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "loginDriver", loginDriver }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.SetDriverStatusToDriveByLogin), new FormUrlEncodedContent(inputData));
        }

        public static async void SetDriverCoordersByLogin(string loginDriver, double latitude, double longitude)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "loginDriver", loginDriver },
                { "coorders", $"{latitude.ToString().Replace(",", ".")},{longitude.ToString().Replace(",", ".")}" }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.SetDriverCoordersByLogin), new FormUrlEncodedContent(inputData));
        }

        public static async Task<string> GetUserPhoneByOrderId(int idOrder)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "idOrder", $"{idOrder}" }
            };

            JsonObjecktOne result = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.GetUserPhoneByOrderId, inputData));

            return result.Value;
        }

        public static async Task<string> GetDriverStatusByLogin(string login)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "login", login }
            };

            JsonObjecktOne result = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.GetDriverStatusByLogin, inputData));

            return result.Value;
        }

        public static async Task<int> GetCurrentDriverOrderIdByDriverLogin(string loginDriver)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "login", loginDriver }
            };

            JsonObjecktOne result = JsonConvert.DeserializeObject<JsonObjecktOne>(await RequestApiGetJson(ApiFile.GetCurrentDriverOrderIdByDriverLogin, inputData));

            return Convert.ToInt32(result.Value);
        }

        public static async Task<JsonFavoriteAddresses> GetFavoriteAddressesByLogin(string login)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "login", login }
            };

            JsonFavoriteAddresses result = JsonConvert.DeserializeObject<JsonFavoriteAddresses>(await RequestApiGetJson(ApiFile.GetFavoriteAddressesByLogin, inputData));

            return result;
        }

        public static async void RemoveFavoriteAddressById(int addressId)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "id", $"{addressId}" }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.RemoveFavoriteAddressById), new FormUrlEncodedContent(inputData));
        }

        public static async void AddFavoriteAddress(string loginUser, string name, string coorders)
        {
            Dictionary<string, string> inputData = new Dictionary<string, string>
            {
                { "login", loginUser },
                { "name", name },
                { "coorders", coorders }
            };

            await _httpClient.PostAsync(GetUrl(ApiFile.AddFavoriteAddress), new FormUrlEncodedContent(inputData));
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
