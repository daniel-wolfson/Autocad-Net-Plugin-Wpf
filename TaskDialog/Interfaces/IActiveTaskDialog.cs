using System.Drawing;

namespace TaskDialogInterop.Interfaces
{
    /// <summary>
    /// Defines methods for manipulating an active dialog during a callback.
    /// </summary>
    public interface IActiveTaskDialog
    {
        // TODO Support more of the methods exposed by VistaActiveTaskDialog class

        /// <summary>
        /// Simulate the action of a button click in the TaskDialog. This can be a DialogResult value 
        /// or the ButtonID set on a TaskDialogButton set on TaskDialog.Buttons.
        /// </summary>
        /// <param name="buttonId">Indicates the button ID to be selected.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool ClickButton(int buttonId);
        /// <summary>
        /// Simulate the action of a command link button click in the TaskDialog.
        /// </summary>
        /// <param name="index">The zero-based index into the button set.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool ClickCommandButton(int index);
        /// <summary>
        /// Simulate the action of a common button click in the TaskDialog.
        /// </summary>
        /// <param name="index">The zero-based index into the button set.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool ClickCommonButton(int index);
        /// <summary>
        /// Simulate the action of a custom button click in the TaskDialog.
        /// </summary>
        /// <param name="index">The zero-based index into the button set.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool ClickCustomButton(int index);
        /// <summary>
        /// Simulate the action of a radio button click in the TaskDialog.
        /// </summary>
        /// <param name="index">The zero-based index into the button set.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool ClickRadioButton(int index);
        /// <summary>
        /// Check or uncheck the verification checkbox in the TaskDialog. 
        /// </summary>
        /// <param name="checkedState">The checked state to set the verification checkbox.</param>
        /// <param name="setKeyboardFocusToCheckBox"><c>true</c> to set the keyboard focus to the checkbox; <c>false</c> to leave focus unchanged.</param>
        void ClickVerification(bool checkedState, bool setKeyboardFocusToCheckBox);
        /// <summary>
        /// Sets the state of a button to enabled or disabled.
        /// </summary>
        /// <param name="buttonId">The id of the button to set.</param>
        /// <param name="enabled"><c>true</c> to enable the button; <c>false</c> to disable</param>
        void SetButtonEnabledState(int buttonId, bool enabled);
        /// <summary>
        /// Sets the state of a command link button to enabled or disabled.
        /// </summary>
        /// <param name="index">The zero-based index of the button to set.</param>
        /// <param name="enabled"><c>true</c> to enable the button; <c>false</c> to disable</param>
        void SetCommandButtonEnabledState(int index, bool enabled);
        /// <summary>
        /// Sets the state of a common button to enabled or disabled.
        /// </summary>
        /// <param name="index">The zero-based index of the button to set.</param>
        /// <param name="enabled"><c>true</c> to enable the button; <c>false</c> to disable</param>
        void SetCommonButtonEnabledState(int index, bool enabled);
        /// <summary>
        /// Sets the state of a custom button to enabled or disabled.
        /// </summary>
        /// <param name="index">The zero-based index of the button to set.</param>
        /// <param name="enabled"><c>true</c> to enable the button; <c>false</c> to disable</param>
        void SetCustomButtonEnabledState(int index, bool enabled);
        /// <summary>
        /// Sets the state of a radio button to enabled or disabled.
        /// </summary>
        /// <param name="index">The zero-based index of the button to set.</param>
        /// <param name="enabled"><c>true</c> to enable the button; <c>false</c> to disable</param>
        void SetRadioButtonEnabledState(int index, bool enabled);
        /// <summary>
        /// Sets the elevation required state of a button, adding a shield icon.
        /// </summary>
        /// <param name="buttonId">The id of the button to set.</param>
        /// <param name="elevationRequired"><c>true</c> to show a shield icon; <c>false</c> to remove</param>
        /// <remarks>
        /// Note that this is purely for visual effect. You will still need to perform
        /// the necessary code to trigger a UAC prompt for the user.
        /// </remarks>
        void SetButtonElevationRequiredState(int buttonId, bool elevationRequired);
        /// <summary>
        /// Sets the elevation required state of a command link button, adding a shield icon.
        /// </summary>
        /// <param name="index">The zero-based index of the button to set.</param>
        /// <param name="elevationRequired"><c>true</c> to show a shield icon; <c>false</c> to remove</param>
        /// <remarks>
        /// Note that this is purely for visual effect. You will still need to perform
        /// the necessary code to trigger a UAC prompt for the user.
        /// </remarks>
        void SetCommandButtonElevationRequiredState(int index, bool elevationRequired);
        /// <summary>
        /// Sets the elevation required state of a common button, adding a shield icon.
        /// </summary>
        /// <param name="index">The zero-based index of the button to set.</param>
        /// <param name="elevationRequired"><c>true</c> to show a shield icon; <c>false</c> to remove</param>
        /// <remarks>
        /// Note that this is purely for visual effect. You will still need to perform
        /// the necessary code to trigger a UAC prompt for the user.
        /// </remarks>
        void SetCommonButtonElevationRequiredState(int index, bool elevationRequired);
        /// <summary>
        /// Sets the elevation required state of a custom button, adding a shield icon.
        /// </summary>
        /// <param name="index">The zero-based index of the button to set.</param>
        /// <param name="elevationRequired"><c>true</c> to enable the button; <c>false</c> to disable</param>
        /// <remarks>
        /// Note that this is purely for visual effect. You will still need to perform
        /// the necessary code to trigger a UAC prompt for the user.
        /// </remarks>
        void SetCustomButtonElevationRequiredState(int index, bool elevationRequired);
        /// <summary>
        /// Used to indicate whether the hosted progress bar should be displayed in marquee mode or not.
        /// </summary>
        /// <param name="marquee">Specifies whether the progress bar sbould be shown in Marquee mode.
        /// A value of true turns on Marquee mode.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool SetMarqueeProgressBar(bool marquee);
        /// <summary>
        /// Sets the state of the progress bar.
        /// </summary>
        /// <param name="newState">The state to set the progress bar.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool SetProgressBarState(VistaProgressBarState newState);
        /// <summary>
        /// Set the minimum and maximum values for the hosted progress bar.
        /// </summary>
        /// <param name="minRange">Minimum range value. By default, the minimum value is zero.</param>
        /// <param name="maxRange">Maximum range value.  By default, the maximum value is 100.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool SetProgressBarRange(short minRange, short maxRange);
        /// <summary>
        /// Set the current position for a progress bar.
        /// </summary>
        /// <param name="newPosition">The new position.</param>
        /// <returns>Returns the previous value if successful, or zero otherwise.</returns>
        int SetProgressBarPosition(int newPosition);
        /// <summary>
        /// Sets the animation state of the Marquee Progress Bar.
        /// </summary>
        /// <param name="startMarquee">true starts the marquee animation and false stops it.</param>
        /// <param name="speed">The time in milliseconds between refreshes.</param>
        void SetProgressBarMarquee(bool startMarquee, uint speed);
        /// <summary>
        /// Updates the window title text.
        /// </summary>
        /// <param name="title">The new value.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool SetWindowTitle(string title);
        /// <summary>
        /// Updates the content text.
        /// </summary>
        /// <param name="content">The new value.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool SetContent(string content);
        /// <summary>
        /// Updates the Expanded Information text.
        /// </summary>
        /// <param name="expandedInformation">The new value.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool SetExpandedInformation(string expandedInformation);
        /// <summary>
        /// Updates the Footer text.
        /// </summary>
        /// <param name="footer">The new value.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool SetFooter(string footer);
        /// <summary>
        /// Updates the Main Instruction.
        /// </summary>
        /// <param name="mainInstruction">The new value.</param>
        /// <returns>If the function succeeds the return value is true.</returns>
        bool SetMainInstruction(string mainInstruction);
        /// <summary>
        /// Updates the main instruction icon. Note the type (standard via enum or
        /// custom via Icon type) must be used when upating the icon.
        /// </summary>
        /// <param name="icon">Task Dialog standard icon.</param>
        void UpdateMainIcon(VistaTaskDialogIcon icon);
        /// <summary>
        /// Updates the main instruction icon. Note the type (standard via enum or
        /// custom via Icon type) must be used when upating the icon.
        /// </summary>
        /// <param name="icon">The icon to set.</param>
        void UpdateMainIcon(Icon icon);
        /// <summary>
        /// Updates the footer icon. Note the type (standard via enum or
        /// custom via Icon type) must be used when upating the icon.
        /// </summary>
        /// <param name="icon">Task Dialog standard icon.</param>
        void UpdateFooterIcon(VistaTaskDialogIcon icon);
        /// <summary>
        /// Updates the footer icon. Note the type (standard via enum or
        /// custom via Icon type) must be used when upating the icon.
        /// </summary>
        /// <param name="icon">The icon to set.</param>
        void UpdateFooterIcon(Icon icon);
    }
}
