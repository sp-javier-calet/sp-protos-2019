using UnityEngine;
using UnityEngine.Assertions;
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
        /**
         * What kind of operation should be applied to the base schedule time and an additional offset time.
         * Some notifications can be scheduled before (Negative) while other can be after (Positive) their real desired time.
         */
        public enum OffsetType
        {
            None,
            Negative,
            Positive,
        };

        public Notification(long fireDelay, OffsetType offsetType)
        {
            FireDelay = fireDelay;
            _offsetType = offsetType;
            MaxDesiredOffset = 0;
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
            get
            { 
                return _offsetType != OffsetType.None; 
            }
        }

        /// <summary>
        /// Max amount of offset this notification should have.
        /// If its set to zero but an offset is required, the default offset will be set by NotificationCenter.
        /// </summary>
        /// <value>The max desired offset.</value>
        public int MaxDesiredOffset
        {
            get;
            set;
        }

        private OffsetType _offsetType;

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
        public long FireDelay
        {
            get;
            private set;
        }

        /// <summary>
        /// Applies the desired offset directly over its actual FireDelay value. Will add or subtract it depending on its offset type.
        /// </summary>
        /// <param name="offset">Offset.</param>
        public void ApplyOffset(long offset)
        {
            Assert.IsTrue(RequiresOffset && offset > 0, "Warning: Invalid offset settings for notification");
            switch(_offsetType)
            {
            case OffsetType.Negative:
                Assert.IsTrue(FireDelay >= offset, "Warning: Notification has a negative offset that will schedule it before current time");
                FireDelay -= offset;
                break;
            case OffsetType.Positive:
                FireDelay += offset;
                break;
            default:
                break;
            }
        }

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
