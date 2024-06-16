using AuthorLM.Client.Models;
using AuthorLM.Client.Services;
using DbLibrary.Entities;
using FB2Library;
using FB2Library.Elements;
using Microsoft.Maui.Storage;
using Plugin.Maui.ScreenBrightness;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace AuthorLM.Client.ViewModels
{
    public delegate void ScrollHandler(double scroll);
    public class ReaderViewModel : ViewModel
    {
        private ReaderSettings _settings = new()
        {
            Theme = "white",
            Brightness = ScreenBrightness.Default.Brightness,
            FontFamily = "OpenSansRegular",
            FontSize = 24,
            LineHeight = 1,
            Margin = 20
        };
        public event ScrollHandler NotifyScroll;
        private ObservableCollection<string> _sections = new();
        private ObservableCollection<string> _fonts = new()
        {
            "OpenSansRegular",
            "Montserrat",
            "Raleway",
            "Roboto"
        };
        private string _fontFamily;
        private string _theme;
        private FB2File _file;
        private readonly ApiCallService _callService;
        private readonly NavigationService _navigationService;
        private Command _changeTheme;
        private Command _prevChapter;
        private Command _nextChapter;
        private Command _changeFont;
        private bool _canMoveBack = false;
        private bool _canMoveForward = false;
        private int _bookId;
        private double _scroll;
        private Color _themeBackground = Colors.White;
        private Color _themeForeground = Colors.Black;
        private double _fontSize = 24;
        private double _marginMock = 20;
        private double _lineHeight = 1;
        private int _section = -1;
        private float _brightness = ScreenBrightness.Default.Brightness;
        private Thickness _margin = new(20, 0);
        private string _text;
        private string _title;
        private string _annotation;

        public ObservableCollection<string> Sections
        {
            get => _sections;
            set
            {
                _sections = value;
                OnPropertyChanged();
            }
        }
        private string _sectionItem;
        public string SectionItem
        {
            get => _sectionItem;
        }
        public int Section
        {
            get => _section;
            set
            {
                _section = value;
                _sectionItem = _sections.ElementAt(value + 1);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SectionItem));
            }
        }
        public string Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                _settings.Theme = value;
                OnPropertyChanged();
            }
        }
        public string FontFamily
        {
            get => _fontFamily;
            set
            {
                if(_fontFamily != value)
                {
                    _fontFamily = value;
                    _settings.FontFamily = value;
                    Preferences.Set("FontFamily", value);
                    OnPropertyChanged();
                }
            }
        }
        public Color ThemeBackground
        {
            get => _themeBackground;
            set
            {
                _themeBackground = value;
                OnPropertyChanged();
            }
        }
        public Color ThemeForeground
        {
            get => _themeForeground;
            set
            {
                _themeForeground = value;
                OnPropertyChanged();
            }
        }
        public double FontSize
        {
            get => _fontSize;
            set
            {
                if(value != _fontSize)
                {
                    _fontSize = value;
                    _settings.FontSize = value;
                    Preferences.Set("FontSize", value);
                    OnPropertyChanged();
                }
            }
        }
        public double LineHeight
        {
            get => _lineHeight;
            set
            {
                if(value != _lineHeight)
                {
                    _lineHeight = value;
                    _settings.LineHeight = value;
                    Preferences.Set("LineHeight", value);
                    OnPropertyChanged();
                }
            }
        }
        public float Brightness
        {
            get => _brightness;
            set
            {
                if(value !=  _brightness)
                {
                    _brightness = value;
                    _settings.Brightness = value;
                    Preferences.Set("Brightness", value);
                    ScreenBrightness.Default.Brightness = value;
                    OnPropertyChanged();
                }
            }
        }
        public double MarginMock
        {
            get => _marginMock;
            set
            {
                if(value != _marginMock)
                {
                    _marginMock = value;
                    OnPropertyChanged();
                    _settings.Margin = value;
                    Preferences.Set("Margin", value);
                    Margin = new(value, 0);
                }
            }
        }
        public double Scroll
        {
            get => _scroll;
            set
            {
                _scroll = value;
                OnPropertyChanged();
            }
        }
        public Thickness Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                OnPropertyChanged();
            }
        }
        public Command ChangeTheme
        {
            get => _changeTheme ??= new((theme) =>
            {
                _settings.Theme = Theme = (string)theme;
                switch ((string)theme)
                {
                    case "white":
                        ThemeBackground = Colors.White;
                        ThemeForeground = Colors.Black;
                        break;
                    case "yellow":
                        ThemeBackground = Color.FromArgb("#fff0e6");
                        ThemeForeground = Colors.Black;
                        break;
                    case "gray":
                        ThemeBackground = Colors.Gray;
                        ThemeForeground = Colors.LightGray;
                        break;
                    case "black":
                        ThemeBackground = Colors.Black;
                        ThemeForeground = Colors.LightGray;
                        break;
                }
                Preferences.Set("Theme", (string)theme);
            });
        }
        public ObservableCollection<string> Fonts
        {
            get => _fonts;
            set
            {
                _fonts = value;
                OnPropertyChanged();
            }
        }
        public bool CanMoveBack
        {
            get => _canMoveBack;
            set
            {
                _canMoveBack = value;
                OnPropertyChanged();
            }
        }
        public bool CanMoveForward
        {
            get => _canMoveForward;
            set
            {
                _canMoveForward = value;
                OnPropertyChanged();
            }
        }
        public Command PrevChapter
        {
            get => _prevChapter ??= new(() =>
            {
                Task.Run(async () =>
                {
                    Title = "";
                    await Task.Delay(10);
                    Annotation = "";
                    await Task.Delay(10);
                    Text = "";
                    await Task.Delay(10);
                    LoadSection(_file, --_section);
                    OnPropertyChanged(nameof(Section));
                });

                NotifyScroll?.Invoke(0);
            });
        }
        public Command Back
        {
            get => new(async () => await _navigationService.NavigateBack());
        }
        public Command NextChapter
        {
            get => _nextChapter ??= new(() =>
            {
                Task.Run(async () =>
                {
                    Title = "";
                    await Task.Delay(10);
                    Annotation = "";
                    await Task.Delay(10);
                    Text = "";
                    await Task.Delay(10);
                    LoadSection(_file, ++_section);
                    OnPropertyChanged(nameof(Section));
                });
                NotifyScroll?.Invoke(0);
            });
        }
        public Command ChangeFont
        {
            get => _changeFont ??= new((font) =>
            {
                _settings.FontFamily = FontFamily = (string)font;
            });
        }
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }
        public string Annotation
        {
            get => _annotation;
            set
            {
                _annotation = value;
                OnPropertyChanged();
            }
        }
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }
        public override async Task OnNavigatingTo(object? parameter)
        {
            await _init((int)parameter);
            //Task.Run(() => _init((int)parameter));
        }

        private async Task _init(int id)
        {
            try
            {
                _bookId = id;
                _settings = new()
                {
                    Theme = Preferences.Get("Theme", "white"),
                    Brightness = Preferences.Get("Brightness", ScreenBrightness.Default.Brightness),
                    FontFamily = Preferences.Get("FontFamily", "OpenSansRegular"),
                    FontSize = Preferences.Get("FontSize", 24d),
                    LineHeight = Preferences.Get("LineHeight", 1d),
                    Margin = Preferences.Get("Margin", 20d)
                };
                ChangeTheme?.Execute(_settings.Theme);
                Brightness = _settings.Brightness;
                LineHeight = _settings.LineHeight;
                FontSize = _settings.FontSize;
                FontFamily = _fonts.First(f => f == _settings.FontFamily);
                MarginMock = _settings.Margin;
                using Stream fileStream = await _callService.GetBookContent(id);
                var readerSettings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore
                };
                var loadSettings = new XmlLoadSettings(readerSettings);
                _file = await new FB2Reader().ReadAsync(fileStream, loadSettings);
                Sections.Add(_file.TitleInfo.BookTitle.Text);
                foreach (var section in _file.MainBody.Sections)
                {
                    string sectionItem = string.Empty;
                    if (section.Title != null)
                    {
                        foreach (var item in section.Title.TitleData)
                        {
                            if (item is ParagraphItem)
                            {
                                foreach (StyleType style in (item as ParagraphItem).ParagraphData)
                                {
                                    if (style is SimpleText)
                                    {
                                        sectionItem += (style as SimpleText).Text + " ";
                                    }
                                }
                            }
                        }
                    }
                    Sections.Add(sectionItem);
                }
                Progress progress = await _callService.GetProgress(id);
                Section = progress.Section;
                NotifyScroll?.Invoke(progress.Scroll);
                Task.Run(() => LoadSection(_file, _section));

            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error loading file : {0}", ex.Message));
            }
        }
        private void LoadSection(FB2File file, int sectionNumber)
        {
            CanMoveForward = sectionNumber < file.MainBody.Sections.Count - 1;
            CanMoveBack = sectionNumber > -1;
            StringBuilder stringBuilder = new StringBuilder();
            if (sectionNumber == -1)
            {
                Title = file.TitleInfo.BookTitle.Text;
                foreach (var item in file.TitleInfo.BookAuthors)
                {
                    Title += $"\n{item.FirstName} {item.MiddleName} {item.LastName}";
                }
                foreach (IFb2TextItem item in file.TitleInfo.Annotation.Content)
                {
                    if (item is ParagraphItem)
                    {
                        foreach (StyleType style in (item as ParagraphItem).ParagraphData)
                        {
                            if (style is SimpleText)
                            {
                                stringBuilder.AppendLine($"\t\t\t\t{(style as SimpleText).Text}");
                                Annotation += $"\t\t\t\t{(style as SimpleText).Text}\n";
                            }
                        }
                    }
                }
            }
            else
            {
                SectionItem section = file.MainBody.Sections[sectionNumber];
                if (section.Title != null)
                {
                    foreach (var item in section.Title.TitleData)
                    {
                        if (item is ParagraphItem)
                        {
                            foreach (StyleType style in (item as ParagraphItem).ParagraphData)
                            {
                                if (style is SimpleText)
                                {
                                    stringBuilder.AppendLine($"\t\t\t\t{(style as SimpleText).Text}\n");
                                    Title += $"\t\t\t\t{(style as SimpleText).Text}\n";
                                }
                            }
                        }
                    }
                }
                if (section.Annotation != null)
                {
                    foreach (IFb2TextItem item in section.Annotation.Content)
                    {
                        if (item is ParagraphItem)
                        {
                            foreach (StyleType style in (item as ParagraphItem).ParagraphData)
                            {
                                if (style is SimpleText)
                                {
                                    stringBuilder.AppendLine($"\t\t\t\t{(style as SimpleText).Text}");
                                    Annotation += $"\t\t\t\t{(style as SimpleText).Text}\n";
                                }
                            }
                        }
                    }
                }
                foreach (IFb2TextItem item in section.Content)
                {
                    if (item is ParagraphItem)
                    {
                        foreach (StyleType style in (item as ParagraphItem).ParagraphData)
                        {
                            if (style is SimpleText)
                            {
                                stringBuilder.AppendLine($"\t\t\t\t{(style as SimpleText).Text}");
                                Text += $"\t\t\t\t{(style as SimpleText).Text}\n";
                            }
                        }
                    }
                }
            }
            //Text = stringBuilder.ToString();
        }
        public ReaderViewModel(ApiCallService callService, NavigationService navigationService)
        {
            _callService = callService;
            _navigationService = navigationService;
            Task.Run(SaveProgress);
        }
        private async Task SaveProgress()
        {
            while (true)
            {
                await Task.Delay(10000);
                if(_bookId != 0)
                {
                    await _callService.SetProgress(_bookId, _section, _scroll);
                }
            }
        }
    }
}
