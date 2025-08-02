using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
    /// Interaction logic for DoctorScheduleWindow.xaml
    /// </summary>
    public partial class DoctorScheduleWindow : Window
    {
        private DateTime _startDate;
        private DateTime _endDate;
        private List<DoctorSchedule> _schedules;
        private DoctorSchedule _selectedSchedule;

        public DoctorScheduleWindow()
        {
            InitializeComponent();
            _startDate = DateTime.Today.StartOfWeek(DayOfWeek.Monday);
            _endDate = _startDate.AddDays(6);
            LoadScheduleAsync();
        }

        private async Task LoadScheduleAsync()
        {
            ScheduleList.Items.Clear();
            ConfirmButton.IsEnabled = false;
            WeekLabel.Text = $"{_startDate:dd/MM/yyyy} - {_endDate:dd/MM/yyyy}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppData.AccessToken);

            string url = $"http://localhost:3000/doctor-schedules/schedule-by-week/token?startDate={_startDate:yyyy-MM-dd}&endDate={_endDate:yyyy-MM-dd}";

            try
            {
                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<DoctorScheduleResponse>(json);
                    _schedules = result.data;

                    foreach (var schedule in _schedules)
                    {
                        ScheduleList.Items.Add($"{schedule.date:yyyy-MM-dd} - {schedule.shiftName} - {schedule.status}");
                    }
                }
                else
                {
                    MessageBox.Show($"Lỗi tải lịch: {json}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSchedule == null)
                return;

            string selectedShift = ((ComboBoxItem)ShiftComboBox.SelectedItem).Content.ToString();
            string url = $"http://localhost:3000/doctor-schedules/{_selectedSchedule._id}/confirm?shiftName={selectedShift}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppData.AccessToken);

            try
            {
                var response = await client.PatchAsync(url, null);
                var responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    StatusText.Text = "✅ Xác nhận thành công!";
                    await LoadScheduleAsync();
                }
                else
                {
                    MessageBox.Show($"❌ Xác nhận thất bại:\n{responseText}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xác nhận: " + ex.Message);
            }
        }

        private void ScheduleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ScheduleList.SelectedIndex;
            if (index >= 0 && index < _schedules.Count)
            {
                _selectedSchedule = _schedules[index];
                ConfirmButton.IsEnabled = !_selectedSchedule.isConfirmed;
            }
        }

        private async void PreviousWeek_Click(object sender, RoutedEventArgs e)
        {
            _startDate = _startDate.AddDays(-7);
            _endDate = _startDate.AddDays(6);
            await LoadScheduleAsync();
        }

        private async void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            _startDate = _startDate.AddDays(7);
            _endDate = _startDate.AddDays(6);
            await LoadScheduleAsync();
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }

    public class DoctorScheduleResponse
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public List<DoctorSchedule> data { get; set; }
    }

    public class DoctorSchedule
    {
        public string _id { get; set; }
        public string doctorID { get; set; }
        public DateTime date { get; set; }
        public string shiftName { get; set; }
        public string status { get; set; }
        public bool isConfirmed { get; set; }
    }
}
