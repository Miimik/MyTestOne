using System.Drawing;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace MyTestOne
{     
    public class GMapMarkerImage : GMapMarker
    {
        private static Image _image = new Bitmap(Properties.Resources.Mapker);

        public bool Selected { get; set; }

        public GMapMarkerImage(PointLatLng p)
           : base(p)
        {
            IsHitTestVisible = true;
            Size = new System.Drawing.Size(_image.Width, _image.Height);
            Offset = new Point(-Size.Width / 2, -Size.Height / 2);
        }

        public override void OnRender(Graphics g)
        {
            g.DrawImage(_image, LocalPosition);
            if (Selected)
                g.DrawRectangle(Pens.Red, LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
        }

        public override void Dispose()
        {
            _image.Dispose();

            base.Dispose();
        }
    }
}
