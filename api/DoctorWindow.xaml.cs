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
    /// Interaction logic for DoctorWindow.xaml
    /// </summary>
    public partial class DoctorWindow : Window
    {
        public DoctorWindow()
        {
            InitializeComponent();
            LoadDoctorInfoAsync();
        }
        private async void LoadDoctorInfoAsync()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppData.AccessToken);

            try
            {
                var response = await client.PostAsync("http://localhost:3000/doctors/token", null);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<DoctorTokenResponse>(json);
                    var doctor = result.data;

                    NameText.Text = doctor.userID.name;
                    EmailText.Text = $"Email: {doctor.userID.email}";
                    PhoneText.Text = $"SĐT: {doctor.userID.phone}";
                    RoomText.Text = $"Phòng: {doctor.room}";
                    SpecializationText.Text = $"Chuyên môn: {doctor.specializations}";
                    DegreeText.Text = $"Bằng cấp: {doctor.degrees}";

                    ExperienceList.Items.Clear();
                    foreach (var exp in doctor.experiences)
                    {
                        ExperienceList.Items.Add($"- {exp}");
                    }

                    if (!string.IsNullOrEmpty(doctor.avatarURL))
                    {
                        AvatarImage.Source = new BitmapImage(new Uri(doctor.avatarURL));
                    }
                }
                else
                {
                    MessageBox.Show($"Không lấy được thông tin bác sĩ:\n{json}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu bác sĩ: " + ex.Message);
            }
        }
        private void CreateSchedule_Click(object sender, RoutedEventArgs e)
        {
            DoctorScheduleWindow scheduleWindow = new DoctorScheduleWindow();
            scheduleWindow.Show();
            this.Close(); // hoặc this.Hide(); nếu bạn không muốn đóng cửa sổ hiện tại
        }
    }
    public class DoctorTokenResponse
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public DoctorData data { get; set; }
    }

    public class DoctorData
    {
        public string _id { get; set; }
        public DoctorUser userID { get; set; }
        public string room { get; set; }
        public string specializations { get; set; }
        public string degrees { get; set; }
        public bool isActive { get; set; }
        public List<string> experiences { get; set; }
        public string avatarURL { get; set; }
    }

    public partial class DoctorUser
    {
        public string email { get; set; }
        public string phone { get; set; }
    }
}
