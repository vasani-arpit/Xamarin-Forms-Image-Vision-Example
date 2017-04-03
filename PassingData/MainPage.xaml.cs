using System;
using Microsoft.ProjectOxford.Vision;
using Plugin.Media;
using Xamarin.Forms;

namespace PassingData
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void MenuItem_OnClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "Sample",
                Name = "test.jpg"
            });

            if (file == null)
                return;

            CapturedImage.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                //file.Dispose();
                return stream;
            });

            var loader = new ActivityIndicator
            {
                IsRunning = true,
                IsVisible = true
            };

            Layout.Children.Add(loader);
            VisionServiceClient visionclient = new VisionServiceClient("[Your-API-Key]");
            VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };
            var results = await visionclient.AnalyzeImageAsync(file.GetStream(), visualFeatures);
            //Emotion[] emotionResults = await GetHappiness(file.GetStream());

            loader.IsRunning = false;
            loader.IsVisible = false;

            //foreach (var emotionResult in emotionResults)
            //{
            //    Debug.WriteLine(emotionResult.Scores.Happiness);
            //}

            TableSection ts = new TableSection();
            TableRoot tr = new TableRoot();
            tr.Add(ts);
            TableView tv = new TableView();
            tv.Root = tr;

            foreach (var resultsCategory in results.Categories)
            {
                addTableCell(ts, "Category", resultsCategory.Name);
            }

            foreach (var descriptionCaption in results.Description.Captions)
            {
                addTableCell(ts, "Caption", descriptionCaption.Text);
            }

            foreach (var descriptionTag in results.Description.Tags)
            {
                addTableCell(ts, "Tag", descriptionTag);
            }

            foreach (var descriptionCaption in results.Tags)
            {
                addTableCell(ts, "Tag", descriptionCaption.Name);
            }


            Layout.Children.Add(tv);

        }

        private void addTableCell(TableSection ts, string label, string value)
        {
            TextCell txtCell = new TextCell
            {
                Text = label,
                Detail = value
            };
            ts.Add(txtCell);
        }

        //private static async Task<Emotion[]> GetHappiness(Stream stream)
        //{
        //    string emotionKey = "[EmotionAPI Key]";
        //    EmotionServiceClient emotionClient = new EmotionServiceClient(emotionKey);
        //    var emotionResults = await emotionClient.RecognizeAsync(stream);
        //    if (emotionResults == null || emotionResults.Count() == 0)
        //    {
        //        throw new Exception("Can't detect face");
        //    }
        //    return emotionResults;
        //}
    }
}
