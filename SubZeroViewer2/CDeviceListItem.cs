using System;

namespace SubZeroViewer2
{
	public class CDeviceListItem
	{

		private string _device_id;

		public CDeviceListItem ()
		{
		}

		public CDeviceListItem ( string device_id ) {
			_device_id = device_id;
		}

		public string DeviceID {
			get {
				return _device_id;
			}
			set {
				_device_id = value;
			}
		}

	}
}

