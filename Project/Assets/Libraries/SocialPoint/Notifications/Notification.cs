using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using SocialPoint.Utils;

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
            _fireDelay = fireDelay;
            _offsetType = offsetType;
            SetMaxOffset(_defaultMaxOffset);
        }

        /**
         * Default max offset to apply to notifications that require it.
         * (Default value of 2 hours)
         */
        private static int _defaultMaxOffset = 7200;

        /**
         * Set the maximun default offset for all notifications
         */
        public static void SetDefaultMaxOffset(int maxOffset)
        {
            Assert.IsTrue(maxOffset > 0, "Warning: Invalid default offset settings for Notification class");
            _defaultMaxOffset = maxOffset;
        }

        /**
         * The delay in seconds from now when the system should deliver the notification
         */
        private long _fireDelay = 0;

        /**
         * Amount of offset to apply to fire delay if needed
         */
        private long _randomOffset = 0;

        private OffsetType _offsetType;

        /**
         * Set the maximun offset for this notification 
         */
        public void SetMaxOffset(int maxOffset)
        {
            Assert.IsTrue(maxOffset > 0, "Warning: Invalid offset settings for notification");
            _randomOffset = RandomUtils.Range(0, maxOffset + 1);//Second param is exclusive for ints, adding 1 to include it 
        }

        /**
         * the title of the action button or slider
         */
        public string Title = string.Empty;

        /**
         * he message displayed in the notification alert
         */
        public string Message = string.Empty;

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
         * The delay in seconds from now when the system should deliver the notification (taking a random offset into account if needed)
         */
        public long FireDelay
        {
            get
            {
                long realFireDelay = _fireDelay;
                switch(_offsetType)
                {
                case OffsetType.Negative:
                    realFireDelay -= _randomOffset;
                    break;
                case OffsetType.Positive:
                    realFireDelay += _randomOffset;
                    break;
                default:
                    break;
                }
                return realFireDelay;
            }
            private set
            {
                _fireDelay = value;
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
