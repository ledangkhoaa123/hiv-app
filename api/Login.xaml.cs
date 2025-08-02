using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace api
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập email và mật khẩu.");
                return;
            }

            var loginPayload = new
            {
                email = email,
                password = password
            };

            string jsonPayload = JsonConvert.SerializeObject(loginPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();

            try
            {
                var response = await client.PostAsync("http://localhost:3000/auth/login", content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseText);

                    AppData.AccessToken = loginResponse.data.access_token;
                    AppData.CurrentUser = loginResponse.data.user;

                    if (AppData.CurrentUser.role.name == "CUSTOMER_ROLE")
                    {
                        new CustomerWindow().Show();
                        this.Close();
                    }
                    else if (AppData.CurrentUser.role.name == "MANAGER_ROLE")
                    {
                        new ManagerWindow().Show();
                        this.Close();
                    }
                    else if (AppData.CurrentUser.role.name == "DOCTOR_ROLE")
                    {
                        new DoctorWindow().Show();
                        this.Close();
                    }
                    else if (AppData.CurrentUser.role.name == "STAFF_ROLE")
                    {
                        new StaffWindow().Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show($"Role chưa hỗ trợ: {AppData.CurrentUser.role.name}");
                    }
                }
                else
                {
                    MessageBox.Show($"Đăng nhập thất bại: {response.StatusCode}\n{responseText}");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Không thể kết nối đến server: " + ex.Message);
            }
        }
    }
    public class LoginResponse
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public LoginData data { get; set; }
    }

    public class LoginData
    {
        public string access_token { get; set; }
        public User user { get; set; }
    }

    public class User
    {
        public string _id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public Role role { get; set; }
    }

    public class Role
    {
        public string _id { get; set; }
        public string name { get; set; }
    }

    public static class AppData
    {
        public static string AccessToken { get; set; }
        public static User CurrentUser { get; set; }
    }
}
