using System;
using System.Collections;
using System.Text;
using System.IO;


namespace SubZeroViewer2
{
	public class CLogFile
	{

		private StreamReader _logFile = null;
		private DateTime _logFileModifiedDate;

		private int _logTimeIndex = -1;
		private int _logTimeTempIndex = -1;

		private ArrayList _ar_logFileLines = new ArrayList();
		private ArrayList _ar_deviceLogFileEntries = new ArrayList();
		private ArrayList _ar_logTime = new ArrayList ();

		private ArrayList _ar_logTimeTemp = new ArrayList ();
		private ArrayList _ar_logAlarms = new ArrayList();
		private ArrayList _ar_logFileDescription = new ArrayList();

		public CLogFile ()
		{
			_ar_logFileLines = new ArrayList ();
		}

		public CLogFile ( StreamReader logFile, DateTime logFileDate, ArrayList LogFileDescription ) {

			_logFile = logFile;
			_ar_logFileLines = new ArrayList ();
			_logFileModifiedDate = logFileDate;
			_ar_logFileDescription = LogFileDescription;

			parseStream ();
		}

		public StreamReader LogFileStream {
			set {
				_logFile = value;
			}
		}

		public int LogFileLineCount {
			get {
				return _ar_logFileLines.Count;
			}
		}

		public ArrayList LogFileTemps {
			get {
				return _ar_logTimeTemp;
			}
		}

		public ArrayList LogFileTime {
			get {
				return _ar_logTime;
			}
		}

		public ArrayList LogFileAlarms {
			get {
				return _ar_logAlarms;
			}
		}

		public ArrayList LogFileEntries {
			get {
				return  _ar_deviceLogFileEntries;
			}
		}

		public int CLogFile_Get_Y_Axis_Code_Index( CDevice device ) {

			CDeviceFileDescription _descr = null;

			for (int i = 0; i < device.DeviceFileDescription.Count; i++) {

				// Find the log time and return the index.
				_descr = (CDeviceFileDescription)device.DeviceFileDescription [i];

				if (_descr.DeviceCode == "CURTEMP") {
					_logTimeIndex = i;
					return i;
				}

			}

			return -1;
		}

		private void parseStream() {

			string line = null;

			if (_logFile != null) {

				// Populate logFileLines.
				for (int i = 0; ( line = _logFile.ReadLine() ) != null; i++ ) {

					if (line != "" && line != "AT+CGREG?" && (line.Substring(0,1).StartsWith("#") != true )) {

						_ar_logFileLines.Add (line);

						//Split lines
						string[] cols = line.Split (',');

						//Populate devi/** TODO: Populate Alarms Table */ceLogFileValues
						_ar_deviceLogFileEntries.Add (cols);

					}

				}


				/**
				 * ===============================
				 * New parse for temps and alarms.
				 * ===============================
				 * Walk through each LogFile Entry. 
				 * For each entry, walk through the Device LogFileDescription to get 
				 * the sensor codes, as well as the array index for the logfile values 
				 * for each Device LogFile Entry.
				 */

				if (_ar_logFileDescription == null)
					_ar_logFileDescription = new CUtillity ().CUTIL_DefaultDeviceFileDescription;

				for (int j = 0; j < _ar_deviceLogFileEntries.Count; j++) {
				
					for (int k = 0; k < _ar_logFileDescription.Count; k++) {

						CDeviceFileDescription __device_descr = (CDeviceFileDescription) _ar_logFileDescription [k];
				
						switch (__device_descr.DeviceCode) {
						case "LOGTEMP":

							try {

							_ar_logTimeTemp.Add (((string[])_ar_deviceLogFileEntries [j]) [k]);

							} catch (IndexOutOfRangeException e) {

								//TODO: Catch array index out of range here.

							}

							break;

						case "LOGTIME":

							int __hour = 0;
							int __minute = 0;
							string __cur_element = ((string[])_ar_deviceLogFileEntries [j]) [k];
				
							try {
								__hour = Convert.ToInt32 (__cur_element.Substring (0, 2));
								__minute = Convert.ToInt32 (__cur_element.Substring (3, 2));
							} catch (FormatException) {

								new Gtk.MessageDialog (null, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok,
									"The file you tried to open was incorrectly formatted.\n" +
									"Please select a different file.", 0).Show ();

								return;
							}

							DateTime dt = new DateTime (
								              _logFileModifiedDate.Year,
								              _logFileModifiedDate.Month,
								              _logFileModifiedDate.Day, __hour, __minute, 0
							              );

							_ar_logTime.Add (dt);

							break;

						case "DID":
							break;

						default:

							string value = ((string[])_ar_deviceLogFileEntries [j]) [k];

							if ( value.Length > 1 ){
								CDeviceLogFileEntry __entry = new CDeviceLogFileEntry ();

								__entry.LogFIleSensorCode = __device_descr.DeviceCode;
								__entry.LogFileSensorDescr = __device_descr.DeviceCodeDescription;
								__entry.LogFileSensorValue = value;

								_ar_logAlarms.Add (__entry);

							}

							break;

						}



					}

				}


			}


		}


	}
}

