namespace EventAggregator.Test
{
    public class Button
    {
        public delegate void Handler(object sender);
        public event Handler Click;
        public void OnClick()
        {
            // Event not subscribed
            if (Click == null)
                return;
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.

            Handler raiseEvent = Click;
            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                // Call to raise the event.
                raiseEvent(this);
            }
        }
    }
}
