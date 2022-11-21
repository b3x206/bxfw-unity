namespace BXFW
{
    /// <summary>
    /// Basic interface for any script / component / object interactable by the player.
    /// </summary>
    public interface IPlayerInteractable
    {
        /// <summary>State of whether the interactable object allows interaction.</summary>
        bool AllowPlayerInteraction { get; }

        /// <summary>Interface command to call after interaction.</summary>
        /// <param name="player">Current interacting player.</param>
        void OnPlayerInteract(PlayerInteraction player);
    }
}