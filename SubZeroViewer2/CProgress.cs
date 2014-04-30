using System;
using Gtk;

namespace SubZeroViewer2
{
	public class CProgress
	{
		private Gtk.Image img_throbber = null;
		private Gtk.Window _win_progress = null;
		private string _message_text = "";
		private string _image_path = "./res/throbber.gif";

		public CProgress ()
		{
		}

		public CProgress ( string message ) {

			_message_text = message;

			_win_progress = new Window (_message_text);
			_win_progress.DefaultWidth = 250;
			_win_progress.DefaultHeight = 50;

			img_throbber = new Gtk.Image (_image_path);
			_win_progress.Add (img_throbber);
			_win_progress.Modal = true;

		}

		~CProgress() {
			_win_progress.Dispose();
			_image_path = null;
			_message_text = null;
			img_throbber.Dispose();
		}

		public void Show() {
			_win_progress.Show ();
		}

		public void start_progress( string message ) {
			_win_progress.Title = message;
			_win_progress.Show ();
		}

		public void stop_progress() {
			_win_progress.Hide ();
		}

	}
}

