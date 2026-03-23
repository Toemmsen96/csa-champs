using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ZumoLib;

public readonly struct ParsedImage
{
	public ParsedImage(int width, int height, ushort[] pixels)
	{
		if (width <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than 0.");
		}

		if (height <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than 0.");
		}

		if (pixels is null)
		{
			throw new ArgumentNullException(nameof(pixels));
		}

		if (pixels.Length != width * height)
		{
			throw new ArgumentException("Pixel array length must match width * height.", nameof(pixels));
		}

		Width = width;
		Height = height;
		Pixels = pixels;
	}

	public int Width { get; }

	public int Height { get; }

	public ushort[] Pixels { get; }
}

public static class ImageParser
{
	public static ParsedImage ParsePng(string filePath)
	{
		using Image<Rgba32> image = Image.Load<Rgba32>(filePath);
		ushort[] pixels = new ushort[image.Width * image.Height];

		for (int y = 0; y < image.Height; y++)
		{
			int baseIndex = y * image.Width;
			for (int x = 0; x < image.Width; x++)
			{
			Rgba32 pixel = image[x, y];
				byte alpha = pixel.A;

				if (alpha == 0)
				{
					pixels[baseIndex + x] = 0;
					continue;
				}

				if (alpha < 255)
				{
					byte r = (byte)((pixel.R * alpha + 127) / 255);
					byte g = (byte)((pixel.G * alpha + 127) / 255);
					byte b = (byte)((pixel.B * alpha + 127) / 255);
					pixels[baseIndex + x] = Display.Rgb565(r, g, b);
					continue;
				}

				pixels[baseIndex + x] = Display.Rgb565(pixel.R, pixel.G, pixel.B);
			}
		}

		return new ParsedImage(image.Width, image.Height, pixels);
	}
}
