# NLog.Windows.Forms
[![Version](https://badge.fury.io/nu/NLog.Windows.Forms.svg)](https://www.nuget.org/packages/NLog.Windows.Forms)
[![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-windows-forms/master.svg)](https://ci.appveyor.com/project/nlog/nlog-windows-forms/branch/master)
[![codecov.io](https://codecov.io/github/NLog/NLog.Windows.Forms/coverage.svg?branch=master)](https://codecov.io/github/NLog/NLog.Windows.Forms?branch=master)

This package provides NLog targets for displaying log output in Windows Forms applications.

- [RichTextBoxTarget](https://github.com/NLog/NLog.Windows.Forms/wiki/RichTextBoxTarget): Displays log messages in a RichTextBox control with colorhighlighting.
- [MessageBoxTarget](https://github.com/NLog/NLog.Windows.Forms/wiki/MessageBoxTarget): Displays log messages in message boxes.
- [FormControlTarget](https://github.com/NLog/NLog.Windows.Forms/wiki/FormControlTarget): Displays log messages in the `Text` property of a Windows Forms control.
- [ToolStripItemTarget](https://github.com/NLog/NLog.Windows.Forms/wiki/ToolStripItemTarget): Displays log messages in the `Text` property of a `ToolStripItem`.

See [list](https://nlog-project.org/config/?tab=targets&search=package:nlog.windows.forms)

## Register Extension
Install the [NLog.Windows.Forms](https://www.nuget.org/packages/NLog.Windows.Forms/) NuGet package and [register the extension-assembly](https://github.com/NLog/NLog/wiki/Register-your-custom-component).

When loading configuration from `NLog.config` file, then register the extension in the <extensions> section:
```xml
<extensions>
  <add assembly="NLog.Windows.Forms"/>
</extensions>
```

Alternative register from code using the [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):
```csharp
NLog.LogManager.Setup().RegisterWindowsForms();
```
