using System;
using System.Collections;

namespace SubZeroViewer2
{

	/** CLASS CUtillity:
	 * All thing misc and cross-platform related goes here. If it cannot
	 * be defined as included in another class, it's finds itself dumped....here!
	 */

	public class CUtillity
	{

		// TODO: Find better cross-platform file path resolution.

		/*
		 * Is there a better way of implementing cross-platform
		 * path resolution? Surely!
		 */

		private const string Subzero2RCFile = "Subzero2.rc";

		private string[] _Gtkrcfile = {
			"./res/theme/default/gtk-2.0/gtkrc",
			"./res/moka/Moka/gtk-2.0/apps/gtkrc",
			"./res/moka/Moka-Dark/gtk-2.0/gtkrc"
		};

		private string[] _Gtkrcfile32 = {
			".\\res\\theme\\default\\gtk-2.0\\gtkrc",
			"./res/moka/Moka/gtk-2.0/apps/gtkrc"
		};

		public enum SubZeroColors {
			DARK_GREY,
			LIGHT_GREY,
			PINK
		};

		private SubZeroColors pcolors;

		public enum t_gtk_rc_file {
			EOG2Blue,
			Azenis
		};

		private string _db_file_unix = "./db/SubZeroViewer2.sqlite3";
		private string _db_file_win32 = ".\\db\\SubZeroViewer2.sqlite3";

		private string _server_icon_unix = "./res/Server4.png";
		private string _server_icon_win32 = ".\\res\\Server4.png";

		private string _device_icon_unix = "./res/Device.png";
		private string _device_icon_win32 = ".\\res\\Device.png";

		private string _file_icon_unix = "./res/csv.png";
		private string _file_icon_win32 = "./res/csv.png";

		CDeviceFileDescription[] _default_description = new CDeviceFileDescription[9]
		{
			new CDeviceFileDescription ("DID", "Device ID", "STRING"),
			new CDeviceFileDescription ("LOGTIME", "Time Of Log Entry", "STRING"),
			new CDeviceFileDescription ("LOGTEMP", "Log Entry Temperature", "STRING"),
			new CDeviceFileDescription ("CRTEMP", "Critical Temprature Alarm", "STRING"),
			new CDeviceFileDescription ("TMPRISE", "Temperature Rise Alarm", "STRING"),
			new CDeviceFileDescription ("DROPEN", "Door Open Alarm", "STRING"),
			new CDeviceFileDescription ("LIGHTON", "Light On Alarm", "STRING"),
			new CDeviceFileDescription ("MAINS", "Mains Lost Alarm", "STRING"),
			new CDeviceFileDescription ("HPALRM", "HP Alarm", "STRING")
		};

		public CUtillity ()
		{
		}

		public Gdk.Color get_dark_grey() {
		
			Gdk.Color _color = new Gdk.Color ();
			Gdk.Color.Parse ("#3B3B3B", ref _color);

			return _color;
		}

		public Gdk.Color get_light_grey() {

			Gdk.Color _color = new Gdk.Color ();
			Gdk.Color.Parse ("#B9B5B5", ref _color);

			return _color;
		}

		public SubZeroColors ColorCodes {
			get {
				return pcolors;
			}
		}

		public ArrayList CUTIL_DefaultDeviceFileDescription {
			get {

				ArrayList _temp = new ArrayList ();

				for (int i = 0; i < _default_description.Length; i++) {
					_temp.Add (_default_description [i]);
				}

				return _temp;
			}
		}

		public string CUTIL_GetServerIcon () {

			PlatformID platform = Environment.OSVersion.Platform;

			if (platform == System.PlatformID.Win32NT) {
				return _server_icon_win32;
			} else {
				return _server_icon_unix;
			}

		}

		public string CUTIL_GetRCFile( int index ) {
			PlatformID platform = Environment.OSVersion.Platform;

			if (platform == System.PlatformID.Win32NT) {
				return _Gtkrcfile32 [index];
			} else {
				return _Gtkrcfile[index];
			}
		}

		public string CUTIL_GetFileIcon() {

			PlatformID platform = Environment.OSVersion.Platform;

			if (platform == System.PlatformID.Win32NT) {
				return _file_icon_win32;
			} else {
				return _file_icon_unix;
			}

		}

		public string CUTIL_GetDeviceIcon() {

			PlatformID platform = Environment.OSVersion.Platform;

			if (platform == System.PlatformID.Win32NT) {
				return _device_icon_win32;
			} else {
				return _device_icon_unix;
			}

		}

		public string CUTIL_GetDBFile() {

			PlatformID platform = Environment.OSVersion.Platform;

			if (platform == System.PlatformID.Win32NT) {
				return _db_file_win32;
			} else {
				return _db_file_unix;
			}

		}

	}
}

