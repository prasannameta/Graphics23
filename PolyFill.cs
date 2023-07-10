using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace A25 {
   class PolyFill {

      public PolyFill() {
         ConstructPoints (File.ReadAllLines ("C:/etc/leaf-fill.txt"));
         ScanLineFillPolygon ();
      }

      public void AddLine (int x0, int y0, int x1, int y1) {
         Lines.Add (new Line () {
             Stroke = LineStroke, StrokeThickness = 2, X1 = x0, Y1 = y0, X2 = x1, Y2 = y1
         });
      }

      public void Fill (GrayBMP.GrayBMP bmp, int color) {
         
      }

      public void ConstructPoints (string[] points) {
         foreach (var line in points) {
            var w = line.Split (' ');
            Point p1 = new (int.Parse (w[0]), int.Parse (w[1]));
            Point p2 = new (int.Parse (w[2]), int.Parse (w[3]));
            Points.Add (p1); Points.Add (p2);
         }
         for (int i = 0; i < Points.Count; i += 2) {
            AddLine (Points[i].X, Points[i].Y,
               Points[i + 1].X, Points[i + 1].Y);
            //mCanvas.Children.Add (line);
         }
      }

      public void ScanLineFillPolygon () {
         var pts = Points.OrderBy (x => x.Y).ToList ();
         var ymin = pts.FirstOrDefault ();
         var ymax = pts.LastOrDefault ();
         List<IEnumerable<Point>> allPts = new ();
         List<Point> interSectionPts = new ();
         for (int y = (int)ymin.Y; (int)y < (int)ymax.Y; y++) {
            int x1, x2, y1, y2;
            for (int k = 0; k < pts.Count - 1; k++) {
               x1 = (int)pts[k].X; x2 = (int)pts[k + 1].X; y1 = (int)pts[k].Y; y2 = (int)pts[k + 1].Y;
               double dX = x2 - x1, dY = y2 - y1;
               var X = (int)Math.Round (x1 + dX / dY * (y - y1));
               if ((y1 <= y && y2 > y) || (y2 <= y && y1 > y)) interSectionPts.Add (new Point (X, y));
            }
            x1 = (int)ymax.X; y1 = (int)ymax.Y;
            x2 = (int)ymin.X; y2 = (int)ymin.Y;
            if ((y1 <= y && y2 > y) || (y2 <= y && y1 > y))
               interSectionPts.Add (new Point ((x1 + (x2 - x1) / (y2 - y1) * (y - y1)), y));
         }
         var polygonpoints = Points.Except (interSectionPts).ToList ();
         for (int i = 0; i < interSectionPts.Count - 1; i += 2) {
            AddLine (interSectionPts[i].X, interSectionPts[i].Y,
               interSectionPts[i + 1].X, interSectionPts[i + 1].Y);
            //mCanvas.Children.Add (line);
         }
      }

      List<Point> Points = new ();
      List<Line> Lines = new ();
      public Brush LineStroke => Brushes.Gray;
   }
}
