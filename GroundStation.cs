using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using GMap.NET;
using System.Net.NetworkInformation;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;
//using System.Text.Json;
using System.Globalization; // GPS verilerini düzgün ayrıştırmak için
using System.Diagnostics;  // Process sınıfını kullanmak için gerekli


namespace RocketInterface13
{
    public partial class GroundStation : Form{
        
        private SerialPort serialPort = new SerialPort();
        private GMapControl gmapcontrol = new GMapControl();
        private GMapOverlay markersOverlay;
        private StringBuilder dataBuffer = new StringBuilder(); // Gelen veriyi biriktirmek için buffer
       

        
        

        public GroundStation(){
            
            
            InitializeComponent();

            // mevcut portlar
            var ports = SerialPort.GetPortNames();
            port1radioButton.Enabled = ports.Contains("COM5");
            port2radioButton.Enabled = ports.Contains("COM6");
            port3radioButton.Enabled = ports.Contains("COM7");
            port4radioButton.Enabled = ports.Contains("COM8");

            // portların durumlarını başlatır
            label1port.Text = "Bağlantı Yok";
            label2port.Text = "Bağlantı Yok";
            label3port.Text = "Bağlantı Yok";
            label4port.Text = "Bağlantı Yok";

            //InitializeMap(); //haritayı başlatır
            
           

        }

        

        /*private void InitializeMap(){

            // GMap.NET temel ayarları
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            // Harita kontrolünü oluştur ve ayarla
            gmapcontrol = new GMapControl{

                Size = new Size(283, 151),
                Location = new Point (476,2),
                //MapProvider = GMapProviders.GoogleMap,
                MapProvider = GMapProviders.OpenStreetMap, //  GoogleMap yerine OSM 
                MinZoom = 5,
                MaxZoom = 18,
                Zoom = 15,
            };


            // İşaretçi katmanını oluştur
            markersOverlay = new GMapOverlay("markers");
            gmapcontrol.Overlays.Add(markersOverlay);
            this.Controls.Add(gmapcontrol);

        }



        public void UpdateMarker(double latitude, double longitude){
            markersOverlay.Markers.Clear();
            var marker = new GMarkerGoogle(new PointLatLng(latitude, longitude), GMarkerGoogleType.red_dot);
            markersOverlay.Markers.Add(marker);
            gmapcontrol.Position = new PointLatLng(latitude, longitude);
            gmapcontrol.Zoom = gmapcontrol.Zoom; // Haritanın yeniden çizilmesini sağlıyor
        }
        */
        
        private void UpdateConnectionStatus(string status){
            Color statusColor = status == "Bağlandı" ? Color.Green : Color.Red;

            // Port durumunu güncelle
            if (port1radioButton.Checked){
                label1port.Text = status;
                label1port.ForeColor = statusColor;
            }
            else if (port2radioButton.Checked){
                label2port.Text = status;
                label2port.ForeColor = statusColor;
            }
            else if (port3radioButton.Checked){
                label3port.Text = status;
                label3port.ForeColor = statusColor;
            }
            else if (port4radioButton.Checked){
                label4port.Text = status;
                label4port.ForeColor = statusColor;
            }

        }


        private void Connectbutton_Click(object sender, EventArgs e){
            try
            {
                if (serialPort.IsOpen){
                    serialPort.Close();
                    UpdateConnectionStatus("Bağlantı Yok");
                }
                else{
                    // Hangi port seçilmişse ona göre COM port bağla
                    if (port1radioButton.Checked && serialPort.PortName != "COM5")
                        serialPort.PortName = "COM5";
                    else if (port2radioButton.Checked && serialPort.PortName != "COM6")
                        serialPort.PortName = "COM6";
                    else if (port3radioButton.Checked && serialPort.PortName != "COM7")
                        serialPort.PortName = "COM7";
                    else if (port4radioButton.Checked && serialPort.PortName != "COM8")
                        serialPort.PortName = "COM8";
                    else{
                        MessageBox.Show("Lütfen bir port seçin veya geçerli bir port seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // pport ayarları
                    serialPort.BaudRate = 9600;
                    serialPort.DataBits = 8;
                    serialPort.Parity = Parity.None; // verinin doğruluğunu kontrol ediyo
                    serialPort.StopBits = StopBits.One; // stop bitini belirtir

                    serialPort.DataReceived += SerialPort_DataReceived; // Veri alındığında çağrılacak fonksiyon
                    serialPort.Open();
                    UpdateConnectionStatus("Bağlandı");

                }


            }

            
            catch (UnauthorizedAccessException){
                MessageBox.Show("Seçilen porta erişim sağlanamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex){
                MessageBox.Show($"Bağlantı sırasında bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        //Parasut aktiflestirme
        private void Button400h600_Click(object sender, EventArgs e){
            labelparasut2pasif.Text = "Aktif";
            labelparasut2pasif.ForeColor = Color.Green;
            pictureBoxparasut2iconclose.Image = Properties.Resources.tik;
        }

        private void Buttonapogee_Click(object sender, EventArgs e){
            labelparasüt1pasif.Text = "Aktif";
            labelparasüt1pasif.ForeColor = Color.Green;
            pictureBoxparasut1iconclose.Image = Properties.Resources.tik;
        }


        
        //formlar arası geçiş
        private void Teststationbutton_Click(object sender, EventArgs e){
            TestStation testStation = new TestStation();
            testStation.Show();
 
        }
        private void Payloadbutton_Click(object sender, EventArgs e)
        {
            PayLoad payLoad = new PayLoad();
            payLoad.Show();
            this.Hide();
           
        }


        private void Connect2button_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen){
                serialPort.Close();
                Connect2button.Text = "Bağlan";
            }
            else{
                serialPort.PortName = comboBoxPorts.SelectedItem.ToString();
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.Open();
                Connect2button.Text = "Bağlantıyı Kes";
            }
        }

        public class TelemetryData{
            public float GyroX { get; set; }
            public float GyroY { get; set; }
            public float GyroZ { get; set; }
            public float AccX { get; set; }
            public float AccY { get; set; }
            public float AccZ { get; set; }
            public float Temperature { get; set; }
            public float Pressure { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }

        }

        private void GroundStation_Load(object sender, EventArgs e){
            // COM portlarını al
            var ports = SerialPort.GetPortNames();

            // ComboBox'a portları ekle
            comboBoxPorts.Items.Clear();  // Var olan öğeleri temizle
            comboBoxPorts.Items.AddRange(ports);  // Yeni portları ekle

            // Eğer hiç port yoksa, bir uyarı göster
            if (ports.Length == 0){
                MessageBox.Show("Mevcut COM portu bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

         private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
         {
             try
             {
                 // Seri porttan gelen ham veriyi oku
                 string data = serialPort.ReadLine();
                 dataBuffer.Append(data); // Buffer'a ekle

                 // Buffer içinde her JSON nesnesi '\n' ile ayrılıyor varsayımı
                 while (dataBuffer.ToString().Contains("\n"))
                 {
                     string completeData = dataBuffer.ToString();
                     int endIndex = completeData.IndexOf("\n"); // İlk satırın sonunu bul
                     string jsonLine = completeData.Substring(0, endIndex).Trim(); // Satırı ayır ve temizle
                     dataBuffer.Remove(0, endIndex + 1); // İşlenen veriyi buffer'dan kaldır

                     // JSON'u doğrula ve ayrıştır
                     if (IsValidJson(jsonLine))
                     {
                         var telemetry = JsonConvert.DeserializeObject<TelemetryData>(jsonLine);

                         // UI elemanlarını güncellemek için Invoke kullan
                         this.Invoke(new Action(() =>
                         {
                             UpdateTelemetryUI(telemetry); // Gelen veriyi UI'ye yansıt

                             // Eğer GPS verisi mevcutsa, haritada güncelle
                             /*if (telemetry.Latitude != 0 && telemetry.Longitude != 0 && !double.IsNaN(telemetry.Latitude) && !double.IsNaN(telemetry.Longitude))
                             {
                                 UpdateMarker(telemetry.Latitude, telemetry.Longitude); // Haritayı güncelle
                             }
                             */
                         }));
                     }
                     else
                     {
                         // Geçersiz JSON durumunda log yazdır
                         Console.WriteLine($"Geçersiz JSON: {jsonLine}");
                     }
                 }
             }
             catch (Exception ex)
             {
                 // Genel hata yönetimi
                 MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }


         }
        
        private bool IsValidJson(string data)
        {
            try
            {
                JsonConvert.DeserializeObject(data);
                return true; // Geçerli JSON
            }
            catch
            {
                return false; // Geçersiz JSON
            }
        }


        private void UpdateTelemetryUI(TelemetryData telemetry)
        {
            // Gelen telemetri verilerini UI elemanlarına güncelle
            labelGyroX.Text = $"Gyro X: {telemetry.GyroX}";
            labelGyroY.Text = $"Gyro Y: {telemetry.GyroY}";
            labelGyroZ.Text = $"Gyro Z: {telemetry.GyroZ}";
            labelAccX.Text = $"Acc X: {telemetry.AccX}";
            labelAccY.Text = $"Acc Y: {telemetry.AccY}";
            labelAccZ.Text = $"Acc Z: {telemetry.AccZ}";
            labelenlemgps.Text = $"Enlem: {telemetry.Latitude.ToString("F6", CultureInfo.InvariantCulture)}";
            labelboylamgps.Text = $"Boylam: {telemetry.Longitude.ToString("F6", CultureInfo.InvariantCulture)}";



            // Grafiği güncelle
            chartTelemetry.Series["Sıcaklık Grafiği"].Points.AddY(telemetry.Temperature);
            chartTelemetry.Series["Basınç Grafiği"].Points.AddY(telemetry.Pressure);

            // Grafikte çok fazla nokta varsa eskileri temizle
            if (chartTelemetry.Series["Sıcaklık Grafiği"].Points.Count > 50)
                chartTelemetry.Series["Sıcaklık Grafiği"].Points.RemoveAt(0);

            if (chartTelemetry.Series["Basınç Grafiği"].Points.Count > 50)
                chartTelemetry.Series["Basınç Grafiği"].Points.RemoveAt(0);
        }

        //SOSYAL MEDYA 
        private void PictureBoxlinkedin_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.linkedin.com/company/ostimtech-tak%C4%B1m%C4%B1/posts/?feedView=all");
        }

        private void PictureBoxinstagram_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.instagram.com/roketostimtech/");
        }

        private void PictureBoxyoutube_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.youtube.com/@OstimTechRoket");
        }

        private void PictureBoxgithub_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/eren-gokce/racoon.git");
        }

        private void Buttongpsconnect_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort = new SerialPort("COM6", 9600); // GPS modülünün bağlı olduğu port
                serialPort.Open(); // Bağlantıyı aç

                MessageBox.Show("GPS modülüne bağlandı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bağlantı hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        
    }


}
