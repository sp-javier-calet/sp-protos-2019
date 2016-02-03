using UnityEngine;
using System;
using System.Collections;

namespace SocialPoint.Notifications
{
    /*  DIFFERENCES (IOS - ANDROID):
            - Both IOS and ANDROID uses the default notification sound used in device
            - ANDROID deletes automatically the clicked notification
            - IOS gets the Unity app icon automatically and ANDROID needs to get the icon from Assets/Plugins/Android/res/drawable/app_icon.png file
            - For compatibility with IOS, ANDROID progress and big style notifications aren't implemented (http://developer.android.com/guide/topics/ui/notifiers/notifications.html)
    */
    public class Notification
    {
        public Notification(bool requiresOffset)
        {
            RequiresOffset = requiresOffset;
        }

        /**
         * the title of the action button or slider
         */
        public string Title = string.Empty;

        /**
         * he message displayed in the notification alert
         */
        public string Message = string.Empty;

        /// <summary>
        /// Flag to mark if the notification time may require a random offset of time applied to it
        /// </summary>
        /// <value><c>true</c> if requires offset; otherwise, <c>false</c>.</value>
        public bool RequiresOffset
        {
            get;
            private set;
        }

        [Obsolete("Use Title")]
        public string AlertAction
        {
            set
            {
                Title = value;
            }

            get
            {
                return Title;
            }
        }

        [Obsolete("Use Message")]
        public string AlertBody
        {
            set
            {
                Message = value;
            }
            
            get
            {
                return Message;
            }
        }

        /**
         * the number to display as the application's icon badge (used for IOs compatibility)
         */
        [Obsolete("Not supported anymore")]
        public int IconBadgeNumber = 0;

        /**
         * the delay in seconds from now when the system should deliver the notification
         */
        public long FireDelay = 0;

        /**
         * the local date and time when the system should deliver the notification 
         */
        [Obsolete("Use FireDelay")]
        public DateTime FireDate
        {
            get
            {
                return DateTime.Now.ToLocalTime().AddSeconds(FireDelay);
            }

            set
            {
                FireDelay = (long)value.Subtract(DateTime.Now.ToLocalTime()).TotalSeconds;
            }
        }

        /**
         * an amount of seconds after which the notification will be repeated
         */
        [Obsolete("Not supported anymore")]
        public long RepeatingSeconds;

        public override string ToString()
        {
            return string.Format("Notification: -- Title: {0}-- Message: {1}  -- FireDelay: {2}", 
                Title, 
                Message, 
                FireDelay);
        }
    }
       
}
