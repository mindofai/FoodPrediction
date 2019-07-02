using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FoodPredictionApp
{
    public partial class MainPage : ContentPage
    {
        private Stream stream;

        public MainPage()
        {
            InitializeComponent();

            CrossMedia.Current.Initialize();

            pickPhoto.Clicked += async (sender, args) =>
            {
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                    return;
                }
                var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,

                });


                if (file == null)
                    return;

                activityIndicator.IsVisible = true;
                stream = file.GetStream();
                await IdentifyFood(stream);
                activityIndicator.IsVisible = false;

                image.Source = ImageSource.FromStream(() =>
                {
                    stream = file.GetStream();
                    file.Dispose();
                    return stream;
                });
            };
        }

        private async void TakePhoto(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "Test",
                SaveToAlbum = true,
                CompressionQuality = 75,
                CustomPhotoSize = 50,
                PhotoSize = PhotoSize.MaxWidthHeight,
                MaxWidthHeight = 2000,
                DefaultCamera = CameraDevice.Front
            });

            if (file == null)
                return;

            activityIndicator.IsVisible = true;
            stream = file.GetStream();
            await IdentifyFood(stream);
            activityIndicator.IsVisible = false;

            image.Source = ImageSource.FromStream(() =>
            {
                stream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }

        public async Task IdentifyFood(Stream image)
        {
            string predictionKey = "4a6f15ad57c941b1b72e215f6f69c410";
            string endpoint = "https://southeastasia.api.cognitive.microsoft.com/";
            Guid projectId = new Guid("5add8d84-ecc6-4782-bd74-5ffbf24abf8a");
            string modelName = "Published";

            CustomVisionPredictionClient client = new CustomVisionPredictionClient()
            {
                ApiKey = predictionKey,
                Endpoint = endpoint
            };


            var result = await client.ClassifyImageAsync(projectId, modelName, image);

            var firstPrediction = result.Predictions.FirstOrDefault();

            resultText.Text = $"{(firstPrediction.Probability * 100).ToString("0.##")}% {firstPrediction.TagName}";
        }

    }
}
