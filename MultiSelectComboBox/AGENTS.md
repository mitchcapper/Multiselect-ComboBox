Make sure you read CLAUDE.md if you are claude.

This is a multi-select combo box control that has two parts.  The top pane the user can type in (and it will auto complete entries) and it also shows each item already selected (with an X in the corner to remove it form the selection).   Then there is the drop down menu.  If they click on the top pane or type in it, it shows all the options based on the current filter.  The Example (MultiSelectComboBox.Example) has countries /languages so when the user types "eng" it filters that list to the ones containing that word.  The user can navigate the drop down using arrow keys and enter/space to select.  They can also click on items to select one or more that way (clicking an already selected one unselects it).


This is originally a WPF control we are working to add a Uno WinUI3 verison.  There is Sdl.MultiSelectComboBox.Shared which contains all the shared code (which is most of it) basically any .cs file.  As there is some framework specific code there is the WINUI precompiler definition we can #if on as you can see.   The xaml is  specific to each framework for WPF that is stored in `Sdl.MultiSelectComboBox\Themes\Generic\MultiSelectComboBox.xaml`  for Uno's WinUI it is `Sdl.MultiSelectComboBox.WinUI\Themes\Generic.xaml`  the primary control is `Sdl.MultiSelectComboBox.Shared\Controls\MultiSelectComboBox.cs`

If at any point you repeatedly fail to edit a file your tooling is broke and you should pause and figure out what.  CLAUDE.md should have the right instructions for editing something but maybe something is wrong.  You should never resort to using sed or some hacky way of editing a file.

Remember at the start before we made any changes to create the ocmmon code / winui the WPF control worked very well.


The `MultiSelectComboBox.cs` is formatted with all tabs and LF line endings no space indenting.