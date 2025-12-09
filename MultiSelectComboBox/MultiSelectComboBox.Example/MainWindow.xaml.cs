using System.Linq;
using System.Reflection;

#if WINUI
namespace Sdl.MultiSelectComboBox.Example.WinUI;
using Microsoft.UI.Xaml;
using Sdl.MultiSelectComboBox.Example.Models;
#else
using System.Windows;
using System.Windows.Controls;
using Sdl.MultiSelectComboBox.Example.Models;

namespace Sdl.MultiSelectComboBox.Example;
#endif

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
#if WINUI
	public LanguageItems DataContext{get;set; }
	public LanguageItems ViewModel => DataContext;
#endif
	public MainWindow()
	{
		InitializeComponent();
#if !WINUI
		if (Application.Current.MainWindow != null)
		{
			var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			
			Application.Current.MainWindow.Title = Application.Current.MainWindow.Title + " (" + fileVersionInfo.FileVersion
			                                       + " - " + GetTargetFramework() + ")";
		}
		Loaded += MainWindow_Loaded;
#else
		Activated += MainWindow_Loaded;
#endif
	}

	private void MainWindow_Loaded(object sender,
#if !WINUI
		RoutedEventArgs e
#else
		WindowActivatedEventArgs e
#endif
		)
	{
		var model = new LanguageItems();
		DataContext = model;
	}
#if !WINUI
	private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is TextBox textBox && textBox.LineCount > 0)
		{
			textBox.ScrollToLine(textBox.LineCount - 1);
		}
	}
#endif
	private string GetTargetFramework()
	{
		var targetFrameworkAttribute = Assembly.GetExecutingAssembly()
			.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false)
			.OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
			.FirstOrDefault();

		return targetFrameworkAttribute.FrameworkName;
	}

	private void MultiSelectComboBox_NewItemAddRequest(object sender, EventArgs.NewItemAddRequestEventArgs e) {
		var itm = new LanguageItem { Name = e.TypedText };
		var li = (DataContext as LanguageItems);
		li.Items.Add(itm);
		li._allItems.Add(itm);
		li.EnableSuggestionProvider = false;
		li.EnableSuggestionProvider = true;
	}
}
