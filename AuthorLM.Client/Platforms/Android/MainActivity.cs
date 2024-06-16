using Android.App;
using Android.Content.PM;
using Android.OS;

namespace AuthorLM.Client
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            new ImageCropper.Maui.Platform().Init(this);
            Window.SetNavigationBarColor(Android.Graphics.Color.Rgb(162, 118, 118));
            base.OnCreate(savedInstanceState);
        }
    }
}
