using CommunityToolkit.Maui.Storage;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

namespace Gauss;

public partial class MainPage : ContentPage
{
	private Bitmap _bitmap;
	private string? _tmp;

	public MainPage()
	{
		InitializeComponent();

		_bitmap = (Bitmap) Bitmap.FromFile(
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) 
				+ "/dotnet_bot.scale-400.png");

        // сохраняем картинку с глубиной цвета 3 байта
        _bitmap = _bitmap.Clone(
			new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), 
			PixelFormat.Format24bppRgb);

		DrawImage();

		SetLabels();
	}

	private async void OnSaveBtnClicked(object sender, EventArgs e)
	{
        await FileSaver.Default.SaveAsync(
			Directory.GetCurrentDirectory(),
			Path.GetRandomFileName() + ".bmp",
			new FileStream(_tmp, FileMode.Open),
			new CancellationToken());
    }

    private async void OnCounterClicked(object sender, EventArgs e)
	{
		var bitmap = await Utils.Utils.PickAndShow(PickOptions.Images);

		if (bitmap == null)
		{
			return;
		}

        _bitmap.Dispose();

        _bitmap = bitmap;

        DrawImage();
	}

	private async void DrawImage()
	{ 
		if (_bitmap == null)
		{
			await DisplayAlert("Внимание", "Фото не выбрано", "OK");

			return;
		}

        string prevTmp = _tmp;

        _tmp = Path.GetTempFileName();

		// создание копии битовой карты для коррекции
		var bitmap = (Bitmap) _bitmap.Clone();

		try
		{
            Utils.Utils.ApplyGamma(ref bitmap, red.Value, green.Value, blue.Value);

            bitmap.Save(_tmp);//, System.Drawing.Imaging.ImageFormat.Bmp);

            GaussImage.Source = ImageSource.FromFile(_tmp);

            if (!string.IsNullOrWhiteSpace(prevTmp))
            {
                File.Delete(prevTmp);
            }
        }
		finally
		{
			bitmap.Dispose();
		}		
	}

    private void OnCommonValueChanged(object sender, EventArgs e)
	{
		red.Value = common.Value;
		green.Value = common.Value;
		blue.Value = common.Value;

		OnSliderValueChanged(sender, e);
	}

    private void OnSliderValueChanged(object sender, EventArgs e)
	{	
		SetLabels();

		DrawImage();
	}

    private void GaussValueChanged(object sender, EventArgs e)
    {
        if (GaussValue == null)
        {
            return;
        }


        if (!double.TryParse(GaussValue.Text, out double value))
        {
            return;
        }

        common.Value = Enumerable.Max(
			new[] { Enumerable.Min(
				new[] { 
					value, 5.0 }), 
					0.2 });

		OnCommonValueChanged(sender, e);
    }

    private void SetLabels()
	{
		redLabel.Text = $"Red: {double.Round(red.Value * 100) / 100}";
        greenLabel.Text = $"Green: {double.Round(green.Value * 100) / 100}";
        blueLabel.Text = $"Blue: {double.Round(blue.Value * 100) / 100}";
		commonLabel.Text = $"All: {double.Round(common.Value * 100) / 100}";
    }
}