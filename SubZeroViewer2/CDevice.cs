using System;
using System.Collections;

namespace SubZeroViewer2
{

	/* CLASS CDevice: 
	 * Used to describe our sensor logging devices. */

	public class CDevice
	{
		private string _device_id = "";
		private string _old_device_id = "";
		private string _device_descr = "";
		private string _device_location = "";
		private string _device_server = "";
		private string _device_server_subdir = "";
		private bool _dirty_flag = false;
		private bool _update = false;

		private ArrayList _device_logFile_entry;
		private ArrayList _device_file_descr;

		public CDevice ()
		{
			_old_device_id = "";
		}

		public CDevice ( string device_id, string device_descr, string device_location, string device_server, 
			string device_server_subdir ) 
		{
			_device_id = device_id;
			_device_descr = device_descr;
			_device_location = device_location;
			_device_server = device_server;
			_device_server_subdir = device_server_subdir;
			_old_device_id = "";

		}

		public string DeviceServerSubDir {
			get {
				return _device_server_subdir;
			}
			set {

				if (string.Compare (_device_server_subdir, value) != 0) {
					_device_server_subdir = value;
					_dirty_flag = true;
				}

			}
		}

		public string DeviceID {
			get {

				return _device_id;
			}
			set {

				if (_old_device_id == "")
					_old_device_id = _device_id;

				if (string.Compare (_device_id, value) != 0) {
					_device_id = value;
					_dirty_flag = true;
				}
			}
		}

		public string DeviceDescription {
			get {
				return _device_descr;
			}
			set {

				if (string.Compare (_device_descr, value) != 0) {
					_device_descr = value;
					_dirty_flag = true;
				}

			}
		}

		public string DeviceLocation {
			get {
				return _device_location;
			}
			set {

				if (string.Compare (_device_location, value) != 0) {
					_device_location = value;
					_dirty_flag = true;
				}

			}
		}

		public ArrayList DeviceLogFile {
			get {
				return _device_logFile_entry;
			}
			set {
				_device_logFile_entry = value;
			}
		}

		public ArrayList DeviceFileDescription {
			get {
				return _device_file_descr;
			}
			set {
				_device_file_descr = value;
			}
		}

		public string DeviceServer {
			get {
				return _device_server;
			}
			set {

				if (string.Compare (_device_server, value) != 0) {
					_device_server = value;
					_dirty_flag = true;
				}

			}
		}

		public string OldDeviceID {
			get {
				return _old_device_id;
			}
		}

		public bool DirtyFlag {
			get {
				return _dirty_flag;
			}
			set {
				_dirty_flag = value;
			}
		}

		public bool Update {
			get {
				return _update;
			}
			set {
				_update = value;
			}
		}

	}
}

