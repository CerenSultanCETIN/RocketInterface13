using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace RocketInterface13{
    public partial class GroundStation : Form{
        
        private SerialPort serialPort = new SerialPort();
        private GMapControl gmapcontrol = new GMapControl();
        private GMapOverlay markersOverlay;

        public GroundStation(){
            
            
            InitializeComponent();

            // mevcut portlar
            var ports = SerialPort.GetPortNames();
            port1radioButton.Enabled = ports.Contains("COM1");
            port2radioButton.Enabled = ports.Contains("COM2");
            port3radioButton.Enabled = ports.Contains("COM3");
            port4radioButton.Enabled = ports.Contains("COM4");

            // portların durumlarını başlatır
            label1port.Text = "Bağlantı Yok";
            label2port.Text = "Bağlantı Yok";
            label3port.Text = "Bağlantı Yok";
            label4port.Text = "Bağlantı Yok";

            InitializeMap(); //haritayı başlatır

        }

        private void InitializeMap(){

            
            
            // Harita kontrolünü oluştur ve ayarla
            gmapcontrol = new GMapControl{
                Size = new Size(258,150),
                Location = new Point (463,3),
                MapProvider = GMapProviders.GoogleMap,
                Position = new PointLatLng(39.92077, 32.85411), // Başlangıç konumu rastgele ankaranın kordinatları.
                MinZoom = 5,
                MaxZoom = 18,
                Zoom = 10,
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
        }

        
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
                    if (port1radioButton.Checked && serialPort.PortName != "COM1")
                        serialPort.PortName = "COM1";
                    else if (port2radioButton.Checked && serialPort.PortName != "COM2")
                        serialPort.PortName = "COM2";
                    else if (port3radioButton.Checked && serialPort.PortName != "COM3")
                        serialPort.PortName = "COM3";
                    else if (port4radioButton.Checked && serialPort.PortName != "COM4")
                        serialPort.PortName = "COM4";
                    else{
                        MessageBox.Show("Lütfen bir port seçin veya geçerli bir port seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // pport ayarları
                    serialPort.BaudRate = 9600;
                    serialPort.DataBits = 8;
                    serialPort.Parity = Parity.None; // verinin doğruluğunu kontrol ediyo
                    serialPort.StopBits = StopBits.One; // stop bitini belirtir

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

        private void Buttonaa_Click(object sender, EventArgs e){
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

       
        
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e){
            string data = serialPort.ReadLine();
            Console.WriteLine("Alınan Veri: " + data); // Gelen veriyi yazdır

            // JSON verisini ayrıştır
            try{
                var telemetry = JsonConvert.DeserializeObject<TelemetryData>(data);

                // Ekrana yazdır
                this.Invoke(new Action(() => {
                    labelGyroX.Text = $"Gyro X: {telemetry.GyroX}";
                    labelGyroY.Text = $"Gyro Y: {telemetry.GyroY}";
                    labelGyroZ.Text = $"Gyro Z: {telemetry.GyroZ}";
                    labelAccX.Text = $"Acc X: {telemetry.AccX}";
                    labelAccY.Text = $"Acc Y: {telemetry.AccY}";
                    labelAccZ.Text = $"Acc Z: {telemetry.AccZ}";

                    // Grafiği güncelle
                    chartTelemetry.Series["Series1"].Points.AddY(telemetry.Temperature);
                    chartTelemetry.Series["Series2"].Points.AddY(telemetry.Pressure);

                    // Eski verileri temizleme kodunu buraya ekle
                    if (chartTelemetry.Series["Series1"].Points.Count > 50){
                        chartTelemetry.Series["Series1"].Points.RemoveAt(0);
                    }
                    if (chartTelemetry.Series["Series2"].Points.Count > 50){
                        chartTelemetry.Series["Series2"].Points.RemoveAt(0);
                    }

                }));
            }
            
            catch (Exception ex){
                // JSON ayrıştırma hatası
                MessageBox.Show($"JSON Ayrıştırma Hatası: {ex.Message}");
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



    }


}
