using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.ComponentModel;

using ZedGraph;
using Gtk;
using GLib;
using Cairo;
using NPlot.Gtk;

using SubZeroViewer2;

public partial class MainWindow: Gtk.Window
{
	private CUtillity CUtil = new CUtillity ();
	private CSQL csql = new CSQL();
	private CDevice c_current_device = null;
	private CServer c_current_server = null;
	private CFTP c_current_ftp_session = null;
	private CLogFile c_current_logfile = null;
	private CGraph c_current_graph = null;
	private CProgress c_current_progress = new CProgress("Working...");
	private PlatformID c_current_platform;

	private CDeviceFileDescription c_file_description_entry;
	private ArrayList ar_file_description;
	private bool _status_stop = true;

	struct t_curTreePathItem {
		public string value;
		public TreeModel model;
		public TreePath path;
	};

	t_curTreePathItem curTreePathItem = new t_curTreePathItem();

	private enum nb_Notebook_Pages {
		HOME,
		CONFIG,
		SERVER_SETTINGS,
		SERVER_BROWSER,
		DEVICE_BROWSER,
		DEVICE_SETTINGS,
		GRAPH,
		FTP_BROWSER
	};

	private enum FileEntryColumns {
		CODE,
		DESCR,
		TYPE
	};

	private Menu mnu_server = new Menu ();
	private MenuItem mnu_server_menu_title = new MenuItem ("SERVER MENU");
	private MenuItem mnu_add_server = new MenuItem ("Add New Server");
	private MenuItem mnu_del_server = new MenuItem ("Delete Server");
	private MenuItem mnu_edit_server = new MenuItem ("Edit Server");

	private Menu mnu_device = new Menu ();
	private MenuItem mnu_device_menu_title = new MenuItem ("DEVICE MENU");
	private MenuItem mnu_add_device = new MenuItem ("Add New Device");
	private MenuItem mnu_del_device = new MenuItem ("Delete Device");
	private MenuItem mnu_edit_device = new MenuItem ("Edit Device");


	/** TODO: mnu_logfile not yet implemented. **/
	private Menu mnu_logfile = new Menu ();
	private MenuItem mnu_logfile_title = new MenuItem ("LOGFILE MENU");
	private MenuItem mnu_logfile_view = new MenuItem ("View Contents");
	private MenuItem mnu_logfile_graph = new MenuItem ("Generate Graph");

	private Menu mnu_graph = new Menu ();

	private string DefaultWindowTitle = "SubZeroViewer2";

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{

		c_current_platform = Environment.OSVersion.Platform;

		build_popups ();
		Gtk.Rc.Parse (CUtil.CUTIL_GetRCFile(0));
	
		Build ();

		//set_widget_colors ();
		hide_tab_labels (true);
		set_general_widget_properties ();
		set_widget_events ();

	}



	private void set_widget_colors() {

		Func<Widget[], Gdk.Color, int> set_widgets_bg  = 
			delegate (Widget[] _widgets, Gdk.Color _color) {

			for (int i = 0; i < _widgets.Length; i++) {

				_widgets [i].ModifyBg (StateType.Active, _color );
				_widgets [i].ModifyBase (StateType.Active, _color);
				_widgets [i].ModifyBg (StateType.Normal, _color);
				_widgets [i].ModifyBase (StateType.Normal, _color);

			}
			return 0;
		};

		nbGraph.ModifyBg (StateType.Normal, CUtil.get_light_grey());
		nbGraph.ModifyBase (StateType.Normal, CUtil.get_light_grey());
		swGraph.ModifyBase (StateType.Normal, CUtil.get_light_grey());
		swAlarms.ModifyBase (StateType.Normal, CUtil.get_light_grey());
		tvwAlarms.ModifyBase (StateType.Normal, CUtil.get_light_grey());
		tvwAlarms.ModifyBg (StateType.Normal, CUtil.get_light_grey());
		set_widgets_bg (tvwAlarms.Children, CUtil.get_light_grey());
		nbFrames.ModifyBase (StateType.Normal, CUtil.get_light_grey());


	}

	private void build_popups() {

		/** ====================
		 * DO SERVER MENU
		 * ====================
		 */


		mnu_add_server.ButtonReleaseEvent += delegate(object o, ButtonReleaseEventArgs args) {

			clear_server_settings();
			c_current_server = new CServer();
			c_current_server.Update = false;
			nbFrames.CurrentPage = (int)nb_Notebook_Pages.SERVER_SETTINGS;

		};

		mnu_del_server.ButtonReleaseEvent += delegate(object o, ButtonReleaseEventArgs args) {

			mnu_server.Popdown();

			MessageDialog dlg = new MessageDialog(this,DialogFlags.DestroyWithParent, MessageType.Question, ButtonsType.YesNo,
				"Are you sure you want to delete the Server \n" +
				"and all it's data?", 0);
			ResponseType res = (ResponseType)dlg.Run();
			dlg.Destroy();

			if (res == ResponseType.Yes) {
				ListStore store = (ListStore) ivServerBrowser.Model;
				TreePath[] path = ivServerBrowser.SelectedItems;
				TreeIter iter;

				store.GetIter (out iter, path[0]);

				if (curTreePathItem.value != null) {
					csql.CSQL_DeleteServer (curTreePathItem.value);
					store.Remove(ref iter);
				}
			}

		};

		mnu_edit_server.ButtonReleaseEvent += delegate(object o, ButtonReleaseEventArgs args) {
			populate_server_settings();
		};


		mnu_server.Add (mnu_server_menu_title);
		mnu_server.Add (new SeparatorMenuItem());
		mnu_server.Add (mnu_add_server);
		mnu_server.Add (mnu_del_server);
		mnu_server.Add (mnu_edit_server);

		mnu_server.ShowAll ();

		mnu_del_server.Visible = false;
		mnu_edit_server.Visible = false;

		/*
		 * ==============
		 * DO DEVICE MENU
		 * ==============
		 */

		mnu_add_device.ButtonReleaseEvent += delegate(object o, ButtonReleaseEventArgs args) {
			clear_device_settings();
			populate_server_combo();
			populate_log_file_columns();
			c_current_device = new CDevice();
			c_current_device.Update = false;

			nbFrames.CurrentPage = (int)nb_Notebook_Pages.DEVICE_SETTINGS;
		};

		mnu_del_device.ButtonReleaseEvent += delegate(object o, ButtonReleaseEventArgs args) {
		
			mnu_device.Popdown();

			MessageDialog dlg = new MessageDialog (this, DialogFlags.DestroyWithParent, MessageType.Question,
				ButtonsType.YesNo, "Are you woud like to delete the device and all it's records?", 0);

			ResponseType res = (ResponseType)dlg.Run();

			if (res == ResponseType.Yes) {
				ListStore store = (ListStore)ivDeviceBrowser.Model;
				TreePath[] path = ivDeviceBrowser.SelectedItems;
				TreeIter iter;

				store.GetIter (out iter, path[0]);

				if (curTreePathItem.value != null) {
					if (csql.CSQL_DeleteDevice (curTreePathItem.value))
						store.Remove (ref iter);
				}
			}

			dlg.Destroy();
		};

		mnu_edit_device.ButtonReleaseEvent += delegate(object o, ButtonReleaseEventArgs args) {
			edit_device();
		};

		mnu_device_menu_title.CanDefault = false;
		mnu_device_menu_title.CanFocus = false;
		mnu_device.Add (mnu_device_menu_title);
		mnu_device.Add (new SeparatorMenuItem ());
		mnu_device.Add (mnu_add_device);
		mnu_device.Add (mnu_del_device);
		mnu_device.Add (mnu_edit_device);

		mnu_device.ShowAll ();
		mnu_del_device.Visible = false;
		mnu_edit_device.Visible = false;

		/*
		 * ====================
		 * DO LOGFILE MENU
		 * ==================
		 */


	}

	protected void OnBtnLogFileAddClicked (object sender, EventArgs e)
	{
		c_file_description_entry = new CDeviceFileDescription ("CODE", "Description...", "TYPE");
		ar_file_description.Add (c_file_description_entry);

		TreeStore store = (TreeStore)nvDeviceFileDescr.Model;
		TreeIter iter = store.AppendValues (c_file_description_entry.DeviceCode, c_file_description_entry.DeviceCodeDescription, c_file_description_entry.DeviceCodeType);
		TreePath path = store.GetPath (iter);
		nvDeviceFileDescr.SetCursor (path, nvDeviceFileDescr.Columns[0], true);

	}

	private void populate_log_file_columns() {

		/**
		 * COLUMNS
		 * =======
		 * device_sensor_code
		 * device_sensor_description
		 * device_sensor_value_type <- combobox
		 * device_sensor_value <- Surely NOT?
		 * nvDeviceFileDesctiption
		 */

		mnu_device.Popdown ();

		TreeStore ls_nvDeviceFileDescription = new TreeStore (typeof(string), typeof(string), typeof(string));
		ListStore ls_dev_sensor_codes = new ListStore (typeof(string));
		ListStore ls_dev_sensor_value_types = new ListStore (typeof(string));

		if (ar_file_description == null) 
			ar_file_description = new ArrayList ();

			if (nvDeviceFileDescr.Columns.Length < 1) { 
				Gtk.TreeViewColumn col_sensor_code = new Gtk.TreeViewColumn ();
				col_sensor_code.Title = "Sensor Code";
				col_sensor_code.Sizing = TreeViewColumnSizing.Autosize;

				Gtk.TreeViewColumn col_sensor_descr = new Gtk.TreeViewColumn ();
				col_sensor_descr.Title = "Sensor Descr";
				col_sensor_descr.Sizing = TreeViewColumnSizing.Autosize;

				Gtk.TreeViewColumn col_sensor_value_type = new Gtk.TreeViewColumn ();
				col_sensor_value_type.Title = "Value Type";
				col_sensor_value_type.Sizing = TreeViewColumnSizing.Autosize;

				CellRendererCombo dev_sensor_codes = new CellRendererCombo ();
				dev_sensor_codes.Editable = true;
				dev_sensor_codes.Edited += delegate(object o, EditedArgs args) {
					int i = int.Parse (args.Path);
					c_file_description_entry = (CDeviceFileDescription)ar_file_description [i];
					c_file_description_entry.DeviceCode = args.NewText;

					TreeIter iter;
					TreeStore store = (TreeStore)nvDeviceFileDescr.Model;
					store.GetIterFromString (out iter, args.Path);
					store.SetValue (iter, 0, c_file_description_entry.DeviceCode);

				};

				CellRendererCombo dev_sensor_value_types = new CellRendererCombo ();
				dev_sensor_value_types.Editable = true;
				dev_sensor_value_types.Edited += delegate(object o, EditedArgs args) {
					int i = int.Parse (args.Path);
					c_file_description_entry = (CDeviceFileDescription)ar_file_description [i];
					c_file_description_entry.DeviceCodeType = args.NewText;

					TreeIter iter;
					TreeStore store = (TreeStore)nvDeviceFileDescr.Model;
					store.GetIterFromString (out iter, args.Path);
					store.SetValue (iter, 2, c_file_description_entry.DeviceCodeType);
				};

				CellRendererText dev_sensor_descr = new CellRendererText ();
				dev_sensor_descr.Editable = true;
				dev_sensor_descr.Edited += delegate(object o, EditedArgs args) {
					int i = int.Parse (args.Path);
					c_file_description_entry = (CDeviceFileDescription)ar_file_description [i];
					c_file_description_entry.DeviceCodeDescription = args.NewText;

					TreeIter iter;
					TreeStore store = (TreeStore)nvDeviceFileDescr.Model;
					store.GetIterFromString (out iter, args.Path);
					store.SetValue (iter, 1, c_file_description_entry.DeviceCodeDescription);
				};
			
				dev_sensor_codes.Model = ls_dev_sensor_codes;
				dev_sensor_codes.TextColumn = 0;

				dev_sensor_value_types.Model = ls_dev_sensor_value_types;
				dev_sensor_value_types.TextColumn = 0;

				col_sensor_code.PackStart (dev_sensor_codes, true);
				col_sensor_code.AddAttribute (dev_sensor_codes, "text", 0);

				col_sensor_descr.PackStart (dev_sensor_descr, true);
				col_sensor_descr.AddAttribute (dev_sensor_descr, "text", 1);

				col_sensor_value_type.PackStart (dev_sensor_value_types, true);
				col_sensor_value_type.AddAttribute (dev_sensor_value_types, "text", 2);

				nvDeviceFileDescr.AppendColumn (col_sensor_code);
				nvDeviceFileDescr.AppendColumn (col_sensor_descr);
			//nvDeviceFileDescr.AppendColumn (col_sensor_value_type);
			}

		nvDeviceFileDescr.DoubleBuffered = true;


		string[] _codes = csql.CSQL_GetDeviceSensorCodes ();
		string[] _types = csql.CSQL_GetDeviceSensorValueTypes ();

		// Do codes.
		if (_codes != null) {
			for (int i = 0; i < _codes.Length; i++) {
				ls_dev_sensor_codes.AppendValues (_codes [i]);
			}
		}

		//Do Types
		if (_types != null) {
			for (int i = 0; i < _types.Length; i++) {
				ls_dev_sensor_value_types.AppendValues (_types [i]);
			}
		}

		//Do any description entries, if available.
		if (ar_file_description.Count > 0) {

			for (int i = 0; i < ar_file_description.Count; i++) {

				CDeviceFileDescription file_descr = 
					(CDeviceFileDescription)ar_file_description [i];

				ls_nvDeviceFileDescription.AppendValues (file_descr.DeviceCode, 
					file_descr.DeviceCodeDescription, file_descr.DeviceCodeType);

			}

		}


		nvDeviceFileDescr.Model = ls_nvDeviceFileDescription;

	}

	private void OnBtnSettingsBrowseClick (object o, EventArgs e) {

		string _server_subfolder = null;
		string _server_string = null;
		string _server_protocol = null;

		int _server_string_length = 0;
		int _dlg_uri_length = 0;
		int _url_diff = 0;

		int _response = -1;

		FileChooserDialog dlg = new FileChooserDialog (
			"Device Sub Folder",
			this,
			FileChooserAction.SelectFolder,
			"Cancel", ResponseType.Cancel,
			"Choose", ResponseType.Accept
		);

		c_current_server = csql.CSQL_GetServer (c_current_device.DeviceServer );
		_server_string = c_current_server.ServerAddr + c_current_server.ServerBaseDir;
		_server_string_length = _server_string.Length;
		_server_protocol = c_current_server.ServerProtocol;

		if (_server_protocol == "file://") {
			dlg.LocalOnly = false;
			_response = dlg.Run ();

			if (_response == (int)ResponseType.Accept) {

				if (dlg.Uri.StartsWith("file://")) {
					entDeviceServerSubDir.Text = dlg.Uri.Substring("file://".Length);

					if (entDeviceServerSubDir.Text.EndsWith (System.IO.Path.DirectorySeparatorChar.ToString()) == false )
						entDeviceServerSubDir.Text += System.IO.Path.DirectorySeparatorChar.ToString();

					c_current_device.DeviceServerSubDir = entDeviceServerSubDir.Text;
				} else {
					entDeviceServerSubDir.Text = dlg.Uri + System.IO.Path.DirectorySeparatorChar;
					c_current_device.DeviceServerSubDir = dlg.Uri + System.IO.Path.DirectorySeparatorChar;
				}

			} else {

			}

		} else {

			dlg.LocalOnly = false;
			dlg.SetUri (_server_string);
			_response = dlg.Run ();

			if (_response == (int)ResponseType.Accept) {

				string s = dlg.Uri;
				_dlg_uri_length = s.Length;
				_url_diff = _dlg_uri_length - _server_string_length;


				if (_url_diff > 0) {

					_server_subfolder = s.Substring (_server_string_length, _dlg_uri_length - _server_string_length);

					if (_server_subfolder.EndsWith ("/") != true)
						_server_subfolder += "/";

				} else {

				}

				entDeviceServerSubDir.Text = _server_subfolder;
				c_current_device.DeviceServerSubDir = _server_subfolder;

			} else {


			}
		}
		dlg.Destroy ();
	}

	private void populate_server_combo() {

		// TODO: Debug populate_server_combo()

		/*
		 * Why is this procedure being called twice when
		 * once is expected?
		 * Why does it stay clear when populated after clear_device_settings()
		 */

		mnu_device.Popdown ();

		Console.Out.WriteLine ("populate_server_combo()");

		cboDeviceServer.Model = new ListStore (typeof(string), typeof(string));
		CServerListItem[] cserver;

		cserver = csql.CSQL_GetServerList ();

		if (cserver != null) {

			for (int i = 0; i < cserver.Length; i++) {
				cboDeviceServer.AppendText (cserver [i].FriendlyName + '\t' +  cserver [i].Address);
			}

			cboDeviceServer.ShowAll ();

		}

	}

	private void clear_device_settings() {
		//TODO: Implement clear_device_settings()
		entDeviceID.Text = "";
		entDeviceDescription.Text = "";
		entDeviceLocation.Text = "";
		entDeviceServerSubDir.Text = "";
		cboDeviceServer.Model = null;

		try {
			ar_file_description.Clear ();
		} catch {
			// Ignore.
		}
	}

	private void mnu_del_server_enabled (bool enabled) {
		mnu_del_server.Visible = enabled;
		mnu_edit_server.Visible = enabled;
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
	}

	protected void OnBtnDeviceSettingsSaveClicked ( object sender, EventArgs args ) {

		// Populate DeviceClass
		string[] ar_server = cboDeviceServer.ActiveText.Split ('\t');
		string server = null;

		if (ar_server[0] == null) {
			server = ar_server[1];
		} else {
			server = ar_server[0];
		}

		c_current_device.DeviceServer = server;
		//TODO: Populate new device_logfile_description from current, re-ordered list.
		c_current_device.DeviceFileDescription = ar_file_description;

		if (c_current_device.Update) {
			csql.CSQL_UpdateDevice (c_current_device);
			sb_status.Push (0, "Device Updated");
		} else {
			csql.CSQL_AddDevice (c_current_device);
			sb_status.Push (0, "New Device Saved");

			ListStore store = (ListStore)ivDeviceBrowser.Model;

			if (store == null) {
				store = new ListStore (typeof(string), typeof(Gdk.Pixbuf));
			}

			store.AppendValues (c_current_device.DeviceID, new Gdk.Pixbuf (CUtil.CUTIL_GetDeviceIcon (), 64, 64));

		}

		nbFrames.CurrentPage = (int)nb_Notebook_Pages.DEVICE_BROWSER;

		clear_device_settings ();

		ResetDeviceUpdateReady ();

	}

	protected void OnConfigureClicked (object sender, EventArgs e)
	{
		nbFrames.CurrentPage = (int)nb_Notebook_Pages.CONFIG;
	}

	protected void OnBtnConfigBackClicked (object sender, EventArgs e)
	{
		nbFrames.CurrentPage = (int)nb_Notebook_Pages.HOME;
	}
		
	protected void OnIvServerBrowserItemActivated (object o, ItemActivatedArgs args)
	{
		//TODO: What do we do for server activation ?
	}

	private void populate_server_settings() {

		mnu_server.Popdown ();

		string server = get_BrowserItem_Value ( ivServerBrowser );

		c_current_server = csql.CSQL_GetServer (server);

		if (c_current_server != null) {
			entServer.Text = c_current_server.ServerAddr;
			entPort.Text = Convert.ToString (c_current_server.ServerPort);
			entFriendlyName.Text = c_current_server.ServerFriendName;
			entBaseDir.Text = c_current_server.ServerBaseDir;
			cbAuth.Active = Convert.ToBoolean (c_current_server.ServerAuthRequired);
			entUname.Text = c_current_server.ServerUserName;
			entPword.Text = c_current_server.ServerPassword;
		}

		//============================================================
		//		POPULATE PROTOCOL COMBO
		//============================================================

		string proto = null;
		TreeIter iter;
		ListStore model = (ListStore)cboProtocol.Model;

		cboProtocol.Model.GetIterFirst (out iter);

		while ( (proto = (string)model.GetValue (iter, 0)) != null) {

			if ( proto == c_current_server.ServerProtocol )
				break;

			model.IterNext (ref iter);
		}

		cboProtocol.SetActiveIter (iter);

		//=============================================================

		c_current_server.Update = true;
		nbFrames.CurrentPage = (int)nb_Notebook_Pages.SERVER_SETTINGS;
	}

	private string get_BrowserItem_Value( Gtk.IconView browser ) {

		ListStore model = null;
		TreePath[] curTreePath = null;
		TreeIter iter;

		curTreePath = browser.SelectedItems;

		if (curTreePath == null)
			return null;

		model = (ListStore)browser.Model;
		model.GetIter (out iter, curTreePath[0]);
			
		browser.SelectPath (curTreePath[0]);

		curTreePathItem.value = model.GetValue (iter, 0).ToString();
		curTreePathItem.model = model;
		curTreePathItem.path = curTreePath[0];

		return curTreePathItem.value;

	}

	private string get_BrowserItem_Value_ByPos ( Gtk.IconView browser ) {

		int pointX = 0, pointY = 0;
		ListStore store = null;
		TreeIter iter;
		TreePath path = null;
		string server = null;

		store = (ListStore)browser.Model;

		if (store == null)
			return null;

		browser.GetPointer (out pointX, out pointY);
		path = browser.GetPathAtPos (pointX, pointY);

		browser.SelectPath (path);

		curTreePathItem.path = path;
		curTreePathItem.model = store;

		store.GetIter (out iter, path);
		server = (string)store.GetValue (iter, 0);

		curTreePathItem.value = server;

		return server;
	}

	protected void OnBtnConfigServersClicked (object sender, EventArgs e)
	{

		/**
		 * 1) Get Server List.
		 * 2) Add Server icons if required.
		 */

		populate_ivServerBrowser ();

		nbFrames.CurrentPage = (int)nb_Notebook_Pages.SERVER_BROWSER;


	}

	private void populate_ivDeviceBrowser() {

		CDeviceListItem[] device_list = csql.CSQL_GetDeviceList ();
		ListStore store = null;

		if (device_list != null) {

			if (ivDeviceBrowser.Model != null) {
				store = (ListStore)ivDeviceBrowser.Model;
				store.Clear ();
			} else {
				store = new ListStore (typeof(string), typeof(Gdk.Pixbuf));
			}

			for (int i = 0; i < device_list.Length; i++) {
				store.AppendValues (device_list [i].DeviceID, new Gdk.Pixbuf (CUtil.CUTIL_GetDeviceIcon(), 64, 64));
			}

			if (ivDeviceBrowser.Model == null) {
				ivDeviceBrowser.MarkupColumn = 0;
				ivDeviceBrowser.PixbufColumn = 1;
				ivDeviceBrowser.Model = store;
			}
		}
	}
	
	private void populate_ivServerBrowser() {
	
		CServerListItem[] server_list = csql.CSQL_GetServerList ();
		string str;
		ListStore store = null;

		if (server_list != null) {

			if (ivServerBrowser.Model != null) {
				store = (ListStore)ivServerBrowser.Model;
				store.Clear ();
			} else {
				store = new ListStore (typeof(string), typeof(Gdk.Pixbuf));
			}

			for (int i = 0; i < server_list.Length; i++) {

				if (server_list [i].FriendlyName == "") {
					str = server_list [i].Address;
				} else {
					str = server_list [i].FriendlyName;
				}

				store.AppendValues (str, new Gdk.Pixbuf(CUtil.CUTIL_GetServerIcon(), 64, 64));

			}

			if (ivServerBrowser.Model == null) {
				ivServerBrowser.MarkupColumn = 0;
				ivServerBrowser.PixbufColumn = 1;
				ivServerBrowser.Model = store;
			} 
		}

	}


	private void clear_server_settings() {

		entFriendlyName.Text = "";
		entServer.Text = "";
		entBaseDir.Text = "";
		entPort.Text = "";
		entUname.Text = "";
		entPword.Text = "";
		cbAuth.Active = false;

	}

	protected void OnBtnSettingsBackClicked (object sender, EventArgs e)
	{
		nbFrames.CurrentPage = (int)nb_Notebook_Pages.SERVER_BROWSER;
		clear_server_settings ();
	}

	protected void OnBtnManageDevicesClicked (object sender, EventArgs e)
	{
		populate_ivDeviceBrowser ();
		nbFrames.CurrentPage = (int)nb_Notebook_Pages.DEVICE_BROWSER;
	}

	//==================ZEDGRAPH=================
	void OnIvFtpBrowserItemActivated (object o, ItemActivatedArgs args) {

		string selected_file = get_BrowserItem_Value (ivFtpBrowser);
		StreamReader file_reader = c_current_ftp_session.CFTP_GetFile (selected_file);

		Viewport _port = new Viewport (new Adjustment(1000, 500, 1000, 5, 0, 500), 
			new Adjustment (500, 500, 500, 0, 0, 500));
		_port.WidthRequest = 2000;
		_port.ResizeMode = ResizeMode.Parent;

		ZedGraphControl g_graph = new ZedGraphControl ();

		c_current_logfile = new CLogFile 
		                    (
			                    file_reader, 
			                    c_current_ftp_session.CFTP_LogFileDate,
			                    c_current_device.DeviceFileDescription
			                   );

		c_current_graph = new CGraph (c_current_logfile.LogFileEntries);
		//c_current_graph.YAxisCodeIndex = c_current_logfile.CLogFile_Get_Y_Axis_Code_Index (c_current_device);
		c_current_graph.Title = selected_file;
		c_current_graph.YAxisCode = "CURTEMP";
		c_current_graph.YAxisLabel = "Current Temperature";
		c_current_graph.XAxisLabel = "Time Stamp";
		c_current_graph.CGRAPH_X_AxisData = c_current_logfile.LogFileTime;
		c_current_graph.CGRAPH_Y_AxisData = c_current_logfile.LogFileTemps;

		//Plot Graph
		c_current_graph.ploy_zedgraph (out g_graph);
		System.Drawing.Bitmap bmp_graph = 
			new System.Drawing.Bitmap (g_graph.Width, g_graph.Height);

		g_graph.DrawToBitmap(bmp_graph, 
			new System.Drawing.Rectangle(0, 0, g_graph.Width, g_graph.Height));

		bmp_graph.Save ("graph.bmp");

		Gtk.Image img = new Image ("graph.bmp");
		Gdk.Image g_img = img.ImageProp;

		img_graph.ImageProp = g_img;
		img_graph.File = "graph.bmp";
		img_graph.QueueDraw ();
		img_graph.ShowAll ();

		/**
		 * Fill Alarms Table
		 */

		FillAlarmsTable (c_current_logfile.LogFileAlarms);

		file_reader.Close ();
		file_reader.Dispose ();

		nbFrames.CurrentPage = (int)nb_Notebook_Pages.GRAPH;
	}

	//========NPLOT===========
//	void OnIvFtpBrowserItemActivated (object o, ItemActivatedArgs args)
//	{
//		//TODO: Implement OnIvFtpBrowserItemActivated
//
//		/* Get and parse file.
//		 * Populate appropriate classes.
//		 * --> LogFileEntries.
//		 * --> Graph Class
//		 * Plot graph and alarms.
//		 */
//
//		NPlot.Gtk.PlotSurface2D graph = null;
//
//
//		string selected_file = get_BrowserItem_Value (ivFtpBrowser);
//		StreamReader file_reader = c_current_ftp_session.CFTP_GetFile (selected_file);
//
//		Viewport _port = new Viewport (new Adjustment(1000, 500, 1000, 5, 0, 500), 
//			new Adjustment (500, 500, 500, 0, 0, 500));
//		_port.WidthRequest = 2000;
//		_port.ResizeMode = ResizeMode.Parent;
//
//		c_current_logfile = new CLogFile 
//            (
//                file_reader, 
//                c_current_ftp_session.CFTP_LogFileDate,
//                c_current_device.DeviceFileDescription
//            );
//
//		c_current_graph = new CGraph (c_current_logfile.LogFileEntries);
//		//c_current_graph.YAxisCodeIndex = c_current_logfile.CLogFile_Get_Y_Axis_Code_Index (c_current_device);
//		c_current_graph.Title = selected_file;
//		c_current_graph.YAxisCode = "CURTEMP";
//		c_current_graph.YAxisLabel = "Current Temperature";
//		c_current_graph.XAxisLabel = "Time Stamp";
//		c_current_graph.CGRAPH_X_AxisData = c_current_logfile.LogFileTime;
//		c_current_graph.CGRAPH_Y_AxisData = c_current_logfile.LogFileTemps;
//
//		/**
//		 * Plot Graph
//		 */
//
//		swGraph.Remove ((Widget)swGraph.Child);
//
//		graph = new PlotSurface2D();
//		graph.WidthRequest = 2000;
//		graph.Allocation  = (new Gdk.Rectangle (0, 0, 2000, 500));
//
//		c_current_graph.plot_nplot (out graph);
//
//		swGraph.ReallocateRedraws = true;
//
//		//swGraph.AddWithViewport (graph);
//		_port.Add (graph);
//		swGraph.ResizeMode = ResizeMode.Queue;
//		swGraph.Add (_port);
//		graph.QueueResize ();
//		swGraph.QueueDraw ();
//		swGraph.ShowAll ();
//
//		/**
//		 * Fill Alarms Table
//		 */
//
//		FillAlarmsTable (c_current_logfile.LogFileAlarms);
//
//		file_reader.Close ();
//		file_reader.Dispose ();
//
//		nbFrames.CurrentPage = (int)nb_Notebook_Pages.GRAPH;
//
//	}

	private void FillAlarmsTable (ArrayList alarms) {

		ListStore store = new ListStore (typeof(string), typeof(string), typeof(string));

		for (int i = 0; i < alarms.Count; i++) {

			CDeviceLogFileEntry __device_entry = (CDeviceLogFileEntry)alarms [i];

			store.AppendValues (
				__device_entry.LogFIleSensorCode,
				__device_entry.LogFileSensorDescr,
				__device_entry.LogFileSensorValue
			);

		}
				
			TreeViewColumn col_device_code = new TreeViewColumn ();
			col_device_code.Title = "Device Code";

			TreeViewColumn col_device_code_descr = new TreeViewColumn ();
			col_device_code_descr.Title = "Device Code Description";

			TreeViewColumn col_device_code_value = new TreeViewColumn ();
			col_device_code_value.Title = "Device Code Value";

			CellRendererText dev_device_code = new CellRendererText ();
			CellRendererText dev_device_code_descr = new CellRendererText ();
			CellRendererText dev_device_code_value = new CellRendererText ();

			col_device_code.PackStart (dev_device_code, true);
			col_device_code.AddAttribute (dev_device_code, "text", 0);

			col_device_code_descr.PackStart (dev_device_code_descr, true);
			col_device_code_descr.AddAttribute (dev_device_code_descr, "text", 1);

			col_device_code_value.PackStart (dev_device_code_value, true);
			col_device_code_value.AddAttribute (dev_device_code_value, "text", 2);

		if (tvwAlarms.Columns.Length < 1) {

			tvwAlarms.AppendColumn (col_device_code);
			tvwAlarms.AppendColumn (col_device_code_descr);
			tvwAlarms.AppendColumn (col_device_code_value);

		}

		tvwAlarms.Model = store;

	}

	protected void OnIvDeviceBrowserItemActivated (object o, ItemActivatedArgs args)
	{
		//TODO: Implement device browser item activated for Ftp.

		/**
		 * - Get item activated from browser model
		 * - Get item activated from sqlite
		 * - Get Server for selected Device
		 * - Get File list
		 * - Build file list model for ftp browser.
		 */

		// Get device details from model and sqlite.
		TreePath[] path = ivDeviceBrowser.SelectedItems;
		TreeIter iter;
		string _device_key = null;
		ArrayList ftp_file_list = new ArrayList();

			ivDeviceBrowser.Model.GetIter (out iter, path [0]);
			_device_key = (string)ivDeviceBrowser.Model.GetValue (iter, 0);

			c_current_device = csql.CSQL_GetDevice (_device_key);
			c_current_server = csql.CSQL_GetServer (c_current_device.DeviceServer);
			c_current_ftp_session = new CFTP (c_current_server, c_current_device);

		if (c_current_server.ServerProtocol == "file://") {

			string __dir_to_list = c_current_device.DeviceServerSubDir;

			foreach (string s_file in Directory.EnumerateFiles (__dir_to_list)) {

				string c_file = s_file;
			
				if (c_current_platform == PlatformID.Win32NT)
					c_file = s_file.Substring (2);
			
				string[] file_name = c_file.Split (System.IO.Path.DirectorySeparatorChar);
				ftp_file_list.Add (file_name[(file_name.Length - 1)]);

			}


		} else {

			try {

				ftp_file_list = c_current_ftp_session.CFTP_GetFileList ();
				
			} catch (WebException e) {

				if (e.Status == WebExceptionStatus.ProtocolError) {

					string msg = "Please check you Server and Device directory settings.";

					new Gtk.MessageDialog (null, Gtk.DialogFlags.Modal, Gtk.MessageType.Error,
						Gtk.ButtonsType.Ok, e.Message + "\n" + msg, 0).Run ();

				} else {

					Console.WriteLine (e.Status + "\n" + e.Message + "\n" + e.Source);
				}

				return;
			}
		}

		if (ftp_file_list != null) {
			populate_FtpFileBrowser (ftp_file_list);
		}

	}

	private void populate_FtpFileBrowser ( ArrayList file_list ) {

		ListStore store = new ListStore (typeof(string), typeof(Gdk.Pixbuf));

		// Populate ListStore.
		for (int i = 0; i < file_list.Count; i++) {
			if (file_list[i] != null)
				store.AppendValues ((string)file_list [i], new Gdk.Pixbuf (CUtil.CUTIL_GetFileIcon (), 64, 64));
		}

		ivFtpBrowser.MarkupColumn = 0;
		ivFtpBrowser.PixbufColumn = 1;
		ivFtpBrowser.Model = store;

		nbFrames.CurrentPage = (int)nb_Notebook_Pages.FTP_BROWSER;

	}

	private void edit_device() {

		// Get device details and populate device settings.
		TreePath[] path = ivDeviceBrowser.SelectedItems;
		TreeIter iter;
		string _device_key = null;

		mnu_device.Popdown ();

		ivDeviceBrowser.Model.GetIter (out iter, path[0]);
		_device_key = (string)ivDeviceBrowser.Model.GetValue (iter, 0);

		c_current_device = csql.CSQL_GetDevice (_device_key);

		populate_device_settings (c_current_device);

		populate_device_file_descr (c_current_device);

		c_current_device.Update = true;
		nbFrames.CurrentPage = (int)nb_Notebook_Pages.DEVICE_SETTINGS;

	}

	private void add_default_descr (object sender, EventArgs args)
	{

		ar_file_description = CUtil.CUTIL_DefaultDeviceFileDescription;
		populate_log_file_columns ();

	}

	private void populate_device_file_descr ( CDevice device ) {

			ar_file_description = device.DeviceFileDescription;
			populate_log_file_columns ();

	}

	private void populate_device_settings ( CDevice device ) {
		//TODO: Fix broken bits for device settings population <- cboServers.

		entDeviceID.Text = device.DeviceID;
		entDeviceDescription.Text = device.DeviceDescription;
		entDeviceLocation.Text = device.DeviceLocation;
		entDeviceServerSubDir.Text = device.DeviceServerSubDir;

		// Do cboDeviceSever
		populate_server_combo ();

			TreeIter iter;
			cboDeviceServer.Model.GetIterFirst (out iter);
			string server = null;

			while ( (server = (string)cboDeviceServer.Model.GetValue (iter, 0)) != null) {

			string[] server_split = server.Split ('\t');

				if ( server_split[0] == device.DeviceServer || server_split[1] == device.DeviceServer )
					break;

				cboDeviceServer.Model.IterNext (ref iter);
			}

			cboDeviceServer.SetActiveIter (iter);

	}

	protected void OnIvDeviceBrowserButtonPressEvent (object o, ButtonPressEventArgs args)
	{
		/**
		 * Check to see if any devices items are selected and make show appropriate
		 * manu items.
		 */

		string device = get_BrowserItem_Value_ByPos ( ivDeviceBrowser );

		if (device == null) {
			mnu_del_device_enabled (false);
			mnu_device.Popup ();
		} else {
			mnu_del_device_enabled (true);
			mnu_device.Popup ();
		}
	}

	private void mnu_del_device_enabled (bool enabled) {
		mnu_del_device.Visible = enabled;
		mnu_edit_device.Visible = enabled;
	}

	protected void OnBtnServerBrowserBackClicked (object sender, EventArgs e)
	{
		nbFrames.CurrentPage = (int)nb_Notebook_Pages.HOME;
	}

	void onBtnServerSettingsSaveClicked (object sender, EventArgs e)
	{

		//Save Server Details.

		Gtk.ListStore store = (Gtk.ListStore) ivServerBrowser.Model;

		if (store == null) {
			store = new ListStore (typeof(string), typeof(Gdk.Pixbuf));
			ivServerBrowser.MarkupColumn = 0;
			ivServerBrowser.PixbufColumn = 1;
			ivServerBrowser.Model = store;
		}

		if (c_current_server.Update == false) {

			csql.CSQL_AddServer (c_current_server);

			if (c_current_server.ServerFriendName == "") {
				store.AppendValues (c_current_server.ServerAddr, new Gdk.Pixbuf (CUtil.CUTIL_GetServerIcon (), 64, 64));
			} else {
				store.AppendValues (c_current_server.ServerFriendName, new Gdk.Pixbuf (CUtil.CUTIL_GetServerIcon (), 64, 64));
			}

			ResetServerUpateReady ();

		} else {

			csql.CSQL_UpdateServer (c_current_server);
			ResetServerUpateReady ();
		}

		nbFrames.CurrentPage = (int)nb_Notebook_Pages.SERVER_BROWSER;

	}

	protected void OnHomeClicked (object sender, EventArgs e)
	{
		nbFrames.CurrentPage = (int)nb_Notebook_Pages.HOME;
	}

	protected void OnIvServerBrowserButtonPressEvent (object o, ButtonPressEventArgs args)
	{
		/**
		 * Check to see if any server items are selected and make show apprpriate
		 * manu items.
		 */

		string server = get_BrowserItem_Value_ByPos ( ivServerBrowser );

		if (server == null) {
			mnu_server.Popup ();
		} else {
			mnu_del_server_enabled (true);
			mnu_server.Popup ();
		}

	}

	void OnBtnDeviceSettingsBackClicked (object sender, EventArgs e)
	{

		string msg_string = "Data for the current Device has changed. \n" +
		                    "Would you like to save the changes?";
		MessageDialog msg = null;
		ResponseType response;

		if (c_current_device.DirtyFlag) {

			msg = new MessageDialog (this, DialogFlags.DestroyWithParent, MessageType.Question, 
				ButtonsType.YesNo, msg_string, 0);
			response = (ResponseType)msg.Run ();

			if (response == ResponseType.No) {
				nbFrames.CurrentPage = (int)nb_Notebook_Pages.DEVICE_BROWSER;

				ResetDeviceUpdateReady ();
			} else {

				csql.CSQL_UpdateDevice ( c_current_device );
				nbFrames.CurrentPage = (int)nb_Notebook_Pages.DEVICE_BROWSER;
				ResetDeviceUpdateReady ();
			}

			msg.Destroy ();			

		} else {
			nbFrames.CurrentPage = (int)nb_Notebook_Pages.DEVICE_BROWSER;
		}

	}

	void OnBtnViewServersClicked (object sender , EventArgs args) {

		populate_ivServerBrowser ();

		nbFrames.CurrentPage = (int)nb_Notebook_Pages.SERVER_BROWSER;

	}

	void OnServerSettingsBackClicked (object sender, EventArgs e)
	{
		string msg_string = "Data for the current Server has changed.\n " +
		                    "Would you like save the settings";
		MessageDialog msg = null;
		ResponseType response;

		if (c_current_server.DirtyFlag) {

			msg = new MessageDialog (this, DialogFlags.DestroyWithParent, MessageType.Question, 
				ButtonsType.YesNo, msg_string, 0);
			response = (ResponseType)msg.Run ();
			msg.Destroy ();

			if (response == ResponseType.No) {
				// Do Nothing.
			} else {

				if (c_current_server.Update) {
					csql.CSQL_UpdateServer (c_current_server);
				} else {
					csql.CSQL_AddServer (c_current_server);
				}
			}

			ResetServerUpateReady ();

		} else {

			ResetServerUpateReady ();
		}

		nbFrames.CurrentPage = (int)nb_Notebook_Pages.SERVER_BROWSER;
	}

	void OnBtnDeviceBrowserBackClicked (object sender, EventArgs e)
	{
		nbFrames.CurrentPage = (int)nb_Notebook_Pages.HOME;
	}

	void set_widget_events ()
	{

		/*
		 * WINDOW EVENTS
		 */
		this.Focused += delegate(object o, FocusedArgs args) {
			this.Maximize();
		};


		/*		
		 * CONFIG WIDGET EVENTS
		 */
		btnConfigServers.Clicked += OnBtnConfigServersClicked;
		btnConfigHome.Clicked += OnHomeClicked;

		/*				
		 * DEVICE WIDGET EVENTS
		 */
		ivDeviceBrowser.ItemActivated += OnIvDeviceBrowserItemActivated;
		ivDeviceBrowser.ButtonPressEvent += OnIvDeviceBrowserButtonPressEvent;

		btnDeviceSettingsSave.Clicked += OnBtnDeviceSettingsSaveClicked;
		btnDeviceSettingsHome.Clicked += OnHomeClicked;
		btnDeviceSettingsBack.Clicked += OnBtnDeviceSettingsBackClicked;

		btnDeviceBrowserBack.Clicked += OnBtnDeviceBrowserBackClicked;
		btnDeviceBrowserHome.Clicked += OnHomeClicked;
		btnLogFileSensorAdd.Clicked += OnBtnLogFileAddClicked;
		btnLogFileSensorDelete.Clicked += del_logfile_sensor;
		btnLogFileSensorUp.Clicked += swap_device_sensor_up;
		btnLogFileSensorDown.Clicked += swap_device_sensor_down;
		btnAddDefaultDescr.Clicked += add_default_descr ;

		btn_ViewDevices.Clicked += OnBtnManageDevicesClicked;
		btnSettingsBrowse.Clicked += OnBtnSettingsBrowseClick;

		entDeviceID.Changed += delegate(object sender, EventArgs e) {
			c_current_device.DeviceID  = entDeviceID.Text;
			CheckDeviceUpdateReady();
		};

		entDeviceDescription.Changed += delegate(object sender, EventArgs e) {
			c_current_device.DeviceDescription = entDeviceDescription.Text;	
			CheckDeviceUpdateReady();
		};

		entDeviceLocation.Changed += delegate(object sender, EventArgs e) {
			c_current_device.DeviceLocation = entDeviceLocation.Text;
			CheckDeviceUpdateReady();
		};

		entDeviceServerSubDir.Changed += delegate(object sender, EventArgs e) {
			c_current_device.DeviceServerSubDir = entDeviceServerSubDir.Text;
			CheckDeviceUpdateReady();
		};

		nvDeviceFileDescr.CursorChanged += delegate(object sender, EventArgs e) {
			c_current_device.DirtyFlag = true;
			CheckDeviceUpdateReady();
		};

		cboDeviceServer.Changed += delegate(object sender, EventArgs e) {

			string[] server_split = cboDeviceServer.ActiveText.Split('\t');

			if (server_split[0] == "") {
				c_current_device.DeviceServer = server_split[1];
			} else {
				c_current_device.DeviceServer = server_split[0];
			}

			CheckDeviceUpdateReady();

		};

		/*				
		 * SERVER WIDGET EVENTS
		 */
		ivServerBrowser.ItemActivated += OnIvServerBrowserItemActivated;
		ivServerBrowser.ButtonPressEvent += OnIvServerBrowserButtonPressEvent;
		btnServerBrowserBack.Clicked += OnBtnServerBrowserBackClicked;
		btnServerBrowserHome.Clicked += OnHomeClicked;
		btnServerSettingsBack.Clicked += OnServerSettingsBackClicked;
		btnServerSettingsSave.Clicked += onBtnServerSettingsSaveClicked;
		btnViewServers.Clicked += OnBtnViewServersClicked;

		entServer.Changed += delegate(object sender, EventArgs e) {
			c_current_server.ServerAddr = entServer.Text;
			CheckServerUpdateReady();
		};

		entFriendlyName.Changed += delegate(object sender, EventArgs e) {
			c_current_server.ServerFriendName = entFriendlyName.Text;
			CheckServerUpdateReady();
		};

		cboProtocol.Changed += delegate(object sender, EventArgs e) {
			c_current_server.ServerProtocol = cboProtocol.ActiveText;
			CheckServerUpdateReady();
		};

		entPort.Changed += delegate(object sender, EventArgs e) {
			try {
				c_current_server.ServerPort = Convert.ToInt32 (entPort.Text);
			} catch { 

				/*TODO: 
				 * Not sure why we get -> System.FormatException: Input string was not in the correct format.
				 * Can be ignored for now?
				 */
			}

			CheckServerUpdateReady();
		};

		entBaseDir.Changed += delegate(object sender, EventArgs e) {
			c_current_server.ServerBaseDir = entBaseDir.Text;
			CheckServerUpdateReady();
		};

		cbAuth.Toggled += delegate(object o, EventArgs args) {
			fraAuth.Visible = cbAuth.Active;
			c_current_server.ServerAuthRequired = Convert.ToInt32 (cbAuth.Active);
			CheckServerUpdateReady();
		};

		entUname.Changed += delegate(object sender, EventArgs e) {
			c_current_server.ServerUserName = entUname.Text;
			CheckServerUpdateReady();
		};

		entPword.Changed += delegate(object sender, EventArgs e) {
			c_current_server.ServerPassword = entPword.Text;
			CheckServerUpdateReady();
		};

		/*				 
		 * GRAPH WIDGET EVENTS
		 */
		btnGraphHome.Clicked += OnHomeClicked;

		btnGraphBack.Clicked += delegate(object sender, EventArgs e) {
			nbFrames.CurrentPage = (int)nb_Notebook_Pages.FTP_BROWSER;
		};

		/*
		 * FTP BROWSER WIDGET EVENTS
		 */
		btnFtpBack.Clicked += delegate(object sender, EventArgs e) {
			nbFrames.CurrentPage = (int)nb_Notebook_Pages.DEVICE_BROWSER;
		};

		ivFtpBrowser.ItemActivated += OnIvFtpBrowserItemActivated;

		/*
		 * MISC WIDGET EVENTS
		 */
		btn_Quit.Clicked += delegate(object sender, EventArgs e) {
			Application.Quit();
		};

	}

	private void del_logfile_sensor( object sender, EventArgs args ) {

		TreeStore store = (TreeStore)nvDeviceFileDescr.Model;
		TreeSelection selection = nvDeviceFileDescr.Selection;
		TreeIter iter;

		selection.GetSelected (out iter);
		TreePath[] path = selection.GetSelectedRows();
		int array_index = Convert.ToInt32 (path [0].ToString ());
		store.Remove (ref iter);
		delete_array_sensor (array_index);

	}

	private void delete_array_sensor ( int index) {
		ar_file_description.Remove (ar_file_description [index]);
	}

	private void swap_device_sensor_up( object sender, EventArgs args ) {

		TreeStore store = (TreeStore)nvDeviceFileDescr.Model;
		TreeSelection selection = nvDeviceFileDescr.Selection;
		TreeIter iterA;
		TreeIter iterB;

		int indexA = 0;
		int indexB = 0;

		TreePath path = null;

		try {
			selection.GetSelected (out iterA);

			path = store.GetPath (iterA);
			indexA = Convert.ToInt32(path.ToString());

			path.Prev();
			indexB = Convert.ToInt32(path.ToString());

			swap_ar_description_entries_up (indexA, indexB);

			store.GetIter (out iterB, path);
			store.Swap(iterA, iterB);
		} catch {
			// Do Nothing.
		}
	}

	private void swap_device_sensor_down( object sender, EventArgs args ) {

		TreeStore store = (TreeStore)nvDeviceFileDescr.Model;
		TreeSelection selection = nvDeviceFileDescr.Selection;
		TreeIter iterA;
		TreeIter iterB;

		int indexA = 0;
		int indexB = 0;

		TreePath path = null;

		try {
			selection.GetSelected (out iterA);

			path = store.GetPath (iterA);
			indexA = Convert.ToInt32(path.ToString());

			path.Next();
			indexB = Convert.ToInt32(path.ToString());

			swap_ar_description_entries_down(indexA, indexB);

			store.GetIter (out iterB, path);
			store.Swap(iterA, iterB);
		} catch {
			// Do nothing
		}
	}

	private void swap_ar_description_entries_up (int indA, int indB) {

		CDeviceFileDescription temp = 
			(CDeviceFileDescription)ar_file_description[indA];

		ar_file_description [indA] = ar_file_description [indB];
		ar_file_description [indB] = temp;

	}

	private void swap_ar_description_entries_down (int indA, int indB) {

		CDeviceFileDescription temp = 
			(CDeviceFileDescription)ar_file_description[indA];

		ar_file_description [indA] = ar_file_description [indB];
		ar_file_description [indB] = temp;

	}

	private void ResetDeviceUpdateReady() {
		c_current_device.DirtyFlag = false;
		c_current_device.Update = false;
		this.Title = DefaultWindowTitle;
		btnDeviceSettingsSave.Visible = false;
	}

	private void ResetServerUpateReady() {
		c_current_server.DirtyFlag = false;
		c_current_server.Update = false;
		this.Title = DefaultWindowTitle;
		btnServerSettingsSave.Visible = false;
	}

	private void CheckDeviceUpdateReady() {
		if ( c_current_device.DirtyFlag ) this.Title = DefaultWindowTitle + " - *";
		btnDeviceSettingsSave.Visible = true;
	}

	private void CheckServerUpdateReady() {
		if (c_current_server.DirtyFlag)	this.Title = DefaultWindowTitle + " - *";
		btnServerSettingsSave.Visible = true;
	}

	private void set_general_widget_properties() {
		this.Maximize ();

		nvDeviceFileDescr.HeightRequest = 300;
		tvwAlarms.RulesHint = true;

		//DO CBOPROTOCOL
		ListStore store = new ListStore (typeof(string));
		string[] s_protocols = new string[] { "ftp://", "ftps://", "http://", "https://", "file://" };

		cboProtocol.Model = store;

		for (int i = 0; i < s_protocols.Length; i++) {
			cboProtocol.AppendText (s_protocols [i]);
		}


	}

	private void hide_tab_labels (bool hide) {
		lbl_nbConfig.Visible = !hide;
		lbl_nbDeviceBrowser.Visible = !hide;
		lbl_nbDeviceSettings.Visible = !hide;
		lbl_nbFtpBrowser.Visible = !hide;
		lbl_nbGraph.Visible = !hide;
		lbl_nbHome.Visible = !hide;
		lbl_nbServerBrowser.Visible = !hide;
		lbl_nbServerSettings.Visible = !hide;
	}
}
