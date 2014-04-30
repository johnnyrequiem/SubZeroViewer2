using System;
using System.Net;
using System.IO;
using System.Text;
using GLib;
using Gtk;
using System.Collections;

namespace SubZeroViewer2
{
	public class CFTP
	{
		private CDevice _device = null;
		private CServer _server = null;
		private FtpWebRequest _ftpRequest = null;
		private string _final_server_path = null;

		private DateTime _file_modified_date;

		public CFTP ()
		{

		}

		public CFTP ( CServer server, CDevice device ) {
			_server = server;
			_device = device;
		}

		public DateTime CFTP_LogFileDate {
			get {
				return _file_modified_date;
			}
		}

		public bool CFTP_Connection ( string requestMethod, string file_name ) {

			string s_server = null;

			if (_server != null) {
				s_server =  _server.ServerProtocol + _server.ServerAddr;
			} else {
				MessageDialog msg = new MessageDialog (null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, 
					"The currenly configured Server for this Device " +
					"is invalid, please reconfigure the device.", 0);
				msg.Run ();
				msg.Destroy ();
				return false;
			}

			//TODO: Fix server and directory strings here.

				if ( requestMethod == (WebRequestMethods.Ftp.DownloadFile )) {

				string _final_server_path = s_server + _server.ServerBaseDir + _device.DeviceServerSubDir + 
					                       file_name;

				Console.WriteLine (_final_server_path);

				_ftpRequest = (FtpWebRequest)WebRequest.Create ( _final_server_path );

				} else {

					// Check server string for completness;
					
				_final_server_path = s_server + _server.ServerBaseDir + _device.DeviceServerSubDir;

				Console.WriteLine (_final_server_path);

				try {

					_ftpRequest = (FtpWebRequest)WebRequest.Create ( s_server + _server.ServerBaseDir + 
						_device.DeviceServerSubDir );

				} catch (WebException e) {
						if (e.Status == WebExceptionStatus.ProtocolError) {
							
						new Gtk.MessageDialog (null, Gtk.DialogFlags.Modal, Gtk.MessageType.Error,
							Gtk.ButtonsType.Ok, e.Message, 0).Show();
							
						}
					}
				
				}

				_ftpRequest.Method = requestMethod;

			//TODO: Error Handling 

				_ftpRequest.Credentials = new NetworkCredential ( 
					_server.ServerUserName, 
					_server.ServerPassword
				);

			return true;

		}

		public StreamReader CFTP_GetFile ( string filename ) {

			if ( _server != null ) {

				if (_server.ServerProtocol == "file://") {

					StreamReader file_reader = new StreamReader (
						new FileStream (_device.DeviceServerSubDir + filename, FileMode.Open ),
						                           true
					                           );

					return file_reader;

				} else {

					CFTP_Connection (WebRequestMethods.Ftp.DownloadFile, filename);

					FtpWebResponse ftp_response = (FtpWebResponse)_ftpRequest.GetResponse ();
					_file_modified_date = ftp_response.LastModified;

					if (_file_modified_date == new DateTime (0001, 01, 01, 00, 00, 00)) {
						_file_modified_date = DateTime.Now;
					}

					Stream responseStream = ftp_response.GetResponseStream ();

					StreamReader file_reader = new StreamReader (responseStream);

					return file_reader;
				}
			}

			return null;
		}

		public ArrayList CFTP_GetFileList (  ) {


			// Innitiate connection.
			if (CFTP_Connection (WebRequestMethods.Ftp.ListDirectory, null) == false)
				return null;

			// Get server response.
			FtpWebResponse response = (FtpWebResponse)_ftpRequest.GetResponse ();

			// Setup stream reader.
			Stream responseStream = response.GetResponseStream ();
			StreamReader reader = new StreamReader (responseStream);

			string serverResponse = reader.ReadToEnd ().ToString ();
			string[] files = serverResponse.Split ('\n');
			ArrayList ftp_files = new ArrayList ();
			int _index = 0;

			for (int i = 0; i < (files.Length); i++) {
							
					if (files [i].EndsWith ("\r"))
						files [i] = files [i].Substring (0, (files [i].Length) - ("\r".Length));

					if (files [i].EndsWith ("CSV"))
					ftp_files.Add (files [i]);
			}

			return ftp_files;

		}

		public CDevice CFTP_Device {
			get {
				return _device;
			}
			set {
				_device = value;
			}
		}

		public CServer CFTP_Server {
			get {
				return _server;
			}
			set {
				_server = value;
			}
		}

	}
}

