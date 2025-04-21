using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RocketInterface13;
using System.Diagnostics;  // Process sınıfını kullanmak için gerekli
using System.IO.Ports;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using Newtonsoft.Json; // JSON verisini ayrıştırmak için


namespace RocketInterface13
{
    public partial class PayLoad : Form
    {
        SerialPort serialPort;
        GMapOverlay markersOverlay;
        GMarkerGoogle marker;
        GMapOverlay routesOverlay;
        List<PointLatLng> routePoints;

        public PayLoad()
        {
            InitializeComponent();
            InitializeMap();
            InitializeSerialPort();
        }

        private void InitializeMap()
        {
            gMapControl1.MapProvider = GMapProviders.GoogleMap; // Harita sağlayıcısını Google Map yap
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            
            gMapControl1.MinZoom = 2;
            gMapControl1.MaxZoom = 18;
            gMapControl1.Zoom = 12;
            gMapControl1.ShowCenter = false;

            // Katmanları oluştur
            markersOverlay = new GMapOverlay("markers");
            routesOverlay = new GMapOverlay("routes");
            routePoints = new List<PointLatLng>();

            gMapControl1.Overlays.Add(markersOverlay);
            gMapControl1.Overlays.Add(routesOverlay);
        }

        private void InitializeSerialPort()
        {
            serialPort = new SerialPort("COM3", 9600);  // COM portunu kendine göre ayarla
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.Open();
        }



        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadLine();
            this.Invoke(new Action(() => ProcessGPSData(data)));
        }

        private void ProcessGPSData(string data)
        {
            try
            {
                /*string[] parts = data.Split(' ');
                if (parts.Length < 4) return;

                double latitude = Convert.ToDouble(parts[1]);
                double longitude = Convert.ToDouble(parts[3]);
                */

                // JSON formatındaki veriyi ayrıştır
                var gpsData = JsonConvert.DeserializeObject<GPSData>(data);
                if (gpsData == null) return;

                double latitude = gpsData.Latitude;
                double longitude = gpsData.Longitude;


                labelenlem.Text = latitude.ToString();
                labelboylam.Text = longitude.ToString();
                

                UpdateMap(latitude, longitude);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void UpdateMap(double lat, double lng)
        {
            PointLatLng point = new PointLatLng(lat, lng);
            routePoints.Add(point); // Yeni noktayı ekle

            gMapControl1.Position = point; // Haritanın merkezini yeni noktaya kaydır

            // Harita işaretçisini güncelle
            markersOverlay.Markers.Clear();
            marker = new GMarkerGoogle(point, GMarkerGoogleType.red_dot);
            markersOverlay.Markers.Add(marker);

            // Rotayı güncelle
            routesOverlay.Routes.Clear();
            GMapRoute route = new GMapRoute(routePoints, "Tracking Route")
            {
                Stroke = new System.Drawing.Pen(System.Drawing.Color.Blue, 3) // Çizgi kalınlığı ve rengi
            };
            routesOverlay.Routes.Add(route);

            gMapControl1.Refresh(); // Haritayı güncelle
        
        }

        // GPS JSON verisi için bir sınıf oluşturduk
        private class GPSData
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        private void Buttongroundstation_Click(object sender, EventArgs e)
        {
            GroundStation groundstation = new GroundStation();
            groundstation.Show();
        }

        private void Buttonteststation_Click(object sender, EventArgs e)
        {
            TestStation teststation = new TestStation();
            teststation.Show();
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
    }
}
