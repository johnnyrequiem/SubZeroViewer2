using Gtk;
using NPlot.Gtk;
using ZedGraph;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using NPlot;
using System.Xml;

namespace SubZeroViewer2
{
	public class CGraph
	{
		private string _title = "Sensor Graph";
		private string _XAxisLabel = "X-Axis Label";
		private string _YAxisLabel = "Y-Axis Label";
		private string _XAxisCode = "X-AxisCode";
		private string _YAxisCode = "Y-AxisCode";

		private ArrayList _c_device_logfile_entries = null;
		private ArrayList _ar_x_axis_data = null;
		private ArrayList _ar_y_axis_data = null;

		private LinePlot _linePlot = new LinePlot();
		private NPlot.Legend _linePlotLegend = new NPlot.Legend();
		private CUtillity cutil = new CUtillity();

		public CGraph ()
		{
		}

		public CGraph ( string title, string x_axisLabel, string y_axisLabel, string x_axisCode, string y_axisCode ) {
			_title = title;
			_XAxisLabel = x_axisLabel;
			_XAxisCode = x_axisCode;
			_YAxisLabel = y_axisLabel;
			_YAxisCode = y_axisCode;
		}

		public CGraph ( ArrayList deviceLogFileEntries ) {
			_c_device_logfile_entries = deviceLogFileEntries;
		}

		public ArrayList CGRAPH_X_AxisData {
			set {
				_ar_x_axis_data = value;
			}
		}

		public ArrayList CGRAPH_Y_AxisData {
			set {
				_ar_y_axis_data = value;
			}
		}

		public string Title {
			get {
				return _title;
			}
			set {
				_title = value;
			}
		}

		public string XAxisLabel {
			get {
				return _XAxisLabel;
			}
			set {
				_XAxisLabel = value;
			}
		}

		public string YAxisLabel {
			get {
				return _YAxisLabel;
			}
			set {
				_YAxisLabel = value;
			}
		}

		public string XAxisCode {
			get {
				return _XAxisCode;
			}
			set {
				_XAxisCode = value;
			}
		}

		public string YAxisCode {
			get {
				return _YAxisCode;
			}
			set {
				_YAxisCode = value;
			}
		}

		public ArrayList LogFileEntries {
			get {
				return _c_device_logfile_entries;
			}
			set {
				_c_device_logfile_entries = value;
			}
		}

		public LinePlot Graph {
			get {
				return _linePlot;
			}
		}

		public void ploy_zedgraph ( out ZedGraphControl g_graph) {

			ZedGraphControl ctl = new ZedGraphControl ();
			GraphPane g_pane = ctl.GraphPane;

			int __width_factor = 45;
			int __graph_width = _c_device_logfile_entries.Count * __width_factor;

			g_pane.Title.Text = "My ZedGraph";
			g_pane.XAxis.Title.Text = "Date/Time Stamp";
			g_pane.YAxis.Title.Text = "Temp (*C)";
			ctl.Width = __graph_width;
			ctl.Height = 500;

			PointPairList g_pane_list = new PointPairList ();

			for (int i = 0; i < _ar_x_axis_data.Count; i++) {
				double x = (double)new XDate ((DateTime)_ar_x_axis_data.ToArray() [i]);
				double y = Convert.ToDouble ( _ar_y_axis_data [i]);

				g_pane_list.Add (x, y );
			}

			CurveItem g_pane_curve = g_pane.AddCurve ("logfile/deviceID", 
				g_pane_list, Color.BlueViolet, SymbolType.Square);

			g_pane.XAxis.Type = AxisType.Date;

			ctl.AxisChange ();
			g_graph = ctl;

		}

		public void plot_nplot(out NPlot.Gtk.PlotSurface2D graph ) {

			DateTime dt = DateTime.Now;
			int __width_factor = 45;
			int __graph_width = _c_device_logfile_entries.Count * __width_factor;
			 
			NPlot.Gtk.PlotSurface2D _graph = new NPlot.Gtk.PlotSurface2D ();

			_graph.SetSizeRequest ( __graph_width, 500);

			_graph.ModifyBg (StateType.Normal, cutil.get_light_grey());

			Bitmap _graphBitmap = new Bitmap (1000, 500);

			DateTimeAxis x = new DateTimeAxis ();
			LinearAxis y = new LinearAxis (-50, 0);

			x.SmallTickSize = 10;
			x.LargeTickStep = new TimeSpan (0, 30, 0);
			x.NumberFormat = "hh:mm";


			_graph.PlotBackImage = _graphBitmap;
			_graph.YAxis1 = y;
			_graph.XAxis1 = x;
			_graph.XAxis1.Label = _XAxisLabel;
			_graph.XAxis1.AutoScaleTicks = false;


			_graph.YAxis1.Label = _YAxisLabel;

			_linePlot.AbscissaData = _ar_x_axis_data;
			_linePlot.OrdinateData = _ar_y_axis_data;
			_linePlot.Label = _title;
			_linePlot.ShowInLegend = true;
			_linePlot.Pen.Width = 2.5f;
			_linePlot.Color = Color.Orange;

			_linePlotLegend.AttachTo ( NPlot.PlotSurface2D.XAxisPosition.Top, NPlot.PlotSurface2D.YAxisPosition.Left);
			_linePlotLegend.VerticalEdgePlacement = NPlot.Legend.Placement.Inside;
			_linePlotLegend.HorizontalEdgePlacement = NPlot.Legend.Placement.Outside;
			_linePlotLegend.BorderStyle = LegendBase.BorderType.Shadow;
			_linePlotLegend.YOffset = -10;
			_linePlotLegend.XOffset = -5;

			_graph.Legend = _linePlotLegend;
			_graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

			NPlot.Grid grid = new Grid ();
			grid.HorizontalGridType = Grid.GridType.Fine;
			grid.VerticalGridType = Grid.GridType.Fine;

			_graph.Add (grid, NPlot.PlotSurface2D.XAxisPosition.Bottom, NPlot.PlotSurface2D.YAxisPosition.Left);
			_graph.Add (_linePlot);

			_graph.QueueDraw ();

			graph = _graph;

		}
	}
}

