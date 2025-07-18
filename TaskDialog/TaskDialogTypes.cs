using System;
using System.ComponentModel;
using System.Drawing;
using TaskDialogInterop.Interfaces;

namespace TaskDialogInterop
{
    /// <summary>
    /// The signature of the callback that recieves notificaitons from a Task Dialog.
    /// </summary>
    /// <param name="dialog">The active dialog. Use this to manipulate various properties of the dialog as it is displayed.</param>
    /// <param name="args">The notification arguments including the type of notification and information for the notification.</param>
    /// <param name="callbackData">The value set on TaskDialog.CallbackData</param>
    /// <returns>
    /// <para>Return value meaning varies depending on the Notification member of args.
    /// In all cases, returning <c>false</c> ensures default behavior.
    /// You can override certain behaviors by returning <c>true</c> in a few cases.</para>
    /// <para>For ButtonClicked: <c>true</c> will keep the dialog open, <c>false</c> will cause the dialog to close and return as normal.</para>
    /// <para>For HyperlinkClicked: <c>true</c> will prevent ShellExecute being called on the hyperlink, <c>false</c> will allow ShellExecute to be called as normal.</para>
    /// <para>For Timer: <c>true</c> will reset the timer tick count; otherwise, <c>false</c> will do nothing.</para>
    /// <para>For all other notifications, the return value is ignored.</para>
    /// </returns>
    public delegate bool TaskDialogCallback(IActiveTaskDialog dialog, VistaTaskDialogNotificationArgs args, object callbackData);

	/// <summary>
	/// Specifies the standard buttons that are displayed on a task dialog.
	/// </summary>
	public enum TaskDialogCommonButtons
	{
		/// <summary>
		/// The message box displays no buttons.
		/// </summary>
		None = 0,
		/// <summary>
		/// The message box displays a Close button.
		/// </summary>
		Close = 1,
		/// <summary>
		/// The message box displays Yes and No buttons.
		/// </summary>
		YesNo = 2,
		/// <summary>
		/// The message box displays Yes, No, and Cancel buttons.
		/// </summary>
		YesNoCancel = 3,
		/// <summary>
		/// The message box displays OK and Cancel buttons.
		/// </summary>
		OKCancel = 4,
		/// <summary>
		/// The message box displays Retry and Cancel buttons.
		/// </summary>
		RetryCancel = 5
	}
	/// <summary>
	/// Defines configuration options for showing a task dialog.
	/// </summary>
	public struct TaskDialogOptions
    {
        //-----------------------------------------------------------------
        /// <summary>
        /// The handle window of the task dialog box.
        /// </summary>
        public IntPtr Handle;
        /// <summary>
        /// Enable Minimize state window of the task dialog box.
        /// </summary>
        public bool CanBeMinimized;
        /// <summary>
        /// The width window of the task dialog box.
        /// </summary>
        public uint Width;
        //-----------------------------------------------------------------
		
        /// <summary>
		/// The default <see cref="T:TaskDialogOptions"/> to be used
		/// by all new <see cref="T:TaskDialog"/>s.
		/// </summary>
		/// <remarks>
		/// Use this to make application-wide defaults, such as for
		/// the caption.
		/// </remarks>
		public static TaskDialogOptions Default;
        
		/// <summary>
		/// The owner window of the task dialog box.
		/// </summary>
        public System.Windows.Window Owner;
		/// <summary>
		/// Caption of the window.
		/// </summary>
		public string Title;
		/// <summary>
		/// A large 32x32 icon that signifies the purpose of the dialog, using
		/// one of the built-in system icons.
		/// </summary>
		public VistaTaskDialogIcon MainIcon;
		/// <summary>
		/// A large 32x32 icon that signifies the purpose of the dialog, using
		/// a custom Icon resource. If defined <see cref="MainIcon"/> will be
		/// ignored.
		/// </summary>
		public Icon CustomMainIcon;
		/// <summary>
		/// Principal text.
		/// </summary>
		public string MainInstruction;
		/// <summary>
		/// Supplemental text that expands on the principal text.
		/// </summary>
		public string Content;
		/// <summary>
		/// Extra text that will be hidden by default.
		/// </summary>
		public string ExpandedInfo;
		/// <summary>
		/// Indicates that the expanded info should be displayed when the
		/// dialog is initially displayed.
		/// </summary>
		public bool ExpandedByDefault;
		/// <summary>
		/// Indicates that the expanded info should be displayed at the bottom
		/// of the dialog's footer area instead of immediately after the
		/// dialog's content.
		/// </summary>
		public bool ExpandToFooter;
		/// <summary>
		/// Standard buttons.
		/// </summary>
		public TaskDialogCommonButtons CommonButtons;
		/// <summary>
		/// Application-defined options for the user.
		/// </summary>
		public string[] RadioButtons;
		/// <summary>
		/// Buttons that are not from the set of standard buttons. Use an
		/// ampersand to denote an access key.
		/// </summary>
		public string[] CustomButtons;
		/// <summary>
		/// Command link buttons.
		/// </summary>
		public string[] CommandButtons;
		/// <summary>
		/// Zero-based index of the button to have focus by default.
		/// </summary>
		public int? DefaultButtonIndex;
		/// <summary>
		/// Text accompanied by a checkbox, typically for user feedback such as
		/// Do-not-show-this-dialog-again options.
		/// </summary>
		public string VerificationText;
		/// <summary>
		/// Indicates that the verification checkbox in the dialog is checked
		/// when the dialog is initially displayed.
		/// </summary>
		public bool VerificationByDefault;
		/// <summary>
		/// A small 16x16 icon that signifies the purpose of the footer text,
		/// using one of the built-in system icons.
		/// </summary>
		public VistaTaskDialogIcon FooterIcon;
		/// <summary>
		/// A small 16x16 icon that signifies the purpose of the footer text,
		/// using a custom Icon resource. If defined <see cref="FooterIcon"/>
		/// will be ignored.
		/// </summary>
		public Icon CustomFooterIcon;
		/// <summary>
		/// Additional footer text.
		/// </summary>
		public string FooterText;
		/// <summary>
		/// Indicates that the dialog should be able to be closed using Alt-F4,
		/// Escape, and the title bar's close button even if no cancel button
		/// is specified the CommonButtons.
		/// </summary>
		/// <remarks>
		/// You'll want to set this to true if you use CustomButtons and have
		/// a Cancel-like button in it.
		/// </remarks>
		public bool AllowDialogCancellation;
		/// <summary>
		/// Indicates that a Progress Bar is to be displayed.
		/// </summary>
		/// <remarks>
		/// You can set the state, whether paused, in error, etc., as well as
		/// the range and current value by setting a callback and timer to
		/// control the dialog at custom intervals.
		/// </remarks>
		public bool ShowProgressBar;
		/// <summary>
		/// Indicates that an Marquee Progress Bar is to be displayed.
		/// </summary>
		/// <remarks>
		/// You can set start and stop the animation by setting a callback and
		/// timer to control the dialog at custom intervals.
		/// </remarks>
		public bool ShowMarqueeProgressBar;
		/// <summary>
		/// A callback that receives messages from the Task Dialog when
		/// various events occur.
		/// </summary>
		public TaskDialogCallback Callback;
		/// <summary>
		/// Reference object that is passed to the callback.
		/// </summary>
		public object CallbackData;
		/// <summary>
		/// Indicates that the task dialog's callback is to be called
		/// approximately every 200 milliseconds.
		/// </summary>
		/// <remarks>
		/// Enable this in order to do updates on the task dialog periodically,
		/// such as for a progress bar, current download speed, or estimated
		/// time to complete, etc.
		/// </remarks>
		public bool EnableCallbackTimer;
	}
	/// <summary>
	/// Provides data for all task dialog buttons.
	/// </summary>
	public class TaskDialogButtonData : INotifyPropertyChanged
	{
		private bool _isEnabled;
		private bool _isElevationRequired;

		/// <summary>
		/// Gets the button's ID value to return when clicked.
		/// </summary>
		public int ID { get; set; }
		/// <summary>
		/// Gets the button's text label.
		/// </summary>
		public string Text { get; set; }
		/// <summary>
		/// Gets a value indicating whether or not the button should be the default.
		/// </summary>
		public bool IsDefault { get; set; }
		/// <summary>
		/// Gets a value indicating whether or not the button should be a cancel.
		/// </summary>
		public bool IsCancel { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether or not the button should be enabled.
		/// </summary>
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				_isEnabled = value;

				RaisePropertyChanged("IsEnabled");
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether or not the button requires elevation.
		/// </summary>
		public bool IsElevationRequired
		{
			get { return _isElevationRequired; }
			set
			{
				_isElevationRequired = value;

				RaisePropertyChanged("IsElevationRequired");
			}
		}
		/// <summary>
		/// Gets the button's command.
		/// </summary>
		public System.Windows.Input.ICommand Command { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TaskDialogButtonData"/> class.
		/// </summary>
		public TaskDialogButtonData()
		{
			_isEnabled = true;
			_isElevationRequired = false;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="TaskDialogButtonData"/> struct.
		/// </summary>
		/// <param name="id">The id value for the button.</param>
		/// <param name="text">The text label.</param>
		/// <param name="command">The command to associate.</param>
		/// <param name="isDefault">Whether the button should be the default.</param>
		/// <param name="isCancel">Whether the button should be a cancel.</param>
		public TaskDialogButtonData(int id, string text, System.Windows.Input.ICommand command = null, bool isDefault = false, bool isCancel = false)
			: this()
		{
			ID = id;
			Text = text;
			Command = command;
			IsDefault = isDefault;
			IsCancel = isCancel;
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the <see cref="E:PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">The property name of the property that has changed.</param>
		protected void RaisePropertyChanged(string propertyName)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Raises the <see cref="E:PropertyChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, e);
		}
	}
}
