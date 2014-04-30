using System;
using System.Collections;
using Mono.Data.Sqlite;

namespace SubZeroViewer2
{

	/* CLASS CSQL:
	 * As the name suggests, all things sql go here. If all goes well
	 * this should translate alright in future db server translations.
	 */

	public class CSQL
	{
		private CUtillity cutil = new CUtillity();

		private SqliteConnection dbh;

		private enum tbl_device_logfile_descr
		{
			DEVICE_SENSOR_VALUE_TYPES,
			DEVICE_SENSOR_VALUE_DESCR,
			DEVICE_SENSOR_CODE,
			DEVICE_LOGFILE_ORDER,
			DEVICE_ID
		};

		private enum tbl_devices
		{
			DEVICE_ID,
			DEVICE_DESCR,
			DEVICE_LOCATION,
			DEVICE_SERVER, 
			DEVICE_SERVER_SUBDIR
		};

		private enum tbl_servers
		{
			SERVER_ADDR,
			SERVER_PORT,
			SERVER_BASE_DIR,
			SERVER_AUTH_REQ,
			SERVER_UNAME,
			SERVER_PNAME,
			SERVER_FRIEND_NAME,
			SERVER_PROTOCOL
		};

		private void cleanup() {


		}

		private bool CSQL_Connected() {

			string dbh_string = "URI=file:" + cutil.CUTIL_GetDBFile() + ", Version=3";		        

			if (dbh == null) dbh = new SqliteConnection (dbh_string);

			if (dbh.State == System.Data.ConnectionState.Open)
				return true;

				try {
					dbh.Open ();
				} catch (Exception e) {
					Console.WriteLine ("==> DBH STATE: Line 35 : " + dbh.State.ToString ());
					Console.WriteLine ("==> DBH ERROR: Line 41 : " + e.Message);
				}

				if (dbh.State == System.Data.ConnectionState.Closed)
					return false;

				Console.WriteLine ("==> DBH STATE: Line 46 : " + dbh.State.ToString ());

			return true;
		}

		public CSQL ()
		{
		}

		public bool CSQL_CloseUp() {

			dbh.Close ();
			dbh.Dispose ();
			dbh = null;

			return true;
		}

		public bool CSQL_DeleteDevice ( string device_id ) {

			string sql_delete_device = "DELETE FROM devices " +
			                           "WHERE devices.device_id like '" + device_id + "';";

			string sql_delete_device_logfile_descr = "DELETE FROM device_logfile_descr " +
			                                         "WHERE device_logfile_descr.device_id like '" + device_id + "';";
			int ret = 0;
			SqliteCommand dbhCmd = null;
			SqliteTransaction dbhTr = null;

			if (CSQL_Connected ()) {

				dbhTr = dbh.BeginTransaction ();

				dbhCmd = new SqliteCommand (sql_delete_device_logfile_descr, dbh);
				ret = dbhCmd.ExecuteNonQuery ();

				dbhCmd = new SqliteCommand (sql_delete_device, dbh);
				ret = dbhCmd.ExecuteNonQuery ();

				dbhTr.Commit ();

			} else {
				return false;
			}

			return true;
		}

		public bool CSQL_UpdateServer ( CServer server ) {

			string sql_update_server = "UPDATE servers " +
			                           "SET server_friend_name = '" + server.ServerFriendName + "', " +
			                           "server_pword = '" + server.ServerPassword + "', " +
			                           "server_uname = '" + server.ServerUserName + "', " +
			                           "server_auth_req = " + server.ServerAuthRequired + ", " +
			                           "server_base_dir = '" + server.ServerBaseDir + "', " +
			                           "server_port = " + server.ServerPort + ", " +
			                           "server_addr = '" + server.ServerAddr + "', " +
			                           "server_protocol = '" + server.ServerProtocol + "' " +
			                           "WHERE server_addr like '" + server.OldServerAddr + "';";

			SqliteCommand dbhCmd = new SqliteCommand (sql_update_server, dbh);
			dbhCmd.ExecuteNonQuery ();

			return true;
		}

		public bool CSQL_UpdateDevice ( CDevice device ) {

			string sql_update_device = "UPDATE devices " +
			                           "SET device_id = '" + device.DeviceID + "', " +
			                           "device_descr = '" + device.DeviceDescription + "', " +
			                           "device_location = '" + device.DeviceLocation + "', " +
			                           "device_server = '" + device.DeviceServer + "', " +
			                           "device_server_subdir = '" + device.DeviceServerSubDir + "' " +
			                           "WHERE device_id like '" + device.OldDeviceID + "';";

			string sql_remove_logfile_descr = "DELETE FROM device_logfile_descr " +
			                                  "WHERE device_id like '" + device.OldDeviceID + "';";

			SqliteCommand dbhCmd = null;
			SqliteTransaction Tr = null;

			Console.WriteLine ("SQL -> " + sql_update_device);

			if (CSQL_Connected ()) {

				/*
				 * UPDATE DEVICE SETTINGS
				 */

				dbhCmd = new SqliteCommand (sql_update_device, dbh);
				Tr = dbh.BeginTransaction ();

				dbhCmd.ExecuteNonQuery ();

				/*
				 * UPDATE logfile description by first removing all and 
				 * then replacing them with the new description.
				 */

				dbhCmd = new SqliteCommand (sql_remove_logfile_descr, dbh);
				dbhCmd.ExecuteNonQuery ();

				CSQL_AddDeviceLogFileDescription (device, Tr.Connection);

				Tr.Commit ();
			}

			return true;
		}

		public CDevice CSQL_GetDevice ( string device_id ) {

			//TODO: Finish CSQL_GetDevice implementation.

			string sql_device = "SELECT * FROM devices " +
			                    "WHERE devices.device_id = '" + device_id + "';";

			string sql_device_descr = "SELECT * FROM device_logfile_descr " +
			                          "WHERE device_logfile_descr.device_id = '" + device_id + "' " +
			                          "ORDER BY device_logfile_descr.device_logfile_order;";

			CDevice _device = null;
			ArrayList _ar_device_logfile_descr;
			CDeviceFileDescription _device_file_descr = null;
			SqliteCommand dbhCmd = null;
			SqliteDataReader dbhReader = null;

			if (CSQL_Connected ()) {

				dbhCmd = new SqliteCommand (sql_device, dbh);
				dbhReader = dbhCmd.ExecuteReader ();

				if (dbhReader.HasRows) {

					dbhReader.Read ();

					_device = new CDevice (dbhReader.GetString ((int)tbl_devices.DEVICE_ID), 
						dbhReader.GetString ((int)tbl_devices.DEVICE_DESCR), 
						dbhReader.GetString ((int)tbl_devices.DEVICE_LOCATION), 
						dbhReader.GetString ((int)tbl_devices.DEVICE_SERVER),
						dbhReader.GetString ((int)tbl_devices.DEVICE_SERVER_SUBDIR));


					//Get device log file description.
					dbhCmd = new SqliteCommand (sql_device_descr, dbh);
					dbhReader = dbhCmd.ExecuteReader ();

					if (dbhReader.HasRows) {

						_ar_device_logfile_descr = new ArrayList ();

						while (dbhReader.Read()) {

							_device_file_descr = new CDeviceFileDescription 
							(
			                     dbhReader.GetString ((int)tbl_device_logfile_descr.DEVICE_SENSOR_CODE),
			                     dbhReader.GetString ((int)tbl_device_logfile_descr.DEVICE_SENSOR_VALUE_DESCR), 
			                     dbhReader.GetString ((int)tbl_device_logfile_descr.DEVICE_SENSOR_VALUE_TYPES)
							);

							_ar_device_logfile_descr.Add (_device_file_descr);

						}

						_device.DeviceFileDescription = _ar_device_logfile_descr;

					} else {
						cleanup ();
						return _device;
					}

				} else {
					cleanup ();
					return null;
				}

			} else {
				cleanup ();
				return null;
			}

			return _device;
		}

		public CServer CSQL_GetServer (string key) {

			string sql = "SELECT * FROM servers " +
			             "WHERE servers.server_addr like '" + key + "'" +
			             " OR servers.server_friend_name like '" + key + "';";

			SqliteCommand dbhCmd;
			SqliteDataReader dbhReader;

			CServer cserver = null;

			if (CSQL_Connected ()) {
				dbhCmd = new SqliteCommand (sql, dbh);
				dbhReader = dbhCmd.ExecuteReader ();

				if (dbhReader.HasRows) {

					dbhReader.Read ();

					cserver = new CServer (
						dbhReader.GetString ((int)tbl_servers.SERVER_ADDR),
						dbhReader.GetInt32 ((int)tbl_servers.SERVER_PORT),
						dbhReader.GetString ((int)tbl_servers.SERVER_BASE_DIR),
						dbhReader.GetInt32 ((int)tbl_servers.SERVER_AUTH_REQ),
						dbhReader.GetString ((int)tbl_servers.SERVER_UNAME),
						dbhReader.GetString ((int)tbl_servers.SERVER_PNAME),
						dbhReader.GetString ((int)tbl_servers.SERVER_FRIEND_NAME),
						dbhReader.GetString ((int)tbl_servers.SERVER_PROTOCOL)
					);

				} else {
					cleanup ();
					return null;
				}
			} else {
				cleanup ();
				return null;
			}

			cleanup ();
			return cserver;
		}

		public bool CSQL_DeleteServer ( string key ) {

			string sql = "DELETE FROM servers " +
			             "WHERE servers.server_addr like '" + key + "'" +
			             " OR servers.server_friend_name like '" + key + "';";

			SqliteCommand dbhCmd;
			SqliteDataReader dbhReader = null;

			if (CSQL_Connected ()) {
				dbhCmd = new SqliteCommand (sql, dbh);
				dbhReader = dbhCmd.ExecuteReader ();
			}


				if (dbhReader.HasRows) {
					cleanup ();
					return true;
				} else {
					cleanup ();
					return false;
				}

		}

		public bool CSQL_AddServer (CServer cserver) {

			string sql = "INSERT INTO servers " +
			             "VALUES ('" + cserver.ServerAddr + "', '" + cserver.ServerPort + "', '" + cserver.ServerBaseDir
			             + "', '" + cserver.ServerAuthRequired + "', '" + cserver.ServerUserName
			             + "', '" + cserver.ServerPassword + "', '" + cserver.ServerFriendName 
			             + "', '" + cserver.ServerProtocol + "' );";


			SqliteCommand dbhCmd;

			if (CSQL_Connected ()) {

				dbhCmd = new SqliteCommand (sql, dbh);
				int num_recs = 0;

				try {
					num_recs = dbhCmd.ExecuteNonQuery ();

				} catch (Exception e) {
					Console.WriteLine ("==> ERROR: Line 139: " + e.Message);
				}

				if (num_recs > 0) {
					cleanup ();
					return true;
				} else {
					cleanup ();
					return false;
				}


			} else {
				cleanup ();
				return false;
			}

		}

	
		public string[] CSQL_GetDeviceSensorValueTypes() {

			string sql = "SELECT * FROM device_sensor_value_types;";
			string sql_count = "SELECT device_sensor_value_types.type , count(*) as TotalTypes " +
								"FROM device_sensor_value_types;";
			string[] _types;

			int type_count = 0;
			int count = 0;

			SqliteCommand dbhCmd;
			SqliteDataReader dbhReader;

			if (CSQL_Connected ()) {
				dbhCmd = new SqliteCommand (sql_count, dbh);
				dbhReader = dbhCmd.ExecuteReader ();

				if (dbhReader.HasRows) {
					dbhReader.Read ();
					type_count = dbhReader.GetInt32 (1);

					if (type_count == 0) {
						cleanup ();
						return null;
					}
				} else {
					cleanup();
					return null;
				}

				_types = new string[type_count];

				dbhCmd = new SqliteCommand (sql, dbh);
				dbhReader = dbhCmd.ExecuteReader ();

				if (dbhReader.HasRows) {
					while (dbhReader.Read ()) {
						_types [count++] = dbhReader.GetString (1);
					}

					cleanup ();
					return _types;
				} else {
					cleanup();
					return null;
				}
			}

			cleanup ();
			return null;

		}

		public string[] CSQL_GetDeviceSensorCodes() {

			string sql = "SELECT * FROM device_sensor_codes;";
			string sql_count = "SELECT device_sensor_codes.code, count(*) as TotalTypes " +
								"FROM device_sensor_codes;";
			string[] _codes;

			int code_count = 0;
			int count = 0;

			SqliteCommand dbhCmd;
			SqliteDataReader dbhReader;

			if (CSQL_Connected ()) {
				dbhCmd = new SqliteCommand (sql_count, dbh);
				dbhReader = dbhCmd.ExecuteReader ();

				if (dbhReader.HasRows) {
					dbhReader.Read ();
					code_count = dbhReader.GetInt32 (1);

					if (code_count == 0) {
						cleanup ();
						return null;
					}
				} else {
					cleanup();
					return null;
				}

				_codes = new string[code_count];

				dbhCmd = new SqliteCommand (sql, dbh);
				dbhReader = dbhCmd.ExecuteReader ();

				if (dbhReader.HasRows) {
					while (dbhReader.Read ()) {
						_codes [count++] = dbhReader.GetString (1);
					}

					cleanup ();
					return _codes;
				} else {
					cleanup();
					return null;
				}
			}

			cleanup ();
			return null;

		}

		public CDeviceListItem[] CSQL_GetDeviceList() {

			string sql = "SELECT devices.device_id " +
			             "FROM devices;";

			string sql_count = "SELECT devices.device_id, count(*) as TotalDevices " +
			                   "FROM devices;";

			SqliteCommand dbhCmd = null;
			SqliteDataReader dbhReader = null;
			CDeviceListItem[] _device_list = null;
			int device_count = 0;
			int count = 0;

			if (CSQL_Connected ()) {

				dbhCmd = new SqliteCommand (sql_count, dbh);
				dbhReader = dbhCmd.ExecuteReader ();

				/**
				 * Get the number of devices for the array size.
				 */
				if (dbhReader.HasRows) {
					dbhReader.Read ();
					device_count = dbhReader.GetInt32 (1);

					if (device_count == 0) {
						cleanup ();
						return null;
					}

				} else {
					cleanup ();
					return null;
				}
			}

			_device_list = new CDeviceListItem[device_count];

			dbhCmd = new SqliteCommand (sql, dbh);
			dbhReader = dbhCmd.ExecuteReader ();

			if (dbhReader.HasRows) {
				while (dbhReader.Read ()) {
					_device_list [count++] = new CDeviceListItem (dbhReader.GetString (0));
				}

				cleanup ();
				return _device_list;
			} else {
				cleanup ();
				return null;
			}

			cleanup ();
			return null;

		}

		public CServerListItem[] CSQL_GetServerList() {

			string sql = "SELECT servers.server_friend_name, servers.server_addr " +
			             "FROM servers;";
		
			string sql_count = "SELECT servers.server_friend_name, count(*) as TotalServers " +
			             "from servers;";

			SqliteCommand dbhCmd;
			SqliteDataReader dbhReader;

			CServerListItem[] _server_list;
			int server_count = 0;
			int count = 0;

			if (CSQL_Connected ()) {

				dbhCmd = new SqliteCommand (sql_count, dbh);
				dbhReader = dbhCmd.ExecuteReader ();

				/**
				 * Get the number of servers for the array size.
				 */
				if (dbhReader.HasRows) {
					dbhReader.Read ();
					server_count = dbhReader.GetInt32 (1);

					if (server_count == 0) {
						cleanup ();
						return null;
					}

				} else {
					cleanup ();
					return null;
				}

				/**
				 * Get server list.
				 */
				_server_list = new CServerListItem[server_count];
				dbhCmd = new SqliteCommand (sql, dbh);
				dbhReader = dbhCmd.ExecuteReader ();

				if (dbhReader.HasRows) {

					while (dbhReader.Read ()) {
						_server_list [count++] = new CServerListItem (
							dbhReader.GetString (0), 
							dbhReader.GetString (1));
					}

					cleanup ();
					return _server_list;

				} else {
					cleanup ();
					return null;
				}

			}

			cleanup ();
			return null;
		}


		public bool CSQL_AddDevice (CDevice device) {

			string sql = "INSERT INTO devices VALUES ('" + 
	                           device.DeviceID + "', '" + 
	                           device.DeviceDescription + "', '" +
					             device.DeviceLocation + "', '" +
			             		device.DeviceServer + "', '" + 
			             		device.DeviceServerSubDir + "');";


			SqliteCommand dbhCmd;
			int num_recs = 0;

			if (CSQL_Connected ()) {

				dbhCmd = new SqliteCommand (sql, dbh);
				num_recs = dbhCmd.ExecuteNonQuery ();

				if (num_recs > 0) {
					CSQL_AddDeviceLogFileDescription (device, dbhCmd.Connection);
				}

			}

			return false;

		}

		private bool CSQL_AddDeviceLogFileDescription (CDevice device, SqliteConnection hconn) {

			string sql = null;
			SqliteCommand dbhCmd = null;
			int num_recs = 0;

			for (int i = 0; i < device.DeviceFileDescription.Count; i++) {
				CDeviceFileDescription descr = (CDeviceFileDescription)device.DeviceFileDescription [i];

				sql = "INSERT INTO device_logfile_descr VALUES ('" +
				              descr.DeviceCodeType + "', '" +
				              descr.DeviceCodeDescription + "', '" +
				              descr.DeviceCode + "', " +
				              i + ", '" +
				              device.DeviceID + "');";

				dbhCmd = new SqliteCommand (sql, hconn);
				num_recs = dbhCmd.ExecuteNonQuery ();

				if (num_recs == 0)
					return false;

			}

			return true;
		}
	}
}

