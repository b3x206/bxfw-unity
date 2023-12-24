namespace BXFW
{
    /// <summary>
    /// Callbacks for a script that happens to be in an object pooler root object.
    /// <br>This <b>ONLY</b> gets called for <b>root objects</b>, children objects of pooled objects don't get this callback.</br>
    /// </summary>
    public interface IPooledBehaviour
    {
        /// <summary>
        /// Called when pooled object is spawned in by the pooler.
        /// <br>This event gets called everytime the pooled object is spawned, unlike methods like 'Start'.</br>
        /// <br>Use this method if you are expecting this object to be pooled.</br>
        /// </summary>
        public void OnPoolSpawn();
        /// <summary>
        /// Called when the pooled object is no longer needed and is to be disabled.
        /// <br>This event gets called everytime the pooled object is to be disabled.</br>
        /// </summary>
        public void OnPoolDespawn();
    }
}
