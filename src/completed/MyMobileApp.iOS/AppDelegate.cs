using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using MyMobileApp.Common;
using UIKit;
using Xam.Plugins.OnDeviceCustomVision;

namespace MyMobileApp.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();
			LoadApplication(new App());

			CrossImageClassifier.Current.Init("shoes", ModelType.General);
			return base.FinishedLaunching(app, options);
		}
	}
}
