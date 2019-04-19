using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace ExpenseLogWebAPI.Helpers
{
    public class ImageUtils
    {
        public byte[] ConvertToGrayscaleJpeg(System.IO.Stream inputImageStream)
        {
            try
            {
                // load original image
                Bitmap bitmap = new Bitmap(inputImageStream);

                //Lock bitmap's bits to system memory
                Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, bitmap.PixelFormat);

                //Scan for the first line
                IntPtr intPtr = bitmapData.Scan0;

                //Declare an array in which your RGB values will be stored
                int numberOfBytes = Math.Abs(bitmapData.Stride) * bitmap.Height;
                byte[] bytes = new byte[numberOfBytes];

                //Copy RGB values in that array
                Marshal.Copy(intPtr, bytes, 0, numberOfBytes);

                try
                {
                    for (int i = 0; i + 3 < bytes.Length; i += 3)
                    {
                        //Set RGB values in a Array where all RGB values are stored
                        byte gray = (byte)(bytes[i] * .21 + bytes[i + 1] * .71 + bytes[i + 2] * .071);
                        bytes[i] = bytes[i + 1] = bytes[i + 2] = gray;
                    }
                }
                catch (Exception ex1)
                {
                    throw new Exception($"Error on Set RGB array: {ex1.GetBaseException().Message}");
                }

                //Copy changed RGB values back to bitmap
                Marshal.Copy(bytes, 0, intPtr, numberOfBytes);

                //Unlock the bits
                bitmap.UnlockBits(bitmapData);

                //Encode to JPEG
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 60L);

                //Stream back the result
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    bitmap.Save(memoryStream, GetEncoder(ImageFormat.Jpeg), encoderParameters);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"ConvertToGrayscaleJpeg failed. {ex.GetBaseException().Message}");
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}