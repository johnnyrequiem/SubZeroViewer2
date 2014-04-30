using System;

namespace SubZeroViewer2
{
	/* CLASS CDeviceLogFileEntry: 
	 * Used in an ArrayList to represent each of the 
	 * logging entries in the ftp log file.
	 */

	public class CDeviceLogFileEntry
	{
		private string _sensor_code;
		private string _sensor_code_descr;
		private string _sensor_value;

		public enum t_log_file_sensor_codes
		{
			//TODO: Define log file sensor codes.
			CRTEMP,
			DROPEN,
			LIGHTON,
			MAIN,
			CURTEMP
		};

		public enum t_log_file_sensor_value_types {
			STRING,
			BOOL,
			TIME,
			INT,
			FLOAT
		};

		public CDeviceLogFileEntry ()
		{
		}

		public CDeviceLogFileEntry ( string DeviceLogFileCode, string DeviceLogFileValue ) {

			_sensor_code = DeviceLogFileCode;
			_sensor_value = DeviceLogFileValue;
		}

		public string LogFIleSensorCode {
			get {
				return _sensor_code;
			}
			set {
				_sensor_code = value;
			}
		}

		public string LogFileSensorDescr {
			get {
				return _sensor_code_descr;
			}
			set {
				_sensor_code_descr = value;
			}
		}

		public string LogFileSensorValue {
			get {
				return _sensor_value;
			}
			set {
				_sensor_value = value;
			}
		}
	}

}

