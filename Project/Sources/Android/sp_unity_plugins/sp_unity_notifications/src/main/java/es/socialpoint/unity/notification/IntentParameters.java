package es.socialpoint.unity.notification;;

public final class IntentParameters {
    public static final String EXTRA_ID = "nId";
    public static final String EXTRA_ALARM_ID = "alarmId";
    public static final String EXTRA_ICON = "icon";
    public static final String EXTRA_TEXT = "text";
    public static final String EXTRA_TITLE = "title";
    public static final String EXTRA_CHANNEL_ID = "channelId";
    
    
    // This needs to match the definition on hydra/app/AppSourceHandler for notifications / app open source to work properly
    public static final String EXTRA_ORIGIN = "sp_origin";
    
    public enum Origin {
        LOCAL_NOTIFICATION("local_notification"),
        PUSH_NOTIFICATION("push_notification"),
        WIDGET("widget");
        
        private String mName;
        private Origin(String name) {
            mName = name;
        }
        
        public String getName() {
            return mName;
        }
    }
}
