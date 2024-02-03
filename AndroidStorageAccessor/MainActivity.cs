using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Android.Widget;
using AndroidStorageAccessor.Model;
using SQLite;
using System.IO;
using AndroidX.Core.Content;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace AndroidStorageAccessor
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private ProgressBar loaderProgressBar;
        private Button insertButton;
        private string _dbPath = "";
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            loaderProgressBar = FindViewById<ProgressBar>(Resource.Id.loaderProgressBar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            // Initialize SQLite database
            _dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "storageaccessor.db");

            try
            {
                using var dbContext = new ApplicationDbContext(_dbPath);

                await dbContext.Database.MigrateAsync();

            }
            catch (Exception ex)
            {
            }

            insertButton = FindViewById<Button>(Resource.Id.insertBtn);
            insertButton.Click += InsertButton_Click;
        }

        private async void InsertButton_Click(object sender, EventArgs e)
        {
            // Implement the logic to get file paths and insert them into SQLite database
            await InsertFilesIntoDatabaseAsync();
        }

        private async Task InsertFilesIntoDatabaseAsync()
        {

            if (CheckSelfPermission(Android.Manifest.Permission.ReadExternalStorage) != Android.Content.PM.Permission.Granted)
            {
                RequestPermissions(new string[] { Android.Manifest.Permission.ReadExternalStorage }, 1);
            }
            if (CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
            {
                RequestPermissions(new string[] { Android.Manifest.Permission.WriteExternalStorage }, 1);

            }
            if (CheckSelfPermission(Android.Manifest.Permission.ManageExternalStorage) != Android.Content.PM.Permission.Granted)
            {
                RequestPermissions(new string[] { Android.Manifest.Permission.ManageExternalStorage }, 1);

            }
            insertButton.Enabled = false;
            loaderProgressBar.Visibility = Android.Views.ViewStates.Visible;
            List<Item> filePaths = new List<Item>();
            await Task.Run(() =>
            {
                filePaths = GetDirectoriesAndFiles((Android.OS.Environment.ExternalStorageDirectory.AbsolutePath)).ToList();
            });





            // Insert file paths into the database
            InsertFileItem(filePaths.Select(filePath=> new FileItem { FilePath = filePath.Path, IsDirectory = filePath.IsDirectory }).ToList());



            loaderProgressBar.Visibility = Android.Views.ViewStates.Gone;
            insertButton.Enabled = true;
            // Optionally, display a message or perform other actions
            Toast.MakeText(this, "Files inserted into database.", ToastLength.Short).Show();
        }

        private List<Item> GetDirectoriesAndFiles(string rootPath)
        {
            List<Item> result = new List<Item>();
            List<Item> Files = new List<Item>();
            // Get all directories and subdirectories while ignoring certain directories because android does not allow to access them.
            string[] directories = Directory.GetDirectories(rootPath).Where(directory => !IgnoredDirectory().Contains(directory)).ToArray();


            foreach (var directory in directories)
            {
                var subDirectories = Directory.GetDirectories(directory, "*", SearchOption.AllDirectories);
                //add all subDirectories paths in results
                foreach (var subDirectory in subDirectories)
                {
                    result.Add(new Item { Path = subDirectory, IsDirectory = true });
                    var files = Directory.GetFiles(subDirectory);
                    Files.AddRange(files.Select(x => new Item { Path = x, IsDirectory = false }));
                }
                result.Add(new Item { Path = directory, IsDirectory = true });
            }

            result.AddRange(Files);

            return result;
        }


        private async void InsertFileItem(List<FileItem> items)
        {
            try
            {

                using var db = new ApplicationDbContext(_dbPath);
                await db.FileItems.AddRangeAsync(items);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                return;
            }
        }

        public class Item
        {
            public string Path { get; set; }
            public bool IsDirectory { get; set; }
        }


        private List<string> IgnoredDirectory()
        {
            // Add conditions to ignore specific directories
            // For example, ignore the "/Android/obb" directory
            return new List<string> { "/storage/emulated/0/Android" };
        }



        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (View.IOnClickListener)null).Show();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
