//	General image manipulation program.
//	Current functions:
//	1.	Generate Thumbnails by specified size and/or quality;
//	2.	Add text and/or watermaker to images.
//
//	Started at: 27/2/2004
//	Author:	Ray Wang

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Dragonfish.Library.Image
{
    /// <summary>
    /// Generate thumbnail by specifying scale percentage or fixed width & hight.
    /// </summary>
    public class ImageHelper : IDisposable
    {
        public enum WatermarkPosition
        {
            /// <summary>
            /// Centered
            /// </summary>
            Centre = 0,

            /// <summary>
            /// Upper left corner
            /// </summary>
            UpperLeft = 1,

            /// <summary>
            /// Upper right corner
            /// </summary>
            UpperRight = 2,

            /// <summary>
            /// Lower left corner
            /// </summary>
            LowerLeft = 3,

            /// <summary>
            /// Lower right corner
            /// </summary>
            LowerRight = 4
        };

        #region Member variables

        private System.Drawing.Image _srcImage;

        private const long DEFAULT_JPG_QUALITY = 85L;
        private const string DEFAULT_ENCODER_INFO = "image/jpeg";

        #endregion Member variables

        #region Constructors

        public ImageHelper(System.Drawing.Image srcImage)
        {
            _srcImage = srcImage;
        }

        /// <summary>
        /// Constructors
        /// </summary>
        /// <param name="inputFileName">Full path & file name</param>
        /// <param name="outputWidth">Thumbnail width</param>
        /// <param name="outputHeight">Thumbnail hight</param>
        /// <param name="outputFileName">Output path & file name</param>
        public ImageHelper(string inputFileName)
        {
            try
            {
                _srcImage = System.Drawing.Image.FromFile(inputFileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Constructors

        #region Methods

        #region thumbnail

        /// <summary>
        /// Save resized image to a give path in specified format.
        /// </summary>
        /// <param name="outputFileName">output file name</param>
        /// <param name="outputWidth">output width</param>
        /// <param name="outputHeight">output height</param>
        /// <param name="lockRatio">lock scacle</param>
        /// <param name="jpegQuality">jpeg quality. (1-100)</param>
        /// <param name="encoderInfo">encoder info i.e. jpeg or png</param>
        public void SaveResizedImage(string outputFileName, int outputWidth, int outputHeight, bool lockRatio, long jpegQuality, string encoderInfo)
        {
            try
            {
                GetResizedImage(outputWidth, outputHeight, lockRatio).Save(outputFileName, GetEncoderInfo(encoderInfo), GetEncoderParameters(jpegQuality));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Save resized image to a give path in jpeg format.
        /// </summary>
        /// <param name="outputFileName">output file name</param>
        /// <param name="outputWidth">output width</param>
        /// <param name="outputHeight">output height</param>
        /// <param name="lockRatio">lock scacle</param>
        /// <param name="jpegQuality">jpeg quality. (1-100)</param>
        public void SaveResizedImage(string outputFileName, int outputWidth, int outputHeight, bool lockRatio, long jpegQuality)
        {
            SaveResizedImage(outputFileName, outputWidth, outputHeight, lockRatio, jpegQuality, DEFAULT_ENCODER_INFO);
        }

        /// <summary>
        /// Save resized image to a give path in specified format with default quality 85%.
        /// </summary>
        /// <param name="outputFileName">output file name</param>
        /// <param name="outputWidth">output width</param>
        /// <param name="outputHeight">output height</param>
        /// <param name="lockRatio">lock scale</param>
        /// <param name="encoderInfo">encoder info either jpeg or png</param>
        public void SaveResizedImage(string outputFileName, int outputWidth, int outputHeight, bool lockRatio, string encoderInfo)
        {
            SaveResizedImage(outputFileName, outputWidth, outputHeight, lockRatio, DEFAULT_JPG_QUALITY, encoderInfo);
        }

        /// <summary>
        /// Save resized image to a give path in jpeg format with default quality 85%.
        /// </summary>
        /// <param name="outputFileName">output file name</param>
        /// <param name="outputWidth">output width</param>
        /// <param name="outputHeight">output height</param>
        /// <param name="lockRatio">lock scale</param>
        public void SaveResizedImage(string outputFileName, int outputWidth, int outputHeight, bool lockRatio)
        {
            SaveResizedImage(outputFileName, outputWidth, outputHeight, lockRatio, DEFAULT_JPG_QUALITY, DEFAULT_ENCODER_INFO);
        }

        public void SaveCroppedImage(string outputFileName, int size)
        {
            SaveCroppedImage(outputFileName, size, 80);
        }

        public void SaveCroppedImage(string outputFileName, int size, long jpegQuality)
        {
            ImageAttributes attributes = null;
            Bitmap bmPhoto = null;
            Graphics grPhoto = null;

            try
            {
                int squareSize = _srcImage.Width > _srcImage.Height ? _srcImage.Height : _srcImage.Width;

                int x = _srcImage.Width > _srcImage.Height ? (int)((_srcImage.Width - _srcImage.Height) / 2) : 0;
                int y = _srcImage.Width > _srcImage.Height ? 0 : (int)((_srcImage.Height - _srcImage.Width) / 2);

                bmPhoto = new Bitmap(size, size, PixelFormat.Format24bppRgb);
                bmPhoto.SetResolution(72, 72);

                attributes = new ImageAttributes();
                attributes.SetWrapMode(WrapMode.TileFlipXY);

                grPhoto = Graphics.FromImage(bmPhoto);
                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grPhoto.DrawImage(_srcImage,
                    new Rectangle(0, 0, size, size),
                    x, y, squareSize, squareSize, GraphicsUnit.Pixel, attributes);
                bmPhoto.Save(outputFileName, GetEncoderInfo("image/jpeg"), GetEncoderParameters(jpegQuality));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (attributes != null)
                    attributes.Dispose();
                if (grPhoto != null)
                    grPhoto.Dispose();
                GC.Collect();
            }
        }

        /// <summary>
        /// get resized image.
        /// </summary>
        /// <param name="outputFileName">output file name</param>
        /// <param name="outputWidth">output width</param>
        /// <param name="outputHeight">output height</param>
        /// <param name="lockScale">lock scacle</param>
        /// <param name="jpegQuality">jpeg quality. (1-100)</param>
        /// <returns>resized image</returns>
        public System.Drawing.Image GetResizedImage(int outputWidth, int outputHeight, bool lockRatio)
        {
            ImageAttributes attributes = null;
            Bitmap bmPhoto = null;
            Graphics grPhoto = null;

            try
            {
                if (_srcImage.Width < outputWidth && _srcImage.Height < outputHeight)
                    return _srcImage;

                if (lockRatio == true)
                {
                    if ((_srcImage.Width / _srcImage.Height) >= (outputWidth / outputHeight))
                        outputHeight = outputWidth * _srcImage.Height / _srcImage.Width;
                    else if ((_srcImage.Width / _srcImage.Height) < (outputWidth / outputHeight))
                        outputWidth = outputHeight * _srcImage.Width / _srcImage.Height;
                    else
                        return _srcImage;
                }

                // do not set pixel format then it can detect automatically 24bit for jpeg or 32bit for png for example
                bmPhoto = new Bitmap(outputWidth, outputHeight);
                bmPhoto.SetResolution(72, 72);

                attributes = new ImageAttributes();
                attributes.SetWrapMode(WrapMode.TileFlipXY);

                grPhoto = Graphics.FromImage(bmPhoto);
                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grPhoto.DrawImage(_srcImage,
                    new Rectangle(0, 0, outputWidth, outputHeight),
                    0, 0, _srcImage.Width, _srcImage.Height, GraphicsUnit.Pixel, attributes);
                return bmPhoto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (attributes != null)
                    attributes.Dispose();
                if (grPhoto != null)
                    grPhoto.Dispose();
                GC.Collect();
            }
        }

        #endregion thumbnail

        #region watermark

        public System.Drawing.Image GetWatermarkedImage(string watermarkFileName, string outputFileName, WatermarkPosition wmPosition, int pixToHorizontalEdge, int pixToVerticalEdge)
        {
            Bitmap bmPhoto = null;
            System.Drawing.Image imgWatermark = null;
            Bitmap bmWatermark = null;
            Graphics grPhoto = null;
            Graphics grWatermark = null;
            ImageAttributes imageAttributes = null;

            try
            {
                imgWatermark = new Bitmap(watermarkFileName);

                bmPhoto = new Bitmap(_srcImage.Width, _srcImage.Height, PixelFormat.Format24bppRgb);
                bmPhoto.SetResolution(_srcImage.HorizontalResolution, _srcImage.VerticalResolution);

                grPhoto = Graphics.FromImage(bmPhoto);
                grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
                grPhoto.DrawImage(_srcImage, new Rectangle(0, 0, _srcImage.Width, _srcImage.Height), 0, 0, _srcImage.Width, _srcImage.Height, GraphicsUnit.Pixel);
                bmWatermark = new Bitmap(bmPhoto);
                bmWatermark.SetResolution(_srcImage.HorizontalResolution, _srcImage.VerticalResolution);

                grWatermark = Graphics.FromImage(bmWatermark);
                imageAttributes = new ImageAttributes();
                ColorMap colorMap = new ColorMap();
                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                ColorMap[] remapTable = { colorMap };

                imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                float[][] colorMatrixElements = {
													new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
													new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
													new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
													new float[] {0.0f,  0.0f,  0.0f,  0.3f, 0.0f},
													new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
												};
                ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);
                imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                int xPosOfWatermark = 0;
                int yPosOfWatermark = 0;

                switch (wmPosition)
                {
                    case WatermarkPosition.Centre:
                        xPosOfWatermark = (_srcImage.Width - imgWatermark.Width) / 2;
                        yPosOfWatermark = (_srcImage.Height - imgWatermark.Height) / 2;
                        break;

                    case WatermarkPosition.UpperLeft:
                        xPosOfWatermark = pixToVerticalEdge;
                        yPosOfWatermark = pixToHorizontalEdge;
                        break;

                    case WatermarkPosition.UpperRight:
                        xPosOfWatermark = _srcImage.Width - imgWatermark.Width - pixToVerticalEdge;
                        yPosOfWatermark = pixToHorizontalEdge;
                        break;

                    case WatermarkPosition.LowerLeft:
                        xPosOfWatermark = pixToVerticalEdge;
                        yPosOfWatermark = _srcImage.Height - imgWatermark.Height - pixToHorizontalEdge;
                        break;

                    case WatermarkPosition.LowerRight:
                        xPosOfWatermark = _srcImage.Width - imgWatermark.Width - pixToVerticalEdge;
                        yPosOfWatermark = _srcImage.Height - imgWatermark.Height - pixToHorizontalEdge;
                        break;
                }

                grWatermark.DrawImage(imgWatermark, new Rectangle(xPosOfWatermark, yPosOfWatermark, imgWatermark.Width, imgWatermark.Height), 0, 0, imgWatermark.Width, imgWatermark.Height, GraphicsUnit.Pixel, imageAttributes);

                return bmWatermark;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (bmPhoto != null)
                    bmPhoto.Dispose();
                if (grPhoto != null)
                    grPhoto.Dispose();
                if (grWatermark != null)
                    grWatermark.Dispose();
                if (imgWatermark != null)
                    imgWatermark.Dispose();
                if (imageAttributes != null)
                    imageAttributes.Dispose();

                GC.Collect();
            }
        }

        public void SaveWatermarkedImage(string watermarkFileName, string outputFileName, WatermarkPosition wmPosition, int pixToHorizontalEdge, int pixToVerticalEdge, long jpegQuality)
        {
            try
            {
                GetWatermarkedImage(watermarkFileName, outputFileName, wmPosition, pixToHorizontalEdge, pixToVerticalEdge).Save(outputFileName, GetEncoderInfo("image/jpeg"), GetEncoderParameters(jpegQuality));
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Save watermarked image to specified path. Image quality set to default which is 85%.
        /// </summary>
        /// <param name="watermarkFileName"></param>
        /// <param name="outputFileName"></param>
        /// <param name="wmPosition"></param>
        /// <param name="pixToHorizontalEdge"></param>
        /// <param name="pixToVerticalEdge"></param>
        public void SaveWatermarkedImage(string watermarkFileName, string outputFileName, WatermarkPosition wmPosition, int pixToHorizontalEdge, int pixToVerticalEdge)
        {
            SaveWatermarkedImage(watermarkFileName, outputFileName, wmPosition, pixToHorizontalEdge, pixToVerticalEdge, DEFAULT_JPG_QUALITY);
        }

        public void GetTextedPicture(string outputFileName, string text, string fontName, Brush brush, float size, Rectangle rec, long jpegQuality)
        {
            try
            {
                Bitmap bmPhoto = new Bitmap(_srcImage);
                // bmPhoto.SetResolution(_srcImage.HorizontalResolution, _srcImage.VerticalResolution);
                Graphics grPhoto = Graphics.FromImage(_srcImage);
                grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
                grPhoto.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                Font font = new Font(fontName, size);
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Near;
                grPhoto.DrawString(text,
                    font,
                    brush,
                    rec,
                    format);

                _srcImage.Save(outputFileName, GetEncoderInfo("image/jpeg"), GetEncoderParameters(jpegQuality));

                bmPhoto.Dispose();
                grPhoto.Dispose();
                GC.Collect();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion watermark

        #region Merge two

        /// <summary>
        ///
        /// </summary>
        /// <param name="ImageBack"></param>
        /// <param name="ImageFore"></param>
        /// <param name="output"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void MergeImages(string ImageBack, string ImageFore, string output, int x, int y, int width, int height)
        {
            System.Drawing.Graphics myGraphic = null;

            System.Drawing.Image imgB;// =new Image.FromFile(ImageBack);
            imgB = System.Drawing.Image.FromFile(ImageBack);
            System.Drawing.Image imgF;// =new Image.FromFile(ImageBack);
            imgF = System.Drawing.Image.FromFile(ImageFore);
            System.Drawing.Image m;
            m = System.Drawing.Image.FromFile(ImageFore);
            myGraphic = Graphics.FromImage(m);
            myGraphic.DrawImageUnscaled(imgB, x, y, width, height);
            myGraphic.DrawImageUnscaled(imgF, 0, 0);
            myGraphic.Save();

            m.Save(output, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        #endregion Merge two

        private EncoderParameters GetEncoderParameters(long jpegQuality)
        {
            Encoder qualityEncoder = Encoder.Quality;
            EncoderParameter ratio = new EncoderParameter(qualityEncoder, jpegQuality);
            EncoderParameters codecParams = new EncoderParameters(1);
            codecParams.Param[0] = ratio;
            return codecParams;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        private ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        public void Dispose()
        {
            if (_srcImage != null)
                _srcImage.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///
        /// </summary>
        ///

        ~ImageHelper()
        {
            this.Dispose();
        }

        #endregion Methods
    }
}