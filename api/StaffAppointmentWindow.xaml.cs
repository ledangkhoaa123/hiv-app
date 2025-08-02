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
    /// Interaction logic for StaffAppointmentWindow.xaml
    /// </summary>
    public partial class StaffAppointmentWindow : Window
    {
        public StaffAppointmentWindow()
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
                var res = await client.GetStringAsync("http://localhost:3000/appointments");
                var result = JsonConvert.DeserializeObject<StaffAppointmentResponse>(res);

                var pendingAppointments = result.data
                    .Where(a => a.status.Trim() == "Đang xét duyệt")
                    .Select(a => new StaffAppointmentDisplay
                    {
                        PatientName = a.patientID?.userID?.name ?? a.patientID?.name ?? "Không rõ",
                        AppointmentId = a._id,
                        DoctorName = a.doctorSlotID?.FirstOrDefault()?.doctorID?.userID?.name ?? "Không rõ",
                        ServiceName = a.serviceID?.name ?? "Không rõ",
                        Time = a.startTime.ToString("HH:mm dd/MM/yyyy"),
                        Status = a.status
                    })
                    .ToList();

                AppointmentDataGrid.ItemsSource = pendingAppointments;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải lịch khám: " + ex.Message);
            }
        }
        private async void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string appointmentId)
            {
                try
                {
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppData.AccessToken);

                    var response = await client.PatchAsync($"http://localhost:3000/appointments/{appointmentId}/confirm", null);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Duyệt lịch thành công!");
                        LoadAppointments(); // Refresh
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Lỗi khi duyệt: {error}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi duyệt lịch: " + ex.Message);
                }
            }
        }

        private async void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string appointmentId)
            {
                var reason = Microsoft.VisualBasic.Interaction.InputBox("Nhập lý do từ chối:", "Từ chối lịch hẹn", "Lý do...");
                if (string.IsNullOrWhiteSpace(reason)) return;

                try
                {
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppData.AccessToken);

                    var payload = new
                    {
                        appoinmentId = appointmentId,
                        reason = reason
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("http://localhost:3000/appointments/cancle/appointment", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Từ chối lịch thành công!");
                        LoadAppointments(); // Refresh
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Lỗi khi từ chối: {error}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi từ chối lịch: " + ex.Message);
                }
            }
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAppointments();
        }
    }
    public class StaffAppointmentResponse
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public List<StaffAppointmentItem> data { get; set; }
    }

    public class StaffAppointmentItem
    {
        public string _id { get; set; }
        public string status { get; set; }
        public AppointmentService serviceID { get; set; }
        public List<StaffDoctorSlot> doctorSlotID { get; set; }
        public DateTime startTime { get; set; }
        public StaffPatient patientID { get; set; }

        public string Summary =>
            $"{serviceID?.name} - {startTime:HH:mm dd/MM/yyyy} - Trạng thái: {status}";
    }

    public class StaffDoctorSlot
    {
        public string _id { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string status { get; set; }
        public StaffDoctor doctorID { get; set; }

    }
    public class StaffAppointmentDisplay
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string ServiceName { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
        public string AppointmentId { get; set; }

    }
    public class StaffDoctor
    {
        public string _id { get; set; }
        public StaffUser userID { get; set; }
    }
    public class StaffUser
    {
        public string _id { get; set; }
        public string name { get; set; }
    }
    public class StaffPatient
    {
        public string _id { get; set; }
        public StaffUser userID { get; set; }
        public string name { get; set; }
    }

}
