using System;
using System.Drawing;
using System.IO;
using System.Reflection;
#if ! WINUI
using System.Windows.Media.Imaging;
#else
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace Sdl.MultiSelectComboBox.Example.Services
{
	public class ImageService
	{
		private static readonly string ExecutingAssemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		public static BitmapImage GetImage(string path, string name, Size imageSize)
		{
			try
			{
				var filePath = Path.Combine(ExecutingAssemblyFolder, path, name + ".ico");
				if (!File.Exists(filePath))
				{
					return null;
				}
#if ! WINUI
				Icon icon;
				using (var stream = new FileStream(filePath, FileMode.Open))
				{
					icon = new Icon(stream, imageSize);
					stream.Flush();
					stream.Close();
				}

				var bitmap = icon.ToBitmap();

				bitmap.MakeTransparent();

				return Convert(bitmap);
#else
				var bitmapImage = new BitmapImage();

				bitmapImage.UriSource = new Uri(filePath, UriKind.Absolute);
				bitmapImage.DecodePixelWidth = imageSize.Width;
				bitmapImage.DecodePixelHeight = imageSize.Height;
				return bitmapImage;
#endif


			}
			catch
			{
				return null;
			}
		}
#if ! WINUI
		private static BitmapImage Convert(object value)
		{
			if (value != null && value is Image image)
			{
				var memoryStream = new MemoryStream();
				var bitmap = new BitmapImage();
				bitmap.BeginInit();
				image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
				memoryStream.Seek(0, SeekOrigin.Begin);
				bitmap.StreamSource = memoryStream;
				bitmap.EndInit();

				bitmap.Freeze();

				return bitmap;
			}

			return null;
		}
#endif
	}
}
