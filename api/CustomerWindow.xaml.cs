using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
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
    /// Interaction logic for CustomerWindow.xaml
    /// </summary>
    public partial class CustomerWindow : Window
    {
        private readonly HttpClient _client;
        private string SelectedSlotId = null!;
        private string SelectedServiceId = null!;
        public CustomerWindow()
        {
            InitializeComponent();
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AppData.AccessToken}");
            LoadDoctorsAndServices();
        }
        private async void LoadDoctorsAndServices()
        {
            try
            {
                var doctorRes = await _client.GetStringAsync("http://localhost:3000/doctors");
                var doctorResult = JsonConvert.DeserializeObject<ApiResponse<List<Doctor>>>(doctorRes);
                DoctorComboBox.ItemsSource = doctorResult.data;

                var serviceRes = await _client.GetStringAsync("http://localhost:3000/services");
                var serviceResult = JsonConvert.DeserializeObject<ApiResponse<List<Service>>>(serviceRes);
                ServiceComboBox.ItemsSource = serviceResult.data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message);
            }
        }

        private async void CheckSlots_Click(object sender, RoutedEventArgs e)
        {
            var doctorId = DoctorComboBox.SelectedValue?.ToString();
            var serviceId = ServiceComboBox.SelectedValue?.ToString();
            var date = DatePicker.SelectedDate?.ToString("yyyy-MM-dd");

            if (doctorId == null || serviceId == null || string.IsNullOrEmpty(date))
            {
                MessageBox.Show("Vui lòng chọn đủ thông tin.");
                return;
            }

            try
            {
                string url = $"http://localhost:3000/doctorSlots/{doctorId}/available-slots?serviceId={serviceId}&date={date}";
                var slotJson = await _client.GetStringAsync(url);
                var slotResult = JsonConvert.DeserializeObject<ApiResponse<List<DoctorSlot>>>(slotJson);

                // Hiển thị dạng "09:00 - 10:00"
                SlotListBox.ItemsSource = slotResult.data;
                SlotListBox.DisplayMemberPath = "TimeDisplay";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lấy lịch trống: " + ex.Message);
            }
        }
        private void SlotListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SlotListBox.SelectedItem is DoctorSlot selectedSlot)
            {
                SelectedSlotId = selectedSlot._id;
            }
        }

        private void ServiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServiceComboBox.SelectedItem is Service selectedService)
            {
                SelectedServiceId = selectedService._id;
            }
        }

        private async Task<string> GetPatientIdFromTokenAsync()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppData.AccessToken);

            var response = await client.PostAsync("http://localhost:3000/patients/by-token", null);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<PatientByTokenResponse>(json);
                return result.data._id;
            }
            else
            {
                MessageBox.Show($"Không lấy được thông tin bệnh nhân: {json}");
                return null;
            }
        }
        private void ViewAppointments_Click(object sender, RoutedEventArgs e)
        {
            var window = new AppointmentListWindow();
            window.ShowDialog();
        }
        private async Task RegisterAppointmentAsync(string doctorSlotId, string serviceId)
        {
            var patientId = await GetPatientIdFromTokenAsync();
            if (string.IsNullOrEmpty(patientId))
                return;

            var payload = new
            {
                patientID = patientId,
                doctorSlotID = new List<string> { doctorSlotId },
                serviceID = serviceId
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppData.AccessToken);

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:3000/appointments", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                
                MessageBox.Show("Đăng ký lịch khám thành công!");
            }
            else
            {
                MessageBox.Show($"Đăng ký thất bại:\n{responseText}");
            }
        }
        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedSlotId) || string.IsNullOrEmpty(SelectedServiceId))
            {
                MessageBox.Show("Vui lòng chọn slot và dịch vụ.");
                return;
            }

            await RegisterAppointmentAsync(SelectedSlotId, SelectedServiceId);
        }
    }
    public class ApiResponse<T>
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public T data { get; set; }
    }

    public class Doctor
    {
        public string _id { get; set; }
        public DoctorUser userID { get; set; }
    }

    public partial class DoctorUser
    {
        public string _id { get; set; }
        public string name { get; set; }
    }

    public class Service
    {
        public string _id { get; set; }
        public string name { get; set; }
    }
    public class DoctorSlot
    {
        public string _id { get; set; }
        public string doctorID { get; set; }
        public DateTime date { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string status { get; set; }
        public string TimeDisplay => $"{startTime:HH:mm} - {endTime:HH:mm}";
    }
    public class PatientByTokenResponse
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public Patient data { get; set; }
    }

    public class Patient
    {
        public string _id { get; set; }
        public string name { get; set; }
        public List<string> medicalRecordID { get; set; }
        public string userID { get; set; }
    }
}
