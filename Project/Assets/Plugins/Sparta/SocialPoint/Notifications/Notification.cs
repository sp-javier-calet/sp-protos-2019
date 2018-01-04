using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Notifications
{
    /*  DIFFERENCES (IOS - ANDROID):
            - Both IOS and ANDROID uses the default notification sound used in device
            - ANDROID deletes automatically the clicked notification
            - IOS gets the Unity app icon automatically and ANDROID needs to get the icon from Assets/Plugins/Android/res/drawable/app_icon.png file
            - For compatibility with IOS, ANDROID progress and big style notifications aren't implemented (http://developer.android.com/guide/topics/ui/notifiers/notifications.html)
            - On ANDROID notifications go through channels, on IOS the channel is ignored
    */
    public sealed class Notification
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
            MaxOffset = _defaultMaxOffset;
        }

        /**
         * Default max offset to apply to notifications that require it.
         * (Default value of 2 hours)
         */
        static int _defaultMaxOffset = 7200;

        /**
         * Set the maximun default offset for all notifications
         */
        public static void SetDefaultMaxOffset(int maxOffset)
        {
            DebugUtils.Assert(maxOffset > 0, "Warning: Invalid default offset settings for Notification class");
            _defaultMaxOffset = maxOffset;
        }

        /**
         * The delay in seconds from now when the system should deliver the notification
         */
        long _fireDelay;

        /**
         * Amount of offset to apply to fire delay if needed
         */
        long _randomOffset;

        OffsetType _offsetType;

        /**
         * Set the maximun offset for this notification 
         */
        public int MaxOffset
        {
            set
            {
                DebugUtils.Assert(value > 0, "Warning: Invalid offset settings for notification");
                _randomOffset = RandomUtils.Range(0, value + 1); //Second param is exclusive for ints, adding 1 to include it 
            }
        }

        /**
         * the title of the action button or slider
         */
        public string Title = string.Empty;

        /**
         * the message displayed in the notification alert
         */
        public string Message = string.Empty;

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
                }
                return realFireDelay;
            }
            private set
            {
                _fireDelay = value;
            }
        }

        /**
         * the identifier of the notifications channel
         */
        public string ChannelID = string.Empty;

        public override string ToString()
        {
            return string.Format("Notification: -- Title: {0}-- Message: {1}  -- FireDelay: {2} -- Channel: {3}", 
                Title, 
                Message, 
                FireDelay,
                ChannelID);
        }
    }
       
}
