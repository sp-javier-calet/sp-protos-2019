using System;

namespace SocialPoint.Alert
{
    public delegate void ResultDelegate(int result);

    public interface IAlertView : IDisposable, ICloneable
    {
        /// <summary>
        /// Set the main alert view message
        /// </summary>
        /// <value>The message.</value>
        string Message { set; }

        /// <summary>
        /// Set the alert view title
        /// </summary>
        /// <value>The title.</value>
        string Title { set; }

        string Signature { set; }

        /// <summary>
        /// Set the button texts
        /// </summary>
        /// <value>The button texts.</value>
        string[] Buttons { set; }

        /// <summary>
        /// Enable showing a text input
        /// </summary>
        /// <value><c>true</c> if text input enabled; otherwise, <c>false</c>.</value>
        bool Input { set; }

        /// <summary>
        /// Gets the input text.
        /// </summary>
        /// <value>The input text.</value>
        string InputText { get; }


        /// <summary>
        /// Show the alert view.
        /// </summary>
        /// <param name="dlg">The delegate that will be called when one of the buttons is pressed</param>
        void Show(ResultDelegate dlg);


    }
}
