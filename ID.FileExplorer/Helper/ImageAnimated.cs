using System;
using System.IO;
using System.Net;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Resources;
using System.Windows.Threading;

namespace FileExplorer.Helper
{
    /// <summary>
    /// Image amination control
    /// </summary>
    public partial class ImageAnim : UserControl
    {
        // Only one of the following (_gifAnimation or _image) should be non null at any given time
        private GifAnimation _gifAnimation = null;
        private Image _image = null;

        private bool isFirstLoaded = true;
        public bool IsSelfDestroy
        {
            get { return (bool)GetValue(IsSelfDestroyProperty); }
            set { SetValue(IsSelfDestroyProperty, value); }
        }

        public static readonly DependencyProperty IsSelfDestroyProperty =
            DependencyProperty.Register("IsSelfDestroy", typeof(bool),
                typeof(ImageAnim), new UIPropertyMetadata(false));

        public ImageAnim()
        {
            this.Loaded += ImageAnim_Loaded;
            this.Unloaded += ImageAnim_Unloaded;
        }

        void ImageAnim_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isFirstLoaded && !IsSelfDestroy)
            {
                this.CreateFromSourceString(Source);
            }
            isFirstLoaded = false;
        }

        void ImageAnim_Unloaded(object sender, RoutedEventArgs e)
        {
            if (IsSelfDestroy)
                Dispose();
            else
            {
                DeletePreviousImage();
            }
        }

        /// <summary>
        /// Force gif anim property
        /// </summary>
        public static readonly DependencyProperty ForceGifAnimProperty =
            DependencyProperty.Register("ForceGifAnim", typeof(bool),
                typeof(ImageAnim), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Force gif anim property
        /// </summary>
        public bool ForceGifAnim
        {
            get { return (bool)this.GetValue(ForceGifAnimProperty); }
            set { this.SetValue(ForceGifAnimProperty, value); }
        }

        /// <summary>
        /// Source property
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(string), typeof(ImageAnim), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnSourceChanged)));

        /// <summary>
        /// Source property
        /// </summary>
        /// <param name="d">Dependency object</param>
        /// <param name="e">Dependency property changed event args</param>
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageAnim obj = (ImageAnim)d;
            string s = (string)e.NewValue;
            obj.CreateFromSourceString(s);
        }

        /// <summary>
        /// Source
        /// </summary>
        public string Source
        {
            get { return (string)this.GetValue(SourceProperty); }
            set
            {
                this.SetValue(SourceProperty, value);
            }
        }

        /// <summary>
        /// Stretch property 
        /// </summary>
        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(ImageAnim), new FrameworkPropertyMetadata(Stretch.Fill, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnStretchChanged)));

        /// <summary>
        /// Stretch property 
        /// </summary>
        /// <param name="d">Dependency object</param>
        /// <param name="e">Dependency property changed event args</param>
        private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageAnim obj = (ImageAnim)d;
            Stretch s = (Stretch)e.NewValue;
            if (obj._gifAnimation != null)
            {
                obj._gifAnimation.Stretch = s;
            }
            else if (obj._image != null)
            {
                obj._image.Stretch = s;
            }
        }

        /// <summary>
        /// Stretch
        /// </summary>
        public Stretch Stretch
        {
            get { return (Stretch)this.GetValue(StretchProperty); }
            set { this.SetValue(StretchProperty, value); }
        }

        /// <summary>
        /// Stretch direction property
        /// </summary>
        public static readonly DependencyProperty StretchDirectionProperty = DependencyProperty.Register("StretchDirection", typeof(StretchDirection), typeof(ImageAnim), new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnStretchDirectionChanged)));

        /// <summary>
        /// Stretch direction property
        /// </summary>
        /// <param name="d">Dependency object</param>
        /// <param name="e">Dependency property changed event args</param>
        private static void OnStretchDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageAnim obj = (ImageAnim)d;
            StretchDirection s = (StretchDirection)e.NewValue;
            if (obj._gifAnimation != null)
            {
                obj._gifAnimation.StretchDirection = s;
            }
            else if (obj._image != null)
            {
                obj._image.StretchDirection = s;
            }
        }

        /// <summary>
        /// Strech direction
        /// </summary>
        public StretchDirection StretchDirection
        {
            get { return (StretchDirection)this.GetValue(StretchDirectionProperty); }
            set { this.SetValue(StretchDirectionProperty, value); }
        }

        /// <summary>
        /// Image amination exception event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Event args</param>
        public delegate void ExceptionRoutedEventHandler(object sender, ImageAnimExceptionRoutedEventArgs args);

        /// <summary>
        /// Image failed event
        /// </summary>
        public static readonly RoutedEvent ImageFailedEvent = EventManager.RegisterRoutedEvent("ImageFailed", RoutingStrategy.Bubble, typeof(ExceptionRoutedEventHandler), typeof(ImageAnim));

        /// <summary>
        /// Image faild event
        /// </summary>
        public event ExceptionRoutedEventHandler ImageFailed
        {
            add { AddHandler(ImageFailedEvent, value); }
            remove { RemoveHandler(ImageFailedEvent, value); }
        }

        void _image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            RaiseImageFailedEvent(e.ErrorException);
        }

        void RaiseImageFailedEvent(Exception exp)
        {
            ImageAnimExceptionRoutedEventArgs newArgs = new ImageAnimExceptionRoutedEventArgs(ImageFailedEvent, this);
            newArgs.ErrorException = exp;
            RaiseEvent(newArgs);
        }

        private void DeletePreviousImage()
        {
            if (_image != null)
            {
                this.RemoveLogicalChild(_image);
                _image = null;
            }
            if (_gifAnimation != null)
            {
                this.RemoveLogicalChild(_gifAnimation);
                _gifAnimation.Reset();
                _gifAnimation = null;
            }
        }

        private void CreateNonGifAnimationImage()
        {
            _image = new Image();
            _image.ImageFailed += new EventHandler<ExceptionRoutedEventArgs>(_image_ImageFailed);
            ImageSource src = (ImageSource)(new ImageSourceConverter().ConvertFromString(Source));
            _image.Source = src;
            _image.Stretch = Stretch;
            _image.StretchDirection = StretchDirection;
            this.Content = null;
            this.AddChild(_image);
        }

        private void CreateGifAnimation(MemoryStream memoryStream)
        {
            _gifAnimation = new GifAnimation();
            _gifAnimation.CreateGifAnimation(memoryStream);
            _gifAnimation.Stretch = Stretch;
            _gifAnimation.StretchDirection = StretchDirection;
            //DeletePreviousImage(); //Why delete?
            this.Content = null;
            this.AddChild(_gifAnimation);
        }

        private void CreateFromSourceString(string source)
        {
            DeletePreviousImage();
            Uri uri;
            try
            {
                uri = new Uri(source, UriKind.RelativeOrAbsolute);
            }
            catch (Exception exp)
            {
                RaiseImageFailedEvent(exp);
                return;
            }

            if (source.Trim().ToUpperInvariant().EndsWith(".GIF", StringComparison.InvariantCulture) || ForceGifAnim)
            {
                if (!uri.IsAbsoluteUri)
                {
                    GetGifStreamFromPack(uri);
                }
                else
                {

                    string leftPart = uri.GetLeftPart(UriPartial.Scheme);

                    if (leftPart == "http://" || leftPart == "ftp://" || leftPart == "file://")
                    {
                        GetGifStreamFromHttp(uri);
                    }
                    else if (leftPart == "pack://")
                    {
                        GetGifStreamFromPack(uri);
                    }
                    else
                    {
                        CreateNonGifAnimationImage();
                    }
                }
            }
            else
            {
                CreateNonGifAnimationImage();
            }
        }

        private delegate void WebRequestFinishedDelegate(MemoryStream memoryStream);

        private void WebRequestFinished(MemoryStream memoryStream)
        {
            CreateGifAnimation(memoryStream);
        }

        private delegate void WebRequestErrorDelegate(Exception exp);

        private void WebRequestError(Exception exp)
        {
            RaiseImageFailedEvent(exp);
        }

        private void WebResponseCallback(IAsyncResult asyncResult)
        {
            WebReadState webReadState = (WebReadState)asyncResult.AsyncState;
            WebResponse webResponse;
            try
            {
                webResponse = webReadState.webRequest.EndGetResponse(asyncResult);
                webReadState.readStream = webResponse.GetResponseStream();
                webReadState.buffer = new byte[100000];
                webReadState.readStream.BeginRead(webReadState.buffer, 0, webReadState.buffer.Length, new AsyncCallback(WebReadCallback), webReadState);
            }
            catch (WebException exp)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestErrorDelegate(WebRequestError), exp);
            }
        }

        private void WebReadCallback(IAsyncResult asyncResult)
        {
            WebReadState webReadState = (WebReadState)asyncResult.AsyncState;
            int count = webReadState.readStream.EndRead(asyncResult);
            if (count > 0)
            {
                webReadState.memoryStream.Write(webReadState.buffer, 0, count);
                try
                {
                    webReadState.readStream.BeginRead(webReadState.buffer, 0, webReadState.buffer.Length, new AsyncCallback(WebReadCallback), webReadState);
                }
                catch (WebException exp)
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestErrorDelegate(WebRequestError), exp);
                }
            }
            else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestFinishedDelegate(WebRequestFinished), webReadState.memoryStream);
            }
        }

        private void GetGifStreamFromHttp(Uri uri)
        {
            try
            {
                WebReadState webReadState = new WebReadState();
                webReadState.memoryStream = new MemoryStream();
                webReadState.webRequest = WebRequest.Create(uri);
                webReadState.webRequest.Timeout = 10000;

                webReadState.webRequest.BeginGetResponse(new AsyncCallback(WebResponseCallback), webReadState);
            }
            catch (SecurityException)
            {
                // Try image load, The Image class can display images from other web sites
                CreateNonGifAnimationImage();
            }
        }

        private void ReadGifStreamSynch(Stream s)
        {
            byte[] gifData;
            MemoryStream memoryStream;
            using (s)
            {
                memoryStream = new MemoryStream((int)s.Length);
                BinaryReader br = new BinaryReader(s);
                gifData = br.ReadBytes((int)s.Length);
                memoryStream.Write(gifData, 0, (int)s.Length);
                memoryStream.Flush();
            }
            CreateGifAnimation(memoryStream);
        }

        private void GetGifStreamFromPack(Uri uri)
        {
            try
            {
                StreamResourceInfo streamInfo;

                if (!uri.IsAbsoluteUri)
                {
                    streamInfo = Application.GetContentStream(uri);
                    if (streamInfo == null)
                    {
                        streamInfo = Application.GetResourceStream(uri);
                    }
                }
                else
                {
                    if (uri.GetLeftPart(UriPartial.Authority).Contains("siteoforigin"))
                    {
                        streamInfo = Application.GetRemoteStream(uri);
                    }
                    else
                    {
                        streamInfo = Application.GetContentStream(uri);
                        if (streamInfo == null)
                        {
                            streamInfo = Application.GetResourceStream(uri);
                        }
                    }
                }
                if (streamInfo == null)
                {
                    //ToDo
                    throw new FileNotFoundException("Resource not found.", uri.ToString());
                }
                ReadGifStreamSynch(streamInfo.Stream);
            }
            catch (Exception exp)
            {
                RaiseImageFailedEvent(exp);
            }
        }
    }

    public partial class ImageAnim : IDisposable
    {
        private bool _alreadyDisposed = false;

        // Summary:
        //     Performs application-defined tasks associated with freeing, releasing, or
        //     resetting unmanaged resources.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_alreadyDisposed)
                return;
            if (isDisposing)
            {
                // TODO: release managed resources
                DeletePreviousImage();

                if (_gifAnimation != null)
                {
                    _gifAnimation.Reset();
                }
                _gifAnimation = null;
                _image = null;
                isFirstLoaded = true;
            }
            // TODO: release unmanaged resources
            _alreadyDisposed = true;
        }

        ~ImageAnim()
        {
            Dispose(false);
        }
    }
}