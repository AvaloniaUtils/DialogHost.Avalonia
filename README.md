# DialogHost.Avalonia

Avalonia DialogHost control that provides a simple way to display a dialog with information or prompt the user when
information is required.

Dialogs in Avalonia have always been somewhat tricky. This implementation is designed to:

* Provide correct styling
* Allow any dialog to be constructed
* Compatible with code-behind
* Compatible with MVVM
* Compatible with pure XAML
* Work in applications with multiple windows

Dialogs *are asynchronous* so at some point you will have to deal with that in your code.

## Getting started

1. Install `DialogHost.Avalonia` [nuget package](https://www.nuget.org/packages/DialogHost.Avalonia/)
   ```shell
   dotnet add package DialogHost.Avalonia
   ```
2. Add DialogHostStyles element to your app styles in `App.axaml`. See the example of `App.axaml`:
    ```xaml
    <Application ...
        xmlns:dialogHostAvalonia="clr-namespace:DialogHostAvalonia;assembly=DialogHost.Avalonia"
        ...>
        
        ...
        <Application.Styles>
            ...
            <dialogHostAvalonia:DialogHostStyles />
        </Application.Styles>
        ...
    </Application>
    ```
    For version below `0.7` instead of `<dialogHostAvalonia:DialogHostStyles />` use `StyleInclude`:
    ```xaml
    <StyleInclude Source="avares://DialogHost.Avalonia/Styles.xaml"/>
    ```

3. Start using control

## Using

The cornerstone of dialogs the DialogHost control. It’s a content control, meaning the underlying content over which the
popup dialog will be displayed can be targeted; to a specific area of your app, or the entire Window content.

```xaml
<Window ...
        xmlns:dialogHost="clr-namespace:DialogHostAvalonia;assembly=DialogHost.Avalonia"
        ...
        Title="DialogHost.Demo">
    <dialogHost:DialogHost CloseOnClickAway="True">
        <dialogHost:DialogHost.DialogContent>
            <!-- put your dialog content here-->
        </dialogHost:DialogHost.DialogContent>
        <!-- put the content over which the dialog is shown here (e.g. your main window grid)-->
    </dialogHost:DialogHost>
</Window>
```

When the dialog is open, the underlying content will be dimmed and disabled.

![preview](https://github.com/AvaloniaUtils/DialogHost.Avalonia/raw/main/wiki/images/preview0.png)

`DialogHost.DialogContent` (associated with `DialogHost.DialogContentTemplate`) is your typical XAML content object
property for setting the content of your dialog. You can infer from this that you can use MVVM to bind content, but
there are multiple ways of populating the content, showing the dialog, closing the dialog, and processing responses.

![previewGif](https://github.com/AvaloniaUtils/DialogHost.Avalonia/raw/main/wiki/images/preview.gif)

### Alternative way

- [HanumanInstitute.MvvmDialogs](https://github.com/mysteryx93/HanumanInstitute.MvvmDialogs/#avaloniadialoghost) allows the use of MVVM design in a simplified way, with DialogHost support. If you opt for that syntax, you do not need most of the documentation below, and the DialogHost container is injected to window automatically at runtime.

---

### Open Dialog Strategies

#### OpenDialogCommand

```xaml
Command="{Binding RelativeSource={RelativeSource FindAncestor, AncestorType=materialDesign:DialogHost}, Path=OpenDialogCommand}"
```

ICommand, typically used on buttons, where DialogContent can be set via CommandParameter.

#### IsOpen

```xaml
<dialogHost:DialogHost IsOpen="True" />
```

Dependency property, to be triggered from XAML, set from code-behind or via a binding. Content must be set in
DialogHost.DialogContent.

#### DialogHost.Show

```c#
DialogHost.Show(viewOrModel);
```

Async/await based static API which can be used purely in code (for example from in a view model). Content can be passed
directly to the dialog. Note that if you have multiple windows and multiple DialogHost instances you can set
the `DialogHost.Identifier` property, and provide the identifier to the .Show(...) method to help find the required
DialogHost.


---

### Close Dialog Strategies

#### CloseDialogCommand

You can bind to the DialogHost's OpenDialogCommand:

```xaml
Command="{Binding RelativeSource={RelativeSource FindAncestor, AncestorType=materialDesign:DialogHost}, Path=CloseDialogCommand}"
```

ICommand, typically used on buttons inside the dialog, where the command parameter will be passed along to the dialog
response.

#### IsOpen

```xaml
<dialogHost:DialogHost IsOpen="False" />
```

Dependency property, to be triggered from XAML, set from code-behind or via a binding.

#### DialogSession.Close

Via any of the methods for handling the opened event, you can get hold of the dialog session. This can be used to close
a dialog via code:

```c#
var result = await DialogHost.Show(myContent, delegate(object sender, DialogOpenedEventArgs args)
{
    args.Session.Close(false);
});
```

or getting DialogSession

```c#
DialogHost.GetDialogSession("DialogHost.Identifier here")?.Close(false);
```

---

### Handle Closing Event Strategies

The DialogClosingEventHandler delegate is key. It provides the parameter provided to DialogHost.CloseDialogCommand, and
allows the pending close to be cancelled.

The following mechanisms allow handling of this event, via code-behind, MVVM practices, or just from the code API:

#### DialogHost.DialogClosing

```xaml
<dialogHost:DialogHost DialogClosing="DialogHost_OnDialogClosing" />
```

Bubbling RoutedEvent, which could be used in code-behind.

#### DialogClosing.DialogClosingCallback

```xaml
<dialogHost:DialogHost DialogClosingCallback="{Binding DialogClosingHandler}" />
```

Standard dependency property which enables the DialogClosingEventHandler implementation to be bound, typically from a
view model.

#### DialogHost.Show

```c#
var result = await DialogHost.Show(viewOrModel, ClosingEventHandler);
```

The async response from this method returns the parameter provided when `DialogHost.CloseDialogCommand` was executed. As
part of the `Show()` signature a `DialogClosingEventHandler` delegate can be provided to intercept the on-closing event,
just prior to the close.
