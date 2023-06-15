using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace A25;

class MyWindow : Window {
   public MyWindow () {
      Width = 800; Height = 600;
      Left = 50; Top = 50;
      WindowStyle = WindowStyle.None;
      Image image = new Image () {
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode (image, EdgeMode.Aliased);

      mBmp = new WriteableBitmap ((int)Width, (int)Height,
         96, 96, PixelFormats.Gray8, null);
      mStride = mBmp.BackBufferStride;
      image.Source = mBmp;
      Content = image;
      MouseDown += OnMouseDown;
      MouseUp += OnMouseUp;
      MouseMove += OnMouseMove;
      DrawMandelbrot (-0.5, 0, 1);
   }

   void DrawMandelbrot (double xc, double yc, double zoom) {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         int dx = mBmp.PixelWidth, dy = mBmp.PixelHeight;
         double step = 2.0 / dy / zoom;
         double x1 = xc - step * dx / 2, y1 = yc + step * dy / 2;
         for (int x = 0; x < dx; x++) {
            for (int y = 0; y < dy; y++) {
               Complex c = new Complex (x1 + x * step, y1 - y * step);
               SetPixel (x, y, Escape (c));
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, dx, dy));
      } finally {
         mBmp.Unlock ();
      }
   }

   byte Escape (Complex c) {
      Complex z = Complex.Zero;
      for (int i = 1; i < 32; i++) {
         if (z.NormSq > 4) return (byte)(i * 8);
         z = z * z + c;
      }
      return 0;
   }

   void OnMouseDown (object sender, MouseButtonEventArgs e) {
      if (e.LeftButton == MouseButtonState.Pressed) {
         if (N1.X == 0 || N1.Y == 0) N1 = e.GetPosition (this);
         var pt = e.GetPosition (this);
         if (N1.X != pt.X || N1.Y != pt.Y) N2 = pt;
         DrawLine (N1, N2);
      }
   }

   void OnMouseUp (object sender, MouseButtonEventArgs e) {
      if (e.LeftButton == MouseButtonState.Released)
         N1 = N2 = new Point (0, 0);
   }

   void OnMouseMove (object sender, MouseEventArgs e) {
      try {
         if (N1.X >= 0 && N1.Y >= 0) {
            var pt = e.GetPosition (this);
            points.Add (pt);
         }
      } finally { }
   }

   void DrawLine (Point n1, Point n2) {
      if (N1.X == 0 || N2.X == 0) return;
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         //m = (y2 - y1) / (x2 - x1);
         SetPixel ((int)n1.X, (int)n1.Y, 255);
         mBmp.AddDirtyRect (new Int32Rect ((int)n1.X, (int)n1.Y, 1, 1));
         foreach (var pt in points) {
            SetPixel ((int)pt.X, (int)pt.Y, 255);
            mBmp.AddDirtyRect (new Int32Rect ((int)pt.X, (int)pt.Y, 1, 1));
         }
         SetPixel ((int)n2.X, (int)n2.Y, 255);
         mBmp.AddDirtyRect (new Int32Rect ((int)n2.X, (int)n2.Y, 1, 1));
      } finally {
         mBmp.Unlock ();
         N1 = N2 = new Point (0, 0);
      }

   }

   void DrawGraySquare () {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         for (int x = 0; x <= 255; x++) {
            for (int y = 0; y <= 255; y++) {
               SetPixel (x, y, (byte)x);
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, 256, 256));
      } finally {
         mBmp.Unlock ();
      }
   }

   void SetPixel (int x, int y, byte gray) {
      unsafe {
         var ptr = (byte*)(mBase + y * mStride + x);
         *ptr = gray;
      }
   }

   WriteableBitmap mBmp;
   int mStride;
   nint mBase;
   Point N1, N2;
   List<Point> points = new List<Point> ();
}

internal class Program {
   [STAThread]
   static void Main (string[] args) {
      Window w = new MyWindow ();
      w.Show ();
      Application app = new Application ();
      app.Run ();
   }
}
