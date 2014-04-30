using System;
using System.Collections;

namespace SubZeroViewer2
{
	/* CLASS CDeviceFileDescription:
	 * Used in a ArrayList to hold the values used to describe
	 * the Device LogFile layout so we know what to expect when
	 * getting the file from the server.
	 */

	public class CDeviceFileDescription
	{
		private string _code;
		private string _descr;
		private string _type;

		public CDeviceFileDescription () {}

		public CDeviceFileDescription ( string code, string descr, string type ) {
			_code = code;
			_descr = descr;
			_type = type;
		}

		public string DeviceCode {
			get {
				return _code;
			}
			set {
				_code = value;
			}
		}

		public string DeviceCodeDescription {
			get {
				return _descr;
			}
			set {
				_descr = value;
			}
		}

		public string DeviceCodeType {
			get {
				return _type;
			}
			set {
				_type = value;
			}
		}

	}
}

