# SetDigitSubstitution

[![Apache 2.0](https://img.shields.io/badge/license-Apache%202.0-green.svg)](LICENSE)

SetDigitSubtititon is a tool to configure digit substition on Windows 10.
The ability to change digit substitution settings has unfortunately been removed from the Control Panel in Windows 10 Version 1903.

![Screenshot](Screenshot.png)

### What is digit substitution?

Digit substitution is a feature of the operating system that replaces ASCII digits 0-9 with native digits from different script anywhere in the user interface during rendering.
The underlying data remain unchanged, it is only a "display effect". For example, a text file with numerals 0123456789 will be shown as ०१२३४५६७८९ when Hindi digits are substituted.

Technical reading:
* [Michael Kaplan's posts on digit substitution](http://archives.miloush.net/michkap/archive/2010/11/12/10088056.html) on Sorting it All Out
* [Digit Shapes](https://docs.microsoft.com/en-us/windows/win32/intl/digit-shapes#digit-substitution) on Windows Dev Center

### How to use the tool

Download and run the [latest release](https://github.com/ujca/SetDigitSubstitution/releases/latest).
A window like above will appear with the current digit substitution settings.
Choose your favourite set of digits and substitution setting and click Apply.

Please note that not all digit options are currently supported by Windows.

##### How to choose the substitution setting:

Setting | Result
--------|-------
Never | Turns off all substitution. The digits are shown as they are in the underlying data. This is the default setting for most languages.
Contextual | If the preceding text is in the same script as the substitution digits, the digits will be substitued, i.e. 0123456789 numerals in the data will be shown in the script of your choice. If the preceding text is in any other script, the numerals will stay unaffected.<br/>If the text starts with a digit with no preceding text, in most cases desktop apps will not substitute the digits but UWP apps including the shell will.<br/>This is the default setting for Arabic, Central Kurdish, Kurdish (Perso-Arabic, Iran) and Persian.
Always | 0123456789 digits are substituted in all cases. This is the default setting for Bangla, Burmese, Dari, Dzongkha, Kashmiri, Khmer, Mazanderani, N'ko, Nepali (India), Northern Luri, Pashto, Punjabi, Sindhi, Urdu (India) and Uzbek.
 
If your favourite script has native digits and you would like to see them sprinkled around a bit but not affecting your work too much, choose _Contextual_. That way, the lock screen and tiles in the Start Menu will use the digits of your choice, but most of the numbers and dates in the system will stay unaffected.

##### Command line syntax (for advanced users):
Although this tool is mainly intended as a GUI for the digit substitution settings, it can be used from command line for automation purposes. Currently, the syntax is limited to
```
SetDigitSubstitution setting digits
```
where the arguments are case-insensitive and as follows:

Argument | Syntax
---------|-------
`setting` | One of the [DigitShapes](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.digitshapes) values: `None`, `Context`, `NativeNational`, corresponding to the settings above.
`digits` | One of the following:<br/>a) Unicode codepoint of the zero digit as a hexadecimal number (without prefix);<br/>b) a name of the script whose digits should be used, e.g. `Tamil` (refer to block names and subheaders in Unicode);<br/>c) a string of digits to replace 0123456789. Please note that Windows will [not accept](http://archives.miloush.net/michkap/archive/2006/01/18/514171.html) arbitrary strings. 

The tool returns control to the command-line before finishing and hence does not report any error code should it fail.

### How does the tool work?
The tool only changes a setting similar to the date format in Windows. It does not need to run in the background and it does not require administrator privileges. In older versions of Windows, you can control the same settings at the bottom of _Control Panel > Region > Formats > Additional settings > Numbers_ instead. Windows does all the substitution work on its own.

From the API point of view, the tool is just a fancy UI to [GetLocaleInfo](https://docs.microsoft.com/en-us/windows/win32/api/winnls/nf-winnls-getlocaleinfow)/[SetLocaleInfo](https://docs.microsoft.com/en-us/windows/win32/api/winnls/nf-winnls-setlocaleinfow) with [LOCALE_IDIGITSUBSTITUTION](https://docs.microsoft.com/en-us/windows/win32/intl/locale-idigitsubstitution) and [LOCALE_SNATIVEDIGITS](https://docs.microsoft.com/en-us/windows/win32/intl/locale-snative-constants) locale data.

### System requirements
Digit substitution has been supported since Windows 2000. This tool requires at least .NET Framework 4.0, which is a built-in component of Windows 8 and newer. For the target audience of Windows 10 users, there are no extra pre-requirements, just run and have fun!
