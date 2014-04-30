using System;

namespace SubZeroViewer2
{
	/* CLASS CServerListItem:
	 * Represent a short list of ip's and friendly names for target
	 * ftp servers. Is used mostly in cboDeviceServer
	 */

	public class CServerListItem
	{
		private string _friendly_name;
		private string _address;

		public CServerListItem() {

			_friendly_name = "MrNoName";
			_address = "000.000.00.00";

		}

		public CServerListItem (string s_frienly_name, string s_ip)
		{
			_friendly_name = s_frienly_name;
			_address = s_ip;
		}

		public string FriendlyName {
			get {
				return _friendly_name;
			}
			set {
				_friendly_name = value;
			}
		}

		public string Address {
			get {
				return _address;
			}
			set {
				_address = value;
			}
		}

	}
}

