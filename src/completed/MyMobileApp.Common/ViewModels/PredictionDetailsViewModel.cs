using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xam.Plugins.OnDeviceCustomVision;
using Xamarin.Forms;

namespace MyMobileApp.Common
{
	public class PredictionDetailsViewModel : BaseViewModel
	{
		public ICommand ResetDataCommand => new Command(ResetData);
		public ICommand TakePictureCommand => new Command(TakePicture);
		public ICommand MakePredictionCommand => new Command(MakePrediction);

		string _status = "Snap a pic of a shoe to get started";
		public string Status
		{
			get { return _status; }
			set { SetProperty(ref _status, value); }
		}

		byte[] _imageBytes;
		ImageSource _imageSource;
		public ImageSource ImageSource
		{
			get { return _imageSource; }
			set { SetProperty(ref _imageSource, value); OnPropertyChanged(nameof(HasImageSource)); }
		}

		public bool HasImageSource => ImageSource != null;

		public PredictionDetailsViewModel()
		{
			Title = "Make Prediction";
		}

		void ResetData()
		{
			ImageSource = null;
			Status = null;
		}

		#region Take/Choose Picture

		async void TakePicture()
		{
			MediaFile file;

			if (!CrossMedia.Current.IsCameraAvailable)
			{
				//Probably a simulator - let's choose a photo from the library
				file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions());
			}
			else
			{
				var options = new StoreCameraMediaOptions
				{
					CompressionQuality = 50,
					PhotoSize = PhotoSize.Small,
				};

				file = await CrossMedia.Current.TakePhotoAsync(options);
			}

			if (file == null)
				return;

			var stream = file.GetStream();
			file.Dispose();

			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				_imageBytes = ms.ToArray();
			}

			stream.Position = 0;
			ImageSource = ImageSource.FromStream(() => { return stream; });
		}

		#endregion

		#region Make Prediction

		async void MakePrediction()
		{
			if(IsBusy)
				return;

			if(ImageSource == null)
			{
				Status = "Please take a picture first";
				return;
			}

			IsBusy = true;
			try
			{
				Status = "Analyzing picture...";
				var tags = await CrossImageClassifier.Current.ClassifyImage(new MemoryStream(_imageBytes));

				var description = "";
				foreach(var tag in tags.Where(t => t.Probability > .70))
					description += $"{tag.Tag} - {tag.Probability.ToString("P")}, ";

				if(string.IsNullOrWhiteSpace(description))
					description = "No tags found";

				Status = description.Trim().TrimEnd(',');
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			finally
			{
				IsBusy = false;
			}
		}

		#endregion
	}
}
