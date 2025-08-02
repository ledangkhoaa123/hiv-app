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
    /// Interaction logic for ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private readonly HttpClient _client;
        public ManagerWindow()
        {
            InitializeComponent();
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AppData.AccessToken}");
            LoadDoctors();
        }
        private async void LoadDoctors()
        {
            try
            {
                var doctorRes = await _client.GetStringAsync("http://localhost:3000/doctors");
                var doctorResult = JsonConvert.DeserializeObject<ApiResponse<List<Doctor>>>(doctorRes);
                DoctorComboBox.ItemsSource = doctorResult.data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách bác sĩ: " + ex.Message);
            }
        }

        private async void CreateSchedule_Click(object sender, RoutedEventArgs e)
        {
            string doctorId = DoctorComboBox.SelectedValue?.ToString();
            string date = DatePicker.SelectedDate?.ToString("yyyy-MM-dd");

            if (doctorId == null || string.IsNullOrEmpty(date))
            {
                MessageBox.Show("Vui lòng chọn đầy đủ bác sĩ và ngày khám.");
                return;
            }

            var payload = new
            {
                doctorID = new[] { doctorId },
                dates = new[] { date }
            };

            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.PostAsync("http://localhost:3000/doctor-schedules", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Tạo lịch khám thành công!");
                }
                else
                {
                    var err = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                    MessageBox.Show($"Tạo lịch thất bại: {err.message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gọi API: " + ex.Message);
            }
        }
    }
    public class ErrorResponse
    {
        public string message { get; set; }
        public string error { get; set; }
        public int statusCode { get; set; }
    }
}
