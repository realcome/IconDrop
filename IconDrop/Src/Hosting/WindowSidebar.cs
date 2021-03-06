﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SciterSharp;
using SciterSharp.Interop;

namespace IconDrop.Hosting
{
#if OSX
	using AppKit;
	using Foundation;

	class WindowDelegate : NSWindowDelegate
	{
		public override void DidResignKey(NSNotification notification)
		{
			//WindowSidebar.HidePopup();
		}

		public override void DidResignMain(NSNotification notification)
		{
			//WindowSidebar.HidePopup();
		}

		[Export("OnIconClick")]
		public void OnIconClick()
		{
			WindowSidebar.ShowPopup();
		}
	}
#endif

	class WindowSidebar : SciterWindow
	{
#if OSX
		private static NSStatusItem _sItem;

		public WindowSidebar()
		{
			var frm = NSScreen.MainScreen.VisibleFrame;

			PInvokeUtils.RECT rc = new PInvokeUtils.RECT()
			{
				right = 670,
				bottom = (int) frm.Height - 50
			};

			var flags = SciterXDef.SCITER_CREATE_WINDOW_FLAGS.SW_ALPHA |
				SciterXDef.SCITER_CREATE_WINDOW_FLAGS.SW_MAIN |
				SciterXDef.SCITER_CREATE_WINDOW_FLAGS.SW_ENABLE_DEBUG |
				SciterXDef.SCITER_CREATE_WINDOW_FLAGS.SW_TOOL;
			var wnd = this;
			wnd.CreateWindow(rc, flags);
			wnd.Title = Consts.AppName;

			var deleg = new WindowDelegate();
			wnd._nsview.Window.Delegate = deleg;
			wnd._nsview.Window.Level = NSWindowLevel.Floating;

			// Create status bar item
			_sItem = NSStatusBar.SystemStatusBar.CreateStatusItem(25);
			_sItem.Image = NSImage.FromStream(File.OpenRead(NSBundle.MainBundle.ResourcePath + @"/drop.png"));
			_sItem.Image.Template = true;
			_sItem.Action = new ObjCRuntime.Selector("OnIconClick");
			_sItem.Target = deleg;
			_sItem.HighlightMode = true;
		}

		public static void ShowPopup()
		{
			NSWindow wnd = (NSWindow) _sItem.ValueForKey(new NSString("window"));
			//var f1 = NSApplication.SharedApplication.CurrentEvent.Window.Frame;
			//var f2 = wnd.Frame;
			var frm = wnd.Frame;

			var pos = new PInvokeUtils.POINT()
			{
				X = ((int)frm.Left - App.AppWnd.Size.cx / 2) + ((int)frm.Width / 2),
				Y = (int)frm.Top - 1
			};

			NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
			App.AppWnd._nsview.Window.OrderFrontRegardless();
			App.AppWnd.CallFunction("View_ShowAnimate", new SciterValue(pos.X), new SciterValue(pos.Y));
		}

		public static void HidePopup()
		{
			if(App.AppWnd != null)
				App.AppWnd.Show(false);
		}

#else

		public WindowSidebar()
		{
			var wnd = this;

			// Create window
			var flags = SciterXDef.SCITER_CREATE_WINDOW_FLAGS.SW_ALPHA |
				SciterXDef.SCITER_CREATE_WINDOW_FLAGS.SW_MAIN |
				SciterXDef.SCITER_CREATE_WINDOW_FLAGS.SW_ENABLE_DEBUG |
				SciterXDef.SCITER_CREATE_WINDOW_FLAGS.SW_TOOL;
			wnd.CreateMainWindow(100, 100, flags);
			wnd.Icon = Properties.Resources.IconMain;
			wnd.Title = Consts.AppName;

			// HideTaskbarIcon
			PInvoke.User32.SetWindowLong(_hwnd,
				PInvoke.User32.WindowLongIndexFlags.GWL_EXSTYLE,
				PInvoke.User32.SetWindowLongFlags.WS_EX_TOOLWINDOW | PInvoke.User32.SetWindowLongFlags.WS_EX_LAYERED);
		}
#endif
	}
}