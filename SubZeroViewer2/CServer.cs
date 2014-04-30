using System;

namespace SubZeroViewer2
{
	public class CServer
	{
		private string _server_addr = "";
		private string _old_server_addr = "";
		private int _server_port = 0;
		private string _server_base_dir = "";
		private int _server_auth_req = 0;
		private string _server_uname = "";
		private string _server_pword = "";
		private string _server_friend_name = "";
		private string _server_protocol = "";
		private bool _dirty_flag = false;
		private bool _update = false;

		public CServer ()
		{
			_old_server_addr = "";
		}

		public CServer ( string server_addr, int server_port, string server_base_dir, int server_auth_req, 
			string server_uname, string server_pword, string server_friend_name, string server_protocol) {

			_server_protocol = server_protocol;
			_server_addr = server_addr;
			_server_port = server_port;
			_server_base_dir = server_base_dir;
			_server_auth_req = server_auth_req;
			_server_uname = server_uname;
			_server_pword = server_pword;
			_server_friend_name = server_friend_name;
			_old_server_addr = "";
		} 

		public string ServerProtocol {
			get {
				return _server_protocol;
			}
			set {
				if (string.Compare (_server_protocol, value) != 0) {
					_server_protocol = value;
					_dirty_flag = true;
				}
			}
		}

		public string ServerAddr {
			get {
				return _server_addr;
			}
			set {

				if (_old_server_addr == "")
					_old_server_addr = _server_addr;

				if (string.Compare (_server_addr, value) != 0) {
					_server_addr = value;
					_dirty_flag = true;
				}

			}
		}

		public string OldServerAddr {
			get {
				return _old_server_addr;
			}
		}

		public string ServerFriendName {
			get {
				return _server_friend_name;
			}
			set {

				if ( string.Compare ( _server_friend_name, value ) != 0 ) {
					_server_friend_name = value;
					_dirty_flag = true;
				}

			}
		}

		public int ServerPort {
			get {
				return _server_port;
			}
			set {

				if ( _server_port != value ) {
					_server_port = value;
					_dirty_flag = true;
				}

			}
		}

		public int ServerAuthRequired {
			get {
				return _server_auth_req;
			}
			set {

				if ( _server_auth_req != value ) {
					_server_auth_req = value;
					_dirty_flag = true;
				}

			}
		}

		public string ServerUserName {
			get {
				return _server_uname;
			}
			set {

				if (string.Compare (_server_uname, value) != 0) {
					_server_uname = value;
					_dirty_flag = true;
				}

			}
		}

		public string ServerPassword {
			get {
				return _server_pword;
			}
			set {

				if (string.Compare (_server_pword, value) != 0) {
					_server_pword = value;
					_dirty_flag = true;
				}

			}
		}

		public string ServerBaseDir {
			get {
				return _server_base_dir;
			}
			set {

				if (string.Compare (_server_base_dir, value) != 0) {

					_server_base_dir = value;
					_dirty_flag = true;
				}

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

