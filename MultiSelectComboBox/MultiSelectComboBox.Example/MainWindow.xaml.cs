using System.Linq;
using System.Reflection;

#if WINUI
namespace Sdl.MultiSelectComboBox.Example.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Sdl.MultiSelectComboBox.Example.Models;
#else
using System.Windows;
using System.Windows.Controls;
using Sdl.MultiSelectComboBox.Example.Models;
using Sdl.MultiSelectComboBox.Themes.Generic;

namespace Sdl.MultiSelectComboBox.Example;
#endif

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
#if WINUI
[Microsoft.UI.Xaml.Data.Bindable]
public partial class MainWindow : Page
	#else
public partial class MainWindow : Window
	#endif
{
#if WINUI
	//public LanguageItems DataContext{get;set; }
	public LanguageItems ViewModel {get;set; } = new LanguageItems();
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
		this.Loaded += MainWindow_Loaded;
		
#endif
        
		//Sdl.MultiSelectComboBox.Themes.Generic.MultiSelectComboBox.IsEditModeProperty.prop
		//IsEditModeProperty
		
	}


    private void MainWindow_Loaded(object sender,

		RoutedEventArgs e


		)
	{
		var model = new LanguageItems();
#if !WINUI
		DataContext = model;
		comboMain.EditModeChanged += (s,e) => model.IsEditMode = e;
		#else
		ViewModel = model;

		#endif
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
