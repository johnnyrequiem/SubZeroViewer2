using System;
using Gtk;

namespace SubZeroViewer2
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			System.DateTime.SpecifyKind (System.DateTime.Now, DateTimeKind.Local);

			MainWindow win = new MainWindow ();
			win.WidthRequest = win.Display.DefaultScreen.Width;
			win.HeightRequest = win.Display.DefaultScreen.Height;
			win.Move (0, 0);
			win.Show ();

			Application.Run ();
		}
	}
}
