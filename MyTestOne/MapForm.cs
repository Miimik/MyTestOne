using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace MyTestOne
{
    public partial class MapForm : Form
    {
        private readonly GMapOverlay _markers = new GMapOverlay("objects");
        private GMapMarkerImage _currentMarker;
        private bool _isMouseDown;

        public MapForm()
        {
            InitializeComponent();

            if (!GMapControl.IsDesignerHosted)
            {      
                gMapControl1.MapProvider = GMapProviders.GoogleMap;
                gMapControl1.Position = new PointLatLng(55, 25);
                gMapControl1.DragButton = MouseButtons.Left;
                gMapControl1.MinZoom = 0;
                gMapControl1.MaxZoom = 24;
                gMapControl1.Zoom = 9;

                gMapControl1.MouseDown += GMapControl1_MouseDown;
                gMapControl1.MouseUp += GMapControl1_MouseUp;
                gMapControl1.MouseMove += GMapControl1_MouseMove;
                gMapControl1.OnMarkerEnter += GMapControl1_OnMarkerEnter;
                gMapControl1.OnMarkerLeave += GMapControl1_OnMarkerLeave;

                GoogleMapProvider.Instance.ApiKey = "AIzaSyCoz0fVRmn6L-zZuLXnIXtRcGLKf2PHI5Q";

                gMapControl1.Overlays.Add(_markers);

                LoadMarkers();
            }
        }

        private void GMapControl1_OnMarkerLeave(GMapMarker item)
        {
            if (!_isMouseDown && _currentMarker != null)
            {
                _currentMarker.Selected = false;
                _currentMarker = null;
                gMapControl1.Refresh();
            }
        }

        private void GMapControl1_OnMarkerEnter(GMapMarker item)
        {
            if (!_isMouseDown)
            {
                _currentMarker = item as GMapMarkerImage;
                _currentMarker.Selected = true;
            }
        }

        private void GMapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _isMouseDown)
            {
                if (_currentMarker != null)
                {
                    _currentMarker.Position = gMapControl1.FromLocalToLatLng(e.X, e.Y);
                    gMapControl1.Refresh();
                }
            }
        }

        private void GMapControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_currentMarker != null)
                    UpdateMarker(_currentMarker);
                _isMouseDown = false;
            }
        }

        private void UpdateMarker(GMapMarkerImage marker)
        {
            using (SqlConnection myConnection = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                myConnection.Open();
                using (SqlCommand myCommand = new SqlCommand("UPDATE Markers SET Lat=@lat, Lng=@Lng WHERE ID = @Id", myConnection))
                {
                    myCommand.Parameters.AddWithValue("@Id", marker.Tag);
                    myCommand.Parameters.AddWithValue("@Lat", marker.Position.Lat);
                    myCommand.Parameters.AddWithValue("@Lng", marker.Position.Lng);
                    myCommand.ExecuteNonQuery();
                }
            }
        }

        private void GMapControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDown = true;
            }
            if (e.Button == MouseButtons.Right)
            {
                var marker = new GMapMarkerImage(gMapControl1.FromLocalToLatLng(e.X, e.Y));
                AddMarketToDb(marker);
                _markers.Markers.Add(marker);
            }
        }

        private void AddMarketToDb(GMapMarkerImage marker)
        {
            using (SqlConnection myConnection = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                myConnection.Open();
                using (SqlCommand myCommand = new SqlCommand("INSERT INTO Markers(Lat, Lng) VALUES(@Lat, @Lng); SELECT SCOPE_IDENTITY()", myConnection))
                {
                    myCommand.Parameters.AddWithValue("@Lat", marker.Position.Lat);
                    myCommand.Parameters.AddWithValue("@Lng", marker.Position.Lng);
                    var id = Convert.ToInt32(myCommand.ExecuteScalar());
                    marker.Tag = id;
                }
            }
        }

        private void LoadMarkers()
        {
            using (SqlConnection myConnection = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                myConnection.Open();
                using (SqlCommand myCommand = new SqlCommand("SELECT Id, Lng, Lat FROM Markers", myConnection))
                {
                    SqlDataReader dataReader = myCommand.ExecuteReader();
                    while (dataReader.Read())
                    {
                        var marker = new GMapMarkerImage(
                            new PointLatLng(
                                Convert.ToDouble(dataReader["Lat"]),
                                Convert.ToDouble(dataReader["Lng"])));
                        marker.Tag = Convert.ToInt32(dataReader["Id"]);
                        _markers.Markers.Add(marker);
                    }
                }
            }
        }
    }
}

