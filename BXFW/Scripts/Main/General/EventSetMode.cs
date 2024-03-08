namespace BXFW
{
    /// <summary>
    /// A way of setting the <see cref="System.MulticastDelegate"/> value if plausible.
    /// <br>
    /// <see cref="Add"/> =&gt; Adds to the <see cref="System.MulticastDelegate"/>.
    /// If this type of addition throws an exception use <see cref="Equals"/>.
    /// </br>
    /// <br>
    /// <see cref="Equals"/> =&gt; Equates to the delegate.
    /// </br>
    /// <br>
    /// <see cref="Subtract"/> =&gt; Removes from the <see cref="System.MulticastDelegate"/>.
    /// </br>
    /// </summary>
    public enum EventSetMode
    {
        Add, Equals, Subtract
    }
}
