using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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
    /// Interaction logic for AppointmentListWindow.xaml
    /// </summary>
    public partial class AppointmentListWindow : Window
    {
        public AppointmentListWindow()
        {
            InitializeComponent();
            LoadAppointments();
        }
        private async void LoadAppointments()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppData.AccessToken);
                var res = await client.GetStringAsync("http://localhost:3000/appointments/patienttoken");
                var result = JsonConvert.DeserializeObject<AppointmentListResponse>(res);
                AppointmentListBox.ItemsSource = result.data;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải lịch khám: " + ex.Message);
            }
        }
        private async void PayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string appointmentId)
            {
                try
                {
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppData.AccessToken);

                    var payload = new { appointmentID = appointmentId };
                    var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("http://localhost:3000/payments/wallet", content);
                    var result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Thanh toán thành công!");
                        LoadAppointments(); // reload nếu bạn có hàm này
                    }
                    else
                    {
                        MessageBox.Show($"Thanh toán thất bại:\n{result}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi thanh toán: " + ex.Message);
                }
            }
        }

    }

    public class AppointmentListResponse
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public List<AppointmentItem> data { get; set; }
    }

    public class AppointmentItem
    {
        public string _id { get; set; }
        public AppointmentDoctor doctorID { get; set; }
        public List<string> doctorSlotID { get; set; }
        public AppointmentPatient patientID { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        public AppointmentService serviceID { get; set; }
        public DateTime startTime { get; set; }

        public string Summary =>
            $"{serviceID?.name} - Bác sĩ: {doctorID?.userID?.name} - {startTime:HH:mm dd/MM/yyyy} - Trạng thái: {status}";
    }


    public class AppointmentDoctor
    {
        public string _id { get; set; }
        public AppointmentUser userID { get; set; }
        public string room { get; set; }
    }

    public class AppointmentPatient
    {
        public string _id { get; set; }
        public AppointmentUser userID { get; set; }
        public string name { get; set; }
    }

    public class AppointmentUser
    {
        public string _id { get; set; }
        public string name { get; set; }
    }

    public class AppointmentService
    {
        public string _id { get; set; }
        public string name { get; set; }
        public int price { get; set; }
        public int durationMinutes { get; set; }
    }

}

